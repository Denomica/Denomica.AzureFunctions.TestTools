using Azure.Data.Tables;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AzureFunctions.TestTools.InProcess
{
    public class OrchestrationContextMocker
    {
        public OrchestrationContextMocker(IServiceCollection services)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private readonly IServiceCollection _services;



        public OrchestrationContextMocker AddOrchestration<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            if(!mi.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if(!mi.HasParameterWithAttribute<OrchestrationTriggerAttribute>()) throw new ArgumentException("The given method does not have an orchestration trigger parameter.");

            var name = mi.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");

            this._services.AddSingleton<TClass>();
            var moc = this.GetMockedOrchestrationContext();

            if (mi.ReturnType.IsGenericType)
            {

            }
            else
            {
                Func<IDurableOrchestrationContext, Task> callback = (context) =>
                {
                    var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                    return mi.Invoke(svc, new object[] { context }) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
                };

                moc.Setup(x => x.CallSubOrchestratorAsync(name, It.IsAny<object>()))
                    .Returns((string fn, object input) => callback(this.GetOrchestrationContext(input)));

                moc.Setup(x => x.CallSubOrchestratorWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<object>()))
                    .Returns((string fn, RetryOptions ro, object input) => callback(this.GetOrchestrationContext(input)));
            }


            return this;
        }


        public IDurableOrchestrationContext GetOrchestrationContext()
        {
            return this.GetMockedOrchestrationContext().Object;
        }

        public IDurableOrchestrationContext GetOrchestrationContext<TInput>(TInput input)
        {
            var moc = this.GetMockedOrchestrationContext();

            moc.Setup(x => x.GetInput<TInput>()).Returns(input);

            return moc.Object;
        }

        public TService? GetService<TService>() where TService : class
        {
            return this.GetServiceProvider().GetService<TService>();
        }

        public object? GetService(Type serviceType)
        {
            return this.GetServiceProvider().GetService(serviceType);
        }

        public TService GetRequiredService<TService>() where TService : class
        {
            return this.GetServiceProvider().GetRequiredService<TService>();
        }

        public object GetRequiredService(Type serviceType)
        {
            return this.GetServiceProvider().GetRequiredService(serviceType);
        }



        private Mock<IDurableOrchestrationContext>? _Mock;
        private Mock<IDurableOrchestrationContext> GetMockedOrchestrationContext()
        {
            if(null == _Mock)
            {
                _Mock = new Mock<IDurableOrchestrationContext>(MockBehavior.Strict);
            }

            return _Mock;
        }

        private IServiceProvider GetServiceProvider()
        {
            return this._services.BuildServiceProvider();
        }

    }
}

using Azure.Data.Tables;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using Microsoft.AspNetCore.Mvc;
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
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace Denomica.AzureFunctions.TestTools.InProcess
{
    public class OrchestrationContextMocker
    {
        public OrchestrationContextMocker()
        {
            this._services = new ServiceCollection();
        }

        public OrchestrationContextMocker(IServiceCollection services)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private readonly IServiceCollection _services;


        public OrchestrationContextMocker AddActivity<TClass>(Expression<Func<TClass, Func<IDurableActivityContext, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableActivityContext, Task> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [input]) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivity(expression, callback);
        }

        public OrchestrationContextMocker AddActivity<TClass>(Expression<Func<TClass, Func<IDurableActivityContext, Task>>> expression, Func<IDurableActivityContext, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallActivityAsync(name, It.IsAny<IDurableActivityContext>()))
                .Returns((string fn, IDurableActivityContext input) => callback(input));

            moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<IDurableActivityContext>()))
                .Returns((string fn, RetryOptions ro, IDurableActivityContext input) => callback(input));

            return this;
        }

        public OrchestrationContextMocker AddActivity<TClass, TResult>(Expression<Func<TClass, Func<IDurableActivityContext, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableActivityContext, Task<TResult>> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [input]) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivity(expression, callback);
        }

        public OrchestrationContextMocker AddActivity<TClass, TResult>(Expression<Func<TClass, Func<IDurableActivityContext, Task<TResult>>>> expression, Func<IDurableActivityContext, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallActivityAsync<TResult>(name, It.IsAny<IDurableActivityContext>()))
                .Returns((string fn, IDurableActivityContext input) => callback(input));

            moc.Setup(x => x.CallActivityWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<IDurableActivityContext>()))
                .Returns((string fn, RetryOptions ro, IDurableActivityContext input) => callback(input));

            return this;
        }

        public OrchestrationContextMocker AddActivity<TClass, TInput>(Expression<Func<TClass, Func<TInput, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<TInput, Task> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [input]) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivity(expression, callback);
        }

        public OrchestrationContextMocker AddActivity<TClass, TInput>(Expression<Func<TClass, Func<TInput, Task>>> expression, Func<TInput, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallActivityAsync(name, It.IsAny<TInput>()))
                .Returns((string fn, TInput input) => callback(input));

            moc.Setup(x => x.CallActivityWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<TInput>()))
                .Returns((string fn, RetryOptions ro, TInput input) => callback(input));

            return this;
        }

        public OrchestrationContextMocker AddActivity<TClass, TInput, TResult>(Expression<Func<TClass, Func<TInput, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            Func<TInput, Task<TResult>> callback = input =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [input]) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddActivity(expression, callback);
        }

        public OrchestrationContextMocker AddActivity<TClass, TInput, TResult>(Expression<Func<TClass, Func<TInput, Task<TResult>>>> expression, Func<TInput, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateActivityMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallActivityAsync<TResult>(name, It.IsAny<TInput>()))
                .Returns((string fn, TInput input) => callback(input));

            moc.Setup(x => x.CallActivityWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<TInput>()))
                .Returns((string fn, RetryOptions ro, TInput input) => callback(input));

            return this;
        }



        public OrchestrationContextMocker AddOrchestration<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableOrchestrationContext, Task> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [context]) as Task ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddOrchestration(expression, callback);
        }

        public OrchestrationContextMocker AddOrchestration<TClass>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task>>> expression, Func<IDurableOrchestrationContext, Task> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallSubOrchestratorAsync(name, It.IsAny<object>()))
                .Returns((string fn, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorAsync(name, It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string fn, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .Returns((string fn, RetryOptions ro, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorWithRetryAsync(name, It.IsAny<RetryOptions>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string fn, RetryOptions ro, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

            return this;
        }

        public OrchestrationContextMocker AddOrchestration<TClass, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task<TResult>>>> expression) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            Func<IDurableOrchestrationContext, Task<TResult>> callback = context =>
            {
                var svc = this.GetServiceProvider().GetRequiredService<TClass>();
                return mi.Invoke(svc, [context]) as Task<TResult> ?? throw new Exception($"The function '{name}' does not return a '{typeof(Task).FullName}' object.");
            };

            return this.AddOrchestration(expression, callback);
        }

        public OrchestrationContextMocker AddOrchestration<TClass, TResult>(Expression<Func<TClass, Func<IDurableOrchestrationContext, Task<TResult>>>> expression, Func<IDurableOrchestrationContext, Task<TResult>> callback) where TClass : class
        {
            MethodInfo mi = expression.ToMethodInfo() ?? throw new ArgumentException("The given expression is not a method expression.");
            this.ValidateOrchestrationMethod<TClass>(mi, out var name, out var moc);

            moc.Setup(x => x.CallSubOrchestratorAsync<TResult>(name, It.IsAny<object>()))
                .Returns((string fn, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorAsync<TResult>(name, It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string fn, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<object>()))
                .Returns((string fn, RetryOptions ro, object input) => callback(this.GetOrchestrationContext(input)));

            moc.Setup(x => x.CallSubOrchestratorWithRetryAsync<TResult>(name, It.IsAny<RetryOptions>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string fn, RetryOptions ro, string instanceId, object input) => callback(this.GetOrchestrationContext(input)));

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

        private void ValidateOrchestrationMethod<TClass>(MethodInfo? method, out string name, out Mock<IDurableOrchestrationContext> mock) where TClass : class
        {
            if(null == method) throw new ArgumentNullException(nameof(method), "Not a method.");
            if (!method.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if (!method.HasParameterWithAttribute<OrchestrationTriggerAttribute>()) throw new ArgumentException("The given method does not have an orchestration trigger parameter.");

            name = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");

            this._services.AddSingleton<TClass>();
            mock = this.GetMockedOrchestrationContext();
        }

        private void ValidateActivityMethod<TClass>(MethodInfo? method, out string name, out Mock<IDurableOrchestrationContext> mock) where TClass : class
        {
            if (null == method) throw new ArgumentNullException(nameof(method), "Not a method.");
            if (!method.HasAttribute<FunctionNameAttribute>()) throw new ArgumentException($"The specified method is not a function method. It lacks the '{typeof(FunctionNameAttribute).FullName}' attribute.");
            if (!method.HasParameterWithAttribute<ActivityTriggerAttribute>()) throw new ArgumentException("The given method does not have an activity trigger parameter.");

            name = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? throw new Exception("This exception should never be thrown since we checked the attribute earlier");

            this._services.AddSingleton<TClass>();
            mock = this.GetMockedOrchestrationContext();

        }
    }
}

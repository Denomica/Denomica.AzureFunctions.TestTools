using Denomica.AzureFunctions.TestTools.InProcess;
using Denomica.AzureFunctions.TestTools.Core;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using InProcessDurableFunctions;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Azure;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace InProcessUnitTests
{
    [TestClass]
    public class BusinessLogicTests
    {
        [TestMethod]
        public async Task Test01()
        {
            var mocker = this.GetMocker();

            var context = mocker.GetOrchestrationContext("fi 1234567-8");
            var blf = mocker.GetRequiredService<OrchestrationFunctions>();
            var validationResult = await blf.ValidateBusinessIdOrchestration(context);
        }

        [TestMethod]
        public async Task Test02()
        {
            var mocker = this.GetMocker()
                .AddOrchestrationFunction<LoggingFunctions>(x => x.LogMessageOrchestration, (ctx) => {
                    var msg = ctx.GetInput<string>();
                    return Task.CompletedTask;
                });
                ;

            var context = mocker.GetOrchestrationContext("fi 989765433");
            var blf = mocker.GetRequiredService<OrchestrationFunctions>();
            var result = await blf.ValidateBusinessIdOrchestration(context);
        }

        [TestMethod]
        public async Task Test03()
        {
            var mocker = this.GetMocker();
            var result = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, int>(x => x.IncreaseCounterOrchestration);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public async Task Test04()
        {
            bool entityWasCalled = false;
            var mocker = this.GetMocker()
                .AddEntityFunction<EntityFunctions>(x => x.VoidEntity, context =>
                {
                    entityWasCalled = true;
                });

            await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions>(x => x.CallVoidEntityOrchestration);

            Assert.IsTrue(entityWasCalled);
        }


        private OrchestrationContextMocker GetMocker()
        {
            var services = Services.GetServices();
            services
                .AddSingleton<OrchestrationContextMocker>(sp =>
                {
                    return new OrchestrationContextMocker(services: services, behavior: Moq.MockBehavior.Strict)
                        .AddOrchestrationFunction<OrchestrationFunctions, bool>(x => x.ValidateBusinessIdOrchestration)
                        .AddOrchestrationFunction<OrchestrationFunctions, string>(x => x.NormalizeBusinessIdOrchestration)
                        .AddOrchestrationFunction<LoggingFunctions>(x => x.LogMessageOrchestration)
                        .AddOrchestrationFunction<OrchestrationFunctions>(x => x.IncreaseCounterOrchestration)
                        .AddOrchestrationFunction<OrchestrationFunctions>(x => x.CallVoidEntityOrchestration)

                        .AddActivityFunction<LoggingFunctions, string>(x => x.LogMessageActivity)
                        .AddActivityFunction<ActivityFunctions, string, string>(x => x.NormalizeBusinessIdActivity)

                        .AddEntityFunction<EntityFunctions, int>(x => x.GenericCounterEntity)
                        .AddEntityFunction<EntityFunctions>(x => x.VoidEntity)
                        ;
                })
                .AddSingleton<EntityContextMocker>(sp =>
                {
                    return new EntityContextMocker(services: services, behavior: Moq.MockBehavior.Strict)
                        ;
                })

                ;

            return services.BuildServiceProvider().GetRequiredService<OrchestrationContextMocker>();
        }
    }
}
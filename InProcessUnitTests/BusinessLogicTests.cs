using Denomica.AzureFunctions.TestTools.InProcess;
using Denomica.AzureFunctions.TestTools.Core;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using InProcessDurableFunctions;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Azure;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

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
            var blf = mocker.GetRequiredService<BusinessLogicFunctions>();
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
            var blf = mocker.GetRequiredService<BusinessLogicFunctions>();
            var result = await blf.ValidateBusinessIdOrchestration(context);
        }

        [TestMethod]
        public async Task Test03()
        {
            var mocker = this.GetMocker();
            var context = mocker.GetOrchestrationContext();
            var blf = mocker.GetRequiredService<BusinessLogicFunctions>();
            var result = await blf.IncreaseCounterOrchestration(context);

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

            await mocker.CallOrchestrationFunctionAsync<BusinessLogicFunctions>(x => x.CallVoidEntityOrchestration);

            Assert.IsTrue(entityWasCalled);
        }


        private OrchestrationContextMocker GetMocker()
        {
            return new OrchestrationContextMocker(Services.GetServices(), Moq.MockBehavior.Strict)
                .AddOrchestrationFunction<BusinessLogicFunctions, bool>(x => x.ValidateBusinessIdOrchestration)
                .AddOrchestrationFunction<BusinessLogicFunctions, string>(x => x.NormalizeBusinessIdOrchestration)
                .AddOrchestrationFunction<LoggingFunctions>(x => x.LogMessageOrchestration)
                .AddOrchestrationFunction<BusinessLogicFunctions>(x => x.IncreaseCounterOrchestration)
                .AddOrchestrationFunction<BusinessLogicFunctions>(x => x.CallVoidEntityOrchestration)

                .AddActivityFunction<LoggingFunctions, string>(x => x.LogMessageActivity)
                .AddActivityFunction<BusinessLogicFunctions, string, string>(x => x.NormalizeBusinessIdActivity)

                .AddEntityFunction<EntityFunctions, int>(x => x.GenericCounterEntity)
                .AddEntityFunction<EntityFunctions>(x => x.VoidEntity)

                .AddMocker(new EntityContextMocker(Services.GetServices()))
                ;
        }
    }
}
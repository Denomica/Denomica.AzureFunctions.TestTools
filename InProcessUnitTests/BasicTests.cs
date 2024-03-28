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
    public class BasicTests
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

        [TestMethod]
        public async Task Test05()
        {
            var mocker = this.GetMocker();
            var id = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, string, string>(x => x.NormalizeBusinessIdOrchestration, "fi 12345678-9");
            Assert.AreEqual("FI123456789", id);
        }

        [TestMethod]
        public async Task Test06()
        {
            var mocker = Services.GetServices()
                .AddOrchestrationContextMocker()
                    .AddOrchestrationFunction<OrchestrationFunctions>(x => x.NormalizeBusinessIdOrchestration)
                    .AddActivityFunction<ActivityFunctions, string, string>(x => x.NormalizeBusinessIdActivity);

            var id = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, string, string>(x => x.NormalizeBusinessIdOrchestration, "fi 12345678");
            Assert.AreEqual("FI12345678", id);
        }

        [TestMethod]
        public async Task Test07()
        {
            var startDate = DateTimeOffset.UtcNow;

            // Call into an orchestration and expect no error.
            var mocker = this.GetMocker();
            var dt = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, DateTimeOffset>(x => x.GetCurrentDateTimeOrchestration);
            Assert.IsTrue(dt > startDate);
        }

        [TestMethod]
        public async Task Test08()
        {
            var mocker = this.GetMocker();
            var dt1 = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, DateTimeOffset>(x => x.GetCurrentDateTimeOrchestration);
            var dt2 = await mocker.CallOrchestrationFunctionAsync<OrchestrationFunctions, DateTimeOffset>(x => x.GetCurrentDateTimeOrchestration);
            Assert.AreNotEqual(dt1, dt2);
        }


        private OrchestrationContextMocker GetMocker()
        {
            return Services.GetServices()
                .AddOrchestrationContextMocker(behavior: Moq.MockBehavior.Strict)
                    .AddCurrentUtcDateTime()
                    
                    .AddOrchestrationFunction<OrchestrationFunctions, bool>(x => x.ValidateBusinessIdOrchestration)
                    .AddOrchestrationFunction<OrchestrationFunctions, string>(x => x.NormalizeBusinessIdOrchestration)
                    .AddOrchestrationFunction<LoggingFunctions>(x => x.LogMessageOrchestration)
                    .AddOrchestrationFunction<OrchestrationFunctions>(x => x.IncreaseCounterOrchestration)
                    .AddOrchestrationFunction<OrchestrationFunctions>(x => x.CallVoidEntityOrchestration)
                    .AddOrchestrationFunction<OrchestrationFunctions>(x => x.GetCurrentDateTimeOrchestration)

                    .AddActivityFunction<LoggingFunctions, string>(x => x.LogMessageActivity)
                    .AddActivityFunction<ActivityFunctions, string, string>(x => x.NormalizeBusinessIdActivity)
                    .AddActivityFunction<ActivityFunctions>(x => x.NoUseActivity)

                    .AddEntityFunction<EntityFunctions, int>(x => x.GenericCounterEntity)
                    .AddEntityFunction<EntityFunctions>(x => x.VoidEntity)
                .Services
                    
                .AddEntityContextMocker(behavior: Moq.MockBehavior.Strict)
                .Services

                .BuildServiceProvider()
                .GetRequiredService<OrchestrationContextMocker>();
        }

    }
}
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
            var mocker = new OrchestrationContextMocker(Services.GetServices())
                .AddOrchestration<BusinessLogicFunctions, bool>(x => x.ValidateBusinessIdOrchestration)
                .AddOrchestration<BusinessLogicFunctions, string>(x => x.NormalizeBusinessIdOrchestration)
                .AddOrchestration<LoggingFunctions>(x => x.LogMessageOrchestration, (ctx) => {
                    var msg = ctx.GetInput<string>();
                    return Task.CompletedTask;
                });
                ;

            var context = mocker.GetOrchestrationContext("fi 989765433");
            var blf = mocker.GetRequiredService<BusinessLogicFunctions>();
            var result = await blf.ValidateBusinessIdOrchestration(context);
        }



        private OrchestrationContextMocker GetMocker()
        {
            return new OrchestrationContextMocker(Services.GetServices())
                .AddOrchestration<BusinessLogicFunctions, bool>(x => x.ValidateBusinessIdOrchestration)
                .AddOrchestration<BusinessLogicFunctions, string>(x => x.NormalizeBusinessIdOrchestration)
                .AddOrchestration<LoggingFunctions>(x => x.LogMessageOrchestration)

                .AddActivity<LoggingFunctions, string>(x => x.LogMessageActivity)
                .AddActivity<BusinessLogicFunctions, string, string>(x => x.NormalizeBusinessIdActivity)
                ;
        }
    }
}
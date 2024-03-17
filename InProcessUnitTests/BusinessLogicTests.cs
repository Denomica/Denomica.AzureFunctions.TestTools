using Denomica.AzureFunctions.TestTools.InProcess;
using Denomica.AzureFunctions.TestTools.Core;
using Denomica.AzureFunctions.TestTools.Core.Reflection;
using InProcessDurableFunctions;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace InProcessUnitTests
{
    [TestClass]
    public class BusinessLogicTests
    {
        [TestMethod]
        public async Task Test01()
        {
            var mocker = new OrchestrationContextMocker(Services.GetServices())
                .AddOrchestration<BusinessLogicFunctions>(x => x.ValidateBusinessIdOrchestration)
                .AddOrchestration<BusinessLogicFunctions>(x => x.DoPeriodicStuffOrchestration)
                .AddOrchestration<LoggingFunctions>(x => x.LogMessageOrchestration)
                ;

            var context = mocker.GetOrchestrationContext("FI12345678");
            var blf = mocker.GetRequiredService<BusinessLogicFunctions>();
            var validationResult = await blf.ValidateBusinessIdOrchestration(context);
        }
    }
}
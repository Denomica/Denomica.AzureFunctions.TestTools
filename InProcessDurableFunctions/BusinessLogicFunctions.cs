using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InProcessDurableFunctions
{
    public class BusinessLogicFunctions
    {
        public BusinessLogicFunctions(ILogger<BusinessLogicFunctions> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly ILogger<BusinessLogicFunctions> Logger;

        [FunctionName(nameof(DoPeriodicStuffOrchestration))]
        public async Task DoPeriodicStuffOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {

        }

        [FunctionName(nameof(ValidateBusinessIdOrchestration))]
        public async Task<bool> ValidateBusinessIdOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var businessId = context.GetInput<string>();
            await context.CallSubOrchestratorAsync(nameof(LoggingFunctions.LogMessageOrchestration), $"Validating Business ID: '{businessId}'");

            return true;
        }

    }
}

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
    public class OrchestrationFunctions
    {
        public OrchestrationFunctions(ILogger<OrchestrationFunctions> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly ILogger<OrchestrationFunctions> Logger;


        [FunctionName(nameof(CallVoidEntityOrchestration))]
        public async Task CallVoidEntityOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallEntityAsync(new EntityId(nameof(EntityFunctions.VoidEntity), "foo"), "bar");
        }

        [FunctionName(nameof(GetCurrentDateTimeOrchestration))]
        public Task<DateTimeOffset> GetCurrentDateTimeOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            DateTimeOffset dto = context.CurrentUtcDateTime;
            return Task.FromResult(dto);
        }

        [FunctionName(nameof(IncreaseCounterOrchestration))]
        public async Task<int> IncreaseCounterOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return await context.CallEntityAsync<int>(new EntityId(nameof(EntityFunctions.GenericCounterEntity), "foo"), "bar");
        }

        [FunctionName(nameof(ValidateBusinessIdOrchestration))]
        public async Task<bool> ValidateBusinessIdOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var businessId = context.GetInput<string>();
            await context.CallSubOrchestratorAsync(nameof(LoggingFunctions.LogMessageOrchestration), $"Validating Business ID: '{businessId}'");
            var normalized = await context.CallSubOrchestratorAsync<string>(nameof(NormalizeBusinessIdOrchestration), businessId);

            return true;
        }

        [FunctionName(nameof(NormalizeBusinessIdOrchestration))]
        public async Task<string> NormalizeBusinessIdOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<string>();
            return await context.CallActivityAsync<string>(nameof(ActivityFunctions.NormalizeBusinessIdActivity), input);
        }

    }
}

using Castle.Core.Logging;
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
    public class LoggingFunctions
    {
        public LoggingFunctions(ILogger<LoggingFunctions> logger)
        {
            this.Logger = logger;
        }

        private readonly ILogger<LoggingFunctions> Logger;


        [FunctionName(nameof(LogMessageOrchestration))]
        public async Task LogMessageOrchestration([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var message = context.GetInput<string>();

        }
    }
}

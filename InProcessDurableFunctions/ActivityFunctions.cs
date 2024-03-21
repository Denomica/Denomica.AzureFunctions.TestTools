using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InProcessDurableFunctions
{
    public class ActivityFunctions
    {
        public ActivityFunctions(ILogger<ActivityFunctions> logger)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly ILogger<ActivityFunctions> Logger;



        [FunctionName(nameof(NormalizeBusinessIdActivity))]
        public async Task<string> NormalizeBusinessIdActivity([ActivityTrigger] string input)
        {
            return input
                ?.Replace(" ", "")
                ?.Replace("-", "")
                ?.ToUpper();

        }
    }
}

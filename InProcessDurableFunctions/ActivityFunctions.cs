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


        [FunctionName(nameof(ActivityWithoutInputOrOutput))]
        public Task ActivityWithoutInputOrOutput([ActivityTrigger] IDurableActivityContext context)
        {
            return Task.CompletedTask;
        }

        [FunctionName(nameof(ActivityWithStringOutputOnly))]
        public Task<string> ActivityWithStringOutputOnly([ActivityTrigger] IDurableActivityContext context)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        [FunctionName(nameof(ActivityWithStringInputAndNoOutput))]
        public Task ActivityWithStringInputAndNoOutput([ActivityTrigger] string input)
        {
            return Task.CompletedTask;
        }

        [FunctionName(nameof(ActivityWithStringInputAndStringOutput))]
        public Task<string> ActivityWithStringInputAndStringOutput([ActivityTrigger] string input)
        {
            return Task.FromResult(input);
        }

        [FunctionName(nameof(NormalizeBusinessIdActivity))]
        public Task<string> NormalizeBusinessIdActivity([ActivityTrigger] string input)
        {
            return Task.FromResult(input
                ?.Replace(" ", "")
                ?.Replace("-", "")
                ?.ToUpper());

        }

        [FunctionName(nameof(NoUseActivity))]
        public Task NoUseActivity([ActivityTrigger] IDurableActivityContext context)
        {

            return Task.CompletedTask;
        }

        [FunctionName(nameof(GetDateTimeActivity))]
        public Task<DateTime> GetDateTimeActivity([ActivityTrigger] IDurableActivityContext context)
        {
            return Task.FromResult(DateTime.Now);
        }

    }
}

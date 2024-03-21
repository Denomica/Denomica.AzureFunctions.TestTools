using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InProcessDurableFunctions
{
    public class EntityFunctions
    {

        [FunctionName(nameof(GenericCounterEntity))]
        public void GenericCounterEntity([EntityTrigger] IDurableEntityContext context)
        {
            var counter = context.GetState<int>() + 1;
            context.SetState(counter);
            context.Return(counter);
        }

        [FunctionName(nameof(VoidEntity))]
        public void VoidEntity([EntityTrigger] IDurableEntityContext context)
        {
            var id = context.EntityId.ToString();
        }
    }
}

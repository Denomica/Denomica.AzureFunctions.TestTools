using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        }
    }
}

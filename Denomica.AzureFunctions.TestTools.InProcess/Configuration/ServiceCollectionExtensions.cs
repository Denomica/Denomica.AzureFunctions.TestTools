using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Denomica.AzureFunctions.TestTools.InProcess;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static OrchestrationContextMocker AddOrchestrationContextMocker(this IServiceCollection services, Moq.MockBehavior behavior = Moq.MockBehavior.Strict)
        {
            var mocker = new OrchestrationContextMocker(services, behavior);
            services.AddSingleton<OrchestrationContextMocker>(mocker);
            return mocker;
        }

        public static EntityContextMocker AddEntityContextMocker(this IServiceCollection services, Moq.MockBehavior behavior = Moq.MockBehavior.Strict)
        {
            var mocker = new EntityContextMocker(services, behavior);
            services.AddSingleton<EntityContextMocker>(mocker);
            return mocker;
        }

    }
}

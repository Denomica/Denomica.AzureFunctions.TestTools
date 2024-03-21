using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Denomica.AzureFunctions.TestTools.InProcess;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for <c>Denomica.AzureFunctions.TestTools.InProcess</c>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds an <see cref="OrchestrationContextMocker"/> instance to the service collection as a singleton service.
        /// </summary>
        /// <param name="services">The service collection to add the mocker instance to.</param>
        /// <param name="behavior">The behavior to configure the mocker with. Defaults to <see cref="Moq.MockBehavior.Strict"/>.</param>
        public static OrchestrationContextMocker AddOrchestrationContextMocker(this IServiceCollection services, Moq.MockBehavior behavior = Moq.MockBehavior.Strict)
        {
            var mocker = new OrchestrationContextMocker(services, behavior);
            services.AddSingleton<OrchestrationContextMocker>(mocker);
            return mocker;
        }

        /// <summary>
        /// Adds an <see cref="EntityContextMocker"/> instance to the service collection as a singleton service.
        /// </summary>
        /// <param name="services">The service collection to add the mocker instance to.</param>
        /// <param name="behavior">The behavior to configure the mocker with. Defaults to <see cref="Moq.MockBehavior.Strict"/>.</param>
        public static EntityContextMocker AddEntityContextMocker(this IServiceCollection services, Moq.MockBehavior behavior = Moq.MockBehavior.Strict)
        {
            var mocker = new EntityContextMocker(services, behavior);
            services.AddSingleton<EntityContextMocker>(mocker);
            return mocker;
        }

    }
}

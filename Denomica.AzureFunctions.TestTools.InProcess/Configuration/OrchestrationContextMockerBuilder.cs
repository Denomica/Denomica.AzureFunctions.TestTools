using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.AzureFunctions.TestTools.InProcess.Configuration
{
    /// <summary>
    /// A builder class that is used during service registration of <see cref="OrchestrationContextMocker"/>.
    /// </summary>
    public class OrchestrationContextMockerBuilder
    {
        /// <summary>
        /// Creates a new instance of the builder.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public OrchestrationContextMockerBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Returns the services collection.
        /// </summary>
        public IServiceCollection Services { get => _services; }

        private readonly IServiceCollection _services;


    }
}

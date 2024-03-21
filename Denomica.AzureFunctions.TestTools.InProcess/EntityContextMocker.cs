using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denomica.AzureFunctions.TestTools.InProcess
{
    /// <summary>
    /// A class that facilitates mocking <see cref="IDurableEntityContext"/> instances.
    /// </summary>
    public class EntityContextMocker
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="services">The service collection to work with.</param>
        /// <param name="behavior">The mocking behavior.</param>
        public EntityContextMocker(IServiceCollection? services = null, MockBehavior behavior = MockBehavior.Strict)
        {
            this.Services = services ?? new ServiceCollection();
            this._behavior = behavior;
        }

        private readonly MockBehavior _behavior;

        private Dictionary<string, Action> MockSetups = new Dictionary<string, Action>();


        /// <summary>
        /// Returns the service collection the mocker is configured in.
        /// </summary>
        public IServiceCollection Services { get; private set; }


        /// <summary>
        /// Returns a mocked durable entity context instance.
        /// </summary>
        public IDurableEntityContext GetEntityContext()
        {
            this.ApplySetups(this._mock);
            return this._mock.Object;
        }

        /// <summary>
        /// Returns a mocked durable entity context instance where <see cref="IDurableEntityContext.GetInput(Type)"/> returns <paramref name="input"/>.
        /// </summary>
        /// <typeparam name="TInput">The type of the input.</typeparam>
        /// <param name="input">The input to specify in the mocked context instance.</param>
        public IDurableEntityContext GetEntityContext<TInput>(TInput input)
        {
            this._mock.Setup(x => x.GetInput<TInput>()).Returns(input);
            this.ApplySetups(this._mock);
            return this._mock.Object;
        }

        private Mock<IDurableEntityContext> _mock = new Mock<IDurableEntityContext>();

        private void ApplySetups(Mock<IDurableEntityContext> mock)
        {
            this.MockSetups.Values.ToList().ForEach(x => x());
            this.MockSetups.Clear();
        }

        private IServiceProvider GetServiceProvider()
        {
            return this.Services.BuildServiceProvider();
        }

    }
}

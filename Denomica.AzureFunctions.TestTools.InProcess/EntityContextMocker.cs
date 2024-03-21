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
    public class EntityContextMocker
    {
        public EntityContextMocker(IServiceCollection? services = null, MockBehavior behavior = MockBehavior.Strict)
        {
            this._services = services ?? new ServiceCollection();
            this._behavior = behavior;
        }

        private readonly IServiceCollection _services;
        private readonly MockBehavior _behavior;

        private Dictionary<string, Action> MockSetups = new Dictionary<string, Action>();



        public IDurableEntityContext GetEntityContext()
        {
            this.ApplySetups(this._mock);
            return this._mock.Object;
        }

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
            return this._services.BuildServiceProvider();
        }

    }
}

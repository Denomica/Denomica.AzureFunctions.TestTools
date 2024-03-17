using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InProcessUnitTests
{
    public static class Services
    {

        public static IServiceCollection GetServices()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddHttpClient()
                .AddOptions()

                ;
        }
    }
}

using CodeDesignPlus.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeDesignPlus.Redis.Event.Bus.Test.Helpers
{
    public class StartupLogic : IStartupServices
    {
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}

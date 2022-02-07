using Microsoft.Extensions.DependencyInjection;
using System;

namespace CodeDesignPlus.Redis.Event.Bus.Extensions
{
    /// <summary>
    /// Provides extension methods to register library services
    /// </summary>
    public static class RedisEventBusExtensions
    {
        /// <summary>
        /// Adds services for redis to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <exception cref="ArgumentNullException">services is null</exception>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddRedisEventBusService(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IRedisEventBusService, RedisEventBusService>();

            return services;
        }
    }
}

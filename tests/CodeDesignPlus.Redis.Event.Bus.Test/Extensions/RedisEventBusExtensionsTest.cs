using CodeDesignPlus.Redis.Event.Bus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace CodeDesignPlus.Redis.Event.Bus.Test.Extensions
{
    /// <summary>
    /// Unit test to <see cref="RedisEventBusExtensions"
    /// </summary>
    public class RedisEventBusExtensionsTestt
    {
        /// <summary>
        /// Verifies that throw exception when argument is null
        /// </summary>
        [Fact]
        public void AddRedisEventBusService_ServiceIsNull_ArgumentNullException()
        {
            // Arrange
            ServiceCollection serviceCollection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddRedisEventBusService());
        }

        /// <summary>
        /// Verifies that add service as singleton
        /// </summary>
        [Fact]
        public void AddRedisEventBusService_RegisterService_Success()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act 
            serviceCollection.AddRedisEventBusService();

            // Assert
            var service = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(IRedisEventBusService) && x.ImplementationType == typeof(RedisEventBusService));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }
    }
}

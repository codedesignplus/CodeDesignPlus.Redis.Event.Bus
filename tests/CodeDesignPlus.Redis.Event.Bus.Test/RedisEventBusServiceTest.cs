using CodeDesignPlus.Event.Bus;
using CodeDesignPlus.Event.Bus.Extensions;
using CodeDesignPlus.Event.Bus.Internal.Queue;
using CodeDesignPlus.Redis.Event.Bus.Extensions;
using CodeDesignPlus.Redis.Event.Bus.Test.Helpers;
using CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Events;
using CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Memory;
using CodeDesignPlus.Redis.Extension;
using CodeDesignPlus.Redis.Option;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CodeDesignPlus.Redis.Event.Bus.Test
{
    /// <summary>
    /// Unit test to <see cref="RedisEventBusService"/>
    /// </summary>
    public class RedisEventBusServiceTest
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initialize a new instance of the <see cref="RedisEventBusServiceTest"/>
        /// </summary>
        public RedisEventBusServiceTest()
        {
            this.configuration = CreateConfiguration();
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when redis service is null
        /// </summary>
        [Fact]
        public void Constructor_RedisIsNull_ArgumentNullException()
        {
            // Arrange
            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RedisEventBusService(null, subscriptionManager, serviceProvider, logger));
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when subscription manager is null
        /// </summary>
        [Fact]
        public void Constructor_SubscriptionManagerIsNull_ArgumentNullException()
        {
            // Arrange
            var redisService = Mock.Of<IRedisService>();
            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RedisEventBusService(redisService, null, serviceProvider, logger));
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when service provider is null
        /// </summary>
        [Fact]
        public void Constructor_ServiceProviderIsNull_ArgumentNullException()
        {
            // Arrange
            var redisService = Mock.Of<IRedisService>();
            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RedisEventBusService(redisService, subscriptionManager, null, logger));
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when logger manager is null
        /// </summary>
        [Fact]
        public void Constructor_LoggerIsNull_ArgumentNullException()
        {
            // Arrange
            var redisService = Mock.Of<IRedisService>();
            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RedisEventBusService(redisService, subscriptionManager, serviceProvider, null));
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when @event is null
        /// </summary>
        [Fact]
        public void PublishAsync_EventIsNull_ArgumentNullException()
        {
            // Arrange
            var redisService = Mock.Of<IRedisService>();
            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            var redisEventBusService = new RedisEventBusService(redisService, subscriptionManager, serviceProvider, logger);

            // Act & Arc
            Assert.ThrowsAsync<ArgumentNullException>(() => redisEventBusService.PublishAsync(null, CancellationToken.None));
        }

        /// <summary>
        /// Verifies that invoke method publish of <see cref="ISubscriber"/>
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PublishAsync_InvokePublish_Succeses(bool methodGeneric)
        {
            // Arrange
            var @event = new UserCreatedEvent()
            {
                Id = 1,
                Names = "Code",
                Lastnames = "Design Plus",
                UserName = "coded",
                Birthdate = new DateTime(2019, 11, 21)
            };

            var @eventSend = new UserCreatedEvent();

            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            var number = (long)new Random().Next(0, int.MaxValue);

            var subscriber = new Mock<ISubscriber>();

            subscriber
                .Setup(x => x.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(number)
                .Callback<RedisChannel, RedisValue, CommandFlags>((channel, value, commandFlags) =>
                {
                    @eventSend = JsonConvert.DeserializeObject<UserCreatedEvent>(value);
                });

            var redisService = new Mock<IRedisService>();

            redisService.SetupGet(x => x.Subscriber).Returns(subscriber.Object);

            var redisEventBusService = new RedisEventBusService(redisService.Object, subscriptionManager, serviceProvider, logger);

            // Act
            if (methodGeneric)
            {
                await redisEventBusService.PublishAsync(@event, CancellationToken.None);
            }
            else
            {
                var notified = await redisEventBusService.PublishAsync<long>(@event, CancellationToken.None);

                Assert.Equal(number, notified);
            }

            // Assert
            Assert.Equal(@event.Id, @eventSend.Id);
            Assert.Equal(@event.Names, @eventSend.Names);
            Assert.Equal(@event.Lastnames, @eventSend.Lastnames);
            Assert.Equal(@event.UserName, @eventSend.UserName);
            Assert.Equal(@event.Birthdate, @eventSend.Birthdate);
        }

        /// <summary>
        /// Verifies that throw ArgumentNullException when @event is null
        /// </summary>
        [Fact]
        public void PublishAsyncGeneric_EventIsNull_ArgumentNullException()
        {
            // Arrange
            var redisService = Mock.Of<IRedisService>();
            var subscriptionManager = Mock.Of<ISubscriptionManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();

            var redisEventBusService = new RedisEventBusService(redisService, subscriptionManager, serviceProvider, logger);

            // Act & Arc
            Assert.ThrowsAsync<ArgumentNullException>(() => redisEventBusService.PublishAsync<long>(null, CancellationToken.None));
        }

        /// <summary>
        /// Verifies that invoke event handler <see cref="UserCreatedEventHandler" /> after the publish event
        /// </summary>
        [Fact]
        public void PublishEvent_InvokeHandler_CheckEvent()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IMemoryService, MemoryService>();

            serviceCollection
                .AddLogging()
                .AddRedisService(this.configuration)
                .AddRedisEventBusService()
                .AddEventBus()
                .AddEventsHandlers<StartupLogic>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.SubscribeEventsHandlers<StartupLogic>();

            var memory = serviceProvider.GetService<IMemoryService>();

            var evenBus = serviceProvider.GetService<IRedisEventBusService>();

            var queue = serviceProvider.GetService<IQueueService<UserCreatedEventHandler, UserCreatedEvent>>();

            var @event = new UserCreatedEvent()
            {
                Id = 1,
                Names = "Code",
                Lastnames = "Design Plus",
                UserName = "coded",
                Birthdate = new DateTime(2019, 11, 21)
            };

            _ = Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (memory.UserEventTrace.Any(x => x.Id == 1))
                    {
                        cancellationTokenSource.Cancel();
                    }

                    Thread.Sleep(1000);
                }
            }, cancellationToken);

            _ = Task.Factory.StartNew(async () =>
            {
                Thread.Sleep(3000);

                await evenBus.PublishAsync(@event, cancellationToken);
            }, cancellationToken);

            // Act
            queue.DequeueAsync(cancellationToken);

            // Assert
            cancellationToken.Register(() =>
            {
                var userEvent = memory.UserEventTrace.FirstOrDefault();

                Assert.NotEmpty(memory.UserEventTrace);
                Assert.Equal(@event.Id, userEvent.Id);
                Assert.Equal(@event.UserName, userEvent.UserName);
                Assert.Equal(@event.Names, userEvent.Names);
                Assert.Equal(@event.Birthdate, userEvent.Birthdate);
                Assert.Equal(@event.Lastnames, userEvent.Lastnames);
                Assert.Equal(@event.EventDate , userEvent.EventDate);
                Assert.Equal(@event.IdEvent, userEvent.IdEvent);
            });
        }

        /// <summary>
        /// Verifies that redis not invoke the event handler
        /// </summary>
        [Fact]
        public void Unsubscribe_InvokeUnsubscribeRedis_NotListenerChannel()
        {
            // Arrange
            var channelUnsubscribe = string.Empty;
            var isInvokedRemoveSubscription = false;

            var serviceProvider = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<RedisEventBusService>>();
            var subscriptionManager = new Mock<ISubscriptionManager>();
            var subscriber = new Mock<ISubscriber>();

            subscriptionManager
                .Setup(x => x.RemoveSubscription<UserCreatedEvent, UserCreatedEventHandler>())
                .Callback(() =>
                {
                    isInvokedRemoveSubscription = true;
                });

            subscriber
                .Setup(x => x.Unsubscribe(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()))
                .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((channel, action, commandFlags) =>
                {
                    channelUnsubscribe =  channel;
                });

            var redisService = new Mock<IRedisService>();

            redisService.SetupGet(x => x.Subscriber).Returns(subscriber.Object);

            var redisEventBusService = new RedisEventBusService(redisService.Object, subscriptionManager.Object, serviceProvider, logger);

            // Act
            redisEventBusService.Unsubscribe<UserCreatedEvent, UserCreatedEventHandler>();

            // Assert
            Assert.Equal(typeof(UserCreatedEvent).Name, channelUnsubscribe);
            Assert.True(isInvokedRemoveSubscription);
        }

        /// <summary>
        /// Create Configuration
        /// </summary>
        /// <returns>Redis Configuration</returns>
        /// <exception cref="InvalidOperationException">certificate not exist</exception>
        public static IConfiguration CreateConfiguration()
        {
            var path = Directory.GetCurrentDirectory();

            var certificate = Path.Combine(path, "Helpers", "Certificate", "aorus with password.pfx");

            if (!File.Exists(certificate))
                throw new InvalidOperationException("Can't run unit test because certificate does not exist");

            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { $"{RedisOptions.Section}:{nameof(RedisOptions.Certificate)}",  certificate},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.Password)}",  "0u193OmSGmxDS4y28Fe1tWS6QwVlkUIlu4BdzKWwDkkNYhpCn/5il7XPECqHnekoc3zIxviuuBFysGlr"},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.ResolveDns)}",  "false"},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.PasswordCertificate)}",  "Temporal1"},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.EndPoints)}:0",  "192.168.20.43:6379"},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.EndPoints)}:1",  "192.168.20.44:6379"},
                { $"{RedisOptions.Section}:{nameof(RedisOptions.EndPoints)}:2",  "192.168.20.45:6379"}
            });

            return configurationBuilder.Build();
        }
    }
}

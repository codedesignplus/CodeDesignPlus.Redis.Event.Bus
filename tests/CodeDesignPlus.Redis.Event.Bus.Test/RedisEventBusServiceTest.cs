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
    public class RedisEventBusServiceTest
    {

        private readonly IConfiguration configuration;

        public RedisEventBusServiceTest()
        {
            this.configuration = CreateConfiguration();
        }

        [Fact]
        public void ListenerEvent()
        {
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


            var queue = serviceProvider.GetService<IQueueService<UserCreatedEventHandler, UserCreatedEvent>>();


            _ = Task.Factory.StartNew(() =>
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    if (memory.UserEventTrace.Any(x => x.Id == 1))
                    {
                        cancellationTokenSource.Cancel();
                    }

                    Thread.Sleep(1000);
                }
            }, cancellationToken);

            queue.DequeueAsync(cancellationToken);

            cancellationToken.Register(() =>
            {
                Assert.NotEmpty(memory.UserEventTrace);
            });
        }


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

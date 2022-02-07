using CodeDesignPlus.Event.Bus;
using CodeDesignPlus.Event.Bus.Abstractions;
using CodeDesignPlus.Event.Bus.Internal.Queue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeDesignPlus.Redis.Event.Bus
{
    public class RedisEventBusService : IRedisEventBusService
    {
        private readonly ILogger<RedisEventBusService> logger;

        private readonly IRedisService redisService;

        private readonly ISubscriptionManager subscriptionManager;

        private readonly IServiceProvider serviceProvider;

        public RedisEventBusService(IRedisService redisService, ISubscriptionManager subscriptionManager, IServiceProvider serviceProvider, ILogger<RedisEventBusService> logger)
        {
            this.redisService = redisService;
            this.subscriptionManager = subscriptionManager;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public Task PublishAsync(EventBase @event, CancellationToken token)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var channel = @event.GetType().Name;

            var message = JsonConvert.SerializeObject(@event);

            return this.redisService.Subscriber.PublishAsync(channel, message);
        }

        public Task<TResult> PublishAsync<TResult>(EventBase @event, CancellationToken token)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return this.PrivatePublishAsync<TResult>(@event);
        }

        private async Task<TResult> PrivatePublishAsync<TResult>(EventBase @event)
        {
            var channel = @event.GetType().Name;

            var message = JsonConvert.SerializeObject(@event);

            var notified = await this.redisService.Subscriber.PublishAsync(channel, message);

            return (TResult)Convert.ChangeType(notified, typeof(TResult));
        }

        public Task SubscribeAsync<TEvent, TEventHandler>()
            where TEvent : EventBase
            where TEventHandler : IEventHandler<TEvent>
        {
            var channel = typeof(TEvent).Name;

            return this.redisService.Subscriber.SubscribeAsync(channel, (_, v) => this.ListenerEvent<TEvent, TEventHandler>(v));
        }

        public void ListenerEvent<TEvent, TEventHandler>(RedisValue value)
            where TEvent : EventBase
            where TEventHandler : IEventHandler<TEvent>
        {
            if (this.subscriptionManager.HasSubscriptionsForEvent<TEvent>())
            {
                var subscriptions = this.subscriptionManager.FindSubscriptions<TEvent>();

                foreach (var subscription in subscriptions)
                {
                    var queueType = typeof(IQueueService<,>);

                    queueType = queueType.MakeGenericType(subscription.EventHandlerType, subscription.EventType);

                    var queue = this.serviceProvider.GetService(queueType);

                    var @event = JsonConvert.DeserializeObject<TEvent>(value);

                    queue.GetType().GetMethod(nameof(IQueueService<TEventHandler, TEvent>.Enqueue)).Invoke(queue, new object[] { @event });
                }
            }
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : EventBase
            where TEventHandler : IEventHandler<TEvent>
        {
            this.subscriptionManager.RemoveSubscription<TEvent, TEventHandler>();
        }
    }
}

using CodeDesignPlus.Event.Bus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeDesignPlus.Redis.Event.Bus
{
    public interface IRedisEventBusService : IEventBus
    {
        Task<TResult> PublishAsync<TResult>(EventBase @event, CancellationToken token);
    }
}

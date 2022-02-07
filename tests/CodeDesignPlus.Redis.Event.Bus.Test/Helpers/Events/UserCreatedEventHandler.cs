using CodeDesignPlus.Event.Bus.Abstractions;
using CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Events
{
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly ILogger logger;

        private readonly IMemoryService memory;

        public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger, IMemoryService memory)
        {
            this.logger = logger;
            this.memory = memory;
        }

        public Task HandleAsync(UserCreatedEvent data, CancellationToken token)
        {
            this.memory.UserEventTrace.Add(data);

            this.logger.LogDebug("Invoked Event: {0}", JsonConvert.SerializeObject(data));

            return Task.CompletedTask;
        }
    }
}

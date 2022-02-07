using CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeDesignPlus.Redis.Event.Bus.Test.Helpers.Memory
{
    public class MemoryService: IMemoryService
    {
        public List<UserCreatedEvent> UserEventTrace { get; private set; } = new List<UserCreatedEvent>();
    }
}

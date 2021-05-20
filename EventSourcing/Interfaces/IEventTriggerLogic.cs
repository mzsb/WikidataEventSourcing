using EventSourcing.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Interfaces
{
    public interface IEventTriggerLogic
    {
        public Task RunAsync(IEnumerable<string> jsonEvents);
        public Task RunAsync(IEnumerable<EventBase> events);
    }
}


using EventSourcing.Enums;

namespace EventSourcing.Events
{
    public class RemoveEvent : EventBase 
    {
        public override EventType Type => EventType.Remove;
    }
}

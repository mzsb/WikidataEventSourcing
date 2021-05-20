using EventSourcing.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Events
{
    public class InsertEvent : EventBase
    {
        public string JsonEntity { get; set; }
        public override EventType Type => EventType.Insert;
    }
}

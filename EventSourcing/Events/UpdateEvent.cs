using EventSourcing.Enums;
using EventSourcing.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace EventSourcing.Events
{
    public class UpdateEvent : EventBase
    {
        public override EventType Type => EventType.Update;

        public List<UpdateSegment> UpdateSegments { get; set; } = new List<UpdateSegment>();
    }
}

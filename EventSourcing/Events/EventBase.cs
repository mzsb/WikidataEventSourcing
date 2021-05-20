using EventSourcing.Attributes;
using EventSourcing.Enums;
using EventSourcing.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace EventSourcing.Events
{
    public abstract class EventBase : IUploadable<Guid>
    {
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual EventType Type { get; }

        public DateTime CreationDate { get; set; }

        [PartitionKey]
        public string EntityId { get; set; }

        public string EntityPartitionKey { get; set; }
    }
}

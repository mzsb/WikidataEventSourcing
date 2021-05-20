using EventSourcing.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Converters
{
    public class EventConverter : JsonConverter
    {
        private readonly List<JsonConverter> _converters;

        public EventConverter()
        {
            _converters = new() { this };
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(EventBase);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var @event = serializer.Deserialize<JObject>(reader); 
            return @event.Value<string>("type") switch
            {
                "Insert" => ToSpecific<InsertEvent>(@event),
                "Remove" => ToSpecific<RemoveEvent>(@event),
                "Update" => ToSpecific<UpdateEvent>(@event),
                _ => throw new Exception($"{@event.Value<string>("type")} is not valid event type.")
            };
        }

        private Type ToSpecific<Type>(JObject general) => 
            JsonConvert.DeserializeObject<Type>(general.ToString(), new JsonSerializerSettings() { Converters = _converters });
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using WikidataClient.Model.WikidataEntity;

namespace WikidataClient.Converter
{
    public class EntityConverter : JsonConverter
    {
        private readonly List<JsonConverter> _converters;

        public EntityConverter()
        {
            _converters = new() { this, new SubjectConverter() };
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(WikidataEntity);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var entity = serializer.Deserialize<JObject>(reader); 
            return entity.Value<string>("type") switch
            {
                "item" => ToSpecific<WikidataItem>(entity),
                "property" => ToSpecific<WikidataProperty>(entity),
                "lexeme" => ToSpecific<WikidataLexeme>(entity),
                "form" => ToSpecific<WikidataForm>(entity),
                "sense" => ToSpecific<WikidataSense>(entity),
                _ => throw new Exception($"{entity.Value<string>("type")} is not valid entity type.")
            };
        }

        private Type ToSpecific<Type>(JObject general) => 
            JsonConvert.DeserializeObject<Type>(general.ToString(), new JsonSerializerSettings() { Converters = _converters });
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WikidataClient.Model.Statement;
using WikidataClient.Model.Statement.Subjects;
using String = WikidataClient.Model.Statement.Subjects.String;
using EventSourcing.Helpers;

namespace WikidataClient.Converter
{
    public class SubjectConverter : JsonConverter
    {
        private readonly List<JsonConverter> _converters;
        public SubjectConverter()
        {
            _converters = new() { this };
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(Subject);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var subject = serializer.Deserialize<JObject>(reader); 
            return subject.Value<string>("dataType") switch
            {
                "commonsMedia" => ToSpecific<CommonsMedia>(subject),
                "external-id" => ToSpecific<ExternalIdentifier>(subject),
                "wikibase-form" => ToSpecific<Form>(subject),
                "geo-shape" => ToSpecific<GeographicShape>(subject),
                "globecoordinate" => ToSpecific<GlobeCoordinate>(subject),
                "wikibase-item" => ToSpecific<Item>(subject),
                "wikibase-lexeme" => ToSpecific<Lexeme>(subject),
                "math" => ToSpecific<MathematicalExpression>(subject),
                "monolingualtext" => ToSpecific<MonolingualText>(subject),
                "musical-notation" => ToSpecific<MusicalNotation>(subject),
                "wikibase-property" => ToSpecific<Property>(subject),
                "quantity" => ToSpecific<Quantity>(subject),
                "wikibase-sense" => ToSpecific<Sense>(subject),
                "string" => ToSpecific<String>(subject),
                "tabular-data" => ToSpecific<TabularData>(subject),
                "time" => ToSpecific<Time>(subject),
                "url" => ToSpecific<URL>(subject),
                _ => ToSpecific<Unknown>(subject),
            };
        }

        private Type ToSpecific<Type>(JObject general) => 
            JsonConvert.DeserializeObject<Type>(general.ToString(), new JsonSerializerSettings() { Converters = _converters });
    }
}

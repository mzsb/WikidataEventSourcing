using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WikidataClient.Helpers
{
    public static class JsonHelper
    {
        public static T Get<T>(this JToken jToken, string propertyName) =>
            jToken is not null ?
            jToken.ToObject<JObject>().TryGetValue(propertyName, out JToken result) ? 
                result.Value<T>() :
                throw new Exception($"{propertyName} porperty not found") :
            throw new Exception($"Parent of {propertyName} was null");

        public static T GetToObject<T>(this JToken jToken, string propertyName) =>
            jToken is not null ?
                jToken.GetOrDefault<JObject>(propertyName) is JObject jObject ? 
                    jObject.ToObject<T>() : 
                    throw new Exception($"{propertyName} porperty not found") :
            throw new Exception($"Parent of {propertyName} was null");

        public static T GetOrDefault<T>(this JToken jToken, string propertyName, T defaultResult = default) =>
            jToken is not null ? 
                jToken.ToObject<JObject>().TryGetValue(propertyName, out JToken result) ? 
                    result.Value<T>() : 
                    defaultResult :
                defaultResult;

        public static JObject GetOrFirst<K>(this JToken jToken, string propertyName, K key) =>
            jToken is not null ?
            jToken.GetOrDefault<JObject>(propertyName) is JObject property ?
                property.ToObject<Dictionary<K, JObject>>().TryGetValue(key, out JObject value) ?
                    value :
                    property.ToObject<Dictionary<K, JObject>>().FirstOrDefault().Value :
                default :
            throw new Exception($"Parent of {propertyName} was null");
    }
}

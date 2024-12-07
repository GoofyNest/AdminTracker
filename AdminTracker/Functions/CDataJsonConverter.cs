using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdminTracker.Functions
{
    public class CDataJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            // Check if the key is #cdata-section
            if (jObject.ContainsKey("#cdata-section"))
            {
                return jObject["#cdata-section"].ToString();
            }

            return jObject.ToString();
        }
    }
}

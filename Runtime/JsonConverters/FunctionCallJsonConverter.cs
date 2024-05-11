using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;

namespace JsonConverters
{
    public class FunctionCallJsonConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is FunctionCall functionCall)
            {
                JObject o = new JObject
                {
                    {"id", functionCall.ToolCallId},
                    {"type","function"},
                    {"function",JToken.FromObject(new JObject
                    {
                        {"name", functionCall.Name},
                        {"arguments", functionCall.Arguments}
                    }) }
                };
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            string name = jObject["function"]?["name"] == null ? string.Empty : jObject["function"]?["name"].ToString();
            FunctionCall result = new FunctionCall
            {
                ToolCallId = jObject["id"]?.ToString(),
                Name = name,
                Arguments = jObject["function"]?["arguments"]?.ToString() ?? string.Empty
            };
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FunctionCall);
        }
    }
}
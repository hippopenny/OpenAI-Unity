using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;

namespace JsonConverters
{
    public class ChatFunctionJsonConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ChatFunction function)
            {
                var jParameters = new JObject
                {
                    { "type", "object" },
                    { "properties", JObject.FromObject(function.Parameters) },
                    { "required",JToken.FromObject(function.Parameters.Where(parameter => parameter.Value.IsOptinal != true).Select(parameter => parameter.Key).ToArray()
                    ) }
                };
                var jFunction = new JObject
                {
                    {"name",function.Name},
                    {"description",function.Description},
                    {"parameters",jParameters},
                };
                var jObject = new JObject
                {
                    { "type", "function" },
                    { "function", jFunction }
                };
                jObject.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ChatFunction);
        }
    }
}
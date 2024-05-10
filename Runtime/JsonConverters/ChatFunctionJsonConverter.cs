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
                JObject jParameters;
                if(function.Parameters != null )
                {
                    jParameters = new JObject
                    {
                        { "type", "object" },
                        { "properties", JObject.FromObject(function.Parameters) },
                        {
                            "required", JToken.FromObject(
                                function.Parameters.Where(parameter => parameter.Value.IsOptinal != true)
                                    .Select(parameter => parameter.Key).ToArray()
                            )
                        }
                    };
                }
                else
                {
                    jParameters = function.jParameter.ToObject<JObject>();
                }
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
            JObject jObject = JObject.Load(reader);
            string name = jObject["function"]?["name"] == null ? string.Empty : jObject["function"]?["name"].ToString();
            string des = jObject["function"]?["description"] == null ? string.Empty : jObject["function"]?["description"].ToString();
            ChatFunction result = new ChatFunction
            {
                Name = name,
                Description = des,
                jParameter = jObject["function"]?["parameters"]
            };
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ChatFunction);
        }
    }
}
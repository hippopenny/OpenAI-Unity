using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;

namespace JsonConverters
{
    public class ToolChoiceJsonConverter: JsonConverter 
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ToolChoice toolChoice)
            {
                
                if (toolChoice.Equals(ToolChoice.None))
                {
                    var jValue =new JValue("none");
                    jValue.WriteTo(writer);
                } else if (toolChoice.Equals(ToolChoice.Auto))
                {
                    var jValue =new JValue("auto");
                    jValue.WriteTo(writer);                }
                else if (toolChoice.Equals(ToolChoice.Required))
                {
                    var jValue =new JValue("required");
                    jValue.WriteTo(writer);   
                } else 
                {
                    var jObject = new JObject();
                    jObject.Add("type", toolChoice.Type);
                    jObject.Add("function", $@"{{""name"":{toolChoice.Function}}}");
                    jObject.WriteTo(writer);
                }
            }        
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ToolChoice);
        }
    }
}
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using UnityEngine;

namespace JsonConverters
{
    public class FunctionCallJsonConverter: JsonConverter
    {
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
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
    
    public class NoConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;

        public override bool CanWrite => false;
    }
}
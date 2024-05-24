using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;

namespace JsonConverters
{
    public class ApiErrorJsonConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            ApiError result = new ApiError()
            {
                Message = jObject["msg"].ToString()
            };
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ApiError);
        }
    }
}
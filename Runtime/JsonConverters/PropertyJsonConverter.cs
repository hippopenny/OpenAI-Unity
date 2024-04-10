using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using UnityEngine;

namespace JsonConverters
{
    public class PropertyJsonConverter: JsonConverter
    {
        private const string Default = "Default";
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is FunctionProperties properties)
            {
                JObject o = SerializeType(properties.Type,properties.Description);
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FunctionProperties);
        }

        //TODO - Refactor add description to parameter
        private JObject SerializeType(Type propertyTyper, string description)
        {
            JObject result = new JObject();
            var (typeName, typeDescription) = GetTypeInfo(propertyTyper);
            result?.AddFirst(new JProperty("type", typeName));
            if (typeDescription != null)
            {
                result?.Add("description",typeDescription);
            } else if (description != null)
            {
                result?.Add("description", description);
            }
            else result?.Add("description", Default);
            
            if (propertyTyper.IsEnum)
            {
                var membersArray = new JArray();
                foreach (var enumMember in Enum.GetNames(propertyTyper))
                {
                    membersArray.Add(enumMember);
                }
                result.Add("enum", membersArray); 
            } else if (propertyTyper.IsArray && propertyTyper.HasElementType)
            {
                var itemType = propertyTyper.GetElementType()!;
                result.Add("items", SerializeType(itemType,null));
            } else if(typeof(IEnumerable).IsAssignableFrom(propertyTyper) && propertyTyper.GenericTypeArguments.Length == 1)
            {
                var itemType = propertyTyper.GenericTypeArguments[0];
                result.Add("items", SerializeType(itemType,null));
            }
            else if (propertyTyper.IsClass)
            {
                var properties = propertyTyper.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
                if (properties.Any())
                {
                    var propertiesObject = new JObject();
                    foreach (var property in properties)
                    {
                        propertiesObject.Add(property.Name, SerializeType(property.PropertyType,null));
                    }
                    result.Add("properties", propertiesObject);
                }
            }
            return result;
        }
        
        private (string name, string? description) GetTypeInfo(Type type)
    {
        if (type == typeof(bool))
        {
            return ("boolean", null);
        }

        if (type == typeof(sbyte))
        {
            return ("integer", "8-bit signed integer from -128 to 127");
        }

        if (type == typeof(byte))
        {
            return ("integer", "8-bit unsigned integer from 0 to 255");
        }

        if (type == typeof(short))
        {
            return ("integer", "16-bit signed integer from -32,768 to 32,767");
        }

        if (type == typeof(ushort))
        {
            return ("integer", "16-bit unsigned integer from 0 to 65,535");
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(nint))
        {
            return ("integer", null);
        }

        if (type == typeof(uint) || type == typeof(ulong) || type == typeof(nuint))
        {
            return ("integer", "unsigned integer, greater than or equal to 0");
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return ("integer", "floating point number");
        }

        if (type == typeof(char))
        {
            return ("string", "single character");
        }

        if (type == typeof(string))
        {
            return ("string", null);
        }

        if (type == typeof(Uri))
        {
            return ("string", "URI in C# .NET format https://example.com/abc");
        }

        if (type == typeof(Guid))
        {
            return ("string", "GUID in C# .NET format separated by hyphens xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return ("string", "date and time in C# .NET ISO 8601 format yyyy-mm-ddThh:mm:ss");
        }

        if (type == typeof(TimeSpan))
        {
            return ("string", "time interval in C# .NET ISO 8601 format hh:mm:ss");
        }

        if (type.IsEnum)
        {
            return ("string", null);
        }

        if (type.IsArray && type.HasElementType)
        {
            return ("array", null);
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 1)
        {
            return ("array", null);
        }

        return ("object", null);
    }
    }
}
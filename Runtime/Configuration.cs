using System;
using System.IO;
using HippoFramework.Auth;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenAI
{
    public class Configuration
    {
        public Auth Auth { get; }
        
        /// Used for serializing and deserializing PascalCase request object fields into snake_case format for JSON. Ignores null fields when creating JSON strings.
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore, 
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CustomNamingStrategy()
            }
        };
        
        public Configuration(string apiKey = null, string organization = null)
        {
            if (apiKey == null)
            {
                var json = Resources.Load<TextAsset>("auth").text;
                if (json != null)
                {
                    
                        Auth = JsonConvert.DeserializeObject<Auth>json,jsonSerializerSettings);
                    }
                    else
                    {
                        Debug.LogError("Access Token is null");
                    }
                
            }
            else
            {
                Auth = new Auth()
                {
                    ApiKey = apiKey,
                    Organization = organization
                };
            }
        }
    }
}

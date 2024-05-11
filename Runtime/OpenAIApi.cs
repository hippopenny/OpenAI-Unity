using System;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace OpenAI
{
    public class OpenAIApi
    {
        /// <summary>
        ///     Reads and sets user credentials from %User%/.openai/auth.json
        ///     Remember that your API key is a secret! Do not share it with others or expose it in any client-side code (browsers, apps).
        ///     Production requests must be routed through your own backend server where your API key can be securely loaded from an environment variable or key management service.
        /// </summary>
        private Configuration configuration;

        private Configuration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = new Configuration();
                }

                return configuration;
            }
        }

        /// OpenAI API base path for requests.
        private const string BASE_PATH = "https://art3.hippopenny.com/api/openai/v1";

        public OpenAIApi(string apiKey = null, string organization = null)
        {
            if (apiKey != null)
            {
                configuration = new Configuration(apiKey, organization);
            }
        }
        
        /// Used for serializing and deserializing PascalCase request object fields into snake_case format for JSON. Ignores null fields when creating JSON strings.
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore, 
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CustomNamingStrategy()
            },
            Culture = CultureInfo.InvariantCulture
        };

        /// <summary>
        ///     Dispatches an HTTP request to the specified path with the specified method and optional payload.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="onComplete"></param>
        /// <param name="payload">An optional byte array of json payload to include in the request.</param>
        /// <typeparam name="T">Response type of the request.</typeparam>
        /// <returns>A Task containing the response from the request as the specified type.</returns>
        private async void DispatchRequest<T>(string path, string method, Action<T> onComplete, byte[] payload = null) where T: IResponse
        {
            T data;
            
            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);
                
                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield() ;
                
                Debug.LogWarning(request.downloadHandler.text);
                
                data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
                
                if (data?.Error != null)
                {
                    ApiError error = data.Error;
                    Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
                }

                if (data?.Warning != null)
                {
                    Debug.LogWarning(data.Warning);
                }
                onComplete?.Invoke(data);
            }
        }
        
        /// <summary>
        ///     Dispatches an HTTP request to the specified path with the specified method and optional payload.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="onResponse">A callback function to be called when a response is updated.</param>
        /// <param name="onComplete">A callback function to be called when the request is complete.</param>
        /// <param name="token">A cancellation token to cancel the request.</param>
        /// <param name="payload">An optional byte array of json payload to include in the request.</param>
        private async void DispatchRequest<T>(string path, string method, Action<List<T>> onResponse, Action onComplete, CancellationTokenSource token, byte[] payload = null) where T: IResponse, new()
        {
            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);
                request.SendWebRequest();
                bool isDone = false;
                do
                {
                    List<T> dataList = new List<T>();
                    string[] lines = request.downloadHandler.text.Split('\n').Where(line => line != "").ToArray();
                    foreach (string line in lines)
                    {
                        var value = line.Replace("data: ", "");
                        if (value.Contains("[DONE]") ) 
                        {
                            isDone = true;
                            break;
                        }
                        Debug.Log(value);
                        var data = new T();
                        try
                        {
                            data = JsonConvert.DeserializeObject<T>(value, jsonSerializerSettings);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(value);
                        }
                        if (data?.Error != null)
                        {
                            ApiError error = data.Error;
                            Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
                        }
                        else
                        {
                            dataList.Add(data);
                        }
                    }
                    onResponse?.Invoke(dataList);
                    
                    await Task.Yield();

                }
                while (!isDone);
                
                onComplete?.Invoke();
            }
        }

        /// <summary>
        ///     Create byte array payload from the given request object that contains the parameters.
        /// </summary>
        /// <param name="request">The request object that contains the parameters of the payload.</param>
        /// <typeparam name="T">type of the request object.</typeparam>
        /// <returns>Byte array payload.</returns>
        private byte[] CreatePayload<T>(T request)
        {
            var json = JsonConvert.SerializeObject(request,  jsonSerializerSettings);
            Debug.Log(json);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest"/></param>
        /// <returns>See <see cref="CreateChatCompletionResponse"/></returns>
        public void CreateChatCompletion(CreateChatCompletionRequest request, Action<CreateChatCompletionResponse> onComplete)
        {
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);
            
            DispatchRequest(path, UnityWebRequest.kHttpVerbPOST,onComplete, payload);
        }
        
        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest"/></param>
        /// <param name="onResponse">Callback function that will be called when stream response is updated.</param>
        /// <param name="onComplete">Callback function that will be called when stream response is completed.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        public void CreateChatCompletionAsync(CreateChatCompletionRequest request, Action<List<CreateChatCompletionResponse>> onResponse, Action onComplete, CancellationTokenSource token)
        {
            request.Stream = true;
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);
            
            DispatchRequest(path, UnityWebRequest.kHttpVerbPOST, onResponse, onComplete, token, payload);
        }
    }
}

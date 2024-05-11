#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAI
{
    #region Common Data Types
    public struct Choice
    {
        public string Text { get; set; }
        public int? Index { get; set; }
        public int? Logprobs { get; set; }
        public string FinishReason { get; set; }
    }

    public struct Usage
    {
        public string PromptTokens { get; set; }
        public string CompletionTokens { get; set; }
        public string TotalTokens { get; set; }
    }
    
    public class OpenAIFile
    {
        public string Prompt { get; set; }
        public object Completion { get; set; }
        public string Id { get; set; }
        public string Object { get; set; }
        public long Bytes { get; set; }
        public long CreatedAt { get; set; }
        public string Filename { get; set; }
        public string Purpose { get; set; }
        public object StatusDetails { get; set; }
        public string Status { get; set; }
    }

    public class OpenAIFileResponse : OpenAIFile, IResponse
    {
        public ApiError Error { get; set; }
        public string Warning { get; set; }
    }

    public class ApiError
    {
        public string Message;
        public string Type;
        public object Param;
        public object Code;
    }

    public struct Auth
    {
        [JsonRequired]
        public string ApiKey { get; set; }
        public string Organization { get; set; }
    }
    #endregion
    
    #region Models API Data Types
    public struct ListModelsResponse: IResponse
    {
        public ApiError Error { get; set; }
        public string Warning { get; set; }
        public string Object { get; set; }
        public List<OpenAIModel> Data { get; set; }
    }

    public class OpenAIModel
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public string OwnedBy { get; set; }
        public long Created { get; set; }
        public string Root { get; set; }
        public string Parent { get; set; }
        public List<Dictionary<string, object>> Permission { get; set; }
    }

    public class OpenAIModelResponse : OpenAIModel, IResponse
    {
        public ApiError Error { get; set; }
        public string Warning { get; set; }
    }
    #endregion

    #region Chat API Data Types
    public class CreateChatCompletionRequest
    {
        public string Model { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public float? Temperature { get; set; } = 1;
        public int N { get; set; } = 1;
        public bool Stream { get; set; } = false;
        public List<string> Stop { get; set; }
        public int? MaxTokens { get; set; }
        public float? PresencePenalty { get; set; } = 0;
        public float? FrequencyPenalty { get; set; } = 0;
        public Dictionary<string, float> LogitBias { get; set; }
        public string User { get; set; }
        public string SystemFingerprint { get; set; }
        
        public List<ChatFunction> Tools { get; set; }
        public ToolChoice? ToolChoice { get; set; }
    }

    [Serializable]
    public class CreateChatCompletionResponse : IResponse
    {
        public ApiError Error { get; set; }
        public string Warning { get; set; }
        public string Model { get; set; }
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public List<ChatChoice> Choices { get; set; }
        public Usage Usage { get; set; }
        public string SystemFingerprint { get; set; }
    }
    
    [JsonConverter(typeof(ToolChoiceJsonConverter))]
    public class ToolChoice
    {
        public static ToolChoice None;

        public static ToolChoice Auto;

        public static ToolChoice Required;
        
        public string Type { get; set; }
        public string Function { get; set; } 
    }
    
    [Serializable]
    [JsonConverter(typeof(PropertyJsonConverter))]
    public record FunctionProperties
    {
        public FunctionProperties()
        {
        }
        
        public Type Type { get; set; }
        
        public string Name { get; set; }
        
        public bool IsOptinal { get; set; } = true;

        public string Description { get; set; }
    }

    [Serializable]
    [JsonConverter(typeof(ChatFunctionJsonConverter))]
    public record ChatFunction
        { 
            public ChatFunction() 
            {
            }

            public ChatFunction(Delegate callback)
            {
                Callback = callback;
            }

            public string Name { get; set; } = null!;

            public string? Description { get; set; }
    
            public Dictionary<string,FunctionProperties> Parameters { get; set; }
            
            public JToken jParameter { get; set;}
            
            public string Type { get; set; }

            public Delegate? Callback { get; set; }
        }
    
    [Serializable]
    public struct ChatChoice
    {
        public ChatMessage Message { get; set; }
        public ChatMessage Delta { get; set; }
        public int? Index { get; set; }
        public string FinishReason { get; set; }
        public string Logprobs { get; set; }
    }

    [Serializable]
    public class ChatMessage
    {
        public ChatMessage()
        {
        }
        
        public ChatMessage(string message, string role)
        {
            Content = message;
            Role = role;
        }

        public ChatMessage(FunctionResult functionResult)
        {
            ToolCallId = functionResult.ToolCallId;
            Name = functionResult.Name;
            Content = functionResult.Value;
            Role = "tool";
        }
        public string Role { get; set; }
        public string Content { get; set; }
        
        public List<FunctionCall> ToolCalls { get; set; }
        
        public string? ToolCallId { get; set; }

        public string Name { get; set; } = null!;
        
    }
    
    [Serializable]
    [JsonConverter(typeof(FunctionCallJsonConverter))]
    public record FunctionCall
    {
        public FunctionCall() { }
        
        public string? ToolCallId { get; set; }

        public string Name { get; set; } = null!;

        public string Arguments { get; set; } = null!;
    }
    
    public record FunctionResult
    {
        public FunctionResult() { }
        
        public string? ToolCallId { get; set; }

        public string Name { get; set; } = null!;
        
        public string Value { get; set; }
    }
    
    #endregion

    #region Static String Types
    public static class ContentType
    {
        public const string ApplicationJson = "application/json";
    }
    
    #endregion
}

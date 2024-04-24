using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenAI.MemoryBank
{
    public class MemoryBank
    {
        public void Init()
        {
            
            var promptHelper = new PromptHelper(4096, 256, 20);
            var serviceContext = ServiceContext.FromDefaults(llmPredictor, promptHelper);

            var allUserMemories = GenerateMemoryDocs(LoadData("../memories/update_memory_0512_eng.json"), "en");
            BuildMemoryIndex(allUserMemories, serviceContext);
        }
        
        private Dictionary<string, List<Document>> GenerateMemoryDocs(Dictionary<string, dynamic> data)
        {
            var allUserMemories = new Dictionary<string, List<Document>>();
            foreach (var (userName, userMemory) in data)
            {
                allUserMemories[userName] = new List<Document>();
                if (!userMemory.ContainsKey("history"))
                    continue;

                foreach (var (date, content) in userMemory["history"])
                {
                    var memoryStr = $"Conversation on {date}：";
                    foreach (var dialog in content)
                    {
                        memoryStr += $"\n{userName}：{dialog["query"].Trim()}";
                        memoryStr += $"\nAI：{dialog["response"].Trim()}";
                    }
                    memoryStr += "\n";

                    if (userMemory.ContainsKey("summary") && userMemory["summary"].ContainsKey(date))
                    {
                        var summary = $"The summary of the conversation on {date} is: {userMemory["summary"][date]}";
                        memoryStr += summary;
                    }

                    allUserMemories[userName].Add(new Document(memoryStr));
                }
            }
            return allUserMemories;
        }

        private Dictionary<string, dynamic> LoadData(string filePath)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(filePath));
        }

        private void BuildMemoryIndex(Dictionary<string, List<Document>> allUserMemories, ServiceContext serviceContext)
        {
            var indexSet = new Dictionary<string, GPTSimpleVectorIndex>();
            foreach (var (userName, memories) in allUserMemories)
            {
                Console.WriteLine($"Building index for user {userName}");
                var curIndex = GPTSimpleVectorIndex.FromDocuments(memories, serviceContext);
                indexSet[userName] = curIndex;
                Directory.CreateDirectory("../memories/memory_index/llamaindex");
                curIndex.SaveToDisk($"../memories/memory_index/llamaindex/{userName}_index.json");
            }
        }
    }
    
    public class Document
    {
        public string Text { get; }

        public Document(string text)
        {
            Text = text;
        }
    }
    
    public class LLMPredictor
    {
        private readonly IOpenAIChat _openAIChat;

        public LLMPredictor(IOpenAIChat openAIChat)
        {
            _openAIChat = openAIChat;
        }

        public async Task<string> PredictAsync(string prompt)
        {
            var completionResult = await _openAIChat.CreateCompletionAsync(new CreateChatCompletionRequest()
            {
                Prompt = prompt,
                MaxTokens = 256,
                N = 1,
                Stop = new List<string> { "\n" },
                Temperature = 0.7f
            });

            return completionResult.Choices[0].Text;
        }
    }

    public class PromptHelper
    {
        private readonly int _maxInputSize;
        private readonly int _numOutput;
        private readonly int _maxChunkOverlap;

        public PromptHelper(int maxInputSize, int numOutput, int maxChunkOverlap)
        {
            _maxInputSize = maxInputSize;
            _numOutput = numOutput;
            _maxChunkOverlap = maxChunkOverlap;
        }
    }

    public class ServiceContext
    {
        private readonly LLMPredictor _llmPredictor;
        private readonly PromptHelper _promptHelper;

        private ServiceContext(LLMPredictor llmPredictor, PromptHelper promptHelper)
        {
            _llmPredictor = llmPredictor;
            _promptHelper = promptHelper;
        }

        public static ServiceContext FromDefaults(LLMPredictor llmPredictor, PromptHelper promptHelper)
        {
            return new ServiceContext(llmPredictor, promptHelper);
        }
    }

    public class GPTSimpleVectorIndex
    {
        private readonly ServiceContext _serviceContext;
        private readonly List<Document> _documents;

        private GPTSimpleVectorIndex(ServiceContext serviceContext, List<Document> documents)
        {
            _serviceContext = serviceContext;
            _documents = documents;
        }

        public static GPTSimpleVectorIndex FromDocuments(List<Document> documents, ServiceContext serviceContext)
        {
            return new GPTSimpleVectorIndex(serviceContext, documents);
        }

        public void SaveToDisk(string filePath)
        {
            // Save the index to disk
        }
    }
}
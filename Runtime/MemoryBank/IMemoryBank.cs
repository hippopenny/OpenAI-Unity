using System.Collections.Generic;

namespace OpenAI.MemoryBank
{
    public interface IMemoryBank
    {
        public string ConvertToTextAsset();
        
        public string RetrieveMemory(string memory);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace OpenAI.MemoryBank
{
    public class MemoryBankManager: MonoBehaviour
    {
        private string userName = "";
        private Dictionary<string, List<string>> _memoryBank;
        
        public Dictionary<string, List<string>> CurrentMemoryBank
        {
            get
            {
                return _memoryBank;
            }
            private set
            {
                _memoryBank = value;
            }
        }
        
        public static MemoryBankManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void SwitchMemoryBank(Dictionary<string,List<string>> memory)
        {
            CurrentMemoryBank = memory;
        }

        public void UpdateMemoryBank(string index, string thought)
        {
            if(!CurrentMemoryBank.ContainsKey(index)) CurrentMemoryBank.Add(index,new List<string>{thought});
            else CurrentMemoryBank[index].Add(thought);
        }

        public List<string> RetrieveFromMemoryBank(string index)
        {
            return CurrentMemoryBank[index];
        }

        private Dictionary<string, Dictionary<string,string>> LoadData(string filePath)
        {
            TextAsset[] memories = Resources.LoadAll<TextAsset>(filePath);
            Dictionary<string, Dictionary<string, string>>
                result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var memory in memories)
            {
                Dictionary<string, string> index =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(memory.text);
                result.Add(memory.name, index);
            }
            return result;
        }

        public void SaveToDisk(string username)
        {
            //TODO - Save current memory to disk
        }

        public string GetBeginningMessage()
        {
            //TODO - Create a begin message 
            return null;
        }
    }
}
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
        
        public static MemoryBankManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void SwitchMemoryBank(Dictionary<string,List<string>> memory)
        {
            _memoryBank = memory;
        }

        public void UpdateMemoryBank(string index, string thought)
        {
            if(!_memoryBank.ContainsKey(index)) _memoryBank.Add(index,new List<string>{thought});
            else if(!_memoryBank[index].Contains(thought)) _memoryBank[index].Add(thought);
        }

        public void RemoveFromMemoryBank(string index, int index1)
        {
            if (!_memoryBank.ContainsKey(index)) throw new Exception($"Key {index} not contained in bank");
            if(_memoryBank[index].Count <= index1) throw new Exception($"Key {index} does not have {index1} memory");
            _memoryBank[index].RemoveAt(index1);
        }
        
        public bool TryRetrieveFromMemoryBank(string index,out List<string> output)
        {
            return _memoryBank.TryGetValue(index, out output);
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
            string fileName = Application.dataPath + $"/Resources/MemoryBank/{username}.json";
            string jsonString = JsonConvert.SerializeObject(_memoryBank);
            File.WriteAllText(fileName,jsonString);
        }
    }
}
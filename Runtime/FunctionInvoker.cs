using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace OpenAI
{
    public class FunctionInvoker
    {
        private static Dictionary<string, object> _arguments = new Dictionary<string, object>();
        public static string InvokeAsync(ChatFunction function, [CanBeNull] string arguments,
            CancellationToken cancellationToken = default)
        {
            if (arguments != null) _arguments = JsonConvert.DeserializeObject<Dictionary<string, object>>(arguments);
            List<object> Argument = new List<object>();
            foreach (var pair in _arguments)
            {
                Argument.Add(DeserializeType(pair.Value,function.Parameters[pair.Key].Type));
            }
            var invocationResult = function.Callback?.DynamicInvoke(Argument.ToArray());
            if (invocationResult is Task task)
            {
                task.ConfigureAwait(false);

                var taskResultProperty = task.GetType().GetProperty("Result");
                if (taskResultProperty != null)
                {
                    invocationResult = taskResultProperty.GetValue(task);
                }
            }

            if (invocationResult == null)
            {
                return JsonConvert.SerializeObject(new { IsSuccess = true });
            }

            return JsonConvert.SerializeObject(invocationResult);
        }
        
        private static object DeserializeType(object value, Type type)
        {
            object result = null;
            if (type.IsEnum)
            {
                if (!Enum.TryParse(type, (string)value, true, out value))
                {
                    result = JsonConvert.SerializeObject(new
                        { Error = $"Value '{value}' is not a valid enum member for parameter '{type}'." });
                }
            } else if (type.IsArray && type.HasElementType)
            {
                result = ((JArray)value).ToObject(type);
            }
            else
            {
                result = Convert.ChangeType(value, type);
            }
            return result;
        }
    }
}
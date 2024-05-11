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
        public static string InvokeAsync(ChatFunction function, [CanBeNull] string arguments,Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            if (arguments != null) _arguments = JsonConvert.DeserializeObject<Dictionary<string, object>>(arguments);
            else
            {
                Debug.LogError("Arguments are null");
                onError.Invoke(new NullReferenceException());
            }
            List<object> Argument = new List<object>();
            foreach (var parameter in function.Callback.Method.GetParameters())
            {
                if (parameter.ParameterType == typeof(CancellationToken))
                {
                    Argument.Add(cancellationToken);
                    continue;
                }
                if (_arguments.ContainsKey(parameter.Name))
                {
                    try
                    {
                        Argument.Add(DeserializeType(_arguments[parameter.Name],
                            function.Parameters[parameter.Name].Type));
                    }
                    catch (Exception e)
                    {
                        if(onError == null) return JsonConvert.SerializeObject(new
                        {
                            Error =
                                $"Value '{_arguments[parameter.Name]}' is not valid for parameter '{parameter.Name}'. Expected type: '{parameter.ParameterType.Name}'."
                        });
                        onError.Invoke(e);
                        return JsonConvert.SerializeObject(new { IsSuccess = true });
                    }
                }
                else if(function.Parameters[parameter.Name].IsOptinal && parameter.DefaultValue != DBNull.Value)
                {
                    Argument.Add(parameter.DefaultValue);
                }
                else
                {
                    if(onError == null) return JsonConvert.SerializeObject(new { Error = $"You must provide a value for the required parameter '{parameter.Name}'." });
                    onError.Invoke(new Exception($"You must provide a value for the required parameter '{parameter.Name}'."));
                    return JsonConvert.SerializeObject(new { IsSuccess = true });
                }
            }

            object invocationResult = "";
            try
            {
                invocationResult = function.Callback?.DynamicInvoke(Argument.ToArray());
            }
            catch (Exception e)
            {
                onError.Invoke(e);
                return JsonConvert.SerializeObject(new { IsSuccess = true });

            }
            
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
                if (!Enum.TryParse(type, (string)value, true, out result))
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
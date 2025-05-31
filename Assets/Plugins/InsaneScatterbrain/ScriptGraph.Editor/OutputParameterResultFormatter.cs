using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public static class OutputParameterResultFormatter
    {
        public static string Format(IReadOnlyDictionary<string, object> latestResult, string key)
        {
            var resultText = "(no results yet, run graph first)";
            
            // Show the latest result, if any
            if (latestResult == null || !latestResult.ContainsKey(key))
            {
                return resultText;
            }
            
            var result = latestResult[key];
            if (result == null)
            {
                return resultText;
            }

            resultText = result.ToString();

            // If the result is an array, show the values as a comma separated string
            if (!(result is IEnumerable enumerable))
            {
                return resultText;
            }
            
            var resultArray = enumerable.Cast<object>().ToArray();
            resultText = string.Join(", ", resultArray);
            resultText = $"[{resultText}]";

            return resultText;
        }
    }
}
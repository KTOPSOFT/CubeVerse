using System.Collections.Generic;
using InsaneScatterbrain.Pooling;

namespace InsaneScatterbrain.MapGraph
{
    public class DictionaryPool<TDictionary, TKey, TValue> : Pool<TDictionary> where TDictionary 
        : class, IDictionary<TKey, TValue>, new()
    {
        protected override TDictionary New() => new TDictionary();
        protected override void Reset(TDictionary instance) => instance.Clear();
    }
}
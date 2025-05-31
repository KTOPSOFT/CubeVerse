using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class CollectionPool<TCollection, TValue> : Pool<TCollection> where TCollection : class, ICollection<TValue>, new()
    {
        protected override TCollection New() => new TCollection();
        protected override void Reset(TCollection instance) => instance.Clear();
    }
}
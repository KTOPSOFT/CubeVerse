using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class StackPool<T> : Pool<Stack<T>>
    {
        protected override Stack<T> New() => new Stack<T>();
        protected override void Reset(Stack<T> instance) => instance.Clear();
    }
}
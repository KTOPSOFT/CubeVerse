using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ScriptGraphPool
    {
        private readonly ScriptGraphGraph original;
        private readonly Stack<ScriptGraphGraph> pool = new Stack<ScriptGraphGraph>();
        private readonly Stack<ScriptGraphGraph> instancesInUse = new Stack<ScriptGraphGraph>();

        public string Name => original.name;
        public object CountInstancesInUse => instancesInUse.Count;
        public object CountInstancesInPool => pool.Count;

        public ScriptGraphPool(ScriptGraphGraph original)
        {
            this.original = original;
            pool.Push(original);
        }

        public ScriptGraphGraph Get()
        {
            var instance = pool.Count > 0 ? pool.Pop() : Object.Instantiate(original);
            
            instancesInUse.Push(instance);
            
            return instance;
        }
        
        public void Clear()
        {
            pool.Clear();
            instancesInUse.Clear();
        }

        public void ReturnAll()
        {
            while (instancesInUse.Count > 0)
            {
                pool.Push(instancesInUse.Pop());
            }
        }
    }
}
using System.Collections.Generic;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ScriptGraphPoolManager : IScriptGraphInstanceProvider
    {
        private readonly Dictionary<ScriptGraphGraph, ScriptGraphPool> pools =
            new Dictionary<ScriptGraphGraph, ScriptGraphPool>();

        public IReadOnlyCollection<ScriptGraphPool> Pools => pools.Values;
        
        public ScriptGraphGraph Get(ScriptGraphGraph original)
        {
            if (!pools.ContainsKey(original))
            {
                pools.Add(original, new ScriptGraphPool(original));
            }

            return pools[original].Get();
        }

        public void ReturnAll()
        {
            foreach (var pool in pools.Values)
            {
                pool.ReturnAll();
            }
        }
        
        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }
    }
}
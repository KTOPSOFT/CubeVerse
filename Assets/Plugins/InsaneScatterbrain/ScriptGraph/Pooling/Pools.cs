namespace InsaneScatterbrain.ScriptGraph
{
    public static class Pools
    {
        public static void Clear()
        {
            ScriptGraphPoolManagerSingleton.Instance.ClearAll();
            PoolManagerSingleton.Instance.ClearAll();
        }
    }
}
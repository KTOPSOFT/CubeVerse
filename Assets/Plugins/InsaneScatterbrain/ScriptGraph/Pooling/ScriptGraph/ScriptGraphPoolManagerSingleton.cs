namespace InsaneScatterbrain.ScriptGraph
{
    public static class ScriptGraphPoolManagerSingleton
    {
        private static ScriptGraphPoolManager instance;

        public static ScriptGraphPoolManager Instance => instance ?? (instance = new ScriptGraphPoolManager());
    }
}
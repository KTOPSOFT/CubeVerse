using System.Threading;

namespace InsaneScatterbrain.ScriptGraph
{
    public static class PoolManagerSingleton
    {
        private static ThreadLocal<PoolManager> instance = new ThreadLocal<PoolManager>();

        public static PoolManager Instance
        {
            get
            {
                if (instance.Value == null)
                {
                    Initialize();
                }

                return instance.Value;
            }
        }

        private static void Initialize()
        {
            instance.Value = new PoolManager();
            PoolManagerInitializer.Initialize(instance.Value);
        }
    }
}
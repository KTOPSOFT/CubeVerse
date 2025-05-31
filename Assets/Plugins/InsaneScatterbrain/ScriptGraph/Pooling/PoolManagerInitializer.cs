using System;
using InsaneScatterbrain.Services;

namespace InsaneScatterbrain.ScriptGraph
{
    public static class PoolManagerInitializer
    {
        public static void Initialize(PoolManager instanceProvider)
        {
            var poolInitializerTypes = Types.ChildrenOf<IPoolInitializer>();
            foreach (var poolInitializerType in poolInitializerTypes)
            {
                if (!poolInitializerType.IsClass || poolInitializerType.IsAbstract) continue;
                
                var poolInitializer = (IPoolInitializer) Activator.CreateInstance(poolInitializerType);
                poolInitializer.Initialize(instanceProvider);
            }
        }
    }
}
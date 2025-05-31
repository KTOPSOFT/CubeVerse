using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.Pooling
{
    public class PoolManager
    {
        private readonly Dictionary<Type, IPool> pools = new Dictionary<Type, IPool>();

        public void AddPool<T>(Pool<T> pool) where T : class => pools.Add(typeof(T), pool);

        public bool Contains<T>() => pools.ContainsKey(typeof(T));

        public T Get<T>() where T : class, new() => (pools[typeof(T)] as Pool<T>).Get();

        public void Return<T>(T instance) where T : class => (pools[typeof(T)] as Pool<T>).Return(instance);

        public bool TryAddPool<T>(Pool<T> pool) where T : class
        {
            if (pools.ContainsKey(typeof(T))) return false;

            AddPool(pool);
            
            return true;
        }
    }
}
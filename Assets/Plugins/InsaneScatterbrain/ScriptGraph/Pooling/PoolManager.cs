using System.Collections.Concurrent;

namespace InsaneScatterbrain.ScriptGraph
{
    public class PoolManager : Pooling.PoolManager, IInstanceProvider
    {
        private readonly BlockingCollection<IPool> pools = new BlockingCollection<IPool>();

        public BlockingCollection<IPool> Pools => pools;

        public bool TryAddPool<T>(Pool<T> pool) where T : class 
        {
            var poolAdded = base.TryAddPool(pool);
            if (poolAdded)
            {
                pools.Add(pool);
            }

            return poolAdded;
        }

        public void ReturnAll()
        {
            foreach (var pool in pools)
            {
                pool.ReturnAll();
            }
        }

        public void ClearAll()
        {
            foreach (var pool in pools)
            {
                pool.Clear();
            }
        }
    }
}
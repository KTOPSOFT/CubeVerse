using System.Collections.Concurrent;

namespace InsaneScatterbrain.ScriptGraph
{
    public abstract class Pool<T> : Pooling.Pool<T>, IPool where T : class
    {
        private readonly ConcurrentDictionary<T, bool> instancesInUse = new ConcurrentDictionary<T, bool>();

        public override T Get()
        {
            var instance = base.Get();
            instancesInUse.TryAdd(instance, true);
            return instance;
        }

        public override void Return(T instance)
        {
            base.Return(instance);
            instancesInUse.TryRemove(instance, out _);
        }

        public void ReturnAll()
        {
            foreach (var instance in instancesInUse.Keys)
            {
                base.Return(instance);
            }
            instancesInUse.Clear();
        }

        public override void Clear()
        {
            instancesInUse.Clear();
            base.Clear();
        }

        public int CountInstancesInUse => instancesInUse.Count;
    }

    public interface IPool : Pooling.IPool
    {
        int CountInstancesInUse { get; }
        void ReturnAll();
    }
}
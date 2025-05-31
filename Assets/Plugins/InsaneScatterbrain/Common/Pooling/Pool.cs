using System.Collections.Concurrent;

namespace InsaneScatterbrain.Pooling
{
    public abstract class Pool<T> : IPool where T : class
    {
        private readonly ConcurrentStack<T> instances = new ConcurrentStack<T>();

        public int CountInstancesInPool => instances.Count;

        protected abstract T New();
        protected abstract void Reset(T instance);

        public virtual T Get()
        {
            T instance = null;
            if (instances.Count > 0)
            {
                instances.TryPop(out instance);
            }

            if (instance == null)
            {
                instance = New();
            }

            Reset(instance);
            return instance;
        }

        public virtual void Return(T instance) => instances.Push(instance);

        public void Return(object instance) => instances.Push((T)instance);

        public virtual void Clear() => instances.Clear();
    }

    public interface IPool
    {
        int CountInstancesInPool { get; }
        void Return(object instance);
        void Clear();
    }
}
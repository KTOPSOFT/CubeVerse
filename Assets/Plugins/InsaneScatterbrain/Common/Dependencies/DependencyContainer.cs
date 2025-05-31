using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.Dependencies
{
    public class DependencyContainer : IDependencyContainer
    {
        private readonly Dictionary<Type, Func<object>> objs = new Dictionary<Type, Func<object>>();
        public void Register<T>(Func<T> getObjFunc) where T : class => objs[typeof(T)] = getObjFunc;
        public void Register(Type type, Func<object> getObjFunc) => objs[type] = getObjFunc;

        public void Register(IDependencyContainer dependencyContainer)
        {
            foreach (var dependency in dependencyContainer.GetDependencies())
            {
                Register(dependency.Key, dependency.Value);
            }
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            if (!objs.ContainsKey(type))
            {
                throw new DependencyNotFoundException(type);
            }
            
            return objs[type]() as T;
        }

        public IReadOnlyDictionary<Type, Func<object>> GetDependencies()
        {
            return objs;
        }

        public virtual void Clear() => objs.Clear();
    }
}
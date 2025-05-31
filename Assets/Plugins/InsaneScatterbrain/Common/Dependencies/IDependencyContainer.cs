using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.Dependencies
{
    public interface IDependencyContainer
    {
        void Register<T>(Func<T> obj) where T : class;
        void Register(Type type, Func<object> getObjFunc);
        void Register(IDependencyContainer dependencyContainer);
        T Get<T>() where T : class;
        IReadOnlyDictionary<Type, Func<object>> GetDependencies();
    }
}
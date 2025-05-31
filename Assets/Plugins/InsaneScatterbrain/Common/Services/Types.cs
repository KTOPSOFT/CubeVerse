using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InsaneScatterbrain.Services
{
    public static class Types
    {
        public static IEnumerable<Assembly> AllAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
        
        public static IEnumerable<Type> All() => AllAssemblies().SelectMany(assembly => assembly.GetTypes());

        public static IEnumerable<Type> ChildrenOf<T>() => All().Where(t => typeof(T).IsAssignableFrom(t));
        
        public static IEnumerable<Type> ConcreteChildrenOf<T>() => All().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        public static IEnumerable<Type> WithAttribute<T>() where T : Attribute => 
            All().Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        
        public static T ConvertTo<T>(object val) => (T) Convert.ChangeType(val, typeof(T));
    }
}

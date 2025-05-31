using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MalbersAnimations
{
    public static class ReflectionUtility
    {
        public static FieldInfo GetFieldViaPath(this Type type, string path)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var parent = type;
            var fi = parent.GetField(path, flags);
            var paths = path.Split('.');

            for (int i = 0; i < paths.Length; i++)
            {
                fi = parent.GetField(paths[i], flags);

                if (fi == null) { continue; }

                // there are only two container field type that can be serialized:
                // Array and List<T>
                if (fi.FieldType.IsArray)
                {
                    parent = fi.FieldType.GetElementType();
                    i += 2;
                    continue;
                }

                if (fi.FieldType.IsGenericType)
                {
                    parent = fi.FieldType.GetGenericArguments()[0];
                    i += 2;
                    continue;
                }

                if (fi != null)
                {
                    parent = fi.FieldType;
                }
                else
                {
                    return null;
                }
            }

            return fi;
        }

        public static List<Type> GetAllTypes<T>()
        {
            // Store the States type.
            Type classtype = typeof(T);

            // Get all the types that are in the same Assembly (all the runtime scripts) as the Reaction type.
            var allTypes = classtype.Assembly.GetTypes();

            // Create an empty list to store all the types that are subtypes of Reaction.
            var SubTypeList = new List<Type>();

            // Go through all the types in the Assembly...
            for (int i = 0; i < allTypes.Length; i++)
            {
                // ... and if they are a non-abstract subclass then add them to the list.
                if (allTypes[i].IsSubclassOf(classtype) && !allTypes[i].IsAbstract)
                {
                    SubTypeList.Add(allTypes[i]);
                }
            }

            // Convert the list to an array and store it.
            return SubTypeList;
        }

        public static List<Type> GetAllTypes(Type type)
        {
            // Get all the types that are in the same Assembly (all the runtime scripts) as the Reaction type.
            var allTypes = type.Assembly.GetTypes();

            // Create an empty list to store all the types that are subtypes of Reaction.
            var SubTypeList = new List<Type>();

            // Go through all the types in the Assembly...
            for (int i = 0; i < allTypes.Length; i++)
            {
                // ... and if they are a non-abstract subclass of Reaction then add them to the list.
                if (allTypes[i].IsSubclassOf(type) && !allTypes[i].IsAbstract)
                {
                    SubTypeList.Add(allTypes[i]);
                }
            }

            // Convert the list to an array and store it.
            return SubTypeList;
        }

        public static Type[] GetTypesByName(string className)
        {
            List<Type> returnVal = new();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                    }
                }
            }

            return returnVal.ToArray();
        }

        public static Type GetTypeByName(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.Name == className);
        }

        public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<PropertyInfo> propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        public static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<MethodInfo> methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }

        public static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static PropertyInfo GetProperty(object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static MethodInfo GetMethod(object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
        }

        public static Type GetListElementType(Type listType)
        {
            if (listType.IsGenericType)
            {
                return listType.GetGenericArguments()[0];
            }
            else
            {
                return listType.GetElementType();
            }
        }

        /// <summary>
        ///	 Get type and all base types of target, sorted as following:
        ///	 <para />[target's type, base type, base's base type, ...]
        /// </summary>
        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new() { target.GetType() };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }
    }
}
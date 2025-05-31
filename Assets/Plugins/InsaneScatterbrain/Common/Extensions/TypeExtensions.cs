using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InsaneScatterbrain.Extensions
{
    /// <summary>
    /// Contains methods to get friendly, human-readable type names even when using generics.
    ///
    /// Original source: https://stackoverflow.com/a/41961738
    /// 
    /// Modified to be able to either use name or fullname and to adhere better to the coding style of this project.
    /// Added some comments.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> DefaultDictionary = new Dictionary<Type, string>
        {
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(bool), "bool"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(char), "char"},
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(void), "void"}
        };

        /// <summary>
        /// Gets a friendly human-readable name of the type, even when generics are involved.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="translations">Dictionary of string representations of types.</param>
        /// <param name="useFullname">Whether to use the fullname or name.</param>
        /// <returns>The friendly type name.</returns>
        private static string GetFriendlyName(this Type type, IReadOnlyDictionary<Type, string> translations, bool useFullname)
        {
            var name = useFullname ? type.FullName : type.Name;

            if (translations.ContainsKey(type))
            {
                return translations[type];
            }
                
            if (type.IsArray)
            {
                var rank = type.GetArrayRank();
                var commas = rank > 1 
                    ? new string(',', rank - 1)
                    : "";
                
                return GetFriendlyName(type.GetElementType(), translations, useFullname) + $"[{commas}]";
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0].GetFriendlyName() + "?";
            }

            if (type.IsGenericType)
            {
                return name
                    .Split('`')[0] 
                       + "<" + string.Join(", ", type.GetGenericArguments()
                           .Select(GetFriendlyName).ToArray()) + ">";
            }
            
            return name;
        }

        /// <summary>
        /// Gets a friendly human-readable name of the type, even when generics are involved.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The friendly type name.</returns>
        public static string GetFriendlyName(this Type type)
        {
            return type.GetFriendlyName(DefaultDictionary, false);
        }

        /// <summary>
        /// Gets a friendly human-readable fullname of the type, even when generics are involved.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The friendly type fullname.</returns>
        public static string GetFriendlyFullname(this Type type)
        {
            return type.GetFriendlyName(DefaultDictionary, true);
        }
        
        public static T GetAttribute<T>(this Type type) where T : Attribute => 
            type.GetAttributes<T>().FirstOrDefault();

        public static IEnumerable<T> GetAttributes<T>(this Type type) where T : Attribute =>
            (IEnumerable<T>) type.GetCustomAttributes(typeof(T), true);
        
        public static IEnumerable<FieldInfo> GetPrivateFields(this Type type) =>
            type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets private fields of a type, including all the private fields of its parent types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Field info for all the private fields.</returns>
        public static IEnumerable<FieldInfo> GetAllPrivateFields(this Type type)
        {
            return type.BaseType == null 
                ? type.GetPrivateFields() 
                : type.GetPrivateFields().Concat(type.BaseType.GetAllPrivateFields());
        }
        
    }
}
using System;
using System.Linq;
using System.Reflection;

namespace InsaneScatterbrain.Extensions
{
    public static class FieldInfoExtensions
    {
        public static T GetAttribute<T>(this FieldInfo fieldInfo) where T : Attribute =>
            fieldInfo.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        
        public static bool HasAttribute<T>(this FieldInfo fieldInfo) where T : Attribute =>
            fieldInfo.GetCustomAttributes(typeof(T), true).Length > 0;
    }
}
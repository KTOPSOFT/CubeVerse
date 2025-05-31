using System;

namespace InsaneScatterbrain.Serialization
{
    public static class DefaultArraySerializer
    {
        public static string Serialize(object obj, Type elementType)
        {
            var array = (Array) Convert.ChangeType(obj, elementType.MakeArrayType());
            var parts = new string[array.Length];
            for (var i = 0; i < array.Length; ++i)
            {
                parts[i] = Serializer.Serialize(array.GetValue(i));
            }

            return string.Join("|", parts);
        }

        public static object Deserialize(string serializedObj, Type elementType)
        {
            var parts = serializedObj.Split('|');
            var array = Array.CreateInstance(elementType, parts.Length);

            for (var i = 0; i < parts.Length; ++i)
            {
                var elementValue = Serializer.Deserialize(elementType, parts[i]);
                array.SetValue(elementValue, i);
            }

            return array;
        }
    }
}
using System;

namespace InsaneScatterbrain.Serialization
{
    public abstract class ArraySerializer<T> : ISerializer, IDeserializer
    {
        public Type Type => typeof(T[]);
        
        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var rectInts = (T[]) obj;
            var parts = new string[rectInts.Length];
            for (var i = 0; i < rectInts.Length; ++i)
            {
                parts[i] = Serializer.Serialize(rectInts[i]);
            }

            return string.Join("|", parts);
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split('|');
            var rectInts = new T[parts.Length];

            for (var i = 0; i < parts.Length; ++i)
            {
                rectInts[i] = (T) Serializer.Deserialize(typeof(T), parts[i]);
            }

            return rectInts;
        }
    }
}
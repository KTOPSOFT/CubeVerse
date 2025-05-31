using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize RectInts.
    /// </summary>
    public class RectIntSerializer : ISerializer, IDeserializer
    {
        public Type Type => typeof(RectInt);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var rect = (RectInt) obj;
            return $"{rect.x},{rect.y},{rect.width},{rect.height}";
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            var w = int.Parse(parts[2]);
            var h = int.Parse(parts[3]);
            return new RectInt(x, y, w, h);
        } 
    }
}
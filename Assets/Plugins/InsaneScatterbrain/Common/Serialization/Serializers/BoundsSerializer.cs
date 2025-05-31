using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize Bounds.
    /// </summary>
    public class BoundsSerializer : ISerializer, IDeserializer
    {
        public Type Type => typeof(Bounds);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var bounds = (Bounds) obj;
            var c = bounds.center;
            var s = bounds.size;
            
            return $"{c.x},{c.y},{c.z},{s.x},{s.y},{s.z}";  
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var center = new Vector3(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2]));
            
            var size = new Vector3(
                float.Parse(parts[3]),
                float.Parse(parts[4]),
                float.Parse(parts[5]));

            return new Bounds(center, size);
        } 
    }
}
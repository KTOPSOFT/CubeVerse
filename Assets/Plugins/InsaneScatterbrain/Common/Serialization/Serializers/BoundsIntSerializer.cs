using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize BoundInts.
    /// </summary>
    public class BoundsIntSerializer : ISerializer, IDeserializer
    {
        public Type Type => typeof(BoundsInt);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var bounds = (BoundsInt) obj;

            return $"{bounds.xMin},{bounds.yMin},{bounds.zMin},{bounds.size.x},{bounds.size.y},{bounds.size.z}";
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var xMin = int.Parse(parts[0]);
            var yMin = int.Parse(parts[1]);
            var zMin = int.Parse(parts[2]);
            var sizeX = int.Parse(parts[3]);
            var sizeY = int.Parse(parts[4]);
            var sizeZ = int.Parse(parts[5]);

            return new BoundsInt(xMin, yMin, zMin, sizeX, sizeY, sizeZ);
        } 
    }
}
using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize Vector3Ints.
    /// </summary>
    public class Vector3IntSerializer : ISerializer, IDeserializer
    {
        
        public Type Type => typeof(Vector3Int);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var vector3Int = (Vector3Int) obj;
            return $"{vector3Int.x},{vector3Int.y},{vector3Int.z}"; 
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            var z = int.Parse(parts[2]);
            return new Vector3Int(x,y,z);
        } 
    }
}
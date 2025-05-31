using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize Vector2Ints.
    /// </summary>
    public class Vector2IntSerializer : ISerializer, IDeserializer
    {
        public Type Type => typeof(Vector2Int);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var vector2Int = (Vector2Int) obj;
            return $"{vector2Int.x},{vector2Int.y}";
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            return new Vector2Int(x,y); 
        } 
    }
}
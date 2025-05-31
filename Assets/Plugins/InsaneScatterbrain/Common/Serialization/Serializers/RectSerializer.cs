using System;
using UnityEngine;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// This class is used to serialize and deserialize Rects.
    /// </summary>
    public class RectSerializer : ISerializer, IDeserializer
    {
        public Type Type => typeof(Rect);

        /// <inheritdoc cref="ISerializer.Serialize"/>
        public string Serialize(object obj)
        {
            var rect = (Rect) obj;
            return $"{rect.x},{rect.y},{rect.width},{rect.height}"; 
        }
        
        /// <inheritdoc cref="IDeserializer.Deserialize"/>
        public object Deserialize(string serializedObj)
        {
            var parts = serializedObj.Split(',');
            var x = float.Parse(parts[0]);
            var y = float.Parse(parts[1]);
            var w = float.Parse(parts[2]);
            var h = float.Parse(parts[2]);
            return new Rect(x, y, w, h);
        } 
    }
}
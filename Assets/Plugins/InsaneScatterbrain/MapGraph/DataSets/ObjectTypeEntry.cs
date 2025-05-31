using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents an entry for a certain object type.
    /// </summary>
    /// <typeparam name="T">The type of Object represented by this entry.</typeparam>
    [Serializable]
    public abstract class ObjectTypeEntry<T> : IObjectTypeEntry<T> where T : Object
    {
        [SerializeField] private float weight = 1f;
        
        /// <inheritdoc cref="IObjectTypeEntry{T}.Value"/>
        public abstract T Value { get; set; }

        /// <inheritdoc cref="IObjectTypeEntry{T}.Weight"/>
        public float Weight
        {
            get => weight;
            set => weight = value;
        }
    }
}
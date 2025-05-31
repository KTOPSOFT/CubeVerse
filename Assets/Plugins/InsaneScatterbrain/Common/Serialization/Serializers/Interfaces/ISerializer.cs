using System;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// Classes implementing this interface are used by the serializer to determine how the type should be serialized.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// The type to serialize.
        /// </summary>
        Type Type { get; }
        
        /// <summary>
        /// Serializes the object to string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized string.</returns>
        string Serialize(object obj);
    }
}
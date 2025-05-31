using System;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// Classes implementing this interface are used by the serializer to determine how the type should be deserialized.
    /// </summary>
    public interface IDeserializer
    {
        /// <summary>
        /// The type to deserialize.
        /// </summary>
        Type Type { get; }
        
        /// <summary>
        /// Deserializes the string to object.
        /// </summary>
        /// <param name="serializedObj">The serialized string.</param>
        /// <returns></returns>
        object Deserialize(string serializedObj);
    }
}
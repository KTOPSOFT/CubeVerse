using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// A static class that serializes types that it can find implementations of ISerializer for and deserializes types
    /// that it can find implementations of IDeserializer for.
    /// </summary>
    public static class Serializer
    {
        private static readonly Dictionary<Type, ISerializer> Serializers;
        private static readonly Dictionary<Type, IDeserializer> Deserializers;
        
        /// <summary>
        /// Whenever serializer is called for the first time, it will look through all the assemblies to find and store
        /// implementations of ISerializer and IDeserializer. Which can then be used to complete calls to Serialize and
        /// Deserialize.
        /// </summary>
        static Serializer()
        { 
            Serializers = new Dictionary<Type, ISerializer>();
            Deserializers = new Dictionary<Type, IDeserializer>();

            var serializerTypes = Types.ChildrenOf<ISerializer>().Where(t => t.IsClass && !t.IsAbstract);
            var deserializerTypes = Types.ChildrenOf<ISerializer>().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var serializerType in serializerTypes)
            {
                var serializer = (ISerializer) Activator.CreateInstance(serializerType); 
                Serializers.Add(serializer.Type, serializer);
            }
            
            foreach (var deserializerType in deserializerTypes)
            {
                var deserializer = (IDeserializer) Activator.CreateInstance(deserializerType);
                Deserializers.Add(deserializer.Type, deserializer);
            }
        } 
        
        /// <summary>
        /// Serializes the given object to string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized string.</returns>
        public static string Serialize(object obj)
        {
            var type = obj.GetType();
            
            if (typeof(Object).IsAssignableFrom(type))
            {
                Debug.LogError("Don't use Serializer to serialize Unity Objects.");
            }

            string serializedVal;
            if (Serializers.ContainsKey(type))
            {
                serializedVal = Serializers[type].Serialize(obj);
            }
            else if (type.IsArray)
            {
                serializedVal = DefaultArraySerializer.Serialize(obj, type.GetElementType());
            }
            else
            {
                serializedVal = type.IsPrimitive || type == typeof(string)
                    ? Convert.ToString(obj, CultureInfo.InvariantCulture)
                    : JsonUtility.ToJson(obj);
            }

            return serializedVal;
        }

        /// <summary>
        /// Deserializes the given string to an object of the given type.
        /// </summary>
        /// <param name="type">The type to serialize to.</param>
        /// <param name="serializedObj">The serialized string.</param>
        /// <returns>The deserialized object.</returns>
        public static object Deserialize(Type type, string serializedObj)
        { 
            object obj;
            
            if (Deserializers.ContainsKey(type))
            {
                obj = Deserializers[type].Deserialize(serializedObj);
            } 
            else if (type.IsArray)
            {
                obj = DefaultArraySerializer.Deserialize(serializedObj, type.GetElementType());
            }
            else
            {
                obj = type.IsPrimitive || type == typeof(string)
                    ? Convert.ChangeType(serializedObj, type, CultureInfo.InvariantCulture) 
                    : JsonUtility.FromJson(serializedObj, type);
            }

            return obj;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.Serialization
{
    /// <summary>
    /// A databag can persist values of different types through Unity's serialization system. Types that are not
    /// supported by Unity's serialization out-of-the-box require implementations of the ISerializer and IDeserializer
    /// interfaces for that type.
    ///
    /// Implementations of supported types will overwrite the default serialization behaviour, with the exception of
    /// classes that inherit from Unity's Object.
    /// </summary>
    [Serializable]
    public class DataBag : ISerializationCallbackReceiver
    {
        [Serializable]
        private class SerializedObjectArray
        {
            public string elementTypeName;
            public Object[] objs;
        }
        
        private Dictionary<string, object> objs = new Dictionary<string, object>();
        
        [SerializeField, HideInInspector] private List<string> names;
        [SerializeField, HideInInspector] private List<string> typeNames;
        [SerializeField, HideInInspector] private List<string> serializedValues;
        
        [SerializeField, HideInInspector] private List<string> objectNames; 
        [SerializeField, HideInInspector] private List<Object> serializedObjects;

        [SerializeField, HideInInspector] private List<string> objectArrayNames;
        [SerializeField, HideInInspector] private List<SerializedObjectArray> serializedObjectArrays;

        public IEnumerable<string> Names => objs.Keys;
        
        /// <summary>
        /// Store the given value under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Set(string name, object value)
        {
            objs[name] = value;
        }

        /// <summary>
        /// Gets the value stored under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The associated value.</returns>
        public object Get(string name)
        {
            return objs[name];
        }

        /// <summary>
        /// Checks if a value is stored under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if the name exists, false otherwise.</returns>
        public bool Contains(string name)
        {
            return objs.ContainsKey(name);
        }

        /// <summary>
        /// Removes the value from the bag that is stored under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Remove(string name)
        {
            objs.Remove(name);
        }
        
        /// <summary>
        /// Called before Unity serializes the databag. It serializes all the objects inside it.
        /// </summary>
        public void OnBeforeSerialize()
        {
            names = new List<string>();
            typeNames = new List<string>();
            serializedValues = new List<string>();
            objectNames = new List<string>();
            serializedObjects = new List<Object>();
            objectArrayNames = new List<string>();
            serializedObjectArrays = new List<SerializedObjectArray>(); 
            
            foreach (var obj in objs)
            {
                var name = obj.Key;
                var value = obj.Value;

                if (value == null)
                {
                    // No value set, nothing to store.
                    continue;
                }
                
                var type = value.GetType();
                
                // Store the Unity objects in a list of objects so that it's serialized by Unity's system normally.
                if (typeof(Object).IsAssignableFrom(type))
                {
                    objectNames.Add(name);
                    serializedObjects.Add((Object) value);
                    continue;
                }

                // Arrays of a type that derive from Unity's Object type are stored in a separate list of.
                // They are wrapped inside a SerializedObjectArray, because directly storing an array in a list
                // won't be picked up by Unity's serialization system.
                var elementType = type.GetElementType();
                if (type.IsArray && elementType != null && typeof(Object).IsAssignableFrom(elementType))
                {
                    objectArrayNames.Add(name);

                    var serializableArray = new SerializedObjectArray
                    {
                        elementTypeName = elementType.AssemblyQualifiedName,
                        objs = (Object[])value
                    };
                    serializedObjectArrays.Add(serializableArray);
                    continue; 
                }

                var typeName = type.AssemblyQualifiedName;
                var serializedVal = Serializer.Serialize(value);
                
                names.Add(name);
                typeNames.Add(typeName);
                serializedValues.Add(serializedVal);
            }
        }

        /// <summary>
        /// Called after Unity deserializes the databag. It deserializes all the objects inside it.
        /// </summary>
        public void OnAfterDeserialize()
        {
            objs = new Dictionary<string, object>();

            for (var i = 0; i < names.Count; ++i)
            {
                var name = names[i];
                var typeName = typeNames[i];
                var type = Type.GetType(typeName);
                var serializedValue = serializedValues[i];

                var value = Serializer.Deserialize(type, serializedValue);

                objs.Add(name, value);
            }
            
            for (var i = 0; i < objectNames.Count; ++i)
            {
                var objectName = objectNames[i];
                var obj = serializedObjects[i]; 
                
                objs.Add(objectName, obj);
            }

            for (var objectArrayIndex = 0; objectArrayIndex < objectArrayNames.Count; ++objectArrayIndex)
            {
                var objectName = objectArrayNames[objectArrayIndex];
                var serializedObjectArray = serializedObjectArrays[objectArrayIndex];
                var objectArray = serializedObjectArray.objs;

                var elementType = Type.GetType(serializedObjectArray.elementTypeName);
                var array = Array.CreateInstance(elementType, objectArray.Length);

                for (var entryIndex = 0; entryIndex < array.Length; ++entryIndex) 
                {
                    var entry = objectArray[entryIndex];
                    array.SetValue(entry != null ? entry : null, entryIndex);
                } 
                
                objs.Add(objectName, array);
            }
        }
    }
}
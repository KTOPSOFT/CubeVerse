using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    [Serializable]
    public class DataDependency : ISerializationCallbackReceiver
    {
        [SerializeField] private ScriptableObject scriptableObject;
        [SerializeField] private string serializedType;

        public ScriptableObject ScriptableObject
        {
            get => scriptableObject;
            set => scriptableObject = value;
        }

        public Type Type { get; set; }
        
        public void OnBeforeSerialize()
        {
            serializedType = Type?.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            Type = string.IsNullOrEmpty(serializedType) ? null : Type.GetType(serializedType);
        }
    }
}

using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    [Serializable]
    public class ComponentDependency : ISerializationCallbackReceiver
    {
        [SerializeField] private GameObject gameObject;
        [SerializeField] private MonoBehaviour component;
        [SerializeField] private string serializedType;

        public GameObject GameObject
        {
            get => gameObject;
            set => gameObject = value;
        }
        
        public MonoBehaviour Component
        {
            get => component;
            set => component = value;
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
using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    [Serializable]
    public class ScriptGraphParameter : DataSetItem, ISerializationCallbackReceiver
    {
        public Type Type { get; private set; }

        public ScriptGraphParameter(string name, Type type) : base(name)
        {
            Type = type;
        }
        
        #region Serialization

        [SerializeField] private string typeName;
        
        public void OnBeforeSerialize()
        {
            typeName = Type != null ? Type.AssemblyQualifiedName : null;
        }

        public void OnAfterDeserialize() 
        {
            Type = Type.GetType(typeName);
        }

        #endregion
    }
}
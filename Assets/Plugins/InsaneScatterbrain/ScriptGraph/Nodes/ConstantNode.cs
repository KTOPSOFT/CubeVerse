using System;
using InsaneScatterbrain.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// The constant node outputs a constant value that can be set in the graph editor.
    /// </summary>
    [Serializable]
    public class ConstantNode : ProviderNode, ISerializationCallbackReceiver
    {
        private Type constType;
        [SerializeField] private string constTypeName;

        /// <summary>
        /// Returns the value type.
        /// </summary>
        public Type ConstType => constType;

        private object val;
        
        /// <summary>
        /// Gets/sets the constant value.
        /// </summary>
        public object Value
        {
            get => val;
            set
            {
                if (!constType.IsInstanceOfType(value))
                {
                    Debug.LogError("Invalid value for const type");
                }
                val = value;
            }
        }

        /// <summary>
        /// Factory method for creating a new constant node of the given type for the given graph.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The new constant node.</returns>
        public static ConstantNode Create(Type type)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            if (type == typeof(string))
            {
                defaultValue = string.Empty;
            }
            
            var constNode = new ConstantNode
            {
                constType = type,
                val = defaultValue
            };
            constNode.OnLoadOutputPorts();
            return constNode;
        }
        
        /// <summary>
        /// Loads the output ports. In the case of the constant node only a single port is being added that returns
        /// the set constant value.
        /// </summary>
        public override void OnLoadOutputPorts()
        {
            var outPort = AddOut(string.Empty, constType);

            if (val is IPreparable preparable)
            {
                preparable.Prepare();
            }
            
            outPort.Set(() => val);
        }

        #region Serialization
        [SerializeField] private string serializedVal;
        [SerializeField] private Object serializedObj;

        public void OnBeforeSerialize()
        {
            constTypeName = constType.AssemblyQualifiedName;
            
            // If it's a Unity object of some kind, just store the reference and let Unity handle it.
            if (typeof(Object).IsAssignableFrom(constType))
            {
                serializedObj = (Object) val; 
                return;
            }
            
            // Otherwise let the serializer serialize the value.
            serializedVal = Serializer.Serialize(val);
        }

        public void OnAfterDeserialize()
        {
            constType = Type.GetType(constTypeName);
            
            // If it's a Unity object of some kind, just store the reference and let Unity handle it.
            if (typeof(Object).IsAssignableFrom(constType))
            {
                val = serializedObj;  
                return;
            }
            
            // Otherwise let the serializer deserialize the value.
            val = Serializer.Deserialize(constType, serializedVal);
        }
        #endregion
    }
}
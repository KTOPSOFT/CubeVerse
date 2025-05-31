using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// The input node is used to output the value of an associated input parameter. 
    /// </summary>
    [Serializable]
    public class InputNode : ProviderNode, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("inputName")] 
        [SerializeField] private string inputParameterId;

        /// <summary>
        /// Gets the input variable's name.
        /// </summary>
        public string InputParameterId
        {
            get => inputParameterId;
            set => inputParameterId = value;
        }

        private Type inputType;

        /// <summary>
        /// Gets the out port.
        /// </summary>
        public OutPort OutPort => OutPorts.First();

        protected InputNode()
        {
        }

        /// <summary>
        /// Factory method to create a new input node with the given name of the given type, for the given graph.
        /// </summary>
        /// <param name="inputParameterId">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>The new input node.</returns>
        public static InputNode Create(string inputParameterId, Type type)
        {
            var inputNode = new InputNode
            {
                inputParameterId = inputParameterId, 
                inputType = type
            };
            inputNode.OnLoadOutputPorts();
            return inputNode;
        }
        
        /// <inheritdoc cref="Create"/>
        public static InputNode Create<T>(string inputParameterId)
        {
            return Create(inputParameterId, typeof(T));
        }
        
        /// <inheritdoc cref="ProviderNode.OnLoadOutputPorts"/>
        public override void OnLoadOutputPorts()
        {
            AddOut(string.Empty, inputType);
        }

        #region Serialization
        [SerializeField] private string inputTypeName;
        
        public void OnBeforeSerialize()
        {
            inputTypeName = inputType.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            inputType = Type.GetType(inputTypeName);
        }
        #endregion
    }
}
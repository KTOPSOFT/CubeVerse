using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// The output node is used to take a value from the script graph and output it as part of the return value when
    /// done processing the script graph.
    /// </summary>
    [Serializable]
    public class OutputNode : ConsumerNode, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("outputName")] 
        [SerializeField] private string outputParameterId;
        
        /// <summary>
        /// The name of the output variable.
        /// </summary>
        public string OutputParameterId
        {
            get => outputParameterId;
            set => outputParameterId = value;
        }

        private Type outputType;

        protected OutputNode()
        {
        }
        
        /// <summary>
        /// Factory method to create a new output node with the given name of the given type, for the given graph.
        /// </summary>
        /// <param name="outputParameterId">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>The new output node.</returns>
        public static OutputNode Create(string outputParameterId, Type type)
        {
            var outputNode = new OutputNode
            {
                outputParameterId = outputParameterId, 
                outputType = type
            };
            outputNode.OnLoadInputPorts();
            return outputNode;
        }

        /// <inheritdoc cref="Create"/>
        public static OutputNode Create<T>(string outputParameterId)
        {
            return Create(outputParameterId, typeof(T));
        }
        
        /// <inheritdoc cref="ConsumerNode.OnLoadInputPorts"/>
        public override void OnLoadInputPorts()
        {
            AddIn(string.Empty, outputType);
        }

        #region Serialization
        [SerializeField] private string outputTypeName;
        
        public void OnBeforeSerialize()
        {
            outputTypeName = outputType.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            outputType = Type.GetType(outputTypeName);
        }
        #endregion
    }
}
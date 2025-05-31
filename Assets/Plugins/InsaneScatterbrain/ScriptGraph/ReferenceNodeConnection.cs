#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class represents a connection between two nodes through a reference node. Only available in the editor.
    /// </summary>
    [Serializable]
    [MovedFrom(false, "InsaneScatterbrain.ScriptGraph", null, "DuplicateConnection")]
    public class ReferenceNodeConnection
    {
        [SerializeReference] private InPort inPort;
        [SerializeReference] private OutPort outPort;
        [FormerlySerializedAs("providerNodeDuplicate")] [SerializeReference] private ReferenceNode referenceNode;

        /// <summary>
        /// Gets the connected reference node.
        /// </summary>
        public ReferenceNode ReferenceNode => referenceNode;
        
        /// <summary>
        /// Gets the in port.
        /// </summary>
        public InPort InPort => inPort;
        
        /// <summary>
        /// Gets the out port.
        /// </summary>
        public OutPort OutPort => outPort;

        /// <summary>
        /// Creates a new reference node connection between the two given ports.
        /// </summary>
        /// <param name="referenceNode">The reference node.</param>
        /// <param name="inPort">The in port.</param>
        /// <param name="outPort">The out port.</param>
        public ReferenceNodeConnection(ReferenceNode referenceNode, InPort inPort, OutPort outPort)
        {
            this.referenceNode = referenceNode;
            this.inPort = inPort;
            this.outPort = outPort;
        }
    }
}
#endif
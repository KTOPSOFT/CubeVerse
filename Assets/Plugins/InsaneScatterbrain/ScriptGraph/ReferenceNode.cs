#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// The reference node is a node that represents a provider node. This is useful to keep a tidy looking
    /// graph. Only available in the editor.
    /// </summary>
    [Serializable]
    [MovedFrom(false, "InsaneScatterbrain.ScriptGraph", null, "ProviderNodeDuplicate")]
    public class ReferenceNode : INode
    {
        [SerializeField] private Rect position;
        [SerializeReference] private IProviderNode providerNode;
        
        [SerializeField] private string id;

        public string Id
        {
            get 
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = Guid.NewGuid().ToString();
                }
                return id;
            }
            set => id = value;
        }

        /// <summary>
        /// Gets/sets the reference node's position in the graph editor.
        /// </summary>
        public Rect Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Gets the original provider node this reference node is referring to.
        /// </summary>
        public IProviderNode ProviderNode => providerNode;
    
        /// <summary>
        /// Creates a new provider node of the given provider node at the given position in the graph editor.
        /// </summary>
        /// <param name="providerNode">The provider node to refer to.</param>
        public ReferenceNode(IProviderNode providerNode)
        {
            this.providerNode = providerNode;
        }
    }
}
#endif
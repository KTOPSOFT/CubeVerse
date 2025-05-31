#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    [Serializable]
    public class GroupNode : INode
    {
        [SerializeReference] private List<IScriptNode> nodes = new List<IScriptNode>();
        [FormerlySerializedAs("duplicateNodes")] [SerializeReference] private List<ReferenceNode> referenceNodes = new List<ReferenceNode>();
        [SerializeField] private string title;
        [SerializeField] private Rect position;
        
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

        private ReadOnlyCollection<IScriptNode> readOnlyNodes;
        private ReadOnlyCollection<ReferenceNode> readOnlyReferenceNodes;

        public ReadOnlyCollection<IScriptNode> Nodes => readOnlyNodes ?? (readOnlyNodes = nodes.AsReadOnly());
        public ReadOnlyCollection<ReferenceNode> ReferenceNodes =>
            readOnlyReferenceNodes ?? (readOnlyReferenceNodes = referenceNodes.AsReadOnly());
        
        public Rect Position
        {
            get => position;
            set => position = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public GroupNode(string title) => this.title = title;
        public void Add(IScriptNode node) => nodes.Add(node);
        public void Add(ReferenceNode node) => referenceNodes.Add(node);
        public void Remove(IScriptNode node) => nodes.Remove(node);
        public void Remove(ReferenceNode node) => referenceNodes.Remove(node);
        public bool Contains(IScriptNode node) => nodes.Contains(node);
        public bool Contains(ReferenceNode node) => referenceNodes.Contains(node);

        public void Clean()
        {
            nodes.RemoveAll(node => node == null);
            referenceNodes.RemoveAll(referenceNode => referenceNode == null);
            referenceNodes.RemoveAll(referenceNode => referenceNode.ProviderNode == null);
        }
    }
}
#endif
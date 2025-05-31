using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class CopyData
    {
        public List<ReferenceNode> ReferenceNodes { get; } = new List<ReferenceNode>();
        public List<IScriptNode> ScriptNodes { get; } = new List<IScriptNode>();
        public List<GroupNode> GroupNodes { get; } = new List<GroupNode>();
        public List<(IConsumerNode, string, IProviderNode, string, ReferenceNode)> Connections { get; } =
            new List<(IConsumerNode, string, IProviderNode, string, ReferenceNode)>();
        
#if UNITY_2020_1_OR_NEWER
        public List<Note> Notes { get; } = new List<Note>();

        public bool IsEmpty => ReferenceNodes.Count == 0 && ScriptNodes.Count == 0 && GroupNodes.Count == 0 && Notes.Count == 0;
#else
        public bool IsEmpty => ReferenceNodes.Count == 0 && ScriptNodes.Count == 0 && GroupNodes.Count == 0;
#endif

        public Vector2 SelectionCenter { get; set; }

        public void Clear()
        {
            ReferenceNodes.Clear();
            ScriptNodes.Clear();
            GroupNodes.Clear();
            Connections.Clear();
#if UNITY_2020_1_OR_NEWER
            Notes.Clear();
#endif
            SelectionCenter = Vector2.zero;
        }
    }
}
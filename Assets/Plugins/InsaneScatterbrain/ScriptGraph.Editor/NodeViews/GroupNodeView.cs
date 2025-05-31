using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class GroupNodeView : Group
    {
        private readonly GroupNode groupNode;
        private readonly ScriptGraphView graphView;

        public GroupNode GroupNode => groupNode;
        
        public GroupNodeView(GroupNode groupNode, ScriptGraphView graphView)
        {
            this.groupNode = groupNode;
            this.graphView = graphView;
        }
        
        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            foreach (var element in elements)
            {
                if (!(element is ScriptNodeView nodeView)) continue;

                var node = nodeView.Node;
                if (nodeView.userData != null)
                {
                    var referenceNode = (ReferenceNode) nodeView.userData;
                    
                    if (groupNode.Contains(referenceNode)) continue;
                    
                    groupNode.Add(referenceNode);

                    continue;
                }
                
                if (groupNode.Contains(node)) continue;

                groupNode.Add(node);
            }
            
            EditorUtility.SetDirty(graphView.Graph);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (graphView.IsClearing) return;   // Don't process changes if it's part of the graph view's clearing process.
            
            base.OnElementsRemoved(elements);
            foreach (var element in elements)
            {
                if (!(element is ScriptNodeView nodeView)) continue;
                
                if (!groupNode.Contains(nodeView.Node)) continue;

                groupNode.Remove(nodeView.Node);
            }
            
            EditorUtility.SetDirty(graphView.Graph);
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);

            if (string.IsNullOrEmpty(newName))
            {
                newName = "[Untitled]";
            }
            groupNode.Title = newName;
            title = newName;
           
            EditorUtility.SetDirty(graphView.Graph);
        }
    }
}
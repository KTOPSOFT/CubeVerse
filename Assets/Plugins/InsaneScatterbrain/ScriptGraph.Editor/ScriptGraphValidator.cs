using System.Collections.Generic;
using UnityEditor;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class ScriptGraphValidator
    {
        public int InvalidConnectionsRemoved { get; private set; }
        public int InvalidReferenceNodesRemoved { get; private set; }
        
        public void ValidateAndRepair(ScriptGraphGraph graph)
        {
            // Start by cleaning the graph, removing any null nodes.
            graph.Clean();
            
            InvalidConnectionsRemoved = 0;
            InvalidReferenceNodesRemoved = 0;
            
            var nodes = new List<IScriptNode>(graph.Nodes);
            foreach (var node in nodes)
            {
                // Check if the nodes the ports are connected to, still exist, if not, remove the connection.
                if (node is IConsumerNode consumerNode)
                {
                    var inPorts = new List<InPort>(consumerNode.InPorts);
                    foreach (var inPort in inPorts)
                    {
                        if (inPort.ConnectedOut == null || graph.Nodes.Contains(inPort.ConnectedOut.Owner)) continue;
                        
                        inPort.Disconnect();
                        InvalidConnectionsRemoved++;
                    }
                }
                
                if (node is IProviderNode providerNode)
                {
                    var outPorts = new List<OutPort>(providerNode.OutPorts);
                    foreach (var outPort in outPorts)
                    {
                        var inPorts = new List<InPort>(outPort.ConnectedInPorts);
                        foreach (var inPort in inPorts)
                        {
                            if (graph.Nodes.Contains(inPort.Owner)) continue;
                            
                            outPort.Disconnect(inPort);
                            InvalidConnectionsRemoved++;
                        }
                    }
                }
                
                // If reference nodes are still around while it's original no longer exist, remove it.
                var referenceNodes = new List<ReferenceNode>(graph.ReferenceNodes);
                foreach (var referenceNode in referenceNodes)
                {
                    if (graph.Nodes.Contains(referenceNode.ProviderNode)) continue;
                    
                    graph.RemoveReferenceNode(referenceNode);
                    InvalidReferenceNodesRemoved++;
                }
            }
            
            // If groups contain nodes that don't exist in the group, remove them from the group
            var groups = new List<GroupNode>(graph.GroupNodes);
            foreach (var group in groups)
            {
                var groupNodes = new List<IScriptNode>(group.Nodes);
                foreach (var node in groupNodes)
                {
                    if (graph.Nodes.Contains(node)) continue; 
                    
                    group.Remove(node);
                }
                
                var referenceNodes = new List<ReferenceNode>(group.ReferenceNodes);
                foreach (var referenceNode in referenceNodes)
                {
                    if (graph.ReferenceNodes.Contains(referenceNode)) continue;
                    
                    group.Remove(referenceNode);
                }
            }
            
            EditorUtility.SetDirty(graph);
        }
    }
}
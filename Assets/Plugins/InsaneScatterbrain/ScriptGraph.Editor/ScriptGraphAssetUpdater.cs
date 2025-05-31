
using System.Collections.Generic;
using System.Threading.Tasks;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class ScriptGraphAssetUpdater
    {
        private readonly HashSet<InPort> inPorts;
        private readonly HashSet<OutPort> outPorts;
        
        private bool currentGraphChanged;
        private bool graphChanged;

        public ScriptGraphAssetUpdater()
        {
            inPorts = new HashSet<InPort>();
            outPorts = new HashSet<OutPort>();
        }
        
        public void Start()
        {
            graphChanged = false; 
            
            // Find all the existing script graphs.
            var graphs = Assets.Find<ScriptGraphGraph>(); 
            
            var removeNodes = new List<IScriptNode>();

            // Check all the graphs for deleted things.
            foreach (var graph in graphs) 
            {
                // Start by cleaning the graph, removing any null nodes.
                graph.Clean();
                
                currentGraphChanged = false;
                
                removeNodes.Clear();
                foreach (var node in graph.Nodes)
                {
                    // If the node is considered to be null, it's no longer valid and can be removed entirely.
                    if (node == null)
                    {
                        removeNodes.Add(node);
                        continue;
                    }
                    
                    // If this node has any in ports, remove any that no longer exist.
                    if (node is IConsumerNode consumerNode)
                    {
                        // Add all the ports to the inactive set.
                        inPorts.Clear();
                        foreach (var inPort in consumerNode.InPorts)
                        {
                            inPorts.Add(inPort);
                        }
                        
                        // Now load all the ports for that node and remove any port that is loaded from the inactive
                        // port set. Any port that remains in that set, is no longer used and can be removed.
                        // Any port that is not in the inactive set, is a new port and should cause the graphchange
                        // flag to be set.
                        consumerNode.OnInPortAdded += UpdateInPort;
                        consumerNode.OnLoadInputPorts();
                        consumerNode.OnInPortAdded -= UpdateInPort; 
                        
                        // Any remaining ports are the inactive ports.
                        foreach (var inactiveInPort in inPorts)
                        {
                            inactiveInPort.Disconnect();    // Make sure the port is disconnected before removing it.
                            currentGraphChanged = true;

                            if (!consumerNode.HasInPort(inactiveInPort.Name))
                            {
                                continue;
                            }
                            
                            var existingPort = consumerNode.GetInPort(inactiveInPort.Name);
                            if (existingPort.Type != inactiveInPort.Type)
                            {
                                // This is not the same port, type has changed, so don't remove it.
                                continue;
                            }
                                
                            consumerNode.RemoveIn(inactiveInPort.Name);
                        }
                    }
            
                    // If this node has any out ports, remove any that no longer exist.
                    if (node is IProviderNode providerNode)
                    {
                        // Add all the ports to the inactive set.
                        outPorts.Clear();
                        foreach (var outPort in providerNode.OutPorts)
                        {
                            outPorts.Add(outPort); 
                        }
                        
                        // Now load all the ports for that node and remove any port that is loaded from the inactive
                        // port set. Any port that remains in that set, is no longer used and can be removed.
                        // Any port that is not in the inactive set, is a new port and should cause the graphchange
                        // flag to be set.
                        providerNode.OnOutPortAdded += UpdateOutPort;
                        providerNode.OnLoadOutputPorts();
                        providerNode.OnOutPortAdded -= UpdateOutPort;

                        // Any remaining ports are the inactive ports.
                        foreach (var inactiveOutPort in outPorts) 
                        {
                            inactiveOutPort.DisconnectAll();    // Make sure the port is disconnected before removing it.
                            currentGraphChanged = true;

                            if (!providerNode.HasOutPort(inactiveOutPort.Name))
                            {
                                continue;
                            }
                            
                            var existingPort = providerNode.GetOutPort(inactiveOutPort.Name);
                            if (existingPort.Type != inactiveOutPort.Type)
                            {
                                // This is not the same port, type has changed, so don't remove it.
                                continue;
                            }
                                
                            providerNode.RemoveOut(inactiveOutPort.Name);
                        }
                    }
                }
                
                foreach (var removeNode in removeNodes)
                {
                    graph.Remove(removeNode);
                    currentGraphChanged = true;
                }
                
                if (!currentGraphChanged)
                    continue;

                graphChanged = true;
                
                EditorUtility.SetDirty(graph);
            }

            if (!graphChanged) 
                return;

            ReloadWindows();
        }
        
        private static async void ReloadWindows()
        {
            await Task.Delay(1);
            ScriptGraphViewWindow.ReloadAll();
        }
        
        private void UpdateInPort(InPort inPort)
        {
            if (!inPorts.Contains(inPort))
            {
                // There's a new port, so the graph has changed.
                currentGraphChanged = true; 
                return;
            }
            inPorts.Remove(inPort);
        }
        
        private void UpdateOutPort(OutPort outPort)
        {
            if (!outPorts.Contains(outPort))
            {
                // There's a new port, so the graph has changed.
                currentGraphChanged = true;
                return;
            }
            outPorts.Remove(outPort);
        }
    }
}
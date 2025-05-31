using InsaneScatterbrain.ScriptGraph.Editor.NodeSearch;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Edge connector listener that triggers the node search menu to be opened whenever an edge connection is dropped
    /// outside a port, displaying only the nodes compatible with the port.
    /// If a new node is created through the menu, the port is connected to the first compatible port found on the
    /// new node.
    /// </summary>
    public class OutPortEdgeConnectListener : IEdgeConnectorListener
    {
        private readonly ScriptGraphPort port;
        private readonly NodeSearchWindow nodeSearchWindow;
        private readonly ScriptGraphView graphView;
        private readonly PortFilter portFilter;
        private readonly ReferenceNode referenceNode;

        public OutPortEdgeConnectListener(
            ScriptGraphView graphView, 
            NodeSearchWindow nodeSearchWindow, 
            ScriptGraphPort port,
            ReferenceNode referenceNode = null)
        {
            this.graphView = graphView;
            this.port = port;
            this.nodeSearchWindow = nodeSearchWindow;
            portFilter = new PortFilter(port);
            this.referenceNode = referenceNode;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 placementPosition)
        {
            // When releasing the edge outside of a node, open up the add node menu and show all the nodes that can be
            // created to connect this port to.
            var menuPosition = GUIUtility.GUIToScreenPoint(placementPosition);
            var placementPos = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, placementPosition);
            nodeSearchWindow.Open(menuPosition, placementPos, portFilter, AutoConnect);
        }

        private void AutoConnect(IScriptNode node)
        {
            // If the port is an in port, find the first compatible out port on the newly created node and
            // connect the port to it.
            if (node is IProviderNode provider && port is InPort autoConnectInPort)
            {
                foreach (var outPort in provider.OutPorts)
                {
                    if (!port.Type.IsAssignableFrom(outPort.Type)) continue;
                    
                    autoConnectInPort.Connect(outPort);
                    graphView.Connect(autoConnectInPort, outPort);
                    break;
                }
            }
            
            // If the port is an out port, find the first compatible in port on the newly created node and
            // connect the port to it.
            if (node is IConsumerNode consumer && port is OutPort autoConnectOutPort)
            {
                foreach (var inPort in consumer.InPorts)
                {
                    if (!inPort.Type.IsAssignableFrom(port.Type)) continue;
                    
                    inPort.Connect(autoConnectOutPort);
                    graphView.Connect(inPort, autoConnectOutPort, referenceNode);
                    break;
                }
            }
        }

        public void OnDrop(GraphView view, Edge edge)
        {
            // Do nothing.
        }
    }
}
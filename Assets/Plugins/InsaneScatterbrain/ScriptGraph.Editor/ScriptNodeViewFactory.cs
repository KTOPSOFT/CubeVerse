using System;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Factory for script node views.
    /// </summary>
    public class ScriptNodeViewFactory
    {
        private readonly ScriptNodeViewTypesRegistry nodeViewTypesRegistry;
        
        /// <summary>
        /// Create the factory.
        /// </summary>
        /// <param name="nodeViewTypesRegistry">The node types registry.</param>
        public ScriptNodeViewFactory(ScriptNodeViewTypesRegistry nodeViewTypesRegistry)
        {
            this.nodeViewTypesRegistry = nodeViewTypesRegistry;
        }
        
        private ScriptNodeView CreateNodeView(IScriptNode node, ScriptGraphView graphView)
        {
            var nodeViewType = typeof(ScriptNodeView);
            if (node is ConstantNode constantNode)
            {
                nodeViewType = nodeViewTypesRegistry.NodeViewTypeByConstantType[constantNode.ConstType];
            }
            else if (nodeViewTypesRegistry.NodeViewTypeByNodeType.ContainsKey(node.GetType())) 
            {
                nodeViewType = nodeViewTypesRegistry.NodeViewTypeByNodeType[node.GetType()];
            }
            
            return (ScriptNodeView) Activator.CreateInstance(nodeViewType, node, graphView);
        }
        
        /// <summary>
        /// Creates a new node view for the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="graphView">The graph view.</param>
        /// <returns>The new node view.</returns>
        public ScriptNodeView CreateNodeViewForNode(IScriptNode node, ScriptGraphView graphView)
        {
            var nodeView = CreateNodeView(node, graphView);
            
            nodeView.Initialize();
            nodeView.SetPosition(node.Position);

            return nodeView;
        }

        public ScriptNodeView CreateNodeViewForReferenceNode(ReferenceNode referenceNode, ScriptGraphView scriptGraphView)
        {
            var referenceNodeView = CreateNodeView(referenceNode.ProviderNode, scriptGraphView);
            referenceNodeView.userData = referenceNode;
            
            referenceNodeView.Initialize();

            referenceNodeView.SetPosition(referenceNode.Position);
            referenceNodeView.title += " *";    // Add a star at the end of the title so we can see that this is a reference node and not the original.

            return referenceNodeView;
        }
    }
}
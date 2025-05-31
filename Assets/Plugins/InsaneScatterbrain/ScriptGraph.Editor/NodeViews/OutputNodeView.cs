using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This is the node view for the output nodes.
    /// </summary>
    [ScriptNodeView(typeof(OutputNode))]
    public class OutputNodeView : ScriptNodeView
    {
        private readonly OutputNode outputNode;
        private readonly string id;
        
        private Label nameLabel;
        
        public OutputNodeView(OutputNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            outputNode = node;
            id = node.OutputParameterId;

            var graph = graphView.Graph;
            
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                graph.OutputParameters.OnRenamed -= ParameterRenamed;
                graph.OutputParameters.OnRenamed += ParameterRenamed;
            });

            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                graph.OutputParameters.OnRenamed -= ParameterRenamed;
            });
        }
        
        private void ParameterRenamed(string renamedId, string oldParamName, string newParamName)
        {
            if (id != renamedId) return;

            nameLabel.text = newParamName;
        }

        /// <inheritdoc cref="ScriptNodeView.Initialize"/>
        public override void Initialize()
        {
            base.Initialize();
            
            var outputParamName = Graph.OutputParameters.GetName(outputNode.OutputParameterId);

            // Add a label that shows which output parameter this node represents.
            nameLabel = new Label(outputParamName);
            outputContainer.Add(nameLabel);
            
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
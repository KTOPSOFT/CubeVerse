using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This is the node view for the input nodes.
    /// </summary>
    [ScriptNodeView(typeof(InputNode))]
    public class InputNodeView : ScriptNodeView
    {
        private readonly InputNode inputNode;
        private readonly string id;
        
        private Label nameLabel;
        
        public InputNodeView(InputNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            inputNode = node;
            id = node.InputParameterId;

            var graph = graphView.Graph;
            
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                graph.InputParameters.OnRenamed -= ParameterRenamed;
                graph.InputParameters.OnRenamed += ParameterRenamed;
            });

            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                graph.InputParameters.OnRenamed -= ParameterRenamed;
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

            var inputParamName = Graph.InputParameters.GetName(inputNode.InputParameterId);
            
            // Add a label that shows which input parameter this node represents.
            nameLabel = new Label(inputParamName);
            inputContainer.Add(nameLabel);
            
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
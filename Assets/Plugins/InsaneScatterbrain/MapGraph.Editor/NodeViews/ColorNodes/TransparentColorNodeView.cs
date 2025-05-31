using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(TransparentColorNode))]
    public class TransparentColorNodeView : ScriptNodeView
    {
        public TransparentColorNodeView(IScriptNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            AddToClassList("no-title");
            AddToClassList("title-in-input");
        }
        
        public override void Initialize()
        {
            base.Initialize();

            var nameLabel = new Label("Transparent Color");
            inputContainer.Add(nameLabel);
            
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
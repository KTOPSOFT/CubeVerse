using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(bool))]
    public class BoolConstantNodeView : ConstantNodeView
    {
        public BoolConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var boolValue = (bool) node.Value;
            AddDefaultField<bool, Toggle>(boolValue);
        }
    }
}
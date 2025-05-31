using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(string))]
    public class StringConstantNodeView : ConstantNodeView
    {
        public StringConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var stringValue = (string) node.Value;
            AddDefaultField<string, TextField>(stringValue);
        }
    }
}
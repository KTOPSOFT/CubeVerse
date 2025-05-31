using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Object))]
    public class ObjectConstantNodeView : ConstantNodeView
    {
        public ObjectConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var objValue = node.Value as Object;
            AddDefaultObjectField(objValue);
        }
    }
}
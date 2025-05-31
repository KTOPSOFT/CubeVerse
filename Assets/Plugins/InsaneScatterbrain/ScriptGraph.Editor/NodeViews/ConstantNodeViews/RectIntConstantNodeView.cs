#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(RectInt))]
    public class RectIntConstantNodeView : ConstantNodeView
    {
        public RectIntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var rectIntValue = (RectInt) node.Value;
            AddDefaultField<RectInt, RectIntField>(rectIntValue);
        }
    }
}
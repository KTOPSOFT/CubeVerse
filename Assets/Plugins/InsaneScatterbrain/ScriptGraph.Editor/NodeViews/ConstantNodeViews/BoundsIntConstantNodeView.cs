#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(BoundsInt))]
    public class BoundsIntConstantNodeView : ConstantNodeView
    {
        public BoundsIntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var boundsIntValue = (BoundsInt) node.Value;
            AddDefaultField<BoundsInt, BoundsIntField>(boundsIntValue);
        }
    }
}
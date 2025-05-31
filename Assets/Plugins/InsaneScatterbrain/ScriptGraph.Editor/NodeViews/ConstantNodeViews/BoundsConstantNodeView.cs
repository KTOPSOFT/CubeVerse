#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Bounds))]
    public class BoundsConstantNodeView : ConstantNodeView
    {
        public BoundsConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var boundsValue = (Bounds) node.Value;
            AddDefaultField<Bounds, BoundsField>(boundsValue);
        }
    }
}
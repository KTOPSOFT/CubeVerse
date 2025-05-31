#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(float))]
    public class FloatConstantNodeView : ConstantNodeView
    {
        public FloatConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var floatValue = (float) node.Value;
            AddDefaultField<float, FloatField>(floatValue);
        }
    }
}
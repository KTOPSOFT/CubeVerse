#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Vector2))]
    public class Vector2ConstantNodeView : ConstantNodeView
    {
        public Vector2ConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var vector2Value = (Vector2) node.Value;
            AddDefaultField<Vector2, Vector2Field>(vector2Value);
        }
    }
}
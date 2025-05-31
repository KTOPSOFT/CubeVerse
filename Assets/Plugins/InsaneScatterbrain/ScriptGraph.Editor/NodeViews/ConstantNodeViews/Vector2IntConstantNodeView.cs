#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Vector2Int))]
    public class Vector2IntConstantNodeView : ConstantNodeView
    {
        public Vector2IntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var vector2IntValue = (Vector2Int) node.Value;
            AddDefaultField<Vector2Int, Vector2IntField>(vector2IntValue);
        }
    }
}
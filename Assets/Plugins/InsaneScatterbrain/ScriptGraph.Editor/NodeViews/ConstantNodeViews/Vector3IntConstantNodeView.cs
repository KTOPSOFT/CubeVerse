#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Vector3Int))]
    public class Vector3IntConstantNodeView : ConstantNodeView
    {
        public Vector3IntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var vector3IntValue = (Vector3Int) node.Value;
            AddDefaultField<Vector3Int, Vector3IntField>(vector3IntValue);
        }
    }
}
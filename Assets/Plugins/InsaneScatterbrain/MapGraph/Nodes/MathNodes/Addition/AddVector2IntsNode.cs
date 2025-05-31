using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Vector2Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2Int)), ExplicitOutPortTypes(typeof(Vector2Int))]
    public class AddVector2IntsNode : BasicMathOperationNode<Vector2Int>
    {
        protected override Vector2Int Calculate(Vector2Int a, Vector2Int b) => a + b;
    }
}
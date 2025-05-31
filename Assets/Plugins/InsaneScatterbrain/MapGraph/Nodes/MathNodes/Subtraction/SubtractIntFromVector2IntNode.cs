using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Subtract Int From Vector2Int", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2Int), typeof(int)), ExplicitOutPortTypes(typeof(Vector2Int))]
    public class SubtractIntFromVector2IntNode : BasicMathOperationNode<Vector2Int, int, Vector2Int>
    {
        protected override Vector2Int Calculate(Vector2Int a, int b) => new Vector2Int(a.x - b, a.y - b);
    }
}
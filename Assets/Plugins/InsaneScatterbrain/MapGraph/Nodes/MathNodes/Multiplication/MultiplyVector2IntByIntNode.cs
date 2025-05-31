using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Multiply Vector2Int By Int", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2Int), typeof(int)), ExplicitOutPortTypes(typeof(Vector2Int))]
    public class MultiplyVector2IntByIntNode : BasicMathOperationNode<Vector2Int, int, Vector2Int>
    {
        protected override Vector2Int Calculate(Vector2Int a, int b) => a * b;
    }
}
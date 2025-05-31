using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide (Vector2Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2Int)), ExplicitOutPortTypes(typeof(Vector2Int))]
    public class DivideVector2IntsNode : BasicMathOperationNode<Vector2Int>
    {
        protected override Vector2Int Calculate(Vector2Int a, Vector2Int b) => new Vector2Int(a.x / b.x, a.y / b.y);
    }
}
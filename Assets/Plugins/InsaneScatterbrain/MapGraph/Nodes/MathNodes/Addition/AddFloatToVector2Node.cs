using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add Float To Vector2", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2), typeof(float)), ExplicitOutPortTypes(typeof(Vector2))]
    public class AddFloatToVector2Node : BasicMathOperationNode<Vector2, float, Vector2>
    {
        protected override Vector2 Calculate(Vector2 a, float b) => new Vector2(a.x + b, a.y + b);
    }
}
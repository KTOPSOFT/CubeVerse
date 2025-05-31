using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Multiply Vector2 By Float", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2), typeof(float)), ExplicitOutPortTypes(typeof(Vector2))]
    public class MultiplyVector2ByFloatNode : BasicMathOperationNode<Vector2, float, Vector2>
    {
        protected override Vector2 Calculate(Vector2 a, float b) => a * b;
    }
}
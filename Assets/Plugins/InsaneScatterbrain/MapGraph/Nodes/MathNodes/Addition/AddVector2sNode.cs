using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Vector2)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2)), ExplicitOutPortTypes(typeof(Vector2))]
    public class AddVector2sNode : BasicMathOperationNode<Vector2>
    {
        protected override Vector2 Calculate(Vector2 a, Vector2 b) => a + b;
    }
}
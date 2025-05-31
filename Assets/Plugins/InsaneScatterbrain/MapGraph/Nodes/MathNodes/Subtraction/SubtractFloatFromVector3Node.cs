using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Subtract Float From Vector3", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3), typeof(float)), ExplicitOutPortTypes(typeof(Vector3))]
    public class SubtractFloatFromVector3Node : BasicMathOperationNode<Vector3, float, Vector3>
    {
        protected override Vector3 Calculate(Vector3 a, float b) => new Vector3(a.x - b, a.y - b, a.z - b);
    }
}
using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide Vector3 By Float", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3), typeof(float)), ExplicitOutPortTypes(typeof(Vector3))]
    public class DivideVector3ByFloatNode : BasicMathOperationNode<Vector3, float, Vector3>
    {
        protected override Vector3 Calculate(Vector3 a, float b) => a / b;
    }
}
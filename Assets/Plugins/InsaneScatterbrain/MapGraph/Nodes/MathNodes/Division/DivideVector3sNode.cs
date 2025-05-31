using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide (Vector3)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3)), ExplicitOutPortTypes(typeof(Vector3))]
    public class DivideVector3sNode : BasicMathOperationNode<Vector3>
    {
        protected override Vector3 Calculate(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}
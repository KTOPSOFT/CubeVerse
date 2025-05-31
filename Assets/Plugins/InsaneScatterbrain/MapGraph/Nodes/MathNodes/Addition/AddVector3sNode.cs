using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Vector3)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3)), ExplicitOutPortTypes(typeof(Vector3))]
    public class AddVector3sNode : BasicMathOperationNode<Vector3>
    {
        protected override Vector3 Calculate(Vector3 a, Vector3 b) => a + b;
    }
}
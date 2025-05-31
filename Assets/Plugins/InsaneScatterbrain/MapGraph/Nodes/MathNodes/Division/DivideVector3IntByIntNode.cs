using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide Vector3Int By Int", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3Int), typeof(int)), ExplicitOutPortTypes(typeof(Vector3Int))]
    public class DivideVector3IntByIntNode : BasicMathOperationNode<Vector3Int, int, Vector3Int>
    {
        protected override Vector3Int Calculate(Vector3Int a, int b) => a / b;
    }
}
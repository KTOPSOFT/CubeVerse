using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide (Vector3Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3Int)), ExplicitOutPortTypes(typeof(Vector3Int))]
    public class DivideVector3IntsNode : BasicMathOperationNode<Vector3Int>
    {
        protected override Vector3Int Calculate(Vector3Int a, Vector3Int b) => new Vector3Int(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}
using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Vector3Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3Int)), ExplicitOutPortTypes(typeof(Vector3Int))]
    public class AddVector3IntsNode : BasicMathOperationNode<Vector3Int>
    {
        protected override Vector3Int Calculate(Vector3Int a, Vector3Int b) => a + b;
    }
}
using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add Int To Vector3Int", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3Int), typeof(int)), ExplicitOutPortTypes(typeof(Vector3Int))]
    public class AddIntToVector3IntNode : BasicMathOperationNode<Vector3Int, int, Vector3Int>
    {
        protected override Vector3Int Calculate(Vector3Int a, int b) => new Vector3Int(a.x + b, a.y + b, a.z + b);
    }
}
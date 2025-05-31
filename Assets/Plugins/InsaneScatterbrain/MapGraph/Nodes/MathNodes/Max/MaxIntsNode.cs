using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Max (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class MaxIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => Mathf.Max(a, b);
    }
}
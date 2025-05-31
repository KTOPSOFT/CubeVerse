using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Min (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class MinIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => Mathf.Min(a, b);
    }
}
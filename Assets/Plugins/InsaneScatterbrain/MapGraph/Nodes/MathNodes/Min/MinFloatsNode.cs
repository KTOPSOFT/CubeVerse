using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Min (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class MinFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => Mathf.Min(a, b);
    }
}
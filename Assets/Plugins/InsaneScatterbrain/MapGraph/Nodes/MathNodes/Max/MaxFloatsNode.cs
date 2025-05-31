using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Max (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class MaxFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => Mathf.Max(a, b);
    }
}
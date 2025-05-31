using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Subtract (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class SubtractFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => a - b;
    }
}
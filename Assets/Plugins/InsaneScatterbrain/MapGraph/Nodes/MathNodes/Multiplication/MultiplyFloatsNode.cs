using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Multiply (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class MultiplyFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => a * b;
    }
}
using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Multiply (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class MultiplyIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => a * b;
    }
}
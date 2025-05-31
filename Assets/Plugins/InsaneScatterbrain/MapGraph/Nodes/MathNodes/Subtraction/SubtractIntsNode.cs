using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Subtract (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class SubtractIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => a - b;
    }
}
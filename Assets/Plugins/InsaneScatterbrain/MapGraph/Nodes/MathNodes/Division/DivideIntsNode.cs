using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class DivideIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => a / b;
    }
}
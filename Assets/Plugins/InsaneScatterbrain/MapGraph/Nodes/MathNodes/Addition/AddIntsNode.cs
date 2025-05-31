using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Int)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class AddIntsNode : BasicMathOperationNode<int>
    {
        protected override int Calculate(int a, int b) => a + b;
    }
}
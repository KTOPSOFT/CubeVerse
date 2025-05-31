using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Divide (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class DivideFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => a / b;
    }
}
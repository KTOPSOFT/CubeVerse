using System;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Add (Float)", "Math"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class AddFloatsNode : BasicMathOperationNode<float>
    {
        protected override float Calculate(float a, float b) => a + b;
    }
}
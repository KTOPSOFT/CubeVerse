using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ExplicitOutPortTypesAttribute : ExplicitPortTypesAttribute
    {
        public ExplicitOutPortTypesAttribute(params Type[] types) : base(types) { }
    }
}
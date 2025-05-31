using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ExplicitInPortTypesAttribute : ExplicitPortTypesAttribute
    {
        public ExplicitInPortTypesAttribute(params Type[] types) : base(types) { }
    }
}
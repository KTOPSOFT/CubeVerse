using System;

namespace InsaneScatterbrain.ScriptGraph
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ExplicitPortTypesAttribute : Attribute
    {
        public Type[] Types { get; }
        
        protected ExplicitPortTypesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}
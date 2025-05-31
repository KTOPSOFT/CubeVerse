using System;

namespace InsaneScatterbrain.ScriptGraph
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class PortAttribute : Attribute
    {
        public string Name { get; }
        public Type Type { get; }
        
        protected PortAttribute(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
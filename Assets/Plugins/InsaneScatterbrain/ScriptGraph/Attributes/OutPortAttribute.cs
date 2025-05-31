using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class OutPortAttribute : PortAttribute
    {
        public OutPortAttribute(string name, Type type) : base(name, type) { }
    }
}
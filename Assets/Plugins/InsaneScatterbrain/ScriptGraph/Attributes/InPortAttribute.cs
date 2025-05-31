using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class InPortAttribute : PortAttribute
    {
        public bool IsConnectionRequired { get; }

        public InPortAttribute(string name, Type type, bool isConnectionRequired = false) : base(name, type)
        {
            IsConnectionRequired = isConnectionRequired;
        }
    }
}
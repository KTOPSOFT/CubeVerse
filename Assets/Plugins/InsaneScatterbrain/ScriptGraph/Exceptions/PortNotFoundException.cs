using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class PortNotFoundException : Exception
    {
        public PortNotFoundException(string portName) : 
            base($"Can't find port with name \"{portName}\".")
        {
        }
    }
}
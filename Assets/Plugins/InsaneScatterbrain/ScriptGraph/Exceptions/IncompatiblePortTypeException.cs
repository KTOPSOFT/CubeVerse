using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class IncompatiblePortTypeException : Exception
    {
        public IncompatiblePortTypeException(string portName, Type expectedType, Type actualType) : 
            base("Expected port type not compatible with actual port type. " +
                 $"Port: \"{portName}\". Expected type: {expectedType}. Actual type: {actualType}.")
        {
        }
    }
}
using System;

namespace InsaneScatterbrain.ScriptGraph
{
    public class RequiredPortNotConnectedException : Exception
    {
        public ScriptGraphPort Port { get; }
        
        public RequiredPortNotConnectedException(ScriptGraphPort port) : 
            base($"Port \"{port.Name}\" needs to be connected, but it's not.")
        {
            Port = port;
        }
    }
}
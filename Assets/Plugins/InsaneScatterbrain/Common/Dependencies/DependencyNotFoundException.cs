using System;

namespace InsaneScatterbrain.Dependencies
{
    public class DependencyNotFoundException : Exception
    {
        public DependencyNotFoundException(Type type) : base($"No dependency of type {type} was registered.")
        {
        }
    }
}
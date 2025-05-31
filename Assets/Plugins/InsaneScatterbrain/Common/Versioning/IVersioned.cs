using System;

namespace InsaneScatterbrain.Versioning
{
    /// <summary>
    /// Interface for versioned objects.
    /// </summary>
    public interface IVersioned
    {
        /// <summary>
        /// Gets sets the version.
        /// </summary>
        Version Version { get; set; }
    }
}
using System;
using UnityEngine;

namespace InsaneScatterbrain.Versioning
{
    /// <summary>
    /// Serializable wrapper for the Version class.
    /// </summary>
    [Serializable]
    public class SerializedVersion : IVersioned, ISerializationCallbackReceiver
    {
        [SerializeField] private string version = null;
        
        /// <summary>
        /// Gets/sets the version.
        /// </summary>
        public Version Version { get; set; } = new Version();

        #region Serialization
        
        public void OnBeforeSerialize()
        {
            if (Version == null) return;

            version = Version.ToString();
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(version)) return;

            Version = new Version(version);
        }
        
        #endregion
    }
}
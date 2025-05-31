using System;
using UnityEngine;

namespace InsaneScatterbrain.Versioning
{
    /// <summary>
    /// Base class for versioned mono behaviours.
    /// </summary>
    public abstract class VersionedMonoBehaviour : MonoBehaviour, IVersioned
    {
        [SerializeField] private SerializedVersion serializedObjectVersion = new SerializedVersion();
        
        /// <summary>
        /// The default version used to assign to newly created mono behaviours.
        /// </summary>
        protected abstract Version DefaultVersion { get; }

        /// <summary>
        /// Gets/sets the version.
        /// </summary>
        public Version Version
        {
            get => serializedObjectVersion.Version;
            set => serializedObjectVersion.Version = value;
        }

        protected virtual void Reset()
        {
            serializedObjectVersion = new SerializedVersion
            {
                Version = DefaultVersion
            };
        }
    }
}
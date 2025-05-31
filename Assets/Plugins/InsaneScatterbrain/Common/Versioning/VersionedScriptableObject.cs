using System;
using UnityEngine;

namespace InsaneScatterbrain.Versioning
{
    /// <summary>
    /// Base class for versioned scriptable objects.
    /// </summary>
    public abstract class VersionedScriptableObject : ScriptableObject, IVersioned
    {
        [SerializeField, HideInInspector] private SerializedVersion serializedObjectVersion = new SerializedVersion();

        /// <summary>
        /// Gets/sets the version.
        /// </summary>
        public Version Version
        {
            get => serializedObjectVersion.Version;
            set => serializedObjectVersion.Version = value;
        }

        /// <summary>
        /// Creates an instance of given type and initializes it with the given version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <typeparam name="T">The type of versioned scriptable object.</typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>(Version version) where T : VersionedScriptableObject
        {
            var instance = CreateInstance<T>();
            instance.Version = version;
            return instance;
        }
    }
}

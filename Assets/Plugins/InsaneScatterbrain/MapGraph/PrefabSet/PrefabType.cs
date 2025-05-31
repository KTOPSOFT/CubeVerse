using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A collection of prefab entries.
    /// </summary>
    [Serializable]
    public class PrefabType : ObjectType<PrefabTypeEntry, GameObject>
    {
        [SerializeField] private List<PrefabTypeEntry> prefabs = new List<PrefabTypeEntry>();

        protected override List<PrefabTypeEntry> OpenEntries => prefabs;
        
        protected override PrefabTypeEntry NewEntry() => new PrefabTypeEntry();

        /// <summary>
        /// Creates a new prefab type with the given name.
        /// </summary>
        /// <param name="name">The prefab type name.</param>
        public PrefabType(string name) : base(name) { }
    }
}
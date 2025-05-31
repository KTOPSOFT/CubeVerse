using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A collection of tilemap entries.
    /// </summary>
    [Serializable]
    public class TilemapType : ObjectType<TilemapTypeEntry, Tilemap>
    {
        [SerializeField] private List<TilemapTypeEntry> tilemapEntries = new List<TilemapTypeEntry>();

        protected override List<TilemapTypeEntry> OpenEntries => tilemapEntries;
        
        protected override TilemapTypeEntry NewEntry() => new TilemapTypeEntry();

        /// <summary>
        /// Prepare all tilemap entries for graph processing.
        /// </summary>
        public void Prepare()
        {
            foreach (var tilemapEntry in Entries)
            {
                tilemapEntry.Prepare();
            }
        }
        
        /// <summary>
        /// Creates a new tilemap type with the given name.
        /// </summary>
        /// <param name="name">The tilemap type name.</param>
        public TilemapType(string name) : base(name) { }
    }
}
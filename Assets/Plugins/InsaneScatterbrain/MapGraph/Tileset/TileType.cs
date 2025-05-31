using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A collection of tile entries.
    /// </summary>
    [Serializable]
    public class TileType : ObjectType<TileTypeEntry, TileBase>
    {
        [SerializeField] private List<TileTypeEntry> tiles = new List<TileTypeEntry>();

        protected override List<TileTypeEntry> OpenEntries => tiles;
        protected override TileTypeEntry NewEntry() => new TileTypeEntry();

        public TileType(string name) : base(name) { }
    }
}
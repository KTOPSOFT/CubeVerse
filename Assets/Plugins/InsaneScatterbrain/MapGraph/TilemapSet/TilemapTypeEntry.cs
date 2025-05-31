using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Entry for a tilemap type.
    /// </summary>
    [Serializable]
    public class TilemapTypeEntry : ObjectTypeEntry<Tilemap>
    {
        [SerializeField] public Tilemap tilemap;

        public override Tilemap Value
        {
            get => tilemap;
            set => tilemap = value;
        }

        [NonSerialized] private TilemapData tilemapData;
        
        /// <summary>
        /// Gets tilemap data.
        /// </summary>
        public TilemapData TilemapData => tilemapData;

        /// <summary>
        /// Prepare all tilemap entries for graph processing.
        /// </summary>
        public void Prepare()
        {
            tilemapData = TilemapData.CreateFromTilemap(tilemap);
        }
    }
}
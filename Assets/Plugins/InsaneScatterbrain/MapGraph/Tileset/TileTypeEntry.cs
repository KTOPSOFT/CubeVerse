using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Entry for a tile type.
    /// </summary>
    [Serializable]
    public class TileTypeEntry : ObjectTypeEntry<TileBase>
    {
        [SerializeField] private TileBase tile;
        
        public override TileBase Value
        {
            get => tile;
            set => tile = value;
        }
    }
}
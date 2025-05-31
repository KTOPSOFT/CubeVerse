using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains tilemap types by name. The names can be used to associate them with a named color.
    /// </summary>
    public class TilemapSet : ObjectTypeSetScriptableObject<TilemapType, TilemapTypeEntry, Tilemap>
    {
        /// <summary>
        /// Prepare all tilemap types and their entries for graph processing.
        /// </summary>
        public override void Prepare()
        {
            base.Prepare();
            foreach (var id in OrderedIds)
            {
                var tilemapType = Get(id);
                tilemapType.Prepare();
            }
        }

        public TilemapData GetRandomTilemapData(string typeName, Random random)
        {
            return GetRandomEntry(typeName, random).TilemapData;
        }

        #region DataSet

        [Serializable] private class OpenTilemapSet : OpenObjectTypeSet<TilemapType, TilemapTypeEntry, Tilemap> { }
        
        [SerializeField] private OpenTilemapSet openTilemapSet = new OpenTilemapSet();

        protected override OpenObjectTypeSet<TilemapType, TilemapTypeEntry, Tilemap> OpenSet => openTilemapSet;

        #endregion
        
        [Obsolete("Tilemap type data is now stored in a data set. Will be removed in version 2.0.")]
        [SerializeField] private List<TilemapType> tilemapTypes = new List<TilemapType>();
    }
}
using System;
using System.Collections;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a texture from a tilemap. Tile names are used to lookup the associated color from the graph's named
    /// color set.
    /// </summary>
    [ScriptNode("Tilemap To Texture", "Tilemaps"), Serializable]
    public class TilemapToTextureNode : ProcessorNode
    {
        [InPort("Tilemap", typeof(Tilemap), true), SerializeReference]
        private InPort tilemapIn = null;
        
        [InPort("Tileset", typeof(Tileset), true), SerializeReference]
        private InPort tilesetIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif
        
        private TextureData textureData;
        private IInstanceProvider instanceProvider;
        private NamedColorSet namedColorSet;
        private Tileset tileset;
        private Tilemap tilemap;
        private BoundsInt bounds;
        private int width;
        private int height;

        private void Prepare()
        {
            instanceProvider = Get<IInstanceProvider>();
            namedColorSet = Get<NamedColorSet>();
            
            tileset = tilesetIn.Get<Tileset>();
            tilemap = tilemapIn.Get<Tilemap>();
            
            bounds = tilemap.cellBounds;

            width = bounds.size.x;
            height = bounds.size.y;

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);
        }
        
        private Color32 GetTileColor(TileBase tile)
        {
            Color32 color;
            if (tile == null)
            {
                color = default;
            }
            else
            {
                var tileTypeName = tileset.GetTypeName(tile);
                color = namedColorSet.GetColorByName(tileTypeName);
            }

            return color;
        }
        
        protected override void OnProcessMainThread()
        {
            Prepare();
            
            var tiles = tilemap.GetTilesBlock(bounds);
            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                textureData[i] = GetTileColor(tile);
            }
            
            textureOut.Set(() => textureData);
        }

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            Prepare();
            
            for (var x = bounds.xMin; x < bounds.xMax; ++x)
            for (var y = bounds.yMin; y < bounds.yMax; ++y)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                
                var i = (y - bounds.yMin) * width + (x - bounds.xMin);
                textureData[i] = GetTileColor(tile);
                
                yield return null;
            }
            
            textureOut.Set(() => textureData);
        }
    }
}
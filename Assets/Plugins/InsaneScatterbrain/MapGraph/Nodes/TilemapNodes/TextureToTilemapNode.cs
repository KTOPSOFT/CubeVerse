using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a tilemap from a texture.
    /// </summary>
    [ScriptNode("Texture To Tilemap Data", "Tilemaps"), Serializable]
    public class TextureToTilemapNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
         
        [InPort("Tileset", typeof(Tileset), true), SerializeReference]
        private InPort tilesetIn = null;
         
        
        [OutPort("Tilemap", typeof(TilemapData)), SerializeReference]
        private OutPort tilemapOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            var namedColorSet = Get<NamedColorSet>();
            
            var textureData = textureIn.Get<TextureData>();
            var tileset = tilesetIn.Get<Tileset>();

            var width = textureData.Width;
            var tilemap = instanceProvider.Get<TilemapData>();
            
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                var color = textureData[i];
                if (color.IsEqualTo(default)) continue;    // Empty space, skip it 
                
                var tileType = namedColorSet.GetName(color); 
                if (tileType == null)
                {
                    Debug.LogError($"Unknown color: {color}");
                }

                var x = i % width;
                var y = i / width;

                var tile = tileset.GetRandomObject(tileType, rng);
                tilemap.SetTile(new Vector2Int(x, y), tile);
            }

            tilemapOut.Set(() => tilemap);
        }
    }
}
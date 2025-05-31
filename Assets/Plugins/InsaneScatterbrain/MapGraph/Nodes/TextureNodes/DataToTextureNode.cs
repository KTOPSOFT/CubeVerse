using System;
using System.Collections;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Converts texture data to texture object.
    /// </summary>
    [ScriptNode("Data To Texture", "Textures"), Serializable]
    public class DataToTextureNode : ProcessorNode
    {
        [InPort("Data", typeof(TextureData), true), SerializeReference]
        private InPort textureDataIn = null;

        [OutPort("Texture", typeof(Texture2D)), SerializeReference]
        private OutPort textureOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcessMainThread"/>
        protected override void OnProcessMainThread()
        {
            var textureData = textureDataIn.Get<TextureData>();
            var texture = textureData.ToTexture2D();
            textureOut.Set(() => texture);
        }

        /// <inheritdoc cref="ProcessorNode.OnProcessMainThreadCoroutine"/>
        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            var textureData = textureDataIn.Get<TextureData>();
            var width = textureData.Width;
            var height = textureData.Height;

            var texture = Texture2DFactory.CreateDefault(width, height, false);
            
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var color = textureData[x + y * width];
                texture.SetPixel(x, y, color);
                yield return null;
            }

            textureOut.Set(() => texture);
        }
    }
}
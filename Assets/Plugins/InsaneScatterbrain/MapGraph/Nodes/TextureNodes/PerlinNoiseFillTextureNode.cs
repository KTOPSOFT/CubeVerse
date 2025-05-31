using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.RandomNumberGeneration;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using Rng = InsaneScatterbrain.Services.Rng;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Fills the texture with a random Perlin noise pattern.
    /// </summary>
    [ScriptNode("Perlin Noise Fill Texture", "Textures"), Serializable]
    public class PerlinNoiseFillTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Filled Color", typeof(Color32)), SerializeReference]
        private InPort filledColorIn = null;
        
        [InPort("Empty Color", typeof(Color32)), SerializeReference]
        private InPort emptyColorIn = null;

        [InPort("Scale", typeof(float)), SerializeReference]
        private InPort scaleIn = null;

        [InPort("Points Per Unit", typeof(Vector2Int)), SerializeReference]
        private InPort ppuIn = null;
        
        [InPort("Threshold", typeof(float)), SerializeReference]
        private InPort thresholdIn = null;

        [InPort("Coordinates", typeof(Vector2Int)), SerializeReference]
        private InPort coordinatesIn = null;

        [InPort("RNG State", typeof(RngState)), SerializeReference]
        private InPort rngStateIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        [OutPort("Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;
        
        [OutPort("Noise Data", typeof(float[])), SerializeReference] 
        private OutPort noiseDataOut = null;
        

        private TextureData textureData;
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
        public TextureData NoiseTextureData { get; private set; }
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var rng = Get<Rng>();
            if (rngStateIn.IsConnected)
            {
                rng.SetState(rngStateIn.Get<RngState>());
            }

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
#if UNITY_EDITOR
            NoiseTextureData = instanceProvider.Get<TextureData>();
            NoiseTextureData.Set(textureData.Width, textureData.Height);
#endif

            var width = textureData.Width;
            var height = textureData.Height;
            
            var mask = maskIn.Get<Mask>();
            var filledColor = filledColorIn.Get<Color32>();
            var emptyColor = emptyColorIn.Get<Color32>();
            var scale = scaleIn.IsConnected ? scaleIn.Get<float>() : 1f;
            var threshold = thresholdIn.IsConnected ? thresholdIn.Get<float>() / 100f : .5f;
            var coordinates = coordinatesIn.Get<Vector2Int>();
            var ppu = ppuIn.Get<Vector2Int>();

            var ppuWidth = ppuIn.IsConnected ? ppu.x : width;
            var ppuHeight = ppuIn.IsConnected ? ppu.y : height;

            var seed = rng.Next() / 10000f;

            var outputMask = instanceProvider.Get<Mask>();
            if (mask != null)
            {
                mask.Clone(outputMask);
            }
            else if (maskOut.IsConnected)
            {

                var unmaskedPoints = instanceProvider.Get<List<int>>();
                unmaskedPoints.EnsureCapacity(width * height);
                for (var i = 0; i < width * height; ++i)
                {
                    unmaskedPoints.Add(i);
                }
                outputMask.Set(unmaskedPoints);
            }
            
            var noiseData = new float[width * height];
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                if (mask != null && mask.IsPointMasked(i)) continue;
                
                var x = i % width + coordinates.x;
                var y = i / width + coordinates.y;
                
                var xNormalized = (float) x / ppuWidth * scale + seed;
                var yNormalized = (float) y / ppuHeight * scale + seed;

                var value = Mathf.PerlinNoise(xNormalized, yNormalized);
                noiseData[i] = value;
                
#if UNITY_EDITOR
                NoiseTextureData[i] = new Color(value, value, value, 1);
#endif

                if (value >= threshold && filledColorIn.IsConnected)
                {
                    textureData[i] = filledColor;
                }
                else if (value < threshold && emptyColorIn.IsConnected)
                {
                    textureData[i] = emptyColor;
                }

                if (textureData[i].a > 0)
                {
                    outputMask?.MaskPoint(i);
                }
            }

            textureOut.Set(() => textureData);
            noiseDataOut.Set(() => noiseData);
            maskOut.Set(() => outputMask);
        }
    }
}
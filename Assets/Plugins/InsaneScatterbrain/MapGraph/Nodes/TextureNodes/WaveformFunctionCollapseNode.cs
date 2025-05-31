using System;
using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;
using Resolution = DeBroglie.Resolution;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Takes an training sample texture and generates a texture of specified size that is locally similar to that sample texture.
    /// </summary>
    [ScriptNode("Wave Function Collapse", "Textures"), Serializable]
    public class WaveformFunctionCollapseNode : ProcessorNode
    {
        [InPort("Sample Texture", typeof(TextureData), true), SerializeReference]
        private InPort sampleIn = null;
        
        [InPort("Output Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort outputSizeIn = null;

        [InPort("N", typeof(int)), SerializeReference]
        private InPort nIn = null;

        [InPort("Tileable Sample?", typeof(bool)), SerializeReference]
        private InPort tileableSampleIn = null;
        
        [InPort("Tileable Output?", typeof(bool)), SerializeReference]
        private InPort tileableOutputIn = null;

        [InPort("Reflectional Symmetry?", typeof(bool)), SerializeReference]
        private InPort reflectionalSymmetryIn = null;

        [InPort("Rotational Symmetry", typeof(RotationalSymmetry)), SerializeReference]
        private InPort rotationalSymmetryIn = null;

        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;

        
#if UNITY_EDITOR
        public TextureData TextureData { get; private set; }
#endif
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            var textureData = sampleIn.Get<TextureData>();
            var outputSize = outputSizeIn.Get<Vector2Int>();
            var n = nIn.IsConnected ? nIn.Get<int>() : 3;
            var tileableSample = tileableSampleIn.Get<bool>();
            var tileableOutput = tileableOutputIn.Get<bool>();
            var reflectionalSymmetry = reflectionalSymmetryIn.Get<bool>();
            var rotationalSymmetry = rotationalSymmetryIn.Get<RotationalSymmetry>();

            var width = textureData.Width;
            var height = textureData.Height;
            
            var colorsPerRow = new Color32[width, height];

            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                var x = i % width;
                var y = i / width;

                colorsPerRow[x, y] = textureData[i];
            }

            var model = new OverlappingModel(TopoArray.Create(colorsPerRow, tileableSample).ToTiles(), n, 
                rotationalSymmetry == RotationalSymmetry.None ? 1 : 360 / (int)rotationalSymmetry, 
                reflectionalSymmetry);

            var topology = new GridTopology(outputSize.x, outputSize.y, tileableOutput);

            // Set propagator to use the node's RNG, so that it stays consistent with its seed.
            var propagatorOptions = new TilePropagatorOptions
            {
                RandomDouble = rng.NextDouble, 
                BacktrackType = BacktrackType.Backjump
            };
            var propagator = new TilePropagator(model, topology, propagatorOptions);
            var status = propagator.Run();

            while (status != Resolution.Decided)
            {
                // The result isn't valid, start over.
                propagator.Clear();
                status = propagator.Run();
            }
        
            var output = propagator.ToValueArray<Color32>();

            var result = instanceProvider.Get<TextureData>();
            result.Set(outputSize.x, outputSize.y);

            for (var x = 0; x < outputSize.x; ++x)
            {
                for (var y = 0; y < outputSize.y; ++y)
                {
                    var index = y * outputSize.x + x;
                    result[index] = output.Get(x, y);
                }
            }
            
#if UNITY_EDITOR
            TextureData = result;
#endif
            
            textureOut.Set(() => result);
        }
    }
}
using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws connecting lines between each pair of points.
    /// </summary>
    [ScriptNode("Draw Connections (Biased Random Walk)", "Drawing"), Serializable]
    public class DrawConnectionsBiasedRandomWalkNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference]
        private InPort drawColorIn = null;

        [InPort("Carve Radius", typeof(int)), SerializeReference]
        private InPort radiusIn = null;

        [InPort("Direction Bias (%)", typeof(float)), SerializeReference]
        private InPort biasIn = null;

        [InPort("Randomness (%)", typeof(float)), SerializeReference]
        private InPort randomnessIn = null;


        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        private RandomPathWalker randomPathWalker = new RandomPathWalker();
        private RandomPathWalker RandomPathWalker => randomPathWalker ?? (randomPathWalker = new RandomPathWalker());
        
        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var rng = Get<Rng>();
            
            var instanceProvider = Get<IInstanceProvider>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var bias = biasIn.IsConnected ? biasIn.Get<float>() / 100 : .5f;
            var randomness = randomnessIn.IsConnected ? randomnessIn.Get<float>() / 100 : 1f;
            var connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var color = drawColorIn.Get<Color32>();
            var radius = radiusIn.IsConnected ? radiusIn.Get<int>() : 1;

            var bounds = new Vector2Int(
                textureData.Width,
                textureData.Height
            );

            var path = instanceProvider.Get<List<Vector2Int>>();
            
            var relativePoints = radius > 1 ? instanceProvider.Get<List<Vector2Int>>() : null;
            
            foreach (var connection in connections)
            {
                RandomPathWalker.CalculatePath(
                    connection.First,
                    connection.Second,
                    bounds,
                    path,
                    rng,
                    bias,
                    randomness
                );
                
                if (radius > 1)
                {
                    textureData.DrawCircles(path, radius, color, relativePoints);
                    continue;
                }

                foreach (var point in path)
                {
                    var currentIndex = bounds.x * point.y + point.x;
                    textureData[currentIndex] = color;
                }
            }

            textureOut.Set(() => textureData);
        } 
    }
}
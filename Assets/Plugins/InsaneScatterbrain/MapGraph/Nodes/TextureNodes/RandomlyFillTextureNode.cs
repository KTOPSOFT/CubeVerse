using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Randomly fills pixels with the given color.
    /// </summary>
    [ScriptNode("Randomly Fill Texture", "Textures"), Serializable]
    public class RandomlyFillTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Fill Percentage", typeof(float)), SerializeReference] 
        private InPort fillPercentageIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference] 
        private InPort fillColorIn = null;

        [InPort("Min. placements", typeof(int)), SerializeReference]
        private InPort minPlacementsIn = null;
        
        [InPort("Max. placements", typeof(int)), SerializeReference] 
        private InPort maxPlacementsIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        [OutPort("Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;
        
        [OutPort("Placements", typeof(Vector2Int[])), SerializeReference] 
        private OutPort placementsOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var rng = Get<Rng>();
            
            var fillColor = fillColorIn.Get<Color32>();

            var fillPercentage = fillPercentageIn.Get<float>();
            var minPlacements = minPlacementsIn.Get<int>();
            var maxPlacements = maxPlacementsIn.Get<int>();
            var mask = maskIn.Get<Mask>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var width = textureData.Width;
            var height = textureData.Height;
            
            Mask outputMask = null;

            var availableTiles = instanceProvider.Get<List<int>>();

            if (!fillPercentageIn.IsConnected && maxPlacementsIn.IsConnected)
            {
                fillPercentage = 100;
            }
            
            if (!fillPercentageIn.IsConnected && !maxPlacementsIn.IsConnected && !minPlacementsIn.IsConnected)
            {
                Debug.LogWarning("Fill Percentage, Min. placements and Max. placements are not connected. This means they will be treated as 0 and the node will never fill anything.");
            }
            else if (maxPlacementsIn.IsConnected && minPlacements > maxPlacements)
            {
                Debug.LogWarning($"Min. placements ({minPlacements}) shouldn't be bigger than max. placements ({maxPlacements}). Max. placements takes priority.");
            }
           
            if (mask != null)
            {
                outputMask = instanceProvider.Get<Mask>();
                mask.Clone(outputMask);
                availableTiles.AddRange(mask.UnmaskedPoints);
            }
            else
            {
                List<int> unmaskedPoints = null;
                
                if (maskOut.IsConnected)
                {
                    unmaskedPoints = instanceProvider.Get<List<int>>();
                }
                
                availableTiles.EnsureCapacity(width * height);
                for (var i = 0; i < width * height; ++i)
                {
                    availableTiles.Add(i);
                    unmaskedPoints?.Add(i);
                }
                
                if (maskOut.IsConnected)
                {
                    outputMask = instanceProvider.Get<Mask>();
                    outputMask.Set(unmaskedPoints);
                }
            }

            availableTiles.Shuffle(rng);
            
            var numOfTiles = Mathf.RoundToInt(availableTiles.Count / 100f * fillPercentage);
            
            if (maxPlacementsIn.IsConnected && numOfTiles > maxPlacements)
            {
                numOfTiles = maxPlacements;
            }
            else if (minPlacementsIn.IsConnected && numOfTiles < minPlacements)
            {
                if (minPlacements > availableTiles.Count)
                {
                    numOfTiles = availableTiles.Count;
                }
                else
                {
                    numOfTiles = minPlacements;
                }
            }

            var placements = availableTiles.GetRange(0, numOfTiles);
            var placementCoords = instanceProvider.Get<List<Vector2Int>>();
            placementCoords.EnsureCapacity(placements.Count);

            foreach (var index in placements)
            {
                textureData[index] = fillColor;
                outputMask?.MaskPoint(index);
                placementCoords.Add(new Vector2Int(index % width, index / width));
            }

            var placementsArray = placementCoords.ToArray(); 
            
            textureOut.Set(() => textureData);
            maskOut.Set(() => outputMask);
            placementsOut.Set(() => placementsArray);
        }
    }
}
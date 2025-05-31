using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws circles around the provided center points.
    /// </summary>
    [ScriptNode("Draw Circles", "Drawing"), Serializable]
    public class DrawCirclesNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Center Points", typeof(Vector2Int[]), true), SerializeReference]
        private InPort centersIn = null;
        
        [InPort("Radius", typeof(int), true), SerializeReference] 
        private InPort radiusIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference] 
        private InPort drawColorIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
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
            
            var fillColor = drawColorIn.Get<Color32>();
            var centers = centersIn.Get<Vector2Int[]>();
            var radius = radiusIn.Get<int>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            var relativePoints = instanceProvider.Get<List<Vector2Int>>();
            textureData.DrawCircles(centers, radius, fillColor, relativePoints);

            textureOut.Set(() => textureData);
        }
    }
}
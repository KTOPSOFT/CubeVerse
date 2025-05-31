using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Applies the color to the given pixel positions.
    /// </summary>
    [ScriptNode("Draw Points", "Drawing"), Serializable]
    public class DrawPointsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference] 
        private InPort drawColorIn = null;
        
        [InPort("Points", typeof(Vector2Int[]), true), SerializeReference] 
        private InPort pointsIn = null;

        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;

        [OutPort("Mask", typeof(Mask)), SerializeReference]
        private OutPort maskOut = null;
        

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
            var points = pointsIn.Get<Vector2Int[]>();
            var drawColor = drawColorIn.Get<Color32>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var width = textureData.Width;
            var height = textureData.Height;

            var mask = maskOut.IsConnected ? instanceProvider.Get<Mask>() : null;
            mask?.Set(width * height);

            foreach (var point in points)
            {
                var i = width * point.y + point.x;
                textureData[i] = drawColor;
                
                mask?.MaskPoint(i);
            }

            textureOut.Set(() => textureData);

            if (mask != null)
            {
                maskOut.Set(() => mask);
            }
        }
    }
}
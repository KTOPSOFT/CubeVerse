using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws rectangles on the texture.
    /// </summary>
    [ScriptNode("Draw Rectangles", "Drawing"), Serializable]
    public class DrawRectsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Rectangles", typeof(RectInt[]), true), SerializeReference] 
        private InPort rectanglesIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference] 
        private InPort drawColorIn = null;
        
        /// <summary>
        /// If true, the rectangles are not filled, but only the outline is drawn.
        /// </summary>
        [InPort("Outline Only?", typeof(bool)), SerializeReference] 
        private InPort outlineOnlyIn = null;

        
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
            var rects = rectanglesIn.Get<RectInt[]>();
            var drawColor = drawColorIn.Get<Color32>();
            var outlineOnly = outlineOnlyIn.Get<bool>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            var width = textureData.Width;

            foreach (var rect in rects)
            {
                if (outlineOnly)
                {
                    var p0 = new Vector2Int(rect.xMin, rect.yMin);
                    var p1 = new Vector2Int(rect.xMax - 1, rect.yMin);
                    var p2 = new Vector2Int(rect.xMax - 1, rect.yMax - 1);
                    var p3 = new Vector2Int(rect.xMin, rect.yMax - 1);
                    
                    textureData.DrawLines(drawColor, p0, p1, p2, p3, p0);
                }
                else
                {
                    for (var x = rect.xMin; x < rect.xMax; ++x)
                    {
                        for (var y = rect.yMin; y < rect.yMax; ++y)
                        {
                            var index = y * width + x;
                            textureData[index] = drawColor;
                        }
                    }
                }
            }

            textureOut.Set(() => textureData);
        }
    }
}
using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws outlines around colored areas (meaning the non-transparent parts).
    /// </summary>
    [ScriptNode("Draw Outline", "Drawing"), Serializable]
    public class DrawOutlineNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Outline Color", typeof(Color32)), SerializeReference] 
        private InPort outlineColorIn = null;

        [InPort("Color To Outline", typeof(Color32)), SerializeReference]
        private InPort colorToOutlineIn = null;
        
        /// <summary>
        /// If set to true, the outline will be drawn on the inside of the shapes instead of added on the outside.
        /// </summary>
        [InPort("Overlap?", typeof(bool)), SerializeReference] 
        private InPort overlapIn = null;
        
        /// <summary>
        /// If set to true, the corner bits of the outline are filled.
        /// </summary>
        [InPort("Corners?", typeof(bool)), SerializeReference] 
        private InPort cornersIn = null;
        
        [InPort("Width", typeof(int)), SerializeReference]
        private InPort widthIn = null;
        
        [InPort("Width Top", typeof(int)), SerializeReference] 
        private InPort widthTopIn = null;
        
        [InPort("Width Bottom", typeof(int)), SerializeReference] 
        private InPort widthBottomIn = null;
        
        [InPort("Width Left", typeof(int)), SerializeReference] 
        private InPort widthLeftIn = null;
        
        [InPort("Width Right", typeof(int)), SerializeReference] 
        private InPort widthRightIn = null;

        [InPort("Mask", typeof(Mask)), SerializeReference]
        private InPort maskIn = null;


        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
        private TextureData textureData;
        
#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif
        
        private Outliner outliner;
        private Outliner Outliner => outliner ?? (outliner = new Outliner());

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var color = outlineColorIn.Get<Color32>();
            var thickness = widthIn.Get<int>();
            var overlap = overlapIn.Get<bool>();
            var corners = cornersIn.Get<bool>();
            var colorToOutline = colorToOutlineIn.Get<Color32>();
            var mask = maskIn.Get<Mask>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            if (!widthIn.IsConnected)
            {
                thickness = 1;
            }

            var thicknessTop = widthTopIn.IsConnected ? widthTopIn.Get<int>() : thickness;
            var thicknessBottom = widthBottomIn.IsConnected ? widthBottomIn.Get<int>() : thickness;
            var thicknessLeft = widthLeftIn.IsConnected ? widthLeftIn.Get<int>() : thickness;
            var thicknessRight = widthRightIn.IsConnected ? widthRightIn.Get<int>() : thickness;
            
            var width = textureData.Width;
            var height = textureData.Height;

            Outliner.Bounds = new Vector2Int(width, height);
            Outliner.Corners = corners;
            Outliner.Thickness = thickness;
            Outliner.ThicknessTop = thicknessTop;
            Outliner.ThicknessBottom = thicknessBottom;
            Outliner.ThicknessLeft = thicknessLeft;
            Outliner.ThicknessRight = thicknessRight;
            Outliner.Mask = mask;

            var pointsToOutline = instanceProvider.Get<List<Vector2Int>>();
            pointsToOutline.EnsureCapacity(textureData.ColorCount);
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                if (!colorToOutlineIn.IsConnected && textureData[i].a == 0) continue;
                if (colorToOutlineIn.IsConnected && !textureData[i].IsEqualTo(colorToOutline)) continue;

                var x = i % width;
                var y = i / width;
                pointsToOutline.Add(new Vector2Int(x, y));
            }

            var outlinePoints = instanceProvider.Get<List<Vector2Int>>();

            if (overlap)
            {
                Outliner.CalculateInline(pointsToOutline, ref outlinePoints);
            }
            else
            {
                Outliner.CalculateOutline(pointsToOutline, ref outlinePoints);
            }

            foreach (var outlinePoint in outlinePoints)
            {
                var index = outlinePoint.y * width + outlinePoint.x;
                textureData[index] = color;
            }
            
            textureOut.Set(() => textureData);
        }
    }
}
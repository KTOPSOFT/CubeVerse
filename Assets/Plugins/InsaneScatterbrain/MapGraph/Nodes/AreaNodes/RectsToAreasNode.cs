using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates an area for each rectangle in the set that contains all the points within that rectangle.
    /// </summary>
    [ScriptNode("Rectangles To Areas", "Convert"), Serializable]
    public class RectsToAreasNode : ProcessorNode
    {
        [InPort("Rectangles", typeof(RectInt[]), true), SerializeReference] 
        private InPort rectsIn = null;
        
        [InPort("Connect Diagonals?", typeof(bool)), SerializeReference]
        private InPort connectDiagonalsIn = null;
        
        
        [OutPort("Areas", typeof(Area[])), SerializeReference] 
        private OutPort areasOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rects = rectsIn.Get<RectInt[]>();
            var connectDiagonals = connectDiagonalsIn.Get<bool>();

            var width = 0;
            var height = 0;
            foreach (var rect in rects)
            {
                if (rect.xMax >= width)
                {
                    width = rect.xMax + 1;
                }
                if (rect.yMax >= height)
                {
                    height = rect.yMax + 1;
                }
            }

            var areaColor = Color.black;

            var textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);
            textureData.DrawRectsFill(rects, areaColor);
            
            var areaExtractor = Get<AreaExtractor>();
            areaExtractor.Reset();
            areaExtractor.ColorToExtract = areaColor;
            areaExtractor.ConnectDiagonals = connectDiagonals;
            
            areaExtractor.ExtractAreas(textureData);

            var areas = areaExtractor.Areas;

            areasOut.Set(() => areas);
        }
    }
}
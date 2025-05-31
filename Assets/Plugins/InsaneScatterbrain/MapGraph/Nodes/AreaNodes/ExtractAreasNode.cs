using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Extracts areas of a certain color from a texture.
    /// </summary>
    [ScriptNode("Extract Areas", "Areas"), Serializable]
    public class ExtractAreasNode : ProcessorNode
    {
        [InPort("Texture",typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Color To Extract",typeof(Color32)), SerializeReference] 
        private InPort areaColorIn = null;
        
        [InPort("Min. Area Size",typeof(int)), SerializeReference] 
        private InPort minAreaSizeIn = null;
        
        [InPort("Max. Area Size",typeof(int)), SerializeReference] 
        private InPort maxAreaSizeIn = null;
        
        [InPort("Connect Diagonals?",typeof(bool)), SerializeReference] 
        private InPort connectDiagonalsIn = null;

        
        [OutPort("Areas",typeof(Area[])), SerializeReference] 
        private OutPort areasOut = null;
        
        [OutPort("Outer Areas",typeof(Area[])), SerializeReference]
        private OutPort outerAreasOut = null;
        
        [OutPort("Inner Areas",typeof(Area[])), SerializeReference]
        private OutPort innerAreasOut = null;
        
        [OutPort("Texture",typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;

        private AreaExtractor areaExtractor;
        private TextureData textureData;
        
#if UNITY_EDITOR
        public IList<Area> Areas => areaExtractor.Areas;
        public TextureData InputTextureData => textureData;
#endif

        private Color32 areaColor;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            areaColor = areaColorIn.Get<Color32>();
            textureData = textureIn.Get<TextureData>();
            var minAreaSize = minAreaSizeIn.Get<int>();
            var maxAreaSize = maxAreaSizeIn.Get<int>();
            var connectDiagonals = connectDiagonalsIn.Get<bool>();

            if (maxAreaSizeIn.IsConnected && minAreaSize > maxAreaSize)
            {
                minAreaSize = -1;
                Debug.LogWarning("Min. Area Size is bigger than Max. Area Size. Max. Area Size will take priority.");
            }
            
            // Setup the area extractor and run it.
            areaExtractor = Get<AreaExtractor>();
            areaExtractor.Reset();
            areaExtractor.ColorToExtract = areaColor;
            areaExtractor.ConnectDiagonals = connectDiagonals;
            areaExtractor.MinAreaSize = minAreaSizeIn.IsConnected ? minAreaSize : -1;
            areaExtractor.MaxAreaSize = maxAreaSizeIn.IsConnected ? maxAreaSize : -1;
            
            areaExtractor.ExtractAreas(textureData);
            
            // If another node uses the texture output, create it now.
            if (textureOut.IsConnected)
            {
                var areasTextureData = instanceProvider.Get<TextureData>();
                areasTextureData.Set(textureData.Width, textureData.Height);
                areasTextureData.DrawAreas(areaExtractor.Areas, areaColor);
                textureOut.Set(() => areasTextureData);
            }

            var areas = areaExtractor.Areas;
            var outerAreas = areaExtractor.OuterAreas;
            var innerAreas = areaExtractor.InnerAreas;

            areasOut.Set(() => areas);
            outerAreasOut.Set(() => outerAreas);
            innerAreasOut.Set(() => innerAreas);
        }
    }
}
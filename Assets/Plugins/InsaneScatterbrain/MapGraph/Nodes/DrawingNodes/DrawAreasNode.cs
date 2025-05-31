using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws all the points of the areas provided.
    /// </summary>
    [ScriptNode("Draw Areas", "Drawing"), Serializable]
    public class DrawAreasNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Areas", typeof(Area[]), true), SerializeReference] 
        private InPort areasIn = null;
        
        [InPort("Color", typeof(Color32)), SerializeReference] 
        private InPort colorIn = null;
        

        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var areas = areasIn.Get<Area[]>();
            var color = colorIn.Get<Color32>();

            textureData.DrawAreas(areas, color);

            textureOut.Set(() => textureData);
        }
    }
}
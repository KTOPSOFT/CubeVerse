using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws connecting lines between each pair of points.
    /// </summary>
    [ScriptNode("Draw Paths", "Drawing"), Serializable]
    public class DrawPathsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Paths", typeof(Vector2Int[][]), true), SerializeReference] 
        private InPort pathsIn = null;
        
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
            
            var paths = pathsIn.Get<Vector2Int[][]>();
            var color = drawColorIn.Get<Color32>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            foreach (var path in paths)
            foreach (var point in path)
            {
                var i = textureData.Width * point.y + point.x;
                textureData[i] = color;
            }

            textureOut.Set(() => textureData);
        }
    }
}
using System;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws connecting lines between each pair of points.
    /// </summary>
    [ScriptNode("Draw Connections (Shortest Path)", "Drawing"), Serializable]
    public class DrawShortestPathNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference]
        private InPort drawColorIn = null;
        
        [InPort("Width", typeof(int)), SerializeReference]
        private InPort widthIn = null;
        
        
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
            var connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var color = drawColorIn.Get<Color32>();
            var lineWidth = widthIn.IsConnected ? widthIn.Get<int>() : 1;
            
            if (lineWidth < 1) lineWidth = 1;

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            foreach (var connection in connections)
            {
                textureData.DrawLine(connection.First, connection.Second, color, lineWidth);
            }

            textureOut.Set(() => textureData);
        }
    }
}
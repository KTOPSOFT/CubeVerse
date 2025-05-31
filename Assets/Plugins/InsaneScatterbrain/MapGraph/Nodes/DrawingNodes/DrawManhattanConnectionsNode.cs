using System;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws connecting lines between each pair of points, using only horizontal and vertical lines.
    /// </summary>
    [ScriptNode("Draw Connections (Manhattan)", "Drawing"), Serializable]
    public class DrawManhattanConnectionsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;
        
        [InPort("Draw Color", typeof(Color32)), SerializeReference] 
        private InPort drawColorIn = null;
        
        [InPort("Connection Width", typeof(int)), SerializeReference] 
        private InPort connectionWidthIn = null;
        
        
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
            var rng = Get<Rng>();
            
            var connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var drawColor = drawColorIn.Get<Color32>();
            var lineWidth = connectionWidthIn.Get<int>();

            if (!connectionWidthIn.IsConnected) lineWidth = 1;

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            foreach (var connection in connections)
            {
                var start = connection.First;
                var end = connection.Second;

                if (start.x == end.x || start.y == end.y)
                {
                    // The points align on either the x or y-axis. So only one line is needed.
                    textureData.DrawLine(start, end, drawColor, lineWidth);
                }
                else
                {
                    // Otherwise we need to lines to connect the two points.
                    Vector2Int cornerPoint;
                    var horizontalPoint = new Vector2Int();
                    var verticalPoint = new Vector2Int();

                    var randomValue = rng.NextDouble();

                    /*
                     * There are two possible points to draw lines from/to to create a connection.
                     *
                     * We pick one at random for more varied results
                     * Example:
                     *
                     * 1-----B
                     * |     |
                     * |     |
                     * A-----2
                     *
                     * If we want to connect A to B using only horizontal or vertical lines. We have to connect
                     * both A and B to either point 1 or point 2.
                     */
                    if (randomValue > .5f)
                    {
                        cornerPoint = new Vector2Int(start.x, end.y);
                        horizontalPoint.x = end.x;
                        horizontalPoint.y = end.y;
                        verticalPoint.x = start.x;
                        verticalPoint.y = start.y;
                    }
                    else
                    {
                        cornerPoint = new Vector2Int(end.x, start.y);
                        horizontalPoint.x = start.x;
                        horizontalPoint.y = start.y;
                        verticalPoint.x = end.x;
                        verticalPoint.y = end.y;
                    }
                    
                    textureData.DrawLine(cornerPoint, horizontalPoint, drawColor, lineWidth);
                    textureData.DrawLine(cornerPoint, verticalPoint, drawColor, lineWidth);
                }
            }

            textureOut.Set(() => textureData);
        }
    }
}
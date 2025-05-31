using System;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Gets the center points from rectangles.
    /// </summary>
    [ScriptNode("Rectangles Center Points", "Rectangles"), Serializable]
    public class RectCenterPointsNode : ProcessorNode
    {
        [InPort("Rectangles", typeof(RectInt[]), true), SerializeReference] 
        private InPort rectanglesIn = null;
        
        
        [OutPort("Center Points", typeof(Vector2Int[])), SerializeReference] 
        private OutPort pointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var rects = rectanglesIn.Get<RectInt[]>();
            
            var points = rects.Select(rect => new Vector2Int(
                Mathf.FloorToInt(rect.center.x), 
                Mathf.FloorToInt(rect.center.y))
            );

            var pointsArray = points.ToArray();
            
            pointsOut.Set(() => pointsArray);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs all the points that make up the given areas.
    /// </summary>
    [ScriptNode("Areas To Points", "Convert"), Serializable]
    public class AreasToPointsNode : ProcessorNode
    {
        [InPort("Areas", typeof(Area[]), true), SerializeReference]
        private InPort areasIn = null;
        
        [OutPort("Points", typeof(Vector2Int[])), SerializeReference] 
        private OutPort pointsOut = null;

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var areas = areasIn.Get<Area[]>();

            var points = instanceProvider.Get<HashSet<Vector2Int>>();
            foreach (var area in areas)
            {
                foreach (var point in area.Points)
                {
                    points.Add(point);
                }
            }

            var pointsArray = points.ToArray();
            pointsOut.Set(() => pointsArray);
        }
    }
}
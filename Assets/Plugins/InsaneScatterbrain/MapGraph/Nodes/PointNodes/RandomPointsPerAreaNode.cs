using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Random Points Per Area", "Points"), Serializable]
    public class RandomPointsPerAreaNode : ProcessorNode
    {
        [InPort("Areas", typeof(Area[]), true), SerializeReference]
        private InPort areasIn = null;
        
        [InPort("Points Per Area", typeof(int), true), SerializeReference]
        private InPort pointsPerAreaIn = null;
        
        
        [OutPort("Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort pointsOut = null;

        private Area[] areas;
        private Vector2Int[] points;
        
    #if UNITY_EDITOR
        public Area[] Areas => areas;
        public Vector2Int[] Points => points;
    #endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            areas = areasIn.Get<Area[]>();
            var pointsList = new List<Vector2Int>();
            var availablePoints = instanceProvider.Get<List<Vector2Int>>();
            
            var pointsPerArea = pointsPerAreaIn.Get<int>();

            // Go through each area and add the required number of points to each of them.
            foreach (var area in areas)
            {
                availablePoints.Clear();
                
                // Add the points from the area to the available points list and shuffle them, so we can pick them randomly.
                availablePoints.AddRange(area.Points);
                availablePoints.Shuffle(rng);
                
                for (var pointIndex = 0; pointIndex < pointsPerArea; pointIndex++)
                {
                    if (availablePoints.Count == 0)
                    {
                        break;
                    }
                    
                    pointsList.Add(availablePoints[pointIndex]);
                }
            }

            points = pointsList.ToArray();
            pointsOut.Set(() => points);
        }
    }
}
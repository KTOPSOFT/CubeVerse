using System;
using System.Collections.Generic;
using DelaunatorSharp;
using InsaneScatterbrain.DelaunatorSharp;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Builder to create connected area graphs of lists of areas.
    /// </summary>
    public class AreaGraphBuilder
    {
        private readonly DelaunatorNoAlloc delaunator = new DelaunatorNoAlloc();
        private readonly List<IEdge> edges = new List<IEdge>();

        private readonly Func<AreaGraphEdge> newEdge;

        [Obsolete("Will probably be removed version 2.0. Please use the other constructor.")]
        public AreaGraphBuilder()
        {
            newEdge = () => new AreaGraphEdge();
        }

        public AreaGraphBuilder(Func<AreaGraphEdge> newEdge)
        {
            this.newEdge = newEdge;
        }

        private bool AreAreasInAStraightLine(IList<Area> areas)
        {
            if (areas == null || areas.Count < 3)
            {
                // If there are 2 or fewer points, they are always collinear
                return true; 
            }
            
            var first = areas[0];
            var second = areas[1];
            
            var firstX = Mathf.RoundToInt((float)first.X);
            var firstY = Mathf.RoundToInt((float)first.Y);
            var secondX = Mathf.RoundToInt((float)second.X);
            var secondY = Mathf.RoundToInt((float)second.Y);
            

            var dx = secondX - firstX;
            var dy = secondY - firstY;
            
            for (var i = 2; i < areas.Count; i++)
            {
                var current = areas[i];
                var currentX = Mathf.RoundToInt((float)current.X);
                var currentY = Mathf.RoundToInt((float)current.Y);

                if (dx * (currentY - firstY) != dy * (currentX - firstX))
                {
                    // Points are not collinear
                    return false; 
                }
                    
            }

            return true;
        }

        /// <summary>
        /// Builds a connected area graph from a list of areas.
        /// </summary>
        /// <param name="areas">The areas.</param>
        /// <param name="graph">The graph.</param>
        /// <returns>The connected area graph.</returns>
        public void BuildGraph(IList<Area> areas, AreaGraph graph)
        {
            graph.Clear();
            if (areas.Count == 0) return;
            
            foreach (var area in areas)
            {
                graph.AddVertex(area);
            }
            
            switch (areas.Count)
            {
                case 1:
                    return;
                case 2:
                {
                    var edge01 = newEdge();
                    edge01.Set(areas[0], areas[1]);
                    
                    graph.AddEdge(edge01);
                    return;
                }
                case 3:
                {
                    var edge01 = newEdge();
                    edge01.Set(areas[0], areas[1]);
                    
                    var edge12 = newEdge();
                    edge12.Set(areas[1], areas[2]);
                    
                    var edge20 = newEdge();
                    edge20.Set(areas[2], areas[0]);
                    
                    graph.AddEdge(edge01);
                    graph.AddEdge(edge12);
                    graph.AddEdge(edge20);
                    return;
                }
            }

            if (AreAreasInAStraightLine(areas))
            {
                // If the areas are in a straight line, we can't triangulate them and we can just connect them in a chain.
                for (var i = 0; i < areas.Count - 1; i++)
                {
                    var edge = newEdge();
                    edge.Set(areas[i], areas[i + 1]);
                    graph.AddEdge(edge);
                }
                
                return;
            }
            
            delaunator.Triangulate(areas);
            delaunator.GetEdges(edges);
            foreach (var edge in edges)
            {
                var areaGraphEdge = newEdge();
                areaGraphEdge.Set(edge.P as Area, edge.Q as Area);
                graph.AddEdge(areaGraphEdge);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using QuikGraph;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A graph of connected areas.
    /// </summary>
    public class AreaGraph : 
        IMutableUndirectedGraph<Area, AreaGraphEdge>,
        ICloneable
    {
        private readonly UndirectedGraph<Area, AreaGraphEdge> graph = new UndirectedGraph<Area, AreaGraphEdge>();
        
        public AreaGraph Clone()
        {
            var clone = graph.Clone();
            var areaGraph = new AreaGraph();
            areaGraph.AddVertexRange(clone.Vertices);
            areaGraph.AddEdgeRange(clone.Edges);
            return areaGraph;
        }

        public bool ContainsVertex(Area vertex) => graph.ContainsVertex(vertex);

        public bool IsDirected => graph.IsDirected;
        public bool AllowParallelEdges => graph.AllowParallelEdges;
        
        public IEnumerable<AreaGraphEdge> AdjacentEdges(Area vertex) => graph.AdjacentEdges(vertex);

        public int AdjacentDegree(Area vertex) => graph.AdjacentDegree(vertex);

        public bool IsAdjacentEdgesEmpty(Area vertex) => graph.IsAdjacentEdgesEmpty(vertex);

        public AreaGraphEdge AdjacentEdge(Area vertex, int index) => graph.AdjacentEdge(vertex, index);

        public bool TryGetEdge(Area source, Area target, out AreaGraphEdge edge) => graph.TryGetEdge(source, target, out edge);

        public bool ContainsEdge(Area source, Area target) => graph.ContainsEdge(source, target);

        public EdgeEqualityComparer<Area> EdgeEqualityComparer => graph.EdgeEqualityComparer;
        
        public bool ContainsEdge(AreaGraphEdge edge) => graph.ContainsEdge(edge);

        public bool IsEdgesEmpty => graph.IsEdgesEmpty;
        public int EdgeCount => graph.EdgeCount;
        public IEnumerable<AreaGraphEdge> Edges => graph.Edges;
        public bool IsVerticesEmpty => graph.IsVerticesEmpty;
        public int VertexCount => graph.VertexCount;
        public IEnumerable<Area> Vertices => graph.Vertices;
        public bool AddVertex(Area vertex) => graph.AddVertex(vertex);

        public int AddVertexRange(IEnumerable<Area> vertices) => graph.AddVertexRange(vertices);

        public bool RemoveVertex(Area vertex) => graph.RemoveVertex(vertex);

        public int RemoveVertexIf(VertexPredicate<Area> predicate) => graph.RemoveVertexIf(predicate);

        public event VertexAction<Area> VertexAdded
        {
            add => graph.VertexAdded += value;
            remove => graph.VertexAdded -= value;
        }
        
        public event VertexAction<Area> VertexRemoved
        {
            add => graph.VertexRemoved += value;
            remove => graph.VertexRemoved -= value;
        }
        
        public void Clear() => graph.Clear();

        public bool AddEdge(AreaGraphEdge edge) => graph.AddEdge(edge);

        public int AddEdgeRange(IEnumerable<AreaGraphEdge> edges) => graph.AddEdgeRange(edges);

        public bool RemoveEdge(AreaGraphEdge edge) => graph.RemoveEdge(edge);

        public int RemoveEdgeIf(EdgePredicate<Area, AreaGraphEdge> predicate) => graph.RemoveEdgeIf(predicate);

        public event EdgeAction<Area, AreaGraphEdge> EdgeAdded
        {
            add => graph.EdgeAdded += value;
            remove => graph.EdgeAdded -= value;
        }
        
        public event EdgeAction<Area, AreaGraphEdge> EdgeRemoved
        {
            add => graph.EdgeRemoved += value;
            remove => graph.EdgeRemoved -= value;
        }
        
        public bool AddVerticesAndEdge(AreaGraphEdge edge) => graph.AddVerticesAndEdge(edge);

        public int AddVerticesAndEdgeRange(IEnumerable<AreaGraphEdge> edges) => graph.AddVerticesAndEdgeRange(edges);

        public int RemoveAdjacentEdgeIf(Area vertex, EdgePredicate<Area, AreaGraphEdge> predicate) => graph.RemoveAdjacentEdgeIf(vertex, predicate);

        public void ClearAdjacentEdges(Area vertex) => graph.ClearAdjacentEdges(vertex);

        object ICloneable.Clone() => Clone();
    }
}
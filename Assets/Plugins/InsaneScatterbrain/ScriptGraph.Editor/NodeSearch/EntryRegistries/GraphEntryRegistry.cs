using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public class GraphEntryRegistry : IEntryRegistry
    {
        private readonly List<SearchTreeEntry> graphEntries = new List<SearchTreeEntry>();
        private readonly List<string> paths = new List<string>();
        
        private readonly ScriptGraphGraph currentGraph;

        private ReadOnlyCollection<SearchTreeEntry> readOnlyGraphEntries;
        private ReadOnlyCollection<string> readOnlyPaths;

        public ReadOnlyCollection<SearchTreeEntry> Entries =>
            readOnlyGraphEntries ?? (readOnlyGraphEntries = graphEntries.AsReadOnly());

        public ReadOnlyCollection<string> Paths => readOnlyPaths ?? (readOnlyPaths = paths.AsReadOnly());

        public GraphEntryRegistry(ScriptGraphGraph graph)
        {
            currentGraph = graph;
        }
        
        public void Build()
        {
            graphEntries.Clear();
            paths.Clear();

            var graphAssets = Assets.Find<ScriptGraphGraph>();
            foreach (var graph in graphAssets)
            {
                if (graph == currentGraph) continue;
                if (!graph.CanBeAddedAsNode) continue;

                var inputs = graph.InputParameters;
                var outputs = graph.OutputParameters;
                
                var inTypes = inputs.OrderedIds.Select(paramId => inputs.GetType(paramId)).ToList();
                var outTypes = outputs.OrderedIds.Select(paramId => outputs.GetType(paramId)).ToList();

                var nodeData = new EntryData(NodeType.Graph, inTypes, outTypes, graph);

                var displayName = $"(G) {graph.name}";
                
                var path = graph.NodePath.Length > 0
                    ? $"{graph.NodePath}/{displayName}"
                    : displayName;

                var level = path.Split('/').Length;
                
                var nodeEntry = EntryFactory.Create(displayName, level, nodeData);
                    
                graphEntries.Add(nodeEntry);

                paths.Add(path);
            }
        }
    }
}
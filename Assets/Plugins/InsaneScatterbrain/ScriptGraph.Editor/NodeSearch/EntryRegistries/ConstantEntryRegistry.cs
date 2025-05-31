using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.Extensions;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    /// <summary>
    /// Builds and contains all the constant node search tree entries.
    /// </summary>
    public class ConstantEntryRegistry : IEntryRegistry
    {
        private readonly List<SearchTreeEntry> constantNodeEntries = new List<SearchTreeEntry>();
        private readonly List<string> paths = new List<string>();
        
        private readonly ScriptNodeViewTypesRegistry nodeViewTypesRegistry;

        private ReadOnlyCollection<SearchTreeEntry> readOnlyEntries;
        private ReadOnlyCollection<string> readOnlyPaths;

        /// <inheritdoc cref="IEntryRegistry.Entries"/>
        public ReadOnlyCollection<SearchTreeEntry> Entries => 
            readOnlyEntries ?? (readOnlyEntries = constantNodeEntries.AsReadOnly());

        public ReadOnlyCollection<string> Paths => 
            readOnlyPaths ?? (readOnlyPaths = paths.AsReadOnly());
        
        public ConstantEntryRegistry(ScriptNodeViewTypesRegistry nodeViewTypesRegistry)
        {
            this.nodeViewTypesRegistry = nodeViewTypesRegistry;
        }
        
        /// <inheritdoc cref="IEntryRegistry.Build"/>
        public void Build()
        {
            constantNodeEntries.Clear();
            paths.Clear();
            
            // Get all the constant node types and order them by name.
            var constantNodeTypes = nodeViewTypesRegistry.NodeViewTypeByConstantType.Keys
                .OrderBy(type => type.GetFriendlyName()).ToList();

            // If there are no constant node types, there's nothing to do.
            if (constantNodeTypes.Count < 1) return;

            const string categoryName = "• Constant";

            foreach (var constantNodeType in constantNodeTypes)
            {
                // Create the search tree entries for each constant node.
                var typeName = constantNodeType.GetFriendlyName();
                var outPortTypes = new List<Type>
                {
                    constantNodeType
                };
                
                var nodeType = new EntryData(NodeType.Constant, outPortTypes: outPortTypes, data: constantNodeType);

                var typeEntry = EntryFactory.Create(typeName, 2, nodeType);
                
                constantNodeEntries.Add(typeEntry);
                paths.Add($"{categoryName}/{typeName}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public abstract class ParametersEntryRegistry : IEntryRegistry
    {
        private readonly List<SearchTreeEntry> entries = new List<SearchTreeEntry>();
        private readonly List<string> paths = new List<string>();
        private readonly ScriptGraphParameters parameters;
        
        private ReadOnlyCollection<SearchTreeEntry> readOnlyEntries;
        private ReadOnlyCollection<string> readOnlyPaths;

        /// <inheritdoc cref="IEntryRegistry.Entries"/>
        public ReadOnlyCollection<SearchTreeEntry> Entries => readOnlyEntries ?? (readOnlyEntries = entries.AsReadOnly());

        public ReadOnlyCollection<string> Paths => readOnlyPaths ?? (readOnlyPaths = paths.AsReadOnly());
        
        protected ParametersEntryRegistry(ScriptGraphParameters parameters)
        {
            this.parameters = parameters;
        }
        
        /// <inheritdoc cref="IEntryRegistry.Build"/>
        public void Build()
        {
            entries.Clear();
            paths.Clear();

            // There are no parameters, so there's nothing to add.
            if (parameters.Names.Count < 1) return;

            var categoryName = $"• {GroupTitle}";

            foreach (var parameterId in parameters.OrderedIds)
            {
                // Add an entry for each parameter node.
                var parameterName = parameters.GetName(parameterId);
                var parameterType = parameters.GetType(parameterId);

                var portTypes = new List<Type>
                {
                    parameterType
                };

                var nodeType = GetEntryData(portTypes, parameterId);

                entries.Add(EntryFactory.Create(parameterName, 2, nodeType));
                paths.Add($"{categoryName}/{parameterName}");
            }
        }

        /// <summary>
        /// Gets the title of the group entry for this set of parameters.
        /// </summary>
        protected abstract string GroupTitle { get; }
        
        /// <summary>
        /// Gets the entry data based on the given port types and parameter.
        /// </summary>
        /// <param name="portTypes">The port types.</param>
        /// <param name="parameterId">The parameter's ID.</param>
        /// <returns>The parameter's entry data.</returns>
        protected abstract EntryData GetEntryData(IEnumerable<Type> portTypes, string parameterId);
    }
}
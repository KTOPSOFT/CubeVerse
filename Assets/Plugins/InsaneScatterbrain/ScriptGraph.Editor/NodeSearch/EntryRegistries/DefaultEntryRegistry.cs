using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.Extensions;
using UnityEditor.Experimental.GraphView;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public class DefaultEntryRegistry : IEntryRegistry
    {
        private readonly List<SearchTreeEntry> defaultEntries = new List<SearchTreeEntry>();
        private readonly List<string> paths = new List<string>();
        
        private ReadOnlyCollection<SearchTreeEntry> readOnlyDefaultEntries;
        private ReadOnlyCollection<string> readOnlyPaths;

        /// <inheritdoc cref="IEntryRegistry.Entries"/>
        public ReadOnlyCollection<SearchTreeEntry> Entries =>
            readOnlyDefaultEntries ?? (readOnlyDefaultEntries = defaultEntries.AsReadOnly());

        public ReadOnlyCollection<string> Paths => readOnlyPaths ?? (readOnlyPaths = paths.AsReadOnly());
        
        /// <inheritdoc cref="IEntryRegistry.Build"/>
        public void Build()
        {
            defaultEntries.Clear();
            paths.Clear();
            
            var nodeTypes = Types.WithAttribute<ScriptNodeAttribute>();
            
            var nodeTypesPerCategory = new Dictionary<string, List<Type>>();
            var nodeNamesPerType = new Dictionary<Type, string>();
            
            // First split up all the nodes by their respective categories.
            foreach (var nodeType in nodeTypes)
            {
                var attribute = nodeType.GetAttribute<ScriptNodeAttribute>();

                var nodeName = attribute.Name;
                var category = attribute.Category;
                
                if (!nodeTypesPerCategory.ContainsKey(category))
                {
                    nodeTypesPerCategory.Add(attribute.Category, new List<Type>());
                }

                nodeNamesPerType.Add(nodeType, nodeName);
                nodeTypesPerCategory[category].Add(nodeType);
            }
            
            // The categories are then sorted by name.
            var categories = nodeTypesPerCategory.Keys.ToList();
            categories.Sort();
            
            // Create all the entries for each category.
            foreach (var category in categories)
            {
                var categoryTypes = nodeTypesPerCategory[category];
                
                // Sort the category entries by name.
                categoryTypes.Sort((typeA, typeB) => 
                    string.Compare(typeA.GetFriendlyName(), typeB.GetFriendlyName(), StringComparison.Ordinal));
                
                foreach (var nodeType in categoryTypes)
                {
                    var nodeName = nodeNamesPerType[nodeType];

                    // Find all the in port fields and store their types.
                    var inPortFields = nodeType.GetPrivateFields()
                        .Where(field => typeof(InPort).IsAssignableFrom(field.FieldType));
                    
                    var inPortTypes = new List<Type>();
                    foreach (var inPortField in inPortFields)
                    {
                        var attribute = inPortField.GetAttribute<InPortAttribute>();
                        inPortTypes.Add(attribute.Type);
                    }

                    var explicitInPortTypesAttributes = nodeType.GetAttributes<ExplicitInPortTypesAttribute>();
                    foreach (var attribute in explicitInPortTypesAttributes)
                    {
                        inPortTypes.AddRange(attribute.Types);
                    }

                    // Same for the out ports.
                    var outPortFields = nodeType.GetPrivateFields()
                        .Where(field => typeof(OutPort).IsAssignableFrom(field.FieldType));

                    var outPortTypes = new List<Type>();
                    foreach (var outPortField in outPortFields)
                    {
                        var attribute = outPortField.GetAttribute<OutPortAttribute>();
                        outPortTypes.Add(attribute.Type);
                    }
                    
                    var explicitOutPortTypesAttributes = nodeType.GetAttributes<ExplicitOutPortTypesAttribute>();
                    foreach (var attribute in explicitOutPortTypesAttributes)
                    {
                        outPortTypes.AddRange(attribute.Types);
                    }

                    var nodeData = new EntryData(NodeType.Default, inPortTypes, outPortTypes, nodeType);
                    
                    var nodeEntry = EntryFactory.Create(nodeName, 2, nodeData);
                    
                    defaultEntries.Add(nodeEntry);
                    paths.Add($"{category}/{nodeName}");
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    /// <summary>
    /// Search filter that filters out entries that aren't compatible with the provided port.
    /// </summary>
    public class PortFilter : ISearchFilter
    {
        private readonly ScriptGraphPort port;
        
        public PortFilter(ScriptGraphPort port)
        {
            this.port = port;
        }

        /// <summary>
        /// Returns a tree consisting of the entries that are compatible with the provided port.
        /// </summary>
        /// <param name="tree">The original tree.</param>
        /// <returns>The tree with only the compatible entries.</returns>
        public List<SearchTreeEntry> Apply(IEnumerable<SearchTreeEntry> tree)
        {
            // Set a tree title based on the type we're looking for and the port direction.
            var treeTitle = port.Type.GetFriendlyName();
            if (port is InPort)
            {
                treeTitle += " Out";
            }
            else if (port is OutPort)
            {
                treeTitle += " In";
            }
            
            var filteredTree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(treeTitle))
            };

            var addedCategories = new HashSet<SearchTreeEntry>();
            SearchTreeEntry currentCategoryEntry = null;
                
            var portTypes = new List<Type>();
            foreach (var entry in tree)
            {
                var nodeData = entry.userData as EntryData;

                // If there's no data set, it means this is a group entry, which we want to keep track off in case
                // we want to add it later.
                if (nodeData == null)
                {
                    currentCategoryEntry = entry;
                    continue;
                }
                
                portTypes.Clear();
                
                // If the port is an in port, check it against the entries output types.
                if (port is InPort && nodeData.OutPortTypes != null)
                {
                    portTypes.AddRange(nodeData.OutPortTypes);
                }
                
                // If the port is an out port, check it against the entries input types.
                if (port is OutPort && nodeData.InPortTypes != null)
                {
                    portTypes.AddRange(nodeData.InPortTypes);
                }
                              
                foreach (var portType in portTypes)
                {
                    // A port is compatible of the out port type is part of the in port's type hierarchy.
                    var isCompatible =
                        port is InPort && port.Type.IsAssignableFrom(portType) ||
                        port is OutPort && portType.IsAssignableFrom(port.Type);

                    if (!isCompatible) continue;
                    
                    // If the current category hasn't been added as a group entry, do so now.
                    if (!addedCategories.Contains(currentCategoryEntry))
                    {
                        filteredTree.Add(currentCategoryEntry);
                        
                        // Keep track of the added categories, so they don't get added twice.
                        addedCategories.Add(currentCategoryEntry);
                    }
                    
                    filteredTree.Add(entry); 
                    break;
                }
            }

            return filteredTree;
        }
    }
}
using System.Collections.ObjectModel;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public interface IEntryRegistry
    {
        /// <summary>
        /// Gets all the registry's entries.
        /// </summary>
        ReadOnlyCollection<SearchTreeEntry> Entries { get; }
        
        ReadOnlyCollection<string> Paths { get; }
        
        /// <summary>
        /// Builds (or rebuilds) the registry's entry list.
        /// </summary>
        void Build();
    }
}
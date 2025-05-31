using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public interface ISearchFilter
    {
        /// <summary>
        /// Applies the search filter to the given tree and returns the filtered tree.
        /// </summary>
        /// <param name="tree">The original tree.</param>
        /// <returns>The tree after the filter applied to it.</returns>
        List<SearchTreeEntry> Apply(IEnumerable<SearchTreeEntry> tree);
    }
}
using InsaneScatterbrain.Services;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public static class EntryFactory
    {
        private static readonly Texture2D indentationImage;
        
        static EntryFactory()
        {
            indentationImage = Texture2DFactory.CreateDefault(1,1);
            indentationImage.Apply();
        }
        
        /// <summary>
        /// Creates a new search tree entry.
        /// </summary>
        /// <param name="typeName">The entry's name.</param>
        /// <param name="level">The level the entry's at.</param>
        /// <param name="data">The data to store with the entry.</param>
        /// <returns>The new search tree entry.</returns>
        public static SearchTreeEntry Create(string typeName, int level, EntryData data)
        {
            return new SearchTreeEntry(new GUIContent(typeName, indentationImage))
            {
                level = level,
                userData = data
            };
        }

        /// <summary>
        /// Creates a new search tree group entry.
        /// </summary>
        /// <param name="name">The group's name.</param>
        /// <param name="level">The group's level.</param>
        /// <returns>The new search tree group entry.</returns>
        public static SearchTreeGroupEntry CreateGroup(string name, int level)
        {
            return new SearchTreeGroupEntry(new GUIContent(name), level);
        }
    }
}
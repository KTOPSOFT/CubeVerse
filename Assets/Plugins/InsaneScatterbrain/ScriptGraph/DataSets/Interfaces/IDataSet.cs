using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Set that contains items with a ID and name.
    /// </summary>
    public interface IDataSet
    {
        /// <summary>
        /// Gets the item names.
        /// </summary>
        IReadOnlyCollection<string> Names { get; }
        
        /// <summary>
        /// Gets the item IDs in the order they are stored in the set.
        /// </summary>
        ReadOnlyCollection<string> OrderedIds { get; }
        
        /// <summary>
        /// Event triggered whenever an item in the set is renamed.
        /// </summary>
        event Action<string, string, string> OnRenamed;
        
        /// <summary>
        /// Event triggered whenever an item is added to the set.
        /// </summary>
        event Action<string> OnAdded;

        /// <summary>
        /// Event triggered whenever an item is removed from the set.
        /// </summary>
        event Action<string, string> OnRemoved;
        
        /// <summary>
        /// Event triggered whenever an item is moved from one position to another within the set.
        /// </summary>
        event Action<int, int> OnMoved;
        
        /// <summary>
        /// Gets the name of an item with the given ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The name.</returns>
        string GetName(string id);
        
        /// <summary>
        /// Gets whether the data set contains an item with the given ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>True if the item exists in the data set, false otherwise.</returns>
        bool ContainsId(string id);
        
        /// <summary>
        /// Gets whether the data set contains an item with the given name.
        /// </summary>
        /// <param name="elementName">The name.</param>
        /// <returns>True if the item exists in the data set, false otherwise.</returns>
        bool ContainsName(string elementName);
        
        /// <summary>
        /// Remove the item with the given ID from the set.
        /// </summary>
        /// <param name="id">The ID to remove.</param>
        void Remove(string id);
        
        /// <summary>
        /// Renames the item with the given ID.
        /// </summary>
        /// <param name="id">The item's ID.</param>
        /// <param name="newName">The item's new name.</param>
        void Rename(string id, string newName);
        
        /// <summary>
        /// Moves the item with the given ID to another index in the set.
        /// </summary>
        /// <param name="id">The item's ID.</param>
        /// <param name="newIndex">The new index.</param>
        void Move(string id, int newIndex);
    }
    
    /// <summary>
    /// Set that contains items with a ID and name.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public interface IDataSet<T> : IDataSet where T : IDataSetItem
    {
        /// <summary>
        /// Adds an item to the dataset.
        /// </summary>
        /// <param name="element">The item to add.</param>
        void Add(T element);
    }
}
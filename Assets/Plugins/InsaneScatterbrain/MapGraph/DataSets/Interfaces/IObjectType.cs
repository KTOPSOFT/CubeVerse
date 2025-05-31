using System.Collections.ObjectModel;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents objects that consists of entries that represent a certain Object type.
    /// </summary>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <typeparam name="TObject">The object type.</typeparam>
    public interface IObjectType<TEntry, TObject> : IDataSetItem 
        where TEntry : IObjectTypeEntry<TObject> 
        where TObject : Object
    {
        /// <summary>
        /// Gets entries of this type.
        /// </summary>
        ReadOnlyCollection<TEntry> Entries { get; }
        
        /// <summary>
        /// The combined weight of all the entries.
        /// </summary>
        float TotalWeight { get; }
        
        /// <summary>
        /// Whether or not weighting is enabled. If disabled, the
        /// weights shouldn't be taken into consideration and
        /// be treated as if they're all equal.
        /// </summary>
        bool IsWeightEnabled { get; set; }

        /// <summary>
        /// Calculates the total weight.
        /// </summary>
        void CalculateTotalWeight();
        
        /// <summary>
        /// Creates and adds a new entry this object type.
        /// </summary>
        void AddNewEntry();
        
        /// <summary>
        /// Removes an entry from the object type.
        /// </summary>
        /// <param name="index">The index of entry.</param>
        void RemoveEntry(int index);

        /// <summary>
        /// Moves the entry's position within the list.
        /// </summary>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        void MoveEntry(int oldIndex, int newIndex);
        
        /// <summary>
        /// Remove any empty entries.
        /// </summary>
        void Clean();
    }
}
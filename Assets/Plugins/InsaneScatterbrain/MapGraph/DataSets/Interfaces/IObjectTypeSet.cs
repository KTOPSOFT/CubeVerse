using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Collection of different object types.
    /// </summary>
    /// <typeparam name="TType">The type of object type.</typeparam>
    /// <typeparam name="TObject">The type of object represented by the entries.</typeparam>
    public interface IObjectTypeSet<TType, TObject> : IDataSet<TType> 
        where TType : IDataSetItem
        where TObject : Object
    {
        /// <summary>
        /// Adds a new entry to the object type of the given ID.
        /// </summary>
        /// <param name="typeId">The object type ID to add a new entry to.</param>
        void AddNewEntry(string typeId);
        
        /// <summary>
        /// Removes a the given entry index from the object type of the given ID.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <param name="entryIndex">The entry's index to delete.</param>
        void RemoveEntry(string typeId, int entryIndex);
        
        /// <summary>
        /// Gets the object of the given type ID and entry index combination.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <param name="entryIndex">The entry's index.</param>
        /// <returns>The object.</returns>
        TObject GetObject(string typeId, int entryIndex);
        
        /// <summary>
        /// Gets the weight of the given type ID and entry index combination.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <param name="entryIndex">The entry's index.</param>
        float GetWeight(string typeId, int entryIndex);
        
        /// <summary>
        /// Gets the object of the given type ID and entry index combination.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <param name="entryIndex">The entry's index.</param>
        /// <param name="obj">The new object.</param>
        void SetObject(string typeId, int entryIndex, TObject obj);

        /// <summary>
        /// Sets the weight of the given type ID and entry index combination.
        /// </summary>
        /// <param name="typeId">The type's ID.</param>
        /// <param name="entryIndex">The entry's index.</param>
        void SetWeight(string typeId, int entryIndex, float weight);

        /// <summary>
        /// Sets whether weighting is enabled for the given type.
        /// </summary>
        /// <param name="typeId">The type's ID.</param>
        /// <param name="enabled">Whether the weighting is enabled.</param>
        void SetWeightEnabled(string typeId, bool enabled);

        /// <summary>
        /// Gets whether weighting is enabled for the given type.
        /// </summary>
        /// <param name="typeId">The type's ID.</param>
        /// <returns>Whether the weighting is enabled.</returns>
        bool IsWeightEnabled(string typeId);
        
        /// <summary>
        /// Gets the number of entries of a given object type.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <returns>The number of entries.</returns>
        int GetEntryCount(string typeId);

        /// <summary>
        /// Moves the entry's position within the type's list.
        /// </summary>
        /// <param name="typeId">The object type's ID.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        void MoveEntry(string typeId, int oldIndex, int newIndex);

        /// <summary>
        /// Remove any empty entries from the object type objects.
        /// </summary>
        void Clean();
    }
}
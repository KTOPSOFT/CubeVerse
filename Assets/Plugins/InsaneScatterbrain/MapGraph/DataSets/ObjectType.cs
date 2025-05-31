using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents objects that consists of entries that represent a certain Object type.
    /// </summary>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <typeparam name="TObject">The object type.</typeparam>
    [Serializable]
    public abstract class ObjectType<TEntry, TObject> : DataSetItem, IObjectType<TEntry, TObject> 
        where TEntry : IObjectTypeEntry<TObject> 
        where TObject : Object
    {
        [SerializeField] private bool isWeightEnabled = false;
        [SerializeField] private float totalWeight = 0f;

        /// <summary>
        /// A less restricted collection of the entries for protected use.
        /// </summary>
        protected abstract List<TEntry> OpenEntries { get; }

        private ReadOnlyCollection<TEntry> readonlyEntries;
        /// <inheritdoc cref="IObjectType{TEntry,TObject}.Entries"/>
        public ReadOnlyCollection<TEntry> Entries => readonlyEntries ?? (readonlyEntries = OpenEntries.AsReadOnly());

        /// <inheritdoc cref="IObjectType{TEntry,TObject}.IsWeightEnabled"/>
        public bool IsWeightEnabled
        {
            get => isWeightEnabled; 
            set => isWeightEnabled = value;
        }

        /// <inheritdoc cref="IObjectType{TEntry,TObject}.TotalWeight"/>
        public float TotalWeight => totalWeight;
        
        /// <inheritdoc cref="IObjectType{TEntry,TObject}.CalculateTotalWeight"/>
        public void CalculateTotalWeight()
        {
            totalWeight = 0;
            foreach (var entry in OpenEntries)
            {
                totalWeight += entry.Weight;
            }
        }

        /// <summary>
        /// Creates and returns an entry for this object type.
        /// </summary>
        /// <returns>The new entry.</returns>
        protected abstract TEntry NewEntry();
        
        /// <inheritdoc cref="IObjectType{TEntry,TObject}.AddNewEntry"/>
        public void AddNewEntry()
        {
            OpenEntries.Add(NewEntry());
        }

        /// <inheritdoc cref="IObjectType{TEntry,TObject}.RemoveEntry"/>
        public void RemoveEntry(int index)
        {
            OpenEntries.RemoveAt(index);
        }

        public void MoveEntry(int oldIndex, int newIndex)
        {
            var entry = OpenEntries[oldIndex];
            
            OpenEntries.RemoveAt(oldIndex);
            OpenEntries.Insert(newIndex, entry);
        }

        /// <inheritdoc cref="IObjectType{TEntry,TObject}.Clean"/>
        public void Clean()
        {
            OpenEntries.RemoveAll(item => item.Value == null);
        }

        protected ObjectType(string name) : base(name) { }
    }
}
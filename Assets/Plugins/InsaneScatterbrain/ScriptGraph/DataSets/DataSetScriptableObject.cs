using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InsaneScatterbrain.Versioning;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Data set stored as a scriptable object.
    /// </summary>
    /// <typeparam name="TItem">Type of the items stored in the data set.</typeparam>
    [Serializable]
    public abstract class DataSetScriptableObject<TItem> : 
        DataSetScriptableObject<TItem, IOpenDataSet<TItem>> 
        where TItem : IDataSetItem {}
    
    /// <summary>
    /// Data set stored as a scriptable object.
    /// </summary>
    /// <typeparam name="TItem">Type of the items stored in the data set.</typeparam>
    /// <typeparam name="TOpenSet">The type of the open set.</typeparam>
    [Serializable]
    public abstract class DataSetScriptableObject<TItem, TOpenSet> : 
        VersionedScriptableObject, IDataSet<TItem> 
        where TItem : IDataSetItem 
        where TOpenSet : IOpenDataSet<TItem>
    {
        /// <summary>
        /// Gets the less restricted set of items for protected use.
        /// </summary>
        protected abstract TOpenSet OpenSet { get; }
        
        /// <inheritdoc cref="IDataSet{T}.Names"/>
        public IReadOnlyCollection<string> Names => OpenSet.Names;
        
        /// <inheritdoc cref="IDataSet{T}.OrderedIds"/>
        public ReadOnlyCollection<string> OrderedIds => OpenSet.OrderedIds;

        /// <summary>
        /// Gets the item with the given ID.
        /// </summary>
        /// <param name="id">The item's ID.</param>
        /// <returns>The item.</returns>
        protected TItem Get(string id)
        {
            return OpenSet.Get(id);
        }
        
        /// <summary>
        /// Gets the item with the given name.
        /// </summary>
        /// <param name="elementName">The item's name.</param>
        /// <returns>The item.</returns>
        protected TItem GetByName(string elementName)
        {
            return OpenSet.GetByName(elementName);
        }

        /// <inheritdoc cref="IDataSet{T}.OnRenamed"/>
        public event Action<string, string, string> OnRenamed
        {
            add => OpenSet.OnRenamed += value;
            remove => OpenSet.OnRenamed -= value;
        }
        
        /// <inheritdoc cref="IDataSet{T}.OnAdded"/>
        public event Action<string> OnAdded
        {
            add => OpenSet.OnAdded += value;
            remove => OpenSet.OnAdded -= value;
        }
        
        /// <inheritdoc cref="IDataSet{T}.OnRemoved"/>
        public event Action<string, string> OnRemoved
        {
            add => OpenSet.OnRemoved += value;
            remove => OpenSet.OnRemoved -= value;
        }
        
        /// <inheritdoc cref="IDataSet{T}.OnMoved"/>
        public event Action<int, int> OnMoved
        {
            add => OpenSet.OnMoved += value;
            remove => OpenSet.OnMoved -= value;
        }
        
        /// <inheritdoc cref="IDataSet{T}.Rename"/>
        public virtual void Rename(string id, string newName)
        {
            OpenSet.Rename(id, newName);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Remove"/>
        public virtual void Remove(string id)
        {
            OpenSet.Remove(id);
        }

        /// <inheritdoc cref="IDataSet{T}.Move"/>
        public virtual void Move(string id, int newIndex)
        {
            OpenSet.Move(id, newIndex);
        }
        
        /// <inheritdoc cref="IDataSet{T}.GetName"/>
        public string GetName(string id)
        {
            return OpenSet.GetName(id);
        }

        /// <inheritdoc cref="IDataSet{T}.ContainsId"/>
        public bool ContainsId(string id)
        {
            return OpenSet.ContainsId(id);
        }
        
        /// <inheritdoc cref="IDataSet{T}.ContainsName"/>
        public bool ContainsName(string elementName)
        {
            return OpenSet.ContainsName(elementName);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Add"/>
        public virtual void Add(TItem element)
        {
            OpenSet.Add(element);
        }
    }
}
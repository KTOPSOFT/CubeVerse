using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <inheritdoc cref="IDataSet{T}"/>
    [Serializable]
    public abstract class DataSet<T> : IDataSet<T>, ISerializationCallbackReceiver where T : IDataSetItem
    {
        [SerializeField] private List<T> elements = new List<T>();
        [SerializeField] private List<string> orderedIds = new List<string>();

        private Dictionary<string, T> elementsById = new Dictionary<string, T>();
        private Dictionary<string, T> elementsByName = new Dictionary<string, T>();

        /// <inheritdoc cref="IDataSet{T}.Names"/>
        public IReadOnlyCollection<string> Names => elementsByName.Keys;

        private ReadOnlyCollection<string> readOnlyOrderedIds;
        /// <inheritdoc cref="IDataSet{T}.OrderedIds"/>
        public ReadOnlyCollection<string> OrderedIds => readOnlyOrderedIds ?? (readOnlyOrderedIds = orderedIds.AsReadOnly());
        
        /// <inheritdoc cref="IDataSet{T}.OnRenamed"/>
        public event Action<string, string, string> OnRenamed;
        
        /// <inheritdoc cref="IDataSet{T}.OnAdded"/>
        public event Action<string> OnAdded;
        
        /// <inheritdoc cref="IDataSet{T}.OnRemoved"/>
        public event Action<string, string> OnRemoved;
        
        /// <inheritdoc cref="IDataSet{T}.OnMoved"/>
        public event Action<int, int> OnMoved;
        
        protected T Get(string id)
        {
            if (!elementsById.ContainsKey(id))
            {
                throw new ArgumentException($"Cannot get item {id}: ID doesn't exist.");
            }
            return elementsById[id];
        }

        protected T GetByName(string elementName)
        {
            if (!elementsByName.ContainsKey(elementName))
            {
                throw new ArgumentException($"Cannot get item {elementName}: Name doesn't exist.");
            }
            return elementsByName[elementName];
        }
        
        /// <inheritdoc cref="IDataSet{T}.GetName"/>
        public string GetName(string id)
        {
            return elementsById[id].Name; 
        }
        
        /// <inheritdoc cref="IDataSet{T}.ContainsId"/>
        public bool ContainsId(string id)
        {
            return elementsById.ContainsKey(id);
        }

        /// <inheritdoc cref="IDataSet{T}.ContainsName"/>
        public bool ContainsName(string elementName)
        {
            return elementsByName.ContainsKey(elementName);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Add"/>
        public virtual void Add(T element)
        {
            if (elementsByName.ContainsKey(element.Name))
            {
                throw new ArgumentException("Element name already exists.");
            }
            
            elements.Add(element);
            orderedIds.Add(element.Id);
            elementsById.Add(element.Id, element);
            elementsByName.Add(element.Name, element);
            
            OnAdded?.Invoke(element.Id);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Rename"/>
        public void Rename(string id, string newName)
        {
            if (elementsByName.ContainsKey(newName))
            {
                throw new ArgumentException("New name already exists.");
            }
            
            var element = elementsById[id];
            var oldName = element.Name;
            elementsByName.Remove(element.Name);
            element.Name = newName;
            elementsByName.Add(newName, element);
            
            OnRenamed?.Invoke(id, oldName, newName);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Remove"/>
        public virtual void Remove(string id)
        {
            if (!elementsById.ContainsKey(id))
            {
                throw new ArgumentException("Id doesn't exist.");
            }
            
            var element = elementsById[id];
            var name = element.Name;
            
            elements.Remove(element);
            orderedIds.Remove(element.Id);
            elementsById.Remove(element.Id);
            elementsByName.Remove(element.Name);
            
            OnRemoved?.Invoke(id, name);
        }
        
        /// <inheritdoc cref="IDataSet{T}.Move"/>
        public void Move(string id, int newIndex)
        {
            var oldIndex = orderedIds.IndexOf(id);
            
            orderedIds.RemoveAt(oldIndex);
            orderedIds.Insert(newIndex, id);
            
            OnMoved?.Invoke(oldIndex, newIndex);
        }

        public void ChangeId(string oldId, string newId)
        {
            if (!elementsById.TryGetValue(oldId, out var element))
            {
                throw new ArgumentException("Id doesn't exist.");
            }

            element.Id = newId;
            
            elementsById.Remove(oldId);
            elementsById.Add(newId, element);
            
            var index = orderedIds.IndexOf(oldId);
            orderedIds[index] = newId;
        }

        #region Serialization
        
        public void OnBeforeSerialize()
        {
            
        }

        public virtual void OnAfterDeserialize()
        {
            elementsById = new Dictionary<string, T>(); 
            elementsByName = new Dictionary<string, T>(); 

            foreach (var element in elements) 
            {
                elementsById.Add(element.Id, element);
                elementsByName.Add(element.Name, element);
            }
        }
        
        #endregion
    }
}
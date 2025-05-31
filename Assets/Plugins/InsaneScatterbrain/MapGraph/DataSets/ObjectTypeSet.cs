using System;
using InsaneScatterbrain.ScriptGraph;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    /// <inheritdoc cref="IObjectTypeSet{TType,TObject}"/>
    /// <typeparam name="TType">The type of object type.</typeparam>
    /// <typeparam name="TEntry">The type of object type's entries.</typeparam>
    /// <typeparam name="TObject">The type of object stored in the entry.</typeparam>
    [Serializable]
    public abstract class ObjectTypeSet<TType, TEntry, TObject> : DataSet<TType>, IObjectTypeSet<TType, TObject> 
        where TType : IObjectType<TEntry, TObject>
        where TEntry : IObjectTypeEntry<TObject>
        where TObject : Object
    {
        public event Action<string, int, TObject> OnEntryRemoved;
        public event Action<string, int, TObject, TObject> OnEntrySet;
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.AddNewEntry" />
        public void AddNewEntry(string typeId)
        {
            var type = Get(typeId);
            type.AddNewEntry();
            
            type.CalculateTotalWeight();
        }

        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.RemoveEntry" />
        public void RemoveEntry(string typeId, int entryIndex)
        {
            var type = Get(typeId);
            var entry = GetObject(typeId, entryIndex);
            type.RemoveEntry(entryIndex);
            OnEntryRemoved?.Invoke(typeId, entryIndex, entry);
            
            type.CalculateTotalWeight();
        }

        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.GetObject" />
        public TObject GetObject(string typeId, int entryIndex)
        {
            var type = Get(typeId);
            return type.Entries[entryIndex].Value;
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.GetWeight" />
        public float GetWeight(string typeId, int entryIndex)
        {
            var type = Get(typeId);
            return type.Entries[entryIndex].Weight;
        }

        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.SetObject" />
        public void SetObject(string typeId, int entryIndex, TObject obj)
        {
            var type = Get(typeId);
            var oldObj = type.Entries[entryIndex].Value;
            type.Entries[entryIndex].Value = obj;
            OnEntrySet?.Invoke(typeId, entryIndex, oldObj, obj);
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.SetWeight" />
        public void SetWeight(string typeId, int entryIndex, float weight)
        {
            var type = Get(typeId);
            type.Entries[entryIndex].Weight = weight;
            
            type.CalculateTotalWeight();
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.SetWeightEnabled" />
        public void SetWeightEnabled(string typeId, bool enabled)
        {
            var type = Get(typeId);
            type.IsWeightEnabled = enabled;
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.IsWeightEnabled" />
        public bool IsWeightEnabled(string typeId)
        {
            var type = Get(typeId);
            return type.IsWeightEnabled;
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.MoveEntry" />
        public void MoveEntry(string typeId, int oldIndex, int newIndex)
        {
            var tileType = Get(typeId);
            tileType.MoveEntry(oldIndex, newIndex);
        }
        
        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.GetEntryCount" />
        public int GetEntryCount(string typeId)
        {
            var prefabType = Get(typeId);
            return prefabType.Entries.Count;
        }

        /// <inheritdoc cref="IObjectTypeSet{TType,TObject}.Clean" />
        public void Clean()
        {
            foreach (var typeId in OrderedIds)
            {
                var type = Get(typeId);
                type.Clean();
            }
        }
    }
}
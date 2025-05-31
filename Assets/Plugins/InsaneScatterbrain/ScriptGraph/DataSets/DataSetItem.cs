using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <inheritdoc cref="IDataSetItem" />
    [Serializable]
    public abstract class DataSetItem : IDataSetItem
    {
        [SerializeField] private string id = Guid.NewGuid().ToString();
        [SerializeField] private string name;

        protected DataSetItem(string name)
        {
            this.name = name;
        }

        /// <inheritdoc cref="IDataSetItem.Id" />
        public string Id
        {
            get => id;
            set => id = value;
        }

        /// <inheritdoc cref="IDataSetItem.Name" />
        public string Name
        {
            get => name;
            set => name = value;
        }
    }
}
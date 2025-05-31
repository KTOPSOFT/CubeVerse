using System;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <inheritdoc cref="IOpenDataSet{T}" />
    [Serializable]
    public class OpenDataSet<T> : DataSet<T>, IOpenDataSet<T> where T : IDataSetItem
    {
        /// <inheritdoc cref="IOpenDataSet{T}.Get" />
        public new T Get(string id)
        {
            return base.Get(id);
        }

        /// <inheritdoc cref="IOpenDataSet{T}.GetByName" />
        public new T GetByName(string name)
        {
            return base.GetByName(name);
        }
    }
}
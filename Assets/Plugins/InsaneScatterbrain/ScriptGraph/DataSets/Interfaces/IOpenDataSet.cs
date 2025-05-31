namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// A less restricted variant of the data set, for protected use.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public interface IOpenDataSet<T> : IDataSet<T> where T : IDataSetItem
    {
        /// <summary>
        /// Gets the item of with the given ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The item.</returns>
        T Get(string id);
        
        /// <summary>
        /// Gets the item of with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The item.</returns>
        T GetByName(string name);
    }
}
namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Item that can be stored in a data set.
    /// </summary>
    public interface IDataSetItem
    {
        /// <summary>
        /// Gets the ID.
        /// </summary>
        string Id { get; set; }
        
        /// <summary>
        /// Gets/sets the name.
        /// </summary>
        string Name { set; get; }
    }
}
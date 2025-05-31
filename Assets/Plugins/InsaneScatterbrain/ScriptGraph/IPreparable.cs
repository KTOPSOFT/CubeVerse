namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Input parameters of types the implement the IPreparable interface will have Prepare
    /// called before running the script graph.
    /// </summary>
    public interface IPreparable
    {
        /// <summary>
        /// Contains logic to prepare this object for script graph processing.
        /// </summary>
        void Prepare();
    }
}
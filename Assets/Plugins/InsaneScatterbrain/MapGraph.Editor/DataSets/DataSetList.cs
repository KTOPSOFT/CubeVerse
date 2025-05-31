using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// The default list to use in the editor for data sets.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public abstract class DataSetList<T> : DataSetListBase<T> where T : IDataSetItem
    {
        protected abstract T New(string name);

        protected DataSetList(IDataSet<T> dataSet) : base(dataSet)
        {
            ReorderableList.onAddCallback = list =>
            {
                string newName;
                
                // Make sure the new name is unique, by incrementing the trailing number until one is found that doesn't exist yet.
                var newNameTrailingNumber = 0;
                do
                {
                    newName = DefaultName;
                    if (newNameTrailingNumber > 0)
                    {
                        newName += $" {newNameTrailingNumber}";
                    }

                    newNameTrailingNumber++;
                } while (dataSet.ContainsName(newName));

                dataSet.Add(New(newName));
                UpdateList();
                SaveAsset();
            };
        }
    }
}
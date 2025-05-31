namespace InsaneScatterbrain.ScriptGraph
{
    public class NewInstanceProvider : IInstanceProvider
    {
        public T Get<T>() where T : class, new() => new T();

        public void ReturnAll()
        {
            // Instances aren't returned, do nothing.
        }
    }
}
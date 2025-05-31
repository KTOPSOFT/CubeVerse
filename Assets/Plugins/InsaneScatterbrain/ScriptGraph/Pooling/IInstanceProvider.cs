namespace InsaneScatterbrain.ScriptGraph
{
    public interface IInstanceProvider
    {
        T Get<T>() where T : class, new();
        void ReturnAll();
    }
}
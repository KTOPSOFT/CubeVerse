namespace InsaneScatterbrain.ScriptGraph
{
    public interface IScriptGraphInstanceProvider
    {
        ScriptGraphGraph Get(ScriptGraphGraph original);
        void ReturnAll();
    }
}
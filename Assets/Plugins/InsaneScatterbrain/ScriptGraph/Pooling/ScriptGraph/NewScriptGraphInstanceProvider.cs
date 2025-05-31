using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class NewScriptGraphInstanceProvider : IScriptGraphInstanceProvider
    {
        public ScriptGraphGraph Get(ScriptGraphGraph original) => Object.Instantiate(original);

        public void ReturnAll()
        {
            // Nothing to return. Do nothing.
        }
    }
}
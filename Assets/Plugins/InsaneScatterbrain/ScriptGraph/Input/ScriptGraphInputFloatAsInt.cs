using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    [AddComponentMenu("Map Graph/Input/Map Graph Input (int as float)")]
    public class ScriptGraphInputFloatAsInt : ScriptGraphInput<int>
    {
        public void Set(float value)
        {
            Runner.SetInById(ParameterId, Mathf.RoundToInt(value));
        }
    }
}
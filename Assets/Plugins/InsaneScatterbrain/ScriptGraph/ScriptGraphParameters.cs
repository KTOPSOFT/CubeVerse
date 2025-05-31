using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class contains parameter definitions for a graph.
    /// </summary>
    [Serializable]
    public class ScriptGraphParameters : DataSet<ScriptGraphParameter>
    {
        public Type GetType(string id)
        {
            return Get(id).Type;
        }

        public string GetId(string parameterName)
        {
            return GetByName(parameterName).Id;
        }

        #region Serialization
        
        [Obsolete("Parameter data is now stored in a script graph parameter object. Will be removed in version 2.0.")]
        [SerializeField] private string[] names;
        
        [Obsolete("Parameter data is now stored in a script graph parameter object. Will be removed in version 2.0.")]
        [SerializeField] private string[] typeNames;

        #endregion Serialization
    }
}
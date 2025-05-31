using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Provides a transparent color.
    /// </summary>
    [ScriptNode("Transparent Color", "Colors", false), Serializable]
    public class TransparentColorNode : ProviderNode
    {
        [OutPort("", typeof(Color32)), SerializeReference]
        private OutPort colorOut = null;

        public override void Initialize()
        {
            base.Initialize();
            
            colorOut.Set(() => default(Color32));
        }
    }
}
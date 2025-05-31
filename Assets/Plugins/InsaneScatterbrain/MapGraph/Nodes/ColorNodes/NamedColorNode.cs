using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Provides a color from the graph's named color set, by the given name.
    /// </summary>
    [ScriptNode("Named Color", "Colors"), Serializable]
    public class NamedColorNode : ProviderNode
    {
        [FormerlySerializedAs("colorName")] 
        [SerializeField] private string namedColorId;
        
        [OutPort("", typeof(Color32)), SerializeReference] 
        private OutPort colorOut = null;

        public string NamedColorId
        {
            get => namedColorId;
            set => namedColorId = value; 
        }

        /// <inheritdoc cref="ProviderNode.Initialize"/>
        public override void Initialize()
        {
            base.Initialize();
            
            var namedColorSet = Get<NamedColorSet>();

            colorOut.Set(() =>
            {
                if (string.IsNullOrEmpty(namedColorId))
                {
                    Debug.LogWarning("No value assigned to named color node.");
                    return default;
                }
                
                if (!namedColorSet.OrderedIds.Contains(namedColorId))
                {
                    Debug.LogError($"Cannot find named color with ID {namedColorId} in named color set: {namedColorSet.name}.");
                    return default;
                }
                
                return namedColorSet.GetColorById(namedColorId);
            });
        }
    }
}
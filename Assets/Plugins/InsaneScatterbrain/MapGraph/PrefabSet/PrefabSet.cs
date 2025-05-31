using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains prefab types by name. The names can be used to associate them with a named color.
    /// </summary>
    public class PrefabSet : ObjectTypeSetScriptableObject<PrefabType, PrefabTypeEntry, GameObject>
    {
        [SerializeField] private float prefabWidth = 1f;
        [SerializeField] private float prefabHeight = 1f;
        [SerializeField] private float prefabDepth = 1f;
        
        /// <summary>
        /// Get/sets the prefabs width in units.
        /// </summary>
        public float PrefabWidth
        {
            get => prefabWidth;
            set => prefabWidth = value;
        }

        /// <summary>
        /// Get/sets the prefabs height in units.
        /// </summary>
        public float PrefabHeight
        {
            get => prefabHeight;
            set => prefabHeight = value;
        }
        
        /// <summary>
        /// Get/sets the prefabs depth in units.
        /// </summary>
        public float PrefabDepth
        {
            get => prefabDepth;
            set => prefabDepth = value;
        }

        [Obsolete("Prefab type data is now stored in a data set. Will be removed in version 2.0.")]
        [SerializeField] private List<PrefabType> prefabTypes = new List<PrefabType>();

        #region DataSet

        [Serializable] private class OpenPrefabSet : OpenObjectTypeSet<PrefabType, PrefabTypeEntry, GameObject> { }
        
        [SerializeField] private OpenPrefabSet openPrefabSet = new OpenPrefabSet();

        protected override OpenObjectTypeSet<PrefabType, PrefabTypeEntry, GameObject> OpenSet => openPrefabSet;

        #endregion
    }
}
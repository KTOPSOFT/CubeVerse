using System;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Entry for a prefab type.
    /// </summary>
    [Serializable]
    public class PrefabTypeEntry : ObjectTypeEntry<GameObject>
    {
        [SerializeField] private GameObject prefab;

        public override GameObject Value
        {
            get => prefab;
            set => prefab = value;
        }
    }
}
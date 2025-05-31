using UnityEngine;
using VargrIntegrations;

namespace MalbersAnimations.VargrMultiplayer
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Muliplayer/Ammo ID", fileName = "New Ammo", order = -1000)]
    public class Ammo : IDs {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<ModeID>();
#endif
        #endregion
        public AmmoType ammoType;
        public GameObject prefab;
    }
}

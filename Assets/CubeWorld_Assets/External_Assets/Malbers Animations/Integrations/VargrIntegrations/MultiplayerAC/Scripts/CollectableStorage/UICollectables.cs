using MalbersAnimations.Scriptables;
using VargrIntegrations;
using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Muliplayer/UI Collectable", fileName = "New UI Collectable", order = -1000)]
    public class UICollectables : IDs {
        #region CalculateID
    #if UNITY_EDITOR
        public static int MaxKey { get{ return 1000000; } }

        private void Reset() => GetID();

        [ContextMenu("Get ID")]
        private void GetID() => FindID<ModeID>();
    #endif
        #endregion
        public ScriptableVar UI_Var;
    }
}

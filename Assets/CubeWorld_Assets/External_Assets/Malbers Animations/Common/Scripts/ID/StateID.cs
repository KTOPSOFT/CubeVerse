namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/State", fileName = "New State ID", order = -1000)]
    public class StateID : IDs 
    {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<StateID>();
#endif
        #endregion
    }
}
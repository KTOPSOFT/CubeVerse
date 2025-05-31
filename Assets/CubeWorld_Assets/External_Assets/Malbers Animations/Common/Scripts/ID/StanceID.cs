namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Stance", fileName = "New Stance ID", order = -1000)]
    public class StanceID : IDs 
    {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<StanceID>();
#endif
        #endregion
    }
}

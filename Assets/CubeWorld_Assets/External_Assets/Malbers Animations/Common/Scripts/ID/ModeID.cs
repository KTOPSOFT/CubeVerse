namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Mode", fileName = "New Mode ID", order = -1000)]
    public class ModeID : IDs
    {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<ModeID>();
#endif
        #endregion
    }
}
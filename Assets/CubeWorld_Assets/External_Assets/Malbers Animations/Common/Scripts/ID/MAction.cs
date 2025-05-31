namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Action", fileName = "New Action ID", order = -1000)]
    public class MAction : IDs
    {
        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<MAction>();
#endif
        #endregion 
    }
}
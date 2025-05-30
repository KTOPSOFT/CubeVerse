using UnityEngine;

namespace Michsky.DreamOS
{
    public class NotificationListClear : MonoBehaviour
    {
        [Header("Resources")]
        public Transform listContent;

        void OnEnable()
        {
            if (listContent == null)
                listContent = transform.GetComponentInChildren<Transform>();

            foreach (Transform child in listContent)
                Destroy(child.gameObject);
        }
    }
}
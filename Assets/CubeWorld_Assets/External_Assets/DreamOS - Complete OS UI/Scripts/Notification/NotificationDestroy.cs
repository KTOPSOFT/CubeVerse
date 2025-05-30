using UnityEngine;

namespace Michsky.DreamOS
{
    public class NotificationDestroy : MonoBehaviour
    {
        void OnEnable()
        {
            Destroy(gameObject);
        }
    }
}
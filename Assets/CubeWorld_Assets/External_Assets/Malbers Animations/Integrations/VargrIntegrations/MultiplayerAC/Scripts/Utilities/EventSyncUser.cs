using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Controller;

namespace MalbersAnimations.VargrMultiplayer
{
    public abstract class EventSyncUser : MonoBehaviour
    {
        [SerializeField] protected int m_ID;
        [SerializeField] protected EventSync m_EventSync;

        protected int indexEncode(int target, bool recived = false)
        {
            if (recived) return target - (m_ID*10);
            return target + (m_ID*10);
        }

        protected virtual void Awake()
        {
            if(m_EventSync != null) m_EventSync.NetEventSync += RecivedEventSync;
        }

        protected virtual void RecivedEventSync(object data, int index, NetEventSource target) { }
    }
}
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Controller;

namespace MalbersAnimations.VargrMultiplayer
{
    public abstract class TriggerEventSyncUser : EventSyncUser
    {
        protected TriggerEventSync m_TriggerEventSync => (m_EventSync is TriggerEventSync) ? (TriggerEventSync) m_EventSync : null;
        protected override void Awake()
        {
            base.Awake();
            if(m_TriggerEventSync != null) m_TriggerEventSync.NetEventTrigger += RecivedEventSync;
        }
    }
}
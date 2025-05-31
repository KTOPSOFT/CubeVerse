using MalbersAnimations.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.VargrMultiplayer
{
    public class EffectSync : MonoBehaviour
    {
        [SerializeField] protected UnityEvent[] m_OnPlay;
        [SerializeField] protected UnityEvent[] m_OnStop;
        public EffectManager effectManager;
        protected EventSync m_eventSync;
        protected bool m_IsPlayer = false;

        protected void Awake()
        {
            var player = GetComponentInParent<PlayerInstance>();
            m_eventSync = GetComponentInParent<EventSync>();

            if(player != null) m_IsPlayer = true;

            if(effectManager == null) effectManager = GetComponent<EffectManager>();

            m_eventSync.NetEventSync += EventSync;
        }

        public void NetPlayer_OnPlay(int target)
        {
            if(!m_eventSync.isController || !m_IsPlayer) return;

            m_OnPlay[target].Invoke();
        }
        public void NetPlayer_OnStop(int target)
        {
            if(!m_eventSync.isController || !m_IsPlayer) return;

            m_OnStop[target].Invoke();
        }
        public void Net_OnSyncStart(int target) => m_eventSync.EventBroadcast(true, target, NetEventSource.EffectSync);
        public void Net_OnSyncStop(int target) => m_eventSync.EventBroadcast(false, target, NetEventSource.EffectSync);

        public void EventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.EffectSync) return;
            if(m_eventSync.isController) return;

            if((bool)data){
                effectManager.PlayEffect(index);
            }else{
                effectManager.StopEffect(index);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Controller;

namespace MalbersAnimations.VargrMultiplayer
{
    public class ZoneUtility : TriggerEventSyncUser
    {
        [SerializeField] protected Zone m_Zone;

        protected override void Awake()
        {
            base.Awake();
            if(m_Zone == null)
                m_Zone = GetComponent<Zone>();

            m_Zone.OnEnter.AddListener(Player_OnEnter);
            m_Zone.OnExit.AddListener(Player_OnExit);
            m_Zone.OnZoneActivation.AddListener(Player_OnZoneActivation);
            m_Zone.OnZoneFailed.AddListener(Player_OnZoneFailed);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if(m_EventSync == null) return;

            if(m_EventSync is SceneEventSync sceneEventSync){
                m_ID = sceneEventSync.userList.IndexOf(this);
                if (m_ID == -1)
                {
                    sceneEventSync.userList.Add(this);
                    m_ID = sceneEventSync.userList.IndexOf(this);
                }
            }
        }
#endif

        [SerializeField] protected UnityEvent<MAnimal> m_Player_OnEnter;
        [SerializeField] protected UnityEvent<MAnimal> m_Player_OnExit;
        [SerializeField] protected UnityEvent<MAnimal> m_Player_OnZoneActivation;
        [SerializeField] protected UnityEvent<MAnimal> m_Player_OnZoneFailed;

        [SerializeField] protected UnityEvent<MAnimal> m_World_OnEnter;
        [SerializeField] protected UnityEvent<MAnimal> m_World_OnExit;
        [SerializeField] protected UnityEvent<MAnimal> m_World_OnZoneActivation;
        [SerializeField] protected UnityEvent<MAnimal> m_World_OnZoneFailed;

        protected void Player_OnEnter(MAnimal target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnEnter.Invoke(target);
        }
        protected void Player_OnExit(MAnimal target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnExit.Invoke(target);
        }
        protected void Player_OnZoneActivation(MAnimal target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnZoneActivation.Invoke(target);
        }
        protected void Player_OnZoneFailed(MAnimal target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnZoneFailed.Invoke(target);
        }

        public void Net_OnEnter(MAnimal target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(0), NetEventSource.Zone);
        public void Net_OnExit(MAnimal target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(1), NetEventSource.Zone);
        public void Net_OnZoneActivation(MAnimal target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(2), NetEventSource.Zone);
        public void Net_OnZoneFailed(MAnimal target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(3), NetEventSource.Zone);

        protected override void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.Zone && index >= indexEncode(0) && index < indexEncode(10) ) return;

            switch(indexEncode(index, true))
            {
                case 0: m_World_OnEnter.Invoke((data as GameObject).GetComponent<MAnimal>()); break;
                case 1: m_World_OnExit.Invoke((data as GameObject).GetComponent<MAnimal>()); break;
                case 2: m_World_OnZoneActivation.Invoke((data as GameObject).GetComponent<MAnimal>()); break;
                case 3: m_World_OnZoneFailed.Invoke((data as GameObject).GetComponent<MAnimal>()); break;
            }
        }
    }
}
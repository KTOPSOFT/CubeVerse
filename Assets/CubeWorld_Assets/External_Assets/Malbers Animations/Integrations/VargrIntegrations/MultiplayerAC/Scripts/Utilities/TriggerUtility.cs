using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.VargrMultiplayer
{
    public class TriggerUtility : TriggerEventSyncUser
    {
        [SerializeField] protected TriggerProxy m_Trigger;

        protected override void Awake()
        {
            base.Awake();

            if(m_Trigger == null)
                m_Trigger = GetComponent<TriggerProxy>();

            m_Trigger.OnTrigger_Enter.AddListener(Player_OnTriggerEnter);
            m_Trigger.OnTrigger_Exit.AddListener(Player_OnTriggerExit);
            m_Trigger.OnTrigger_Stay.AddListener(Player_OnTriggerStay);
            m_Trigger.OnGameObjectEnter.AddListener(Player_OnGameObjectEnter);
            m_Trigger.OnGameObjectExit.AddListener(Player_OnGameObjectExit);
            m_Trigger.OnGameObjectStay.AddListener(Player_OnGameObjectStay);
        }

        [SerializeField] protected UnityEvent<Collider> m_Player_OnTriggerEnter;
        [SerializeField] protected UnityEvent<Collider> m_Player_OnTriggerExit;
        [SerializeField] protected UnityEvent<Collider> m_Player_OnTriggerStay;
        [SerializeField] protected UnityEvent<GameObject> m_Player_OnGameObjectEnter;
        [SerializeField] protected UnityEvent<GameObject> m_Player_OnGameObjectExit;
        [SerializeField] protected UnityEvent<GameObject> m_Player_OnGameObjectStay;

        [SerializeField] protected UnityEvent<Collider> m_World_OnTriggerEnter;
        [SerializeField] protected UnityEvent<Collider> m_World_OnTriggerExit;
        [SerializeField] protected UnityEvent<Collider> m_World_OnTriggerStay;
        [SerializeField] protected UnityEvent<GameObject> m_World_OnGameObjectEnter;
        [SerializeField] protected UnityEvent<GameObject> m_World_OnGameObjectExit;
        [SerializeField] protected UnityEvent<GameObject> m_World_OnGameObjectStay;
        [SerializeField] protected UnityEvent m_World_OnEmpty;

        protected void Player_OnTriggerEnter(Collider target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnTriggerEnter.Invoke(target);
        }
        protected void Player_OnTriggerExit(Collider target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player.isOwner && player is PlayerInstance) m_Player_OnTriggerExit.Invoke(target);
        }
        protected void Player_OnTriggerStay(Collider target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnTriggerStay.Invoke(target);
        }

        protected void Player_OnGameObjectEnter(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnGameObjectEnter.Invoke(target);
        }
        protected void Player_OnGameObjectExit(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnGameObjectExit.Invoke(target);
        }
        protected void Player_OnGameObjectStay(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.GetComponent<AnimalInstance>();
            if(player == null) return;
            if(player.isOwner && player is PlayerInstance) m_Player_OnGameObjectStay.Invoke(target);
        }
        

        public void Net_OnTriggerEnter(Collider target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(0), NetEventSource.TriggerUtility);
        public void Net_OnTriggerExit(Collider target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(1), NetEventSource.TriggerUtility);
        public void Net_OnTriggerStay(Collider target) => m_TriggerEventSync?.EventTrigger(target != null ? target.gameObject : null, indexEncode(2), NetEventSource.TriggerUtility);

        public void Net_OnGameObjectEnter(GameObject target) => m_TriggerEventSync?.EventTrigger(target, indexEncode(3), NetEventSource.TriggerUtility);
        public void Net_OnGameObjectExit(GameObject target) => m_TriggerEventSync?.EventTrigger(target, indexEncode(4), NetEventSource.TriggerUtility);
        public void Net_OnGameObjectStay(GameObject target) => m_TriggerEventSync?.EventTrigger(target, indexEncode(5), NetEventSource.TriggerUtility);
        public void Net_OnEmpty() => m_TriggerEventSync?.EventTrigger(true, indexEncode(6), NetEventSource.TriggerUtility);

        protected override void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.TriggerUtility && index >= indexEncode(0) && index < indexEncode(10) ) return;

            switch(indexEncode(index, true))
            {
                case 0: m_World_OnTriggerEnter.Invoke((data as GameObject).GetComponent<Collider>()); break;
                case 1: m_World_OnTriggerExit.Invoke((data as GameObject).GetComponent<Collider>()); break;
                case 2: m_World_OnTriggerStay.Invoke((data as GameObject).GetComponent<Collider>()); break;
                case 3: m_World_OnGameObjectEnter.Invoke((GameObject)data); break;
                case 4: m_World_OnGameObjectExit.Invoke((GameObject)data); break;
                case 5: m_World_OnGameObjectStay.Invoke((GameObject)data); break;
                case 6: m_World_OnEmpty.Invoke(); break;

            }
        }
    }
}
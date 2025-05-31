using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using UnityEngine;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class UnarmedUtility : NetworkBehaviour
    {    
        //protected EventSync m_eventSync;
        [SerializeField] private MDamager m_weapon;
        [SerializeField] private AnimalInstance m_netAnimal;
        [SerializeField] private TransformEvent m_PlayerOnHit;
        [SerializeField] private Vector3Event m_PlayerOnHitPosition;
        [SerializeField] private IntEvent m_PlayerOnHitInteractable;
        [SerializeField] private IntEvent m_PlayerOnProfileChange;
        [SerializeField] private BoolEvent m_PlayerOnAttackTrigger;
        protected bool isPlayerOwned => m_netAnimal != null ? (m_netAnimal is PlayerInstance player && player.isPlayerOwned) : false;
        
#if UNITY_EDITOR
    #if AC_FISHNET
        protected override void OnValidate() {
            base.OnValidate();
    #else
        protected void OnValidate() {
    #endif
            if(m_weapon == null) 
                m_weapon = GetComponent<MAttackTrigger>();

            if(m_netAnimal == null) 
                m_netAnimal = GetComponentInParent<AnimalInstance>();
        }
#endif

        protected virtual void Awake()
        {
            if(m_weapon == null)
                m_weapon = GetComponent<MDamager>();

            if(m_netAnimal == null)
                m_netAnimal = GetComponentInParent<AnimalInstance>();

            //m_eventSync = gameObject.FindComponent<EventSync>();

            m_weapon.OnHit.AddListener((data) => { if(isPlayerOwned) m_PlayerOnHit.Invoke(data); } );
            m_weapon.OnHitPosition.AddListener((data) => { if(isPlayerOwned) m_PlayerOnHitPosition.Invoke(data); } );
            m_weapon.OnHitInteractable.AddListener((data) => { if(isPlayerOwned) m_PlayerOnHitInteractable.Invoke(data); } );
            m_weapon.OnProfileChanged.AddListener((data) => { if(isPlayerOwned) m_PlayerOnProfileChange.Invoke(data); } );

            /*
            m_eventSync.NetEventSync += RecivedEventSync;
            */
            if(m_weapon is MAttackTrigger attackTrigger)
            {
                attackTrigger.OnAttackBegin.AddListener(() => OnAttackTrigger(true)); 
                attackTrigger.OnAttackEnd.AddListener(() => OnAttackTrigger(false)); 
            }
        }
        /*
        protected virtual void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.UnarmedUtility) return;
            if(isController) return;
            int l_index = index - (m_weapon.Index*10);
            if(l_index < 0) return;

            switch(l_index)
            {
                case 0: m_weapon.OnHit.Invoke((Transform)data); break;
                case 1: m_weapon.OnHitPosition.Invoke((Vector3)data); break;
                case 2: m_weapon.OnHitInteractable.Invoke((int)data); break;
                case 3: m_weapon.OnProfileChanged.Invoke((int)data); break;
            }
        }
        */

#region Invokes
#if AC_FISHNET
        public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
		public bool isServer => IsServerStarted;

        protected void OnAttackTrigger(bool data)
        {
            if(isPlayerOwned) m_PlayerOnAttackTrigger.Invoke(data);
            
            if(isController)
            {
                if(isServer){
                    SyncAttackRpc(data);
                }else{
                    CmdSyncAttackRpc(data);
                }
            } 
        }

		[ServerRpc]
        private void CmdSyncAttackRpc(bool state)
        {
            m_weapon.SetEnable(state);
            SyncAttackRpc(state);
        }

		[ObserversRpc(ExcludeOwner = true)]
        private void SyncAttackRpc(bool state)
        {
            if(isController || isServer) return;
            m_weapon.SetEnable(state);
        }
#elif AC_PURRNET
        protected void OnAttackTrigger(bool data)
        {
            if(isPlayerOwned) m_PlayerOnAttackTrigger.Invoke(data);
            
            if(isController) SyncAttackRpc(data);
        }

		[ObserversRpc(requireServer: false)]
        private void SyncAttackRpc(bool state)
        {
            if(isController || isServer) return;
            m_weapon.SetEnable(state);
        }
#endif
#endregion
    }
}
using UnityEngine;

using UnityEngine.Events;

#if AC_FISHNET
using FishNet.Object;
using FishNet.Connection;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public struct StatEvents
	{
		public int ID;
		public UnityEvent<int, float, bool> Event;

		public StatEvents(int id)
		{
			ID = id;
			Event = new UnityEvent<int, float, bool>();
		}
		public void SyncValues(float amount)
		{
			//Debug.Log("Stat "+ID+" Sync "+amount);
			Event.Invoke(ID, amount, false);
		}
		public void SyncValuesMax(float amount)
		{
			//Debug.Log("Stat "+ID+" Sync "+amount);
			Event.Invoke(ID, amount, true);
		}
	}
	
	public partial class StatsSync : NetworkBehaviour
    {
		public Stats Vitals;
		public MDamageable Damageable;
		protected StatEvents[] m_statSync;
        protected virtual void Awake()
		{
			if(Damageable == null) Damageable = GetComponent<MDamageable>();
			if(Vitals == null) Vitals = GetComponent<Stats>();
		}
#if AC_FISHNET
		public bool isController => IsController;
		public bool isServer => IsServerStarted;
		public override void OnStartNetwork()
		{
			base.OnStartNetwork();

			UpdateOwner();

			if(isServer){
				m_statSync = new StatEvents[Vitals.stats.Count];
				for (int i = 0; i < Vitals.stats.Count; i++)
				{
					m_statSync[i] = new StatEvents(Vitals.stats[i].ID.ID);
					m_statSync[i].Event.AddListener(StatSyncRpc);
					Vitals.stats[i].OnValueChange.AddListener(m_statSync[i].SyncValues);
					Vitals.stats[i].OnMaxValueChange.AddListener(m_statSync[i].SyncValuesMax);
				}

				AttributesSetup();

				Damageable.events.OnDamager.AddListener(ReturnDamage);
				Damageable.events.OnReceivingDamage.AddListener(SendDamageRpc);
				Damageable.events.OnCriticalDamage.AddListener(SendCritRpc);
			}else{
				CmdSpawnSyncRpc();
				
				foreach (Stat target in Vitals.stats)
				{
					target.Regenerate = false;
					target.Degenerate = false;
				}
			}
		}
#elif AC_PURRNET
		protected override void OnSpawned()
		{
			UpdateOwner();

			if(isServer){
				m_statSync = new StatEvents[Vitals.stats.Count];
				for (int i = 0; i < Vitals.stats.Count; i++)
				{
					m_statSync[i] = new StatEvents(Vitals.stats[i].ID.ID);
					m_statSync[i].Event.AddListener(StatSyncRpc);
					Vitals.stats[i].OnValueChange.AddListener(m_statSync[i].SyncValues);
					Vitals.stats[i].OnMaxValueChange.AddListener(m_statSync[i].SyncValuesMax);
				}

				AttributesSetup();

				Damageable.events.OnDamager.AddListener(ReturnDamage);
				Damageable.events.OnReceivingDamage.AddListener(SendDamageRpc);
				Damageable.events.OnCriticalDamage.AddListener(SendCritRpc);
			}else{
				CmdSpawnSyncRpc();
				
				foreach (Stat target in Vitals.stats)
				{
					target.Regenerate = false;
					target.Degenerate = false;
				}
			}
		}
#endif

		protected virtual void ReturnDamage(GameObject go)
		{

		}

		protected virtual void UpdateOwner()
		{
			if(!(Damageable is NetDamageable)){
				// Try and Block things when not using NetDamageable
				if(Damageable.reaction != null) Damageable.reaction.Active = isController;
				if(Damageable.damagerReaction != null) Damageable.damagerReaction.Active = !isController;
			}
		}
		public void StatDegenerate_On(StatID ID)
		{
			if(!isController) return;

			if(isServer){
					Vitals.Stat_Degenerate_On(ID);
			}else{
				CmdDegenerateRpc(ID.ID, true);
			}
		}
		public void StatDegenerate_Off(StatID ID)
		{
			if(!isController) return;

			if(isServer){
					Vitals.Stat_Degenerate_Off(ID);
			}else{
				CmdDegenerateRpc(ID.ID, false);
			}
		}
		partial void AttributesSpawnSync();
		partial void AttributesSetup();

#if AC_FISHNET
#region FISHNET RPC
		[ServerRpc(RequireOwnership = false)]
        public void CmdSpawnSyncRpc(NetworkConnection conn = null)
        {
            if(!isServer) return;

            for (int i = 0; i < Vitals.stats.Count; i++)
			{
				TargetStatSyncRpc(conn, Vitals.stats[i].ID.ID, Vitals.stats[i].Value, Vitals.stats[i].MaxValue);
			}
			AttributesSpawnSync();
        }

		[ObserversRpc]
		private void SendDamageRpc(float target)
		{
			// Make Sure we are not the Server
            if(isServer) return;
			Damageable.events.OnReceivingDamage.Invoke(target);
		}

        [ObserversRpc]
		private void SendCritRpc()
		{
			// Make Sure we are not the Server
            if(isServer) return;
			Damageable.events.OnCriticalDamage.Invoke();
		}
		[ServerRpc]
		private void CmdDegenerateRpc(int id, bool active)
		{
			if(active){
				Vitals.Stat_Degenerate_On(Vitals.Stat_Get(id).ID);
			}else{
				Vitals.Stat_Degenerate_Off(Vitals.Stat_Get(id).ID);
			}
		}
        [ObserversRpc]
		protected void StatSyncRpc(int id, float amount, bool max)
		{
			//Debug.Log("Stat "+id+" Sync "+amount);
			if(isServer) return;
			if(max){
				Vitals.Stat_Get(id).MaxValue = amount;
			}else{
				Vitals.Stat_Get(id).Value = amount;
			}
		}

		[TargetRpc]
		protected void TargetStatSyncRpc(NetworkConnection conn, int id, float current, float max)
		{
			if(isServer) return;
			Vitals.Stat_Get(id).MaxValue = max;
			Vitals.Stat_Get(id).Value = current;
		}
#endregion
#elif AC_PURRNET
#region PURNET RPC
		[ServerRpc(requireOwnership: false)]
        public void CmdSpawnSyncRpc(RPCInfo info = default)
        {
            if(!isServer) return;

            for (int i = 0; i < Vitals.stats.Count; i++)
			{
				TargetStatSyncRpc(info.sender, Vitals.stats[i].ID.ID, Vitals.stats[i].Value, Vitals.stats[i].MaxValue);
			}
			AttributesSpawnSync();
        }

		[ObserversRpc]
		private void SendDamageRpc(float target)
		{
			// Make Sure we are not the Server
            if(isServer) return;
			Damageable.events.OnReceivingDamage.Invoke(target);
		}

        [ObserversRpc]
		private void SendCritRpc()
		{
			// Make Sure we are not the Server
            if(isServer) return;
			Damageable.events.OnCriticalDamage.Invoke();
		}
		[ServerRpc]
		private void CmdDegenerateRpc(int id, bool active)
		{
			if(active){
				Vitals.Stat_Degenerate_On(Vitals.Stat_Get(id).ID);
			}else{
				Vitals.Stat_Degenerate_Off(Vitals.Stat_Get(id).ID);
			}
		}
        [ObserversRpc]
		protected void StatSyncRpc(int id, float amount, bool max)
		{
			//Debug.Log("Stat "+id+" Sync "+amount);
			if(isServer) return;
			if(max){
				Vitals.Stat_Get(id).MaxValue = amount;
			}else{
				Vitals.Stat_Get(id).Value = amount;
			}
		}

		[TargetRpc]
		protected void TargetStatSyncRpc(PlayerID target, int id, float current, float max)
		{
			if(isServer) return;
			Vitals.Stat_Get(id).MaxValue = max;
			Vitals.Stat_Get(id).Value = current;
		}
#endregion
#endif
	}
}
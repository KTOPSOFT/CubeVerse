#if AC_PURRNET
using UnityEngine;
using MalbersAnimations.IK;
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
#if AC_PURRNET
[RequireComponent(typeof(NetworkAnimator), typeof(NetworkTransform), typeof(EventSync))]
	public partial class AnimalInstance : NetworkBehaviour, IAimSyncer
    {
#region SyncVars
        protected readonly SyncVar<Vector3> m_aimerDirection = new SyncVar<Vector3>(Vector3.zero, ownerAuth: true);
		protected readonly SyncVar<float> m_attackCharge = new SyncVar<float>(0, ownerAuth: true);
		protected readonly SyncVar<int> m_activeWeapon = new SyncVar<int>(0, ownerAuth: true);

		private void OnAttackCharge(float next)
        {
            if(HasAuthority || WeaponManager == null) return;
			WeaponManager.WeaponChargeSync?.Invoke(next);
        }

		private void OnWeaponChange(int next)
        {
            if(HasAuthority || WeaponManager == null) return;
			WeaponManager.CheckActiveWeapon(false);
        }

		public Vector3 AimDirection
		{
			get
			{
				return m_aimerDirection.value;
			}
			set
			{
				if(m_aimerDirection.value == value) return;
				if(!isOwner) return;

				m_aimerDirection.value = value;
			}
		}

		public float AttackCharge
		{
			get
			{
				return m_attackCharge.value;
			}
			set
			{
				if(m_attackCharge.value == value) return;
				if(!isOwner) return;

				m_attackCharge.value = value;
			}
		}

		public int ActiveWeapon
		{
			get
			{
				return m_activeWeapon.value-1;
			}
			set
			{
				if(m_activeWeapon.value-1 == value) return;
				if(!isOwner) return;

				m_activeWeapon.value = value+1;
			}
		}
#endregion

#region Startups
		
		protected override void OnInitializeModules()
        {
            base.OnInitializeModules();
        }

		protected override void OnSpawned()
		{
			base.OnSpawned();
			SpawnedEvent?.Invoke(isServer);
			if(!isServer){
				Debug.Log("ANIMAL CLIENT START");
				CheckWeapons();
				CmdSpawnSyncRpc();
				HAPSpawnSync();
			}else{
				Debug.Log("ANIMAL Server START");
			}
			if(!HasAuthority){
				if(WeaponManager != null) WeaponManager.CheckActiveWeapon(true);
			}
		}

        protected override void OnDespawned()
        {
            base.OnDespawned();
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            base.OnOwnerChanged(oldOwner, newOwner, asServer);
			if(!asServer && isServer) return;
			Debug.Log("OWNERCHANGED: O:"+oldOwner+" - N: "+newOwner+" - C:"+owner +" - asServer: "+asServer);
            UpdateOwner(asServer);
        }

        protected override void OnOwnerReconnected(PlayerID ownerId)
        {
            base.OnOwnerReconnected(ownerId);
        }
#endregion

#region SpawnSync RPC
        [ServerRpc(requireOwnership:false)]
        public void CmdSpawnSyncRpc(RPCInfo info = default)
        {
            if(!isServer) return;
			if(m_IKManager != null)
			{
				foreach( IKSet item in m_IKManager.sets)
				{
					TargetIKSyncRpc(info.sender, item.name, item.active, item.Weight);
				}
			}
        }
#endregion

#region WeaponSet RPC
        protected void SetActiveHolster(int holster)
		{
			if(!HasAuthority) return;
			HolsterSetRpc(holster);
		}

        [ObserversRpc(requireServer:false)]
		private void HolsterSetRpc(int holster)
		{
			if(HasAuthority) return;
			if(WeaponManager == null) return;
			//Debug.Log(gameObject.name+" HolsterSetRpc");
			WeaponManager.SetActiveHolster?.Invoke(holster);
		}
#endregion

#region IK RPC
        protected void SyncIK(string target, bool state, float value)
		{
			if(!HasAuthority) return;
			IKSyncRpc(target, state, value);
		}
        [ObserversRpc(requireServer:false)]
        private void IKSyncRpc(string target, bool state, float value)
		{
			if(HasAuthority) return;
			
			var IK_Set = m_IKManager.FindSet(target);
			if(IK_Set == null) return;
			if(IK_Set.active != state) IK_Set.Enable(state);
			IK_Set.Weight = value;
		}

        [TargetRpc]
		protected void TargetIKSyncRpc(PlayerID conn, string ikTarget, bool state, float value)
		{
			if(isServer || HasAuthority) return;
			
			var IK_Set = m_IKManager.FindSet(ikTarget);
			if(IK_Set == null) return;
			IK_Set.active = state;
			IK_Set.Weight = value;
		}
#endregion

#region Projectile RPC
		public void SyncProjectile(Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed = 0)
		{
			if(!HasAuthority) return;

			uint tick = 0;
			if(InstanceHandler.NetworkManager != null) tick = InstanceHandler.NetworkManager.tickModule.syncedTick;

			FireProjectileRpc(tick, velocity, position, forward, charge, seed);
		}

        [ObserversRpc(requireServer:false, excludeOwner:true)]
		private void FireProjectileRpc (uint tick, Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed)
		{
			if(HasAuthority) return;
			if(WeaponManager == null) return;

			uint passedTime = 0;

			if(tick > 0){
				passedTime = InstanceHandler.NetworkManager.tickModule.syncedTick - tick;
			}

			if(WeaponManager.Weapon.GetComponent<RangedUtility>() == null) Debug.Log("WEAPON UTILITY NULL");

			WeaponManager.Weapon.GetComponent<RangedUtility>().FireProjectile(velocity, position, forward, charge, (int)passedTime, seed);
		}

#endregion

#region Animator
		protected void SetupAnimator()
		{
			if(m_netAnimator == null) m_netAnimator = GetComponent<NetworkAnimator>();
			Animal.SetTriggerParameter += SendAnimTrigger;
		}
		protected void SendAnimTrigger(int hash)
		{
			if(!isController) return;
			m_netAnimator.SetTrigger(hash);
		}
#endregion
#region OTHERS
		public bool HasAuthority => isController; 

		protected void NameAddID()
        {
            gameObject.name += "-"+objectId;
        }

		public void SetParent(Transform target)
        {
            // Need to make sure we are on the ROOT
            transform.parent = target;
        }

		protected void SetupWeaponManager()
		{
			if(WeaponManager == null) return;

			m_attackCharge.onChanged += OnAttackCharge;
			m_activeWeapon.onChanged += OnWeaponChange;
			WeaponManager.SetActiveHolster += SetActiveHolster;
		}
#endregion
    }
#endif
}
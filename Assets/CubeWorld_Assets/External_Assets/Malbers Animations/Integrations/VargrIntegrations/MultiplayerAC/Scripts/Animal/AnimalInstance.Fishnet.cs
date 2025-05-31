using UnityEngine;
using System;
using System.Collections.Generic;
using MalbersAnimations.IK;
using FishNet.Component.Transforming;


#if AC_FISHNET
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
#if AC_FISHNET
	[RequireComponent( typeof(NetworkAnimator), typeof(NetworkTransform), typeof(EventSync))]
	public partial class AnimalInstance : NetworkBehaviour, IAimSyncer
    {
#region SyncVars
		protected readonly SyncVar<Vector3> m_aimerDirection = new SyncVar<Vector3>(new SyncTypeSettings(ReadPermission.ExcludeOwner));
		[ServerRpc(RunLocally = true)] protected void CmdAimerDirectionRpc(Vector3 value) => m_aimerDirection.Value = value;

		protected readonly SyncVar<float> m_attackCharge = new SyncVar<float>(new SyncTypeSettings(ReadPermission.ExcludeOwner));
		[ServerRpc(RunLocally = true)] protected void CmdAttackChargeRpc(float value) => m_attackCharge.Value = value;

		protected readonly SyncVar<int> m_activeWeapon = new SyncVar<int>(new SyncTypeSettings(ReadPermission.ExcludeOwner));
		[ServerRpc(RunLocally = true)] protected void CmdActiveWeaponRpc(int value) => m_activeWeapon.Value = value+1;

		private void OnAttackCharge(float prev, float next, bool asServer)
        {
            if(isController || WeaponManager == null) return;
			WeaponManager.WeaponChargeSync?.Invoke(next);
        }

		private void OnWeaponChange(int prev, int next, bool asServer)
        {
            if(isController || WeaponManager == null) return;
			WeaponManager.CheckActiveWeapon(false);
        }

		public Vector3 AimDirection
		{
			get
			{
				return m_aimerDirection.Value;
			}
			set
			{
				if(m_aimerDirection.Value == value) return;

				if(IsServerInitialized && !Owner.IsValid){
					m_aimerDirection.Value = value;
				}else{
					if(isController) CmdAimerDirectionRpc(value);
				}
			}
		}

		public float AttackCharge
		{
			get
			{
				return m_attackCharge.Value;
			}
			set
			{
				if(m_attackCharge.Value == value) return;
				
				if(IsServerInitialized && !Owner.IsValid){
					m_attackCharge.Value = value;
				}else{
					if(isController) CmdAttackChargeRpc(value);
				}
			}
		}

		public int ActiveWeapon
		{
			get
			{
				return m_activeWeapon.Value-1;
			}
			set
			{
				if(m_activeWeapon.Value-1 == value) return;
				
				if(IsServerInitialized && !Owner.IsValid){
					m_activeWeapon.Value = value+1;
				}else{
					if(isController) CmdActiveWeaponRpc(value);
				}
			}
		}
#endregion

#region Startups
		public override void OnStartServer()
		{
			base.OnStartNetwork();
			if(IsHostStarted) return;
			// This is here in case we need the server to do something with on spawn while there is no client.
			OnSpawned();
		}

        public override void OnStartClient()
        {
            base.OnStartClient();
			OnSpawned();
        }

        protected virtual void OnSpawned()
		{
			SpawnedEvent?.Invoke(isServer);
			if(!isServer){
				//Debug.Log("ANIMAL CLIENT START");
				CheckWeapons();
				CmdSpawnSyncRpc();
				HAPSpawnSync();
			}else{
				//Debug.Log("ANIMAL Server START");
			}
			if(!isController){
				Debug.Log("HasAuth Works in OnSpawned");
				if(WeaponManager != null) WeaponManager.CheckActiveWeapon(true); // look for a new home for this>?
			}
		}

		public override void OnOwnershipServer(NetworkConnection prevOwner)
		{
			if(isClient) return;

			if(Owner.IsLocalClient) return;

            UpdateOwner(true);

			Animal.RB.isKinematic = !isController; // Added to Fix issue currently with Fishnet not changing this as needed.
		}

		public override void OnOwnershipClient(NetworkConnection prevOwner)
		{
            if(isServer){
				if(Owner.IsLocalClient){
					UpdateOwner(true);
				}
				
				return;
			}
            UpdateOwner(false);

			Animal.RB.isKinematic = !isController; // Added to Fix issue currently with Fishnet not changing this as needed.
		}
#endregion

#region SpawnSync RPC
		[ServerRpc(RequireOwnership = false)]
        public void CmdSpawnSyncRpc(NetworkConnection conn = null)
        {
            if(!isServer || conn == null) return;
			if(m_IKManager != null)
			{
				foreach( IKSet item in m_IKManager.sets)
				{
					TargetIKSyncRpc(conn, item.name, item.active, item.Weight);
				}
			}
        }
#endregion

#region WeaponSet RPC
        protected void SetActiveHolster(int holster)
		{
			if(!isController) return;
			if(isServer){
				HolsterSetRpc(holster);
			}else{
				CmdHolsterSetRpc(holster);
			}
		}

		[ServerRpc]
		private void CmdHolsterSetRpc(int holster)
		{
			if(WeaponManager == null) return;
			HolsterSetRpc(holster);
			//Debug.Log(gameObject.name+" CMDHolsterSetRpc");
			WeaponManager.SetActiveHolster?.Invoke(holster);
		}

        [ObserversRpc(ExcludeOwner = true)]
		private void HolsterSetRpc(int holster)
		{
			if(isServer || isController) return;
			if(WeaponManager == null) return;
			//Debug.Log(gameObject.name+" HolsterSetRpc");
			WeaponManager.SetActiveHolster?.Invoke(holster);
		}
#endregion

#region IK RPC
		protected void SyncIK(string target, bool state, float value)
		{
			if(!isController) return;
			if(isServer){
				IKSyncRpc(target, state, value);
			}else{
				CmdIKSyncRpc(target, state, value);
			}
		}

		[ServerRpc]
		private void CmdIKSyncRpc(string target, bool state, float value)
		{
			IKSyncRpc(target, state, value);

			var IK_Set = m_IKManager.FindSet(target);
			if(IK_Set == null) return;
			if(IK_Set.active != state) IK_Set.Enable(state);
			IK_Set.Weight = value;
		}

        [ObserversRpc(ExcludeOwner = true)]
		private void IKSyncRpc(string target, bool state, float value)
		{
			if(isServer || isController) return;
			
			var IK_Set = m_IKManager.FindSet(target);
			if(IK_Set == null) return;
			if(IK_Set.active != state) IK_Set.Enable(state);
			IK_Set.Weight = value;
		}

		[TargetRpc]
		protected void TargetIKSyncRpc(NetworkConnection conn, string target, bool state, float value)
		{
			if(isServer || isController) return;
			
			var IK_Set = m_IKManager.FindSet(target);
			if(IK_Set == null) return;
			IK_Set.active = state;
			IK_Set.Weight = value;
		}

#endregion

#region Projectile RPC
		public void SyncProjectile(Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed = 0)
		{
			if(!isController) return;

			if(isServer){
				FireProjectileRpc(base.TimeManager.Tick, velocity, position, forward, charge, seed);
			}else{
				CmdFireProjectileRpc(base.TimeManager.Tick, velocity, position, forward, charge, seed);
			}
		}

        [ServerRpc]
		private void CmdFireProjectileRpc (uint tick, Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed)
		{
			if(WeaponManager == null) return;
			FireProjectileRpc(tick, velocity, position, forward, charge, seed);
			uint stepsMissed = base.TimeManager.Tick - tick;
			WeaponManager.Weapon.GetComponent<RangedUtility>().FireProjectile(velocity, position, forward, charge, (int)stepsMissed, seed);
			
		}

        [ObserversRpc(ExcludeOwner = true)]
		private void FireProjectileRpc (uint tick, Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed)
		{
			if(WeaponManager == null) return;
			if(isServer) return;
			uint stepsMissed = base.TimeManager.Tick - tick;
			if(WeaponManager.Weapon.GetComponent<RangedUtility>() == null) Debug.Log("WEAPON UTILITY NULL");

			WeaponManager.Weapon.GetComponent<RangedUtility>().FireProjectile(velocity, position, forward, charge, (int)stepsMissed, seed);
		}

#endregion

#region Animator
		public enum ParamType {FLOAT, INT, BOOL};

		[Serializable]
		public struct ParamCheck
		{
			public string Parameter;
			public int Hash;
			public ParamType ParameterType;

			public ParamCheck(string parameterName, ParamType parameterType, int hash = -1)
			{
				Parameter = parameterName;
				Hash = hash;
				ParameterType = parameterType;
			}
		}

		private int hash_mode, hash_state;
		
		[SerializeField] protected ParamCheck[] m_OnMode_Parameters = new ParamCheck[] { new ParamCheck("Mode", ParamType.INT) , new ParamCheck("ModeStatus", ParamType.INT), new ParamCheck("ModePower", ParamType.FLOAT)};
		[SerializeField] protected ParamCheck[] m_OnState_Parameters = new ParamCheck[] { new ParamCheck("State", ParamType.INT), new ParamCheck("LastState", ParamType.INT), new ParamCheck("StateEnterStatus", ParamType.INT), new ParamCheck("StateExitStatus", ParamType.INT), new ParamCheck("StateProfile", ParamType.INT), new ParamCheck("StateFloat", ParamType.FLOAT)};
		
		protected void SetupAnimator()
		{
			List<ParamCheck> modeCheck = new List<ParamCheck>();
			List<ParamCheck> stateCheck = new List<ParamCheck>();

			hash_mode = Animator.StringToHash("ModeOn");
			hash_state = Animator.StringToHash("StateOn");

			foreach(ParamCheck item in m_OnMode_Parameters) if(Animal.Anim.ContainsParam(item.Parameter)) modeCheck.Add(new ParamCheck(item.Parameter, item.ParameterType, Animator.StringToHash(item.Parameter)));

			foreach(ParamCheck item in m_OnState_Parameters) if(Animal.Anim.ContainsParam(item.Parameter)) stateCheck.Add(new ParamCheck(item.Parameter, item.ParameterType, Animator.StringToHash(item.Parameter)));

			m_OnMode_Parameters = modeCheck.ToArray();
			m_OnState_Parameters = stateCheck.ToArray();

			Animal.SetFloatParameter += SendAnimParameter;
			Animal.SetIntParameter += SendAnimParameter;
			Animal.SetBoolParameter += SendAnimParameter;
			Animal.SetTriggerParameter += SendAnimParameter;
		}

#region Trigger RPC
		protected virtual void SendAnimTrigger(int hash)
		{
			if(!isController) return;

			if(hash != hash_mode && hash != hash_state) return;

			ParamCheck[] targetSet = hash == hash_mode ? m_OnMode_Parameters : m_OnState_Parameters;

			float[] dataset = new float[targetSet.Length];

			for (int i = 0; i < dataset.Length; i++)
			{
				switch(targetSet[i].ParameterType)
				{
					case ParamType.FLOAT:
						dataset[i] = Animal.Anim.GetFloat(targetSet[i].Hash);
						break;
					case ParamType.INT:
						dataset[i] = Animal.Anim.GetInteger(targetSet[i].Hash);
						break;
					case ParamType.BOOL:
						dataset[i] = Animal.Anim.GetBool(targetSet[i].Hash) ? 1 : 0;
						break;
				}
			}
			if(isServer){
				OnTriggerRpc(hash, dataset);
			}else{
				CmdTriggerRpc(hash, dataset);
			}
		}

		[ServerRpc]
		protected virtual void CmdTriggerRpc(int hash, float[] dataset){
			if(!isServer) return;
			if(hash != hash_mode && hash != hash_state) return;
			OnTriggerRpc(hash, dataset);
			if(isController) return;
			ParamCheck[] targetSet = hash == hash_mode ? m_OnMode_Parameters : m_OnState_Parameters;
			for (int i = 0; i < dataset.Length; i++)
			{
				switch(targetSet[i].ParameterType)
				{
					case ParamType.FLOAT:
						Animal.Anim.SetFloat(targetSet[i].Hash, dataset[i]);
						break;
					case ParamType.INT:
						Animal.Anim.SetInteger(targetSet[i].Hash, (int)dataset[i]);
						break;
					case ParamType.BOOL:
						Animal.Anim.SetBool(targetSet[i].Hash, dataset[i] == 1 ? true : false);
						break;
				}
			}
			Animal.Anim.SetTrigger(hash);
		}

        [ObserversRpc(ExcludeOwner = true)]
		protected virtual void OnTriggerRpc(int hash, float[] dataset){
			if(isServer || isController) return;
			if(hash != hash_mode && hash != hash_state) return;
			ParamCheck[] targetSet = hash == hash_mode ? m_OnMode_Parameters : m_OnState_Parameters;
			for (int i = 0; i < dataset.Length; i++)
			{
				switch(targetSet[i].ParameterType)
				{
					case ParamType.FLOAT:
						Animal.Anim.SetFloat(targetSet[i].Hash, dataset[i]);
						break;
					case ParamType.INT:
						Animal.Anim.SetInteger(targetSet[i].Hash, (int)dataset[i]);
						break;
					case ParamType.BOOL:
						Animal.Anim.SetBool(targetSet[i].Hash, dataset[i] == 1 ? true : false);
						break;
				}
			}
			Animal.Anim.SetTrigger(hash);
		}
#endregion	

#region Manual AnimSync RPC
		protected virtual void SendAnimParameter(int hash, float value)
		{
			if(!isController) return;
			if(m_netAnimator != null && m_netAnimator.enabled) return;
			if(isServer){
				AnimFloatRpc(hash, value);
			}else{
				CmdAnimFloatRpc(hash, value);
			}
		}
		protected virtual void SendAnimParameter(int hash, int value)
		{
			if(!isController) return;
			if(m_netAnimator != null && m_netAnimator.enabled) return;
			if(isServer){
				AnimIntRpc(hash, value);
			}else{
				CmdAnimIntRpc(hash, value);
			}
		}
		protected virtual void SendAnimParameter(int hash, bool value)
		{
			if(!isController) return;
			if(m_netAnimator != null && m_netAnimator.enabled) return;
			if(isServer){
				AnimBoolRpc(hash, value);
			}else{
				CmdAnimBoolRpc(hash, value);
			}
		}
		protected virtual void SendAnimParameter(int hash)
		{
			if(!isController) return;

			if(m_netAnimator != null && m_netAnimator.enabled){
				SendAnimTrigger(hash);
				return;
			}

			if(isServer){
				AnimTriggerRpc(hash);
			}else{
				CmdAnimTriggerRpc(hash);
			}
		}

		[ServerRpc]
		protected void CmdAnimTriggerRpc(int hash)
		{
			if(!isServer) return;
			AnimTriggerRpc(hash);
			if(isController) return;
			Animal.Anim.SetTrigger(hash);
		}

        [ObserversRpc(ExcludeOwner = true)]
		protected void AnimTriggerRpc(int hash)
		{
			if(isServer) return;
			Animal.Anim.SetTrigger(hash);
		}

		[ServerRpc]
		protected void CmdAnimFloatRpc(int hash, float value)
		{
			if(!isServer) return;
			AnimFloatRpc(hash, value);
			if(isController) return;
			Animal.Anim.SetFloat(hash, value);
		}

        [ObserversRpc(ExcludeOwner = true)]
		protected void AnimFloatRpc(int hash, float value)
		{
			if(isServer) return;
			Animal.Anim.SetFloat(hash, value);
		}

		[ServerRpc]
		protected void CmdAnimIntRpc(int hash, int value)
		{
			if(!isServer) return;
			AnimIntRpc(hash, value);
			if(isController) return;
			Animal.Anim.SetInteger(hash, value);
		}

        [ObserversRpc(ExcludeOwner = true)]
		protected void AnimIntRpc(int hash, int value)
		{
			if(isServer) return;
			Animal.Anim.SetInteger(hash, value);
		}

		[ServerRpc]
		protected void CmdAnimBoolRpc(int hash, bool value)
		{
			if(!isServer) return;
			AnimBoolRpc(hash, value);
			if(isController) return;
			Animal.Anim.SetBool(hash, value);
		}

        [ObserversRpc(ExcludeOwner = true)]
		protected void AnimBoolRpc(int hash, bool value)
		{
			if(isServer) return;
			Animal.Anim.SetBool(hash, value);
		}
#endregion
#endregion

#region OTHERS
		public bool isServer => IsServerStarted;
		public bool isServerOnly => IsServerOnlyStarted;
		public bool isClient => IsClientOnlyStarted;
		public bool isHost => IsHostStarted;
		public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
		public bool isOwner => IsOwner;
		public NetworkConnection owner => Owner;

		protected void SetupWeaponManager()
		{
			if(WeaponManager == null) return;

			m_attackCharge.OnChange += OnAttackCharge;
			m_activeWeapon.OnChange += OnWeaponChange;
			WeaponManager.SetActiveHolster += SetActiveHolster;
		}

		protected void NameAddID()
        {
            gameObject.name += "-"+ObjectId;
        }

		public void SetParent(Transform target)
        {
            if(target == null){
                NetworkObject.UnsetParent();
                return;
            }
            NetworkObject.SetParent(target.GetComponent<NetworkBehaviour>());
        }
#endregion
    }
#endif

}
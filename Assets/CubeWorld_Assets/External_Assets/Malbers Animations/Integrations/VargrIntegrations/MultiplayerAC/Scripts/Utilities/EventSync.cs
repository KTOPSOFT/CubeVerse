using VargrIntegrations;
using UnityEngine;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public partial class EventSync : NetworkBehaviour
    {
		public event System.Action<object, int, NetEventSource> NetEventSync;
#if AC_FISHNET
#region FISHNET FUNCTIONS

		public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
		public bool isServer => IsServerStarted;

		#region BOOL
		public void EventBroadcast(bool data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(bool data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(bool data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region GAMEOBJECT

		public void EventBroadcast(GameObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(GameObject data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}

		[ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(GameObject data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region INT
		public void EventBroadcast(int data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(int data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(int data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region UINT
		public void EventBroadcast(uint data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(uint data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(uint data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region FLOAT
		public void EventBroadcast(float data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(float data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(float data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR3
		public void EventBroadcast(Vector3 data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(Vector3 data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(Vector3 data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR2
		public void EventBroadcast(Vector2 data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(Vector2 data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(Vector2 data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region STRING
		public void EventBroadcast(string data, int index, NetEventSource target)
		{
			if(!isController) return;
			CmdEventBroadcastRpc(data, index, target);
		}

        [ServerRpc]
		private void CmdEventBroadcastRpc(string data, int index, NetEventSource target)
		{
			if(index >= 0) EventBroadcastRpc(data, index, target);
			NetEventSync?.Invoke(data, index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventBroadcastRpc(string data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region NSO
		public void EventBroadcast(ScriptableObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			int key = RSOManager.GetKey(data);
			if(key == 0) return;
			CmdEventNSORpc(key, index, target);
		}

        [ServerRpc]
		private void CmdEventNSORpc(int data, int index, NetEventSource target)
		{
			EventNSORpc(data, index, target);
			NetEventSync?.Invoke(RSOManager.Get(data), index, target);
		}


        [ObserversRpc(ExcludeOwner = true)]
		private void EventNSORpc(int data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventSync?.Invoke(RSOManager.Get(data), index, target);
		}
		#endregion
#endregion


#elif AC_PURRNET
#region PURRNET FUNCTTIONS


		#region BOOL
		public void EventBroadcast(bool data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(bool data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region GAMEOBJECT

		public void EventBroadcast(GameObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

		[ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(GameObject data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region INT
		public void EventBroadcast(int data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(int data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region UINT
		public void EventBroadcast(uint data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(uint data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region FLOAT
		public void EventBroadcast(float data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(float data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR3
		public void EventBroadcast(Vector3 data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}


        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(Vector3 data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR2
		public void EventBroadcast(Vector2 data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(Vector2 data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region STRING
		public void EventBroadcast(string data, int index, NetEventSource target)
		{
			if(!isController) return;
			EventBroadcastRpc(data, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventBroadcastRpc(string data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(data, index, target);
		}
		#endregion
		#region NSO
		public void EventBroadcast(ScriptableObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			int key = RSOManager.GetKey(data);
			if(key == 0) return;
			EventNSORpc(key, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventNSORpc(int data, int index, NetEventSource target)
		{
			if((isServer && index < 0 )|| isController) return;
			NetEventSync?.Invoke(RSOManager.Get(data), index, target);
		}
		#endregion
#endregion
#endif
    }
	public enum NetEventSource
	{
		Animal,
		Rider,
		Mount,
		WeaponManager,
		UnarmedUtility,
		ActiveWeapon,
		Damageable,
		Pickable,
		PickUp,
		Zone,
		ZoneTrigger,
		TriggerUtility,
		EffectSync,
		Aimer,
		GenericEventUtility,
		GenericTriggerUtility,
		Custom
	}
}

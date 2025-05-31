using VargrIntegrations;
using UnityEngine;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public partial class TriggerEventSync : EventSync
    {
        public event System.Action<object, int, NetEventSource> NetEventTrigger;
#if AC_FISHNET
#region FISHNET
        #region BOOL
		public void EventTrigger(bool data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        
		[ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(bool data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(bool data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region GAMEOBJECT
        public void EventTrigger(GameObject data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);

        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(GameObject data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

		[ObserversRpc]
		private void EventTriggerRpc(GameObject data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region INT
		public void EventTrigger(int data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(int data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(int data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region UINT
		public void EventTrigger(uint data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(uint data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(uint data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region FLOAT
		public void EventTrigger(float data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(float data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(float data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR3
		public void EventTrigger(Vector3 data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(Vector3 data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(Vector3 data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR2
		public void EventTrigger(Vector2 data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(Vector2 data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(Vector2 data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region STRING
		public void EventTrigger(string data, int index, NetEventSource target) => CmdEventTriggerRpc(data, index, target);
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerRpc(string data, int index, NetEventSource target)
		{
			EventTriggerRpc(data, index, target);
			NetEventTrigger?.Invoke(data, index, target);
		}

        [ObserversRpc]
		private void EventTriggerRpc(string data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region NSO
		public void EventTrigger(ScriptableObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			int key = RSOManager.GetKey(data);
			if(key == 0) return;
			CmdEventTriggerNSORpc(key, index, target);
		}
        [ServerRpc(RequireOwnership = false)]
		private void CmdEventTriggerNSORpc(int data, int index, NetEventSource target)
		{
			EventTriggerNSORpc(data, index, target);
			NetEventTrigger?.Invoke(RSOManager.Get(data), index, target);
		}
        [ObserversRpc]
		private void EventTriggerNSORpc(int data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(RSOManager.Get(data), index, target);
		}
		#endregion
#endregion
#elif AC_PURRNET
#region PURRNET
#region BOOL
		public void EventTrigger(bool data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(bool data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region GAMEOBJECT
        public void EventTrigger(GameObject data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

		[ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(GameObject data, int index, NetEventSource target)
		{
			if(isServer || isController) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region INT
		public void EventTrigger(int data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(int data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region UINT
		public void EventTrigger(uint data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(uint data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region FLOAT
		public void EventTrigger(float data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(float data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR3
		public void EventTrigger(Vector3 data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(Vector3 data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region VECTOR2
		public void EventTrigger(Vector2 data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(Vector2 data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region STRING
		public void EventTrigger(string data, int index, NetEventSource target) => EventTriggerRpc(data, index, target);

        [ObserversRpc(requireServer:false)]
		private void EventTriggerRpc(string data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(data, index, target);
		}
		#endregion
		#region NSO
		public void EventTrigger(ScriptableObject data, int index, NetEventSource target)
		{
			if(!isController) return;
			int key = RSOManager.GetKey(data);
			if(key == 0) return;
			EventTriggerNSORpc(key, index, target);
		}

        [ObserversRpc(requireServer:false)]
		private void EventTriggerNSORpc(int data, int index, NetEventSource target)
		{
			if(isServer) return;
			NetEventTrigger?.Invoke(RSOManager.Get(data), index, target);
		}
		#endregion
#endregion
#endif
    }
}

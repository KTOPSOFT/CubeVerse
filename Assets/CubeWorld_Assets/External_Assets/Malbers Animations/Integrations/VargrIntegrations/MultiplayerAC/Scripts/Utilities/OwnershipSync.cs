using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;


#if AC_FISHNET
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class OwnershipSync : EventSync
    {
        [SerializeField] protected bool m_ReturnOnDisconnect = true;
        [SerializeField] protected bool m_UnParentDisconnect = true;
        public GameObjectEvent OnOwnerChange;
        public UnityEvent OnOtherOwner;
        public UnityEvent PreReturnServer;
        public UnityEvent OnReturnServer;
        public bool ParentTransform;
        public bool TryOwning(Transform requester) => TryOwning(requester.gameObject);
        public bool TryOwning(MAnimal requester) => TryOwning(requester.gameObject);

// Need to Test if this is procing when join late and someone owns it.

#if AC_FISHNET
#region FISHNET
		public bool isServerOnly => IsServerOnlyStarted;
		public bool isClient => IsClientOnlyStarted;
		public bool isHost => IsHostStarted;
        public bool isOwner => IsOwner;
        public bool hasOwner => Owner.IsValid;

		public NetworkConnection owner => Owner;
        protected readonly SyncVar<GameObject> m_ownerObj = new SyncVar<GameObject>();
        public GameObject ownerObject { get { return (m_ownerObj.Value != null) ? m_ownerObj.Value.gameObject : null; } }

        protected virtual void Awake()
        {
            m_ownerObj.OnChange += (prev, next, asServer) => OwnerChange(next);
        }

        public override void OnStartClient()
        {
            if(ownerObject != null && !isController) OnOtherOwner?.Invoke();
        }

        public void ServerGiveOwnership(GameObject ownerObj, NetworkConnection conn){
            if(!isServer || ownerObject != null) return;
            GiveOwnership(conn);
            m_ownerObj.Value = ownerObj;
        }

        [ServerRpc(RequireOwnership = false)]
        protected void CmdTakeOwnershipRpc(GameObject ownerObj, NetworkConnection conn = null)
        {
            // on the server make sure the box is not in use by someone else, then callback to players.
            if(ownerObject != null) return;
            GiveOwnership(conn);
            m_ownerObj.Value = ownerObj;
        }

        [ServerRpc(RequireOwnership = false)]
        protected void CmdReleaseOwnershipRpc(NetworkConnection conn = null)
        {
            if (conn != this.Owner) return;
            // if they're the one interacting, then they are in control and can exit, closing the box. 
            RemoveOwnership();
            NetworkObject.UnsetParent();
            m_ownerObj.Value = null;
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
#elif AC_PURRNET
#region PURRNET
        protected SyncVar<GameObject> m_ownerObj = new SyncVar<GameObject>(null);
        public GameObject ownerObject { get { return (m_ownerObj.value != null) ? m_ownerObj.value : null; } }
        
        protected virtual void Awake()
        {
            m_ownerObj.onChanged += OwnerChange;
            
            if(ownerObject != null && !isController) OnOtherOwner?.Invoke();
        }

        public void ServerGiveOwnership(GameObject ownerObj, PlayerID conn){
            if(ownerObject != null || !isServer) return;
            GiveOwnership(conn);
            m_ownerObj.value = ownerObj;
        }

        [ServerRpc(requireOwnership:false)]
        protected void CmdTakeOwnershipRpc(GameObject target, RPCInfo info = default)
        {
            // on the server make sure the box is not in use by someone else, then callback to players.
            if(ownerObject != null || !isServer) return;
            GiveOwnership(info.sender);
            m_ownerObj.value = target;
        }

        [ServerRpc(requireOwnership:false)]
        protected void CmdReleaseOwnershipRpc(RPCInfo info = default)
        {
            if(info.sender != owner || !isServer) return;
            // if they're the one interacting, then they are in control and can exit, closing the box. 
            RemoveOwnership();
            //if(ParentTransform) transform.parent = null;
            m_ownerObj.value = null;
        }

        public void SetParent(Transform target)
        {
            // Need to make sure we are on the ROOT
            transform.parent = target;
        }
#endregion
#endif
#region COMMON
        public bool TryOwning(GameObject requester)
        {
            if(ownerObject != null) return ownerObject == requester.gameObject;
            CmdTakeOwnershipRpc(requester);
            return true;
        }

        public bool ReturnOwnership()
        {
            if(isServer && !hasOwner) return false;
            if(ownerObject == null) return false;
            if(!isOwner) return false;
            PreReturnServer?.Invoke();
            CmdReleaseOwnershipRpc();
            return true;
        }

        protected virtual void OwnerChange(GameObject next)
        {
            OnOwnerChange?.Invoke(next);
            if(next == null){
                OnReturnServer?.Invoke();
            }else{
                if(!isOwner) OnOtherOwner?.Invoke();
            }
        }
#endregion
    }
}

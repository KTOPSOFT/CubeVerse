using UnityEngine;
using UnityEngine.Events;

#if AC_FISHNET
using FishNet.Connection;
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif
namespace MalbersAnimations.VargrMultiplayer
{
	public partial class AnimalInstance : NetworkBehaviour, IAimSyncer
    {
        public UnityEvent<GameObject> HAPObjectChanged = new();
        [SerializeField]
        protected NetRider m_Rider;
        public NetRider Rider { get{ return m_Rider; } private set{ m_Rider = value; } }
#if AC_FISHNET
        public void UpdateStoredMount(GameObject target)
        {
            if(!isController) return;
            CmdStoredMountUpdateRpc(target);
        }

        public void MonturaSync(GameObject target, int side)
        {
            if(!isController) return;
            CmdMonturaSyncRpc(target, side);
        }

        [ServerRpc]
        protected void CmdSpawnMountRpc(bool onstart)
        {
            if(!isServer || Rider == null || Rider.m_MountStored.Value == null) return;
            GameObject InsMount;
            if(onstart){
                InsMount = Instantiate(Rider.m_MountStored.Value, transform.position - transform.forward, Quaternion.identity);
            }else{
                Vector3 InstantiatePosition = Rider.SpawnRaycast();
                InsMount = Instantiate(Rider.m_MountStored.Value, InstantiatePosition, Quaternion.identity);
            }
            Spawn(InsMount, owner);
            TargetSpawnedMountRpc(owner, InsMount, onstart);
        }

        [ServerRpc]
        protected void CmdStoredMountUpdateRpc(GameObject target)
        {
            if(!isServer || Rider == null) return;
            if(!isController) Rider.Set_StoredMount(target);
            StoredMountUpdateRpc(target);
        }

        [ObserversRpc]
        protected void StoredMountUpdateRpc(GameObject target)
        {
            if(isController || isServer || Rider == null) return;
            Rider.Set_StoredMount(target);
        }

        [TargetRpc]
        protected void TargetSpawnedMountRpc(NetworkConnection conn, GameObject target, bool onstart)
        {
            if(Rider == null) return;
            Rider.Set_StoredMount(target);
            if(onstart){
                Rider.Start_Mounted();
            }else{
                Rider.CallAnimal(Rider.ToggleCall);
            }
        }
        [ServerRpc(RequireOwnership = false)]
        protected void CmdHAPSpawnSyncRpc(NetworkConnection conn = null)
        {
            if(!IsServerStarted || conn == null) return;

            if(this is MountInstance mountInst){
                TargetRiderSyncRpc(conn, mountInst.Mount.Rider != null ? mountInst.Mount.Rider.RiderAnimal.gameObject : null, mountInst.Mount.Mounted);
            }else{
                TargetRiderSyncRpc(conn, Rider.MountStored != null ? Rider.MountStored.Animal.gameObject : null, Rider.Mounted);
            }
        }
        [TargetRpc]
        protected void TargetRiderSyncRpc(NetworkConnection conn, GameObject target, bool mounted)
        {
            if(isServer) return;

            if(this is MountInstance mountInst){
                mountInst.Mount.JoinSync(target, mounted);
            }else{
                Rider.JoinSync(target, mounted);
            }
        }
        [ServerRpc]
        public void CmdMonturaSyncRpc(GameObject target, int side)
        {
            if(!IsServerStarted || Rider == null) return;
            Rider.Montura = target != null ? target.FindComponent<NetMount>() : null;
            if(Rider.Montura != null) Rider.MountTrigger = Rider.Montura.MountTriggers.Find(x => x.MountID == side);
            MonturaSyncRpc(target, side);
        }
        
        [ObserversRpc]
        protected void MonturaSyncRpc(GameObject target, int side)
        {
            if(isController || isServer) return;
            Rider.Montura = target != null ? target.FindComponent<NetMount>() : null;
            if(Rider.Montura != null) Rider.MountTrigger = Rider.Montura.MountTriggers.Find(x => x.MountID == side);
        }
#elif AC_PURRNET
        [ServerRpc]
        protected void CmdSpawnMountRpc(bool onstart, RPCInfo info = default)
        {
            if(!isServer || Rider == null || Rider.m_MountStored.Value == null) return;
            GameObject InsMount;
            if(onstart){
                InsMount = Instantiate(Rider.m_MountStored.Value, transform.position - transform.forward, Quaternion.identity);
            }else{
                Vector3 InstantiatePosition = Rider.SpawnRaycast();
                InsMount = Instantiate(Rider.m_MountStored.Value, InstantiatePosition, Quaternion.identity);
            }
            InsMount?.GetComponent<AnimalInstance>().GiveOwnership(owner);
            TargetSpawnedMountRpc(info.sender, InsMount, onstart);
        }

        public void UpdateStoredMount(GameObject target)
        {
            if(!isController) return;
            StoredMountUpdateRpc(target);
        }

        [ObserversRpc(requireServer:false)]
        protected void StoredMountUpdateRpc(GameObject target)
        {
            if(isController || Rider == null) return;
            Rider.Set_StoredMount(target);
        }

        [TargetRpc]
        protected void TargetSpawnedMountRpc(PlayerID conn, GameObject target, bool onstart)
        {
            if(Rider == null) return;
            Rider.Set_StoredMount(target);
            if(onstart){
                Rider.Start_Mounted();
            }else{
                Rider.CallAnimal(Rider.ToggleCall);
            }
        }
        
        [ServerRpc(requireOwnership:false)]
        protected void CmdHAPSpawnSyncRpc(RPCInfo info = default)
        {
            if(!isServer) return;
            if(this is MountInstance mountInst){
                TargetRiderSyncRpc(info.sender, mountInst.Mount.Rider != null ? mountInst.Mount.Rider.RiderAnimal.gameObject : null, mountInst.Mount.Mounted);
            }else{
                TargetRiderSyncRpc(info.sender, Rider.MountStored != null ? Rider.MountStored.Animal.gameObject : null, Rider.Mounted);
            }
        }
        [TargetRpc]
        protected void TargetRiderSyncRpc(PlayerID conn, GameObject target, bool mounted)
        {
            if(isServer) return;

            if(this is MountInstance mountInst){
                mountInst.Mount.JoinSync(target, mounted);
            }else{
                Rider.JoinSync(target, mounted);
            }
        }
        public void MonturaSync(GameObject target, int side)
        {
            if(!isController) return;
            MonturaSyncRpc(target, side);
        }
        
        [ServerRpc(requireOwnership:false)]
        protected void MonturaSyncRpc(GameObject target, int side)
        {
            if(isController || isServer) return;
            Rider.Montura = target != null ? target.FindComponent<NetMount>() : null;
            if(Rider.Montura != null) Rider.MountTrigger = Rider.Montura.MountTriggers.Find(x => x.MountID == side);
        }
#elif AC_UNITYNGO

#endif
        public void SpawnMount(bool onstart = false)
        {
            if(!isServer && !isController) return;
            CmdSpawnMountRpc(onstart);
        }

        partial void SetupHAP(){
            if(!(this is MountInstance)) Rider = gameObject.GetComponent<NetRider>();
        }
        
        partial void HAPSpawnSync()
        {
            if(isController) return;
            Debug.Log("HAP SPAWNSYNC", this);
            if(this is MountInstance mountInst){
                if(mountInst.Mount == null) return;
            }else{
                if(Rider == null) return;
            }
            CmdHAPSpawnSyncRpc();
        }
    }
}
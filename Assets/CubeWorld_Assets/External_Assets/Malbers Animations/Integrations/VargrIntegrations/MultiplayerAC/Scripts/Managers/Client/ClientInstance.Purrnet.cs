using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    #if AC_PURRNET
    public partial class ClientInstance : NetworkBehaviour
    {
        public static Dictionary<PlayerID, ClientInstance> ClientDirectory = new Dictionary<PlayerID, ClientInstance>();

        public static void AddClient(PlayerID conn, ClientInstance client)
        {
            if(ClientDirectory.ContainsKey(conn)){
                if(ClientDirectory[conn] != client) Debug.LogError("Client Miss Match Error");
                return;
            }

            ClientDirectory.Add(conn, client);
        }
        public static void RemoveClient(PlayerID conn)
        {
            if(ClientDirectory.ContainsKey(conn)) ClientDirectory.Remove(conn);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            gameObject.name = "CLIENT-"+objectId;
            if(!isServer) ClientInstance.AddClient((PlayerID)owner, this);

            Debug.Log("CLIENT OWNER: "+owner, this);
            if(isOwner) CmdVerifyVersionRpc(VERSION_CODE);
        }

        protected override void OnDestroy()
        {   
            base.OnDestroy();
            ClientInstance.RemoveClient((PlayerID)owner);
        }

        [ServerRpc]
        private void CmdVerifyVersionRpc(int versionCode, RPCInfo info = default)
        {
            if(!isServer) return;
            bool pass = versionCode == VERSION_CODE;
            if(pass){
                ClientInstance.AddClient(info.sender, this);
                PlayerJoinedRpc();
                OnPlayerJoin?.Invoke(this);
            }
            TargetVerifyVersionRpc(info.sender, pass);
        }

        [ObserversRpc(excludeOwner:true)]
        private void PlayerJoinedRpc()
        {
            if(isServer) return;
            OnPlayerJoin?.Invoke(this);
        }

        [TargetRpc]
        private void TargetVerifyVersionRpc(PlayerID conn, bool pass)
        {
            if (!pass)
            {
                Debug.LogError("Your executable is out of date. Please update.");
                return;
            }
	        gameObject.name += "-LOCAL";
            OnLocalPlayer?.Invoke(this);
            Instance = this;

            TryRespawn();
        }

        [ServerRpc]
        private void CmdRespawnRpc()
        {
            if(!isServer) return;

            GameObject go = SpawnPlayer();

            if (go == null){
                Debug.LogError("No GameObject to Spawn.");
                return;
            }

            go.GetComponent<PlayerInstance>().GiveOwnership(owner);
        }
    }
    #endif
}
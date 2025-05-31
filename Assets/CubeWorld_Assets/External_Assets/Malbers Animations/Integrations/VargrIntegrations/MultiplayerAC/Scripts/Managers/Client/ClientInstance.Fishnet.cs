using UnityEngine;
using System.Collections.Generic;
using static BackPack;
using static CustomizationManager;

using static HatProp;
using SoftKitty.PCW;



#if AC_FISHNET
using FishNet.Connection;
using FishNet.Object;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
#if AC_FISHNET
    public partial class ClientInstance : NetworkBehaviour
    {
        public NetworkConnection owner => Owner;
        public bool isServer => IsServerStarted;
        public bool isServerOnly => IsServerOnlyStarted;
        public bool isClient => IsClientOnlyStarted;
        public bool isHost => IsHostStarted;
        public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
        public bool isOwner => IsOwner;
        public bool hasOwner => Owner.IsValid;

        public CustomCharacterInfo customCharacterInfo;
        public static Dictionary<NetworkConnection, ClientInstance> ClientDirectory = new Dictionary<NetworkConnection, ClientInstance>();
        public static void AddClient(NetworkConnection conn, ClientInstance client)
        {
            if (ClientDirectory.ContainsKey(conn))
            {
                if (ClientDirectory[conn] != client) Debug.LogError("Client Miss Match Error");
                return;
            }

            ClientDirectory.Add(conn, client);
        }
        public static void RemoveClient(NetworkConnection conn)
        {
            if (ClientDirectory.ContainsKey(conn)) ClientDirectory.Remove(conn);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            gameObject.name = "CLIENT-" + ObjectId;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isServer) ClientInstance.AddClient(Owner, this);
            if (IsOwner)
            {
                string[] parts = gameObject.name.Split('-');
                int number = int.Parse(parts[1]);
                string newString = $"PLAYER-{number + 1}";
                customCharacterInfo = GetCharacterInfo();
                Debug.Log(newString + " --- " + customCharacterInfo.body_index);
                CmdVerifyVersionRpc(VERSION_CODE , newString, customCharacterInfo);
            }
        }
        public virtual void OnDestroy()
        {
            ClientInstance.RemoveClient(Owner);
        }

        public CustomCharacterInfo GetCharacterInfo()
        {
            CustomCharacterInfo characterInfo = new CustomCharacterInfo();

            // Retrieve values from CustomizationManager
            characterInfo.sex_category = CharacterDataReceiver.instance.CharacterInfo.sex_category;
            characterInfo.skin_category = CharacterDataReceiver.instance.CharacterInfo.skin_category;
            characterInfo.body_index = CharacterDataReceiver.instance.CharacterInfo.body_index;
            characterInfo.backpack_category = CharacterDataReceiver.instance.CharacterInfo.backpack_category;
            characterInfo.backpack_index = CharacterDataReceiver.instance.CharacterInfo.backpack_index;
            characterInfo.hair_index = CharacterDataReceiver.instance.CharacterInfo.hair_index;
            characterInfo.beard_index = CharacterDataReceiver.instance.CharacterInfo.beard_index;
            characterInfo.hat_category = CharacterDataReceiver.instance.CharacterInfo.hat_category;
            characterInfo.hat_index = CharacterDataReceiver.instance.CharacterInfo.hat_index;
            characterInfo.face_index = CharacterDataReceiver.instance.CharacterInfo.face_index;
            characterInfo.bodyprop_index = CharacterDataReceiver.instance.CharacterInfo.bodyprop_index;

            return characterInfo;
        }

        [ServerRpc]
        private void CmdVerifyVersionRpc(int versionCode , string object_ID , CustomCharacterInfo customCharacterInfo)
        {
            if (!isServer) return;
            bool pass = versionCode == VERSION_CODE;
            if (pass)
            {
                ClientInstance.AddClient(base.Owner, this);
                GameObject.Find("ServerManagementObject(Clone)").GetComponent<ServerManagementObject>().SaveCharacterData(object_ID, customCharacterInfo);
                PlayerJoinedRpc();
                OnPlayerJoin?.Invoke(this);
            }
            TargetVerifyVersionRpc(Owner, pass);
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void PlayerJoinedRpc()
        {
            if (isServer) return;
            OnPlayerJoin?.Invoke(this);
        }

        [TargetRpc]
        private void TargetVerifyVersionRpc(NetworkConnection conn, bool pass)
        {
            if (!pass)
            {
                NetworkManager.ClientManager.StopConnection();
                FailedJoin();
                return;
            }
            gameObject.name += "-LOCAL";
            OnLocalPlayer?.Invoke(this);
            Instance = this;

            GetWorldDataRequest(conn);
        }

        [ServerRpc]
        private void GetWorldDataRequest(NetworkConnection conn)
        {
            GetWorldDataResponse(conn , ServerManagementObject.world_data);
        }

        [TargetRpc]
        private void GetWorldDataResponse(NetworkConnection conn , string world_data)
        {
            Debug.Log("WorldData is " + world_data);
            GameObject.Find("CubeWorldGenerator").GetComponent<BlockGenerator>().LoadSavedWorld(world_data);
            TryRespawn();
        }

        [ServerRpc]
        private void CmdRespawnRpc()
        {
            if (!isServer) return;

            GameObject go = SpawnPlayer();

            if (go == null)
            {
                Debug.LogError("No GameObject to Spawn.");
                return;
            }
            Spawn(go, Owner);
        }
    }
#endif
}

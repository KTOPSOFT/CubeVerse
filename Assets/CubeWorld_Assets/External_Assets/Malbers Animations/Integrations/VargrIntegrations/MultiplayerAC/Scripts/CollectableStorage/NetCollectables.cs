using UnityEngine;
using VargrIntegrations;
using System.Collections.Generic;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Linq;
using System;

#if AC_FISHNET
using FishNet.Connection;
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    [AddComponentMenu("Malbers//Muliplayer/Collectable Storage")]
    public class NetCollectables : NetworkBehaviour
    {
        [Serializable]
        public struct StartingCollection
        {
            public ScriptableObject Item;
            public int Count;
            public StartingCollection(ScriptableObject item, int count)
            {
                Item = item;
                Count = count;
            }
        }
        public List<StartingCollection> startingItems = new List<StartingCollection>();
        private AnimalInstance m_animal;
        protected Dictionary<ScriptableObject, int> m_collection = new();
        protected Dictionary<AmmoType, Ammo> m_AmmoEquiped = new();

        protected virtual void Awake()
        {
            m_animal = GetComponent<AnimalInstance>();
            if(m_animal.WeaponManager != null) m_animal.WeaponManager.AmmoConsummed += AmmoConsummed;
        }
#if AC_FISHNET
        public override void OnStartNetwork()
		{
			base.OnStartNetwork();

			if(isServer){
                foreach (var item in startingItems)
                {
                    UpdateCollectable(item.Item, item.Count);
                }
			}else{
                CmdCollectionSyncRpc();
			}
		}

        public bool isServer => IsServerStarted;
        public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
        public bool isOwner => IsOwner;
#elif AC_PURRNET
        protected override void OnSpawned()
		{
			if(isServer){
                foreach (var item in startingItems)
                {
                    UpdateCollectable(item.Item, item.Count);
                }
			}else{
                CmdCollectionSyncRpc();
			}
		}
#endif
        #region Methods
        protected virtual void AmmoConsummed(IDs ammo, int count)
        {
            if(!isController || m_animal.WeaponManager == null) return;

            if(ammo is AmmoType ammoType){
                if(!m_AmmoEquiped.ContainsKey(ammoType))
                {
                    m_animal.WeaponManager.OnAmmoUpdate?.Invoke(ammo, 0);
                    return;
                }

                m_animal.WeaponManager.OnAmmoUpdate?.Invoke(ammo, m_collection[m_AmmoEquiped[ammoType]]); // Send what we have befor removal as the Weapon will do this localy

                if(m_collection[m_AmmoEquiped[ammoType]] >= count){
                    m_collection[m_AmmoEquiped[ammoType]] -= count;
                }else{
                    m_collection[m_AmmoEquiped[ammoType]] = 0;
                }
                ProcessUIs(ammoType, m_collection[m_AmmoEquiped[ammoType]]);
                return;
            }else{
                // Could Run for some other types?
            }
        }   
        #endregion
        public void CycleAmmo(AmmoType ammoType)
        {
            // Need to add a few more parts here to make sure we cycle thought all the Ammos
            foreach(var item in m_collection)
            {
                if(item.Key is Ammo ammo){
                    if(ammo.ammoType != ammoType) continue;

                    if(m_AmmoEquiped.ContainsKey(ammoType)){
                        if(m_AmmoEquiped[ammoType] == ammo){
                            continue;
                        }else{
                            UpdateEquipedAmmo(ammo);
                            return;
                        }
                    }else{
                        UpdateEquipedAmmo(ammo);
                        return;
                    }
                }
            }
        }
        public void UpdateEquipedAmmo(Ammo ammo)
        {
            if(!m_collection.ContainsKey(ammo)) return;

            if(m_AmmoEquiped.ContainsKey(ammo.ammoType)){
                m_AmmoEquiped[ammo.ammoType] = ammo;
            }else{
                m_AmmoEquiped.Add(ammo.ammoType, ammo);
            }

            ProcessUIs(ammo.ammoType, m_collection[ammo]);

            if(isController){
                if(isServer){
                    AmmoSyncRpc(RSOManager.GetKey(ammo));
                }else{
                    CmdAmmoSyncRpc(RSOManager.GetKey(ammo));
                }
            }
        }

        public bool UpdateCollectable(ScriptableObject so, int count)
        {
            if(!m_collection.ContainsKey(so)){
                if(count < 0) return false;
                m_collection.Add(so, count);
                if(so is Ammo ammo && !m_AmmoEquiped.ContainsKey(ammo.ammoType)){
                    m_AmmoEquiped.Add(ammo.ammoType, ammo);
                }
            }else{
                if((m_collection[so] + count) < 0) return false;
                m_collection[so] += count;
                if(m_collection[so] < 0 ) m_collection[so] = 0;
            }
            if(isServer){
                UpdateTargetRpc(RSOManager.GetKey(so), m_collection[so]);
            }else{
                CmdUpdateTargetRpc(RSOManager.GetKey(so), m_collection[so]);
            }

            ProcessUpdate(so);
            return true;
        }

        protected virtual void ProcessUpdate(ScriptableObject target)
        {
            if(target is UICollectables targetUI) ProcessUIs(targetUI, m_collection[targetUI]);
            else if(target is ActiveMeshObject targetMesh) ProcessMesh(targetMesh);
            else if(target is Ammo targetAmmo) ProcessAmmo(targetAmmo);
        }
        protected void ProcessUIs(UICollectables target, int count) => ProcessUIs(target, (float)count);
        protected void ProcessUIs(UICollectables target, float count)
        {
            if(!(m_animal is PlayerInstance) && !isOwner) return;

            if(target.UI_Var == null) return;

            if(target.UI_Var is FloatVar floatTarget){ floatTarget.Value = count; }

            else if(target.UI_Var is IntVar intTarget){ intTarget.Value = (int)count; }

            else if(target.UI_Var is BoolVar boolTarget){ boolTarget.Value = count > 0 ? true : false; }
        }

        protected void ProcessMesh(ActiveMeshObject target)
        {
            if(m_collection[target] <=0) return;
            
            target.AddMesh(gameObject);
        }
        protected void ProcessAmmo(Ammo ammo)
        {
            if(m_AmmoEquiped.ContainsKey(ammo.ammoType)){
                if(m_AmmoEquiped[ammo.ammoType] == ammo) ProcessUIs(ammo.ammoType, m_collection[ammo]);

                return;
            }

            UpdateEquipedAmmo(ammo);
        }

        public void MeshChanged(int index, int indexMesh)
        {
            if(!isController) return;

            if(isServer){
                MeshSyncRpc(index, indexMesh);
            }else{
                CmdMeshSyncRpc(index, indexMesh);
            }
        }


#if AC_FISHNET
        #region FISHNET RPC
        [ServerRpc]
        private void CmdUpdateTargetRpc(int key, int count)
        {
            UpdateTargetRpc(key, count);

            ScriptableObject so = RSOManager.Get(key);

            if(!m_collection.ContainsKey(so)){
                if(count <= 0) return;
                m_collection.Add(so, count);
            }else{
                m_collection[so] = count;
            }

            ProcessUpdate(so);
        }

        [ObserversRpc]
        private void UpdateTargetRpc(int key, int count)
        {
            if(isController || isServer) return;

            ScriptableObject so = RSOManager.Get(key);

            if(!m_collection.ContainsKey(so)){
                if(count <= 0) return;
                m_collection.Add(so, count);
            }else{
                m_collection[so] = count;
            }

            ProcessUpdate(so);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CmdCollectionSyncRpc(NetworkConnection conn = null)
        {
            if(conn == null) return;
        
            if(!isServer) return;
            // send item syncs
            int[] item = new int[m_collection.Count];
            int[] count = new int[m_collection.Count];

            var collectionArray = m_collection.ToArray();
            for (int i = 0; i < collectionArray.Length; i++)
            {
                item[i] = RSOManager.GetKey(collectionArray[i].Key);
                count[i] = collectionArray[i].Value;
            }

            int[] ammo = new int[m_AmmoEquiped.Count];
            var ammoArray = m_AmmoEquiped.ToArray();
            for (int i = 0; i < ammoArray.Length; i++)
            {
                ammo[i] = RSOManager.GetKey(ammoArray[i].Value);
            }
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            TargetCollectionSyncRpc(conn, item, count, ammo);

            if(mesh != null){
                TargetMeshSyncRpc(conn, mesh.AllIndex);
            }
        }

        [TargetRpc]
		protected void TargetCollectionSyncRpc(NetworkConnection conn, int[] items, int[] count, int[] ammo)
		{
            if(isServer) return;
            return;
            m_collection = new Dictionary<ScriptableObject, int>();

            for (int i = 0; i < items.Length; i++)
            {
                Debug.Log("RSO index " + i + items[i]);
                ScriptableObject so = RSOManager.Get(items[i]);
                m_collection.Add(so, count[i]);
                ProcessUpdate(so);
            }

            m_AmmoEquiped = new Dictionary<AmmoType, Ammo>();

            for (int i = 0; i < ammo.Length; i++)
            {
                Ammo newAmmo = (Ammo)RSOManager.Get(ammo[i]);
                m_AmmoEquiped.Add(newAmmo.ammoType, newAmmo);
            }
		}

        [TargetRpc]
        protected void TargetMeshSyncRpc(NetworkConnection conn, string mesh_index)
        {
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.AllIndex = mesh_index;
        }

        [ServerRpc]
        private void CmdMeshSyncRpc(int index, int indexMesh)
        {
            MeshSyncRpc(index, indexMesh);
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.ChangeMesh(index, indexMesh);
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void MeshSyncRpc(int index, int indexMesh)
        {
            if(isController || isServer) return;
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.ChangeMesh(index, indexMesh);
        }

        [ServerRpc]
        private void CmdAmmoSyncRpc(int key)
        {
            AmmoSyncRpc(key);
            UpdateEquipedAmmo((Ammo)RSOManager.Get(key));
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void AmmoSyncRpc(int key)
        {
            if(isController || isServer) return;
            UpdateEquipedAmmo((Ammo)RSOManager.Get(key));
        }
        #endregion
#elif AC_PURRNET
        #region PURRNET RPC
        [ServerRpc]
        private void CmdUpdateTargetRpc(int key, int count)
        {
            UpdateTargetRpc(key, count);

            ScriptableObject so = RSOManager.Get(key);

            if(!m_collection.ContainsKey(so)){
                if(count <= 0) return;
                m_collection.Add(so, count);
            }else{
                m_collection[so] = count;
            }

            ProcessUpdate(so);
        }

        [ObserversRpc]
        private void UpdateTargetRpc(int key, int count)
        {
            if(isController || isServer) return;

            ScriptableObject so = RSOManager.Get(key);

            if(!m_collection.ContainsKey(so)){
                if(count <= 0) return;
                m_collection.Add(so, count);
            }else{
                m_collection[so] = count;
            }

            ProcessUpdate(so);
        }

        [ServerRpc(requireOwnership:false)]
        private void CmdCollectionSyncRpc(RPCInfo info = default)
        {
            if(!isServer) return;
            // send item syncs
            int[] item = new int[m_collection.Count];
            int[] count = new int[m_collection.Count];

            var collectionArray = m_collection.ToArray();
            for (int i = 0; i < collectionArray.Length; i++)
            {
                item[i] = RSOManager.GetKey(collectionArray[i].Key);
                count[i] = collectionArray[i].Value;
            }

            int[] ammo = new int[m_AmmoEquiped.Count];
            var ammoArray = m_AmmoEquiped.ToArray();
            for (int i = 0; i < ammoArray.Length; i++)
            {
                ammo[i] = RSOManager.GetKey(ammoArray[i].Value);
            }
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            TargetCollectionSyncRpc(info.sender, item, count, ammo);

            if(mesh != null){
                TargetMeshSyncRpc(info.sender, mesh.AllIndex);
            }
        }

        [TargetRpc]
		protected void TargetCollectionSyncRpc(PlayerID target, int[] items, int[] count, int[] ammo)
		{
            if(isServer) return;

            m_collection = new Dictionary<ScriptableObject, int>();

            for (int i = 0; i < items.Length; i++)
            {
                ScriptableObject so = RSOManager.Get(items[i]);
                m_collection.Add(so, count[i]);
                ProcessUpdate(so);
            }

            m_AmmoEquiped = new Dictionary<AmmoType, Ammo>();

            for (int i = 0; i < ammo.Length; i++)
            {
                Ammo newAmmo = (Ammo)RSOManager.Get(ammo[i]);
                m_AmmoEquiped.Add(newAmmo.ammoType, newAmmo);
            }
		}

        [TargetRpc]
        protected void TargetMeshSyncRpc(PlayerID target, string mesh_index)
        {
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.AllIndex = mesh_index;
        }

        [ServerRpc]
        private void CmdMeshSyncRpc(int index, int indexMesh)
        {
            MeshSyncRpc(index, indexMesh);
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.ChangeMesh(index, indexMesh);
        }

        [ObserversRpc(excludeOwner:true)]
        private void MeshSyncRpc(int index, int indexMesh)
        {
            if(isController || isServer) return;
            ActiveMeshes mesh = gameObject.FindComponent<ActiveMeshes>();
            if(mesh == null) return;

            mesh.ChangeMesh(index, indexMesh);
        }

        [ServerRpc]
        private void CmdAmmoSyncRpc(int key)
        {
            AmmoSyncRpc(key);
            UpdateEquipedAmmo((Ammo)RSOManager.Get(key));
        }

        [ObserversRpc(excludeOwner:true)]
        private void AmmoSyncRpc(int key)
        {
            if(isController || isServer) return;
            UpdateEquipedAmmo((Ammo)RSOManager.Get(key));
        }
        #endregion
#else
#endif
    }
}

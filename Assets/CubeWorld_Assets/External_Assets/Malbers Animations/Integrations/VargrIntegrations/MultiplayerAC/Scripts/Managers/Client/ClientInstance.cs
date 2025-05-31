using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public partial class ClientInstance : NetworkBehaviour
    {
#region Constants.
        /// <summary>
        /// Version of this build.
        /// </summary>
        private const int VERSION_CODE = 0;
#endregion

#region Public.
        // Statics for the Global use, such as Events and Singlton refrance.
        public static ClientInstance Instance;
        public static UnityEvent<ClientInstance> OnPlayerJoin = new UnityEvent<ClientInstance>();
        public static UnityEvent<ClientInstance> OnLocalPlayer = new UnityEvent<ClientInstance>();
        public static GameObject[] Prefabs;
        // End of Statics

    	public GameObjectEvent OnRemoveUILinks;
	    public GameObjectEvent OnAddUILinks;
        public PlayerInstance PlayerInstance { get; protected set; }
        [SerializeField] protected GameObject[] m_characterPrefabs;
        public Tag TeamTag;
        protected SpawnPoint m_lastSpawn;

        public CustomCharacterInfo characterInfo;
#endregion

#region COMMON
        public virtual void TryRespawn()
        {
            if(!isOwner) return;
            CmdRespawnRpc();
        }

        protected virtual void FailedJoin()
        {
            Debug.LogError("Your executable is out of date. Please update.");
        }

        protected virtual GameObject SpawnPlayer()
        {
            if(!isServer) return null;

            m_lastSpawn = SpawnManager.GetNextSpawnPoint(false, m_lastSpawn);
            if (m_lastSpawn == null){
                Debug.LogError("All spawns are occupied.");
                return null;
            }
            
            // Need to setup a track for what Prefab ID it has in the NETWORK and store that for saved chara and use for spawn.
            GameObject go = Instantiate(Prefabs == null ? m_characterPrefabs[0] : Prefabs[0]);
            m_lastSpawn.OrientCharactor(go.transform);
            m_lastSpawn.useSpawn();
            return go;
        }

        public void UpdateCharacter(PlayerInstance target)
        {
            if(PlayerInstance != null){
                if(isServer) PlayerInstance.RemoveOwnership();
                if(isOwner) RemoveUILinks(PlayerInstance.gameObject);
                PlayerInstance = null;
            }
            if(target == null) return;

            PlayerInstance = target;

            AddTags(target);

            if(isOwner) AddUILinks(PlayerInstance.gameObject);
        }

        protected virtual void AddTags(PlayerInstance target)
        {
            if(TeamTag != null) target.AnimalTags.AddTag(TeamTag);
        }

        protected virtual void RemoveUILinks(GameObject target)
        {
            if(!isOwner || target == null) return;
			OnRemoveUILinks?.Invoke(target);
        }
        protected virtual void AddUILinks(GameObject target)
        {
            if(!isOwner || target == null) return;
			OnAddUILinks?.Invoke(target);
        }
#endregion
/*
        #region ShapeShift Tools
        public static Dictionary<uint, NetworkStoredState> StoredShiftState = new Dictionary<uint, NetworkStoredState>();
        private static uint m_lastShiftKey;
        public static uint NextShiftKey()
        {
            if(m_lastShiftKey == 0) m_lastShiftKey = 1;
            m_lastShiftKey = (uint)(m_lastShiftKey * 279470273uL % 4294967291uL);
            return m_lastShiftKey;
        }
        #endregion
*/
    }

}

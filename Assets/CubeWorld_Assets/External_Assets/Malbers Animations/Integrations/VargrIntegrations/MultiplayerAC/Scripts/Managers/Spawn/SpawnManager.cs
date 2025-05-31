using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.VargrMultiplayer
{
	public class SpawnManager : MonoBehaviour
	{
        [SerializeField, Tooltip("How the next spawn point is chosen: **RoundRobin** picks each spawn point in sequence, **FirstValid** starting at the first registered spawwn point and iterating until a valid one is found, **Random** picked at random until a valid point is found.")]
		private SpawnMode m_SpawnMode = SpawnMode.RoundRobin;
        [SerializeField] private LayerMask m_spawnBlockers;

        public enum SpawnMode
		{
			LastPointOnly,
            LastPoint,
            RoundRobin,
			FirstValid,
			Random
		}

        protected void Awake ()
		{
            spawnMode = m_SpawnMode;
            spawnBlockers = m_spawnBlockers;

            // Check for null spawn points
            // (should only be possible in the editor, but no harm doing anyway)
            for (int i = spawnPoints.Count - 1; i >= 0; --i)
            {
                if (spawnPoints[i] == null)
                    spawnPoints.RemoveAt(i);
            }
            
        }

        #region STATIC

        private static int m_LastIndex = -1;
		public static SpawnMode spawnMode { get; protected set; }
        public static LayerMask spawnBlockers { get; protected set; }
        public static event UnityAction<SpawnPoint> onSpawnPointAdded;
        public static event UnityAction<SpawnPoint> onSpawnPointRemoved;
        private static List<SpawnPoint> spawnPoints = new List<SpawnPoint> ();

		public static void AddSpawnPoint (SpawnPoint sp)
		{
            if (sp != null && !spawnPoints.Contains(sp))
            {
                spawnPoints.Add(sp);
                onSpawnPointAdded?.Invoke(sp);
            }
		}

		public static void RemoveSpawnPoint (SpawnPoint sp)
		{
            if (sp != null && spawnPoints.Contains(sp))
            {
                spawnPoints.Remove(sp);
                onSpawnPointRemoved?.Invoke(sp);
            }
        }

        public static bool CheckSpawnPoint (SpawnPoint sp)
        {
            if(!spawnPoints.Contains(sp)) return false;

            return true;
        }
        // Possibly not needed any more
        public static SpawnPoint GetBackStage(bool force)
        {
            for (int i = 0; i < spawnPoints.Count; ++i)
            {
                if ((force || spawnPoints[i].CanSpawnCharacter()) && spawnPoints[i].backStage )
                    return spawnPoints[i];
            }
            return null;
        }

        public static SpawnPoint GetSpawnFromVector(Vector3 position) => spawnPoints.Find( x => x.transform.position == position);

        public static SpawnPoint GetNextSpawnPoint(bool force, SpawnPoint sp = null)
        {
            switch (spawnMode)
            {
                case SpawnMode.RoundRobin:
                    {
                        for (int i = 0; i < spawnPoints.Count; ++i)
                        {
                            // Get wrapped index
                            int index = 1 + m_LastIndex + i;
                            while (index >= spawnPoints.Count)
                                index -= spawnPoints.Count;

                            // Check spawn
                            if ((force || spawnPoints[index].CanSpawnCharacter()) && !spawnPoints[index].backStage)
                            {
                                m_LastIndex = index;
                                return spawnPoints[index];
                            }
                        }
                    }
                    break;
                case SpawnMode.Random:
                    {
                        // Clone list to check each one
                        List<SpawnPoint> untried = new List<SpawnPoint>(spawnPoints.Count);
                        for (int i = 0; i < spawnPoints.Count; ++i)
                            untried.Add(spawnPoints[i]);
                        // Try at random until none left
                        while (untried.Count > 0)
                        {
                            // Get random index
                            int index = Random.Range(0, untried.Count);
                            // Spawn character
                            // Check spawn
                            if ((force || untried[index].CanSpawnCharacter()) && !untried[index].backStage)
                                return untried[index];
                            else
                            {
                                // Remove invalid spawn point from pool
                                untried.RemoveAt(index);
                            }
                        }
                    }
                    break;
                case SpawnMode.LastPointOnly:
                    if(sp != null){
                        if(!sp.backStage && sp.CanSpawnCharacter()) return sp;
                        break;
                    }else{
                        for (int i = 0; i < spawnPoints.Count; ++i)
                        {
                            if ((force || spawnPoints[i].CanSpawnCharacter()) && !spawnPoints[i].backStage )
                                return spawnPoints[i];
                        }
                        break;
                    }
                case SpawnMode.LastPoint:
                case SpawnMode.FirstValid:
                    if(sp != null && !sp.backStage && sp.CanSpawnCharacter()) return sp;
                    
                    for (int i = 0; i < spawnPoints.Count; ++i)
                    {
                        if ((force || spawnPoints[i].CanSpawnCharacter()) && !spawnPoints[i].backStage )
                            return spawnPoints[i];
                    }
                    
                    break;

            }
            return null;
        }
        #endregion
    }
}
using UnityEngine;

namespace MalbersAnimations
{
    public class SpawnerGrid : MonoBehaviour
    {
        public GameObject prefabToSpawn;
        public int Row = 10;
        public int Column = 10;
        public float spacing = 5;

        private void Start()
        {
            SpawnGrid();
        }

        private void SpawnGrid()
        {
            for (int x = 0; x < Row; x++)
            {
                for (int y = 0; y < Column; y++)
                {
                    Vector3 spawnPosition = transform.position + new Vector3(x * spacing, 0f, y * spacing);
                    Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                }
            }
        }

        private void OnDrawGizmos()
        {
            for (int x = 0; x < Row; x++)
            {
                for (int y = 0; y < Column; y++)
                {
                    Vector3 spawnPosition = transform.position + new Vector3(x * spacing, 0f, y * spacing);
                    Gizmos.DrawWireCube(spawnPosition, Vector3.one * spacing * 0.1f);
                }
            }
        }


    }
}

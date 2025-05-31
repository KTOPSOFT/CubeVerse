using System.Collections;
using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    public class DemoRespawn : MonoBehaviour
    {
        public void Respawn()
        {
            StartCoroutine(Respawner());
        }

        IEnumerator Respawner()
        {
            //Wait for 5 seconds
            yield return new WaitForSeconds(5);

            //ClientInstance.Instance.TryRespawn();
        }
    }
}

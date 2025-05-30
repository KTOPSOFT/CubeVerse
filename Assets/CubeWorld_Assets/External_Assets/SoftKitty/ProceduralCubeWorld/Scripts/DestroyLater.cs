using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.PCW
{
    public class DestroyLater : MonoBehaviour
    {
        public float WaitTime = 2.5F;
        IEnumerator Start()
        {
            yield return new WaitForSeconds(WaitTime);
            Destroy(gameObject);
        }

        
    }
}

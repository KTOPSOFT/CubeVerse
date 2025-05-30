using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    public class MessageAnimation : MonoBehaviour
    {
        [Header("Resources")]
        public Animator animator;

        [Header("Settings")]
        public float destroyAfter = 0.5f;

        void Start()
        {
            if (animator == null)
                animator = gameObject.GetComponent<Animator>();

            StartCoroutine("DestroyComponents");
        }

        IEnumerator DestroyComponents()
        {
            yield return new WaitForSeconds(destroyAfter);
            Destroy(animator);
            Destroy(this);
        }
    }
}
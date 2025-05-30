using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    public class BootManager : MonoBehaviour
    {
        // Events
        public UnityEvent onBootStart;
        public UnityEvent eventsAfterBoot;

        // Resources
        public Animator bootAnimator;
        [SerializeField] private UserManager userManager;

        // Settings
        [Range(0, 30)] public float bootTime = 3f;

        // Editor variables
#if UNITY_EDITOR
        public int currentEditorTab;
#endif

        void Start()
        {
            StartCoroutine("BootEventStart");
        }

        public void InvokeEvents()
        {
            bootAnimator.gameObject.SetActive(true);
            bootAnimator.Play("Boot Start");
            StartCoroutine("BootEventStart");
        }

        IEnumerator BootEventStart()
        {
            yield return new WaitForSeconds(bootTime);

            if (bootAnimator.gameObject.activeSelf == true) { bootAnimator.Play("Boot Out"); }
            userManager.lockScreen.gameObject.SetActive(true);
            eventsAfterBoot.Invoke();

            StopCoroutine("BootEventStart");
            StartCoroutine("DisableBootScreenHelper");
        }

        IEnumerator DisableBootScreenHelper()
        {
            yield return new WaitForSeconds(1f);
            bootAnimator.gameObject.SetActive(false);
        }
    }
}
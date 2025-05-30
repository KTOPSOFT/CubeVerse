using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Michsky.DreamOS
{
    public class RebootManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private GameObject mainCanvas;
        [SerializeField] private BootManager bootManager;
        [SerializeField] private Animator restartScreen;

        // Resources
        [Range(0, 2f)] public float waitTime = 1.5f;

        bool isFirstTime = true;

        void OnEnable()
        {
            // Reboot the system if parent (such as Canvas) is disabled and enabled again. 
            if (isFirstTime == false)
                RecoverSystem();

            if (restartScreen != null)
            {
                StartCoroutine("DisableRestartScreen");
                restartScreen.transform.SetAsLastSibling();
            }

            bootManager.onBootStart.Invoke();
            isFirstTime = false;
        }

        public void RebootSystem()
        {
            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("In");
                StartCoroutine("DisableRestartScreen");
            }

            if (bootManager.bootAnimator != null) { StartCoroutine("BootScreenHelper", 1); }
            StartCoroutine("WaitForRestart");
        }

        public void RecoverSystem()
        {
            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("Out");
                StartCoroutine("DisableRestartScreen");
            }

            bootManager.StartCoroutine("BootEventStart");
        }

        public void ShutdownSystem()
        {
            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("In");
                StartCoroutine("DisableRestartScreen");
            }

            StartCoroutine("WaitForShutdown");
        }

        public void RunSystem()
        {
            mainCanvas.SetActive(true);
        }

        public void WipeSystem()
        {
            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("In");
                StartCoroutine("DisableRestartScreen");
            }

            StartCoroutine("WaitForWipe");
        }

        IEnumerator WaitForRestart()
        {
            yield return new WaitForSeconds(waitTime);
            mainCanvas.SetActive(false);
            mainCanvas.SetActive(true);

            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("Out");
                StartCoroutine("DisableRestartScreen");
            }

            bootManager.bootAnimator.gameObject.SetActive(true);
            bootManager.StartCoroutine("BootEventStart");
            StopCoroutine("WaitForRestart");
        }

        IEnumerator WaitForWipe()
        {
            yield return new WaitForSeconds(waitTime);

            if (restartScreen != null)
            {
                restartScreen.gameObject.SetActive(true);
                restartScreen.Play("Out");
                StartCoroutine("DisableRestartScreen");
            }
        }

        IEnumerator WaitForShutdown()
        {
            yield return new WaitForSeconds(waitTime);
            mainCanvas.SetActive(false);
        }

        IEnumerator DisableRestartScreen()
        {
            yield return new WaitForSeconds(4f);
            restartScreen.gameObject.SetActive(false);
        }

        IEnumerator BootScreenHelper(float timeMultiplier)
        {
            yield return new WaitForSeconds(timeMultiplier);
            bootManager.InvokeEvents();
        }
    }
}
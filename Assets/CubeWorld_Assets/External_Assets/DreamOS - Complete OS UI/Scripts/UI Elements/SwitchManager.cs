using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Button))]
    public class SwitchManager : MonoBehaviour
    {
        // Events
        public UnityEvent OnEvents;
        public UnityEvent OffEvents;

        // Saving
        public bool saveValue = true;
        public string switchTag = "Switch";

        // Settings
        public bool isOn = true;
        public bool invokeAtStart = true;

        // Multi Instance Support
        public UserManager userManager;

        // Resources
        public Animator switchAnimator;
        public Button switchButton;

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { switchTag = userManager.machineID + switchTag; }
            if (switchAnimator == null) { switchAnimator = gameObject.GetComponent<Animator>(); }
            if (switchButton == null) { switchButton = gameObject.GetComponent<Button>(); }

            switchButton.onClick.AddListener(AnimateSwitch);
            CheckForData();

            if (invokeAtStart == true && isOn == true) { OnEvents.Invoke(); }
            else if (invokeAtStart == true && isOn == false) { OffEvents.Invoke(); }
        }

        void OnEnable()
        {
            CheckForData();
        }

        public void CheckForData()
        {
            StopCoroutine("DisableAnimator");
            switchAnimator.enabled = true;

            if (saveValue == true)
            {
                if (!PlayerPrefs.HasKey(switchTag + "Switch"))
                {
                    if (isOn == true)
                    {
                        isOn = true;
                        if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch On"); }
                        PlayerPrefs.SetString(switchTag + "Switch", "true");
                    }

                    else
                    {
                        isOn = false;
                        if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch Off"); }
                        PlayerPrefs.SetString(switchTag + "Switch", "false");
                    }
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "true")
                {
                    isOn = true;
                    if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch On"); }
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "false")
                {
                    isOn = false;
                    if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch Off"); }
                }

                if (gameObject.activeInHierarchy == true) { StartCoroutine("DisableAnimator"); }
            }

            else { UpdateUI(); }
        }

        public void UpdateUI()
        {
            StopCoroutine("DisableAnimator");
            switchAnimator.enabled = true;

            if (isOn == true && switchAnimator != null && switchAnimator.gameObject.activeInHierarchy == true)
            {
                isOn = true;
                if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch On"); }
            }

            else if (isOn == false && switchAnimator != null && switchAnimator.gameObject.activeInHierarchy == true)
            {
                isOn = false;
                if (switchAnimator.gameObject.activeInHierarchy == true) { switchAnimator.Play("Switch Off"); }
            }

            if (gameObject.activeInHierarchy == true) { StartCoroutine("DisableAnimator"); }
        }

        public void AnimateSwitch()
        {
            StopCoroutine("DisableAnimator");
            switchAnimator.enabled = true;

            if (isOn == true)
            {
                switchAnimator.Play("Switch Off");
                isOn = false;
                OffEvents.Invoke();

                if (saveValue == true) { PlayerPrefs.SetString(switchTag + "Switch", "false"); }
            }

            else
            {
                switchAnimator.Play("Switch On");
                isOn = true;
                OnEvents.Invoke();

                if (saveValue == true) { PlayerPrefs.SetString(switchTag + "Switch", "true"); }
            }

            if (gameObject.activeInHierarchy == true) { StartCoroutine("DisableAnimator"); }
        }

        public void InvokeEvents()
        {
            if (isOn == true) { OnEvents.Invoke(); }
            else { OffEvents.Invoke(); }
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(0.5f);
            switchAnimator.enabled = false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Michsky.DreamOS
{
    public class BSODManager : MonoBehaviour
    {
        // List
        [SerializeField] private List<StepItem> steps = new List<StepItem>();

        // Resources
        [SerializeField] private GameObject BSODScreen;
        [SerializeField] private GameObject dreamOSCanvas;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI errorText;

        // Events
        public UnityEvent onCrash;
        public UnityEvent onCrashEnd;

        // Settings
        public string progressSuffix = "% complete";

        int currentStep;

        [System.Serializable]
        public class StepItem
        {
            [Range(0, 100)] public int progress;
            [Range(0, 10)] public int duration;
            public UnityEvent onStepChanged;
        }

        void Awake()
        {
            BSODScreen.SetActive(false);
        }

        public void CreateBSOD(string errorID)
        {
            onCrash.Invoke();
            BSODScreen.SetActive(true);

            if (errorText != null)
                errorText.text = errorID;

            progressText.text = "0" + progressSuffix;
            StartCoroutine(StoryTellerHelper(steps[0].duration));
        }

        IEnumerator StoryTellerHelper(float timer)
        {
            yield return new WaitForSeconds(timer);

            if (currentStep <= steps.Count - 1)
            {
                progressText.text = steps[currentStep].progress + progressSuffix;
                StartCoroutine(StoryTellerHelper(steps[currentStep].duration));
                currentStep++;
            }

            else
            {
                currentStep = 0;
                dreamOSCanvas.SetActive(false);
                dreamOSCanvas.SetActive(true);
                BSODScreen.SetActive(false);
                onCrashEnd.Invoke();
            }
        }
    }
}
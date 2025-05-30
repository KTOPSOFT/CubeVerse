using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    public class WindowPanelManager : MonoBehaviour
    {
        // List
        public List<PanelItem> panels = new List<PanelItem>();

        // Settings
        public bool recoverPanels;
        public int currentPanelIndex = 0;
        private int currentButtonIndex = 0;
        private int newPanelIndex;
        public bool cullPanels = true;
        public bool initializeButtons = true;

        // Events
        [System.Serializable] public class PanelChangeEvent : UnityEvent<int> { }
        public PanelChangeEvent onPanelChanged;

        private GameObject currentPanel;
        private GameObject nextPanel;
        private GameObject currentButton;
        private GameObject nextButton;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;
        private Animator currentButtonAnimator;
        private Animator nextButtonAnimator;

        private string panelFadeIn = "Panel In";
        private string panelFadeOut = "Panel Out";
        private string buttonFadeIn = "Closed to Open";
        private string buttonFadeOut = "Open to Closed";

        bool isInitialized = false;

        [System.Serializable]
        public class PanelItem
        {
            public string panelName;
            public GameObject panelObject;
            public GameObject buttonObject;
        }

        void Awake()
        {
            if (panels.Count == 0)
                return;

            InitializePanels();
        }

        void OnEnable()
        {
            if (isInitialized == false) { return; }
            if (recoverPanels == false && currentPanelIndex != 0) { OpenPanelByIndex(0); return; }

            if (nextPanelAnimator == null)
            {
                currentPanelAnimator.Play(panelFadeIn);
                if (currentButtonAnimator != null) { currentButtonAnimator.Play(buttonFadeIn); }
            }

            else if (nextPanelAnimator != null)
            {
                nextPanelAnimator.Play(panelFadeIn);
                if (nextButtonAnimator != null) { nextButtonAnimator.Play(buttonFadeIn); }
            }
        }

        public void InitializePanels()
        {
            if (panels[currentPanelIndex].buttonObject != null)
            {
                currentButton = panels[currentPanelIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeIn);
            }

            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            onPanelChanged.Invoke(currentPanelIndex);
            isInitialized = true;

            for (int i = 0; i < panels.Count; i++)
            {
                if (i != currentPanelIndex && cullPanels == true) { panels[i].panelObject.SetActive(false); }
                if (panels[i].buttonObject != null && initializeButtons == true)
                {
                    string tempName = panels[i].panelName;
                    NavDrawerButton tempButton = panels[i].buttonObject.GetComponent<NavDrawerButton>();

                    if (tempButton == null)
                        continue;

                    tempButton.onClickEvents.RemoveAllListeners();
                    tempButton.onClickEvents.AddListener(() => OpenPanel(tempName));
                }
            }
        }

        public void OpenPanel(string newPanel)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].panelName == newPanel)
                {
                    newPanelIndex = i;
                    break;
                }
            }

            if (newPanelIndex != currentPanelIndex)
            {
                if (cullPanels == true)
                    StopCoroutine("DisablePreviousPanel");

                currentPanel = panels[currentPanelIndex].panelObject;

                if (panels[currentPanelIndex].buttonObject != null)
                    currentButton = panels[currentPanelIndex].buttonObject;

                currentPanelIndex = newPanelIndex;
                nextPanel = panels[currentPanelIndex].panelObject;
                nextPanel.SetActive(true);

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                if (cullPanels == true)
                    StartCoroutine("DisablePreviousPanel");

                if (panels[currentButtonIndex].buttonObject != null)
                {
                    currentButton = panels[currentButtonIndex].buttonObject;
                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    currentButtonAnimator.Play(buttonFadeOut);
                }

                currentButtonIndex = newPanelIndex;

                if (panels[currentButtonIndex].buttonObject != null)
                {
                    nextButton = panels[currentButtonIndex].buttonObject;
                    nextButtonAnimator = nextButton.GetComponent<Animator>();
                    nextButtonAnimator.Play(buttonFadeIn);
                }

                onPanelChanged.Invoke(currentPanelIndex);
            }
        }

        public void OpenPanelByIndex(int panelIndex)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].panelName == panels[panelIndex].panelName)
                {
                    OpenPanel(panels[panelIndex].panelName);
                    break;
                }
            }
        }

        public void NextPanel()
        {
            if (currentPanelIndex <= panels.Count - 2)
            {
                OpenPanelByIndex(currentPanelIndex + 1);
            }
        }

        public void PrevPanel()
        {
            if (currentPanelIndex >= 1)
            {
                OpenPanelByIndex(currentPanelIndex - 1);
            }
        }

        IEnumerator DisablePreviousPanel()
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < panels.Count; i++)
            {
                if (i == currentPanelIndex)
                    continue;

                panels[i].panelObject.SetActive(false);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class SetupManager : MonoBehaviour
    {
        // List
        public List<StepItem> steps = new List<StepItem>();

        // Resources
        public UserManager userManager;
        [SerializeField] private TMP_InputField firstNameInput;
        [SerializeField] private TMP_InputField lastNameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField passwordRetypeInput;
        [SerializeField] private Button infoContinueButton;
        [SerializeField] private Button privacyContinueButton;
        [SerializeField] private Animator errorMessageObject;
        [SerializeField] private TextMeshProUGUI errorMessageText;

        // Settings
        [SerializeField] private int currentPanelIndex = 0;
        [SerializeField] private bool enableBackgroundAnim = true;
        [SerializeField][TextArea] private string firstNameLengthError;
        [SerializeField][TextArea] private string lastNameLengthError;
        [SerializeField][TextArea] private string passwordLengthError;
        [SerializeField][TextArea] private string passwordRetypeError;

        private GameObject currentStep;
        private GameObject currentPanel;
        private GameObject nextPanel;
        private GameObject currentBG;
        private GameObject nextBG;

        [HideInInspector] public Animator currentStepAnimator;
        [HideInInspector] public Animator currentPanelAnimator;
        [HideInInspector] public Animator currentBGAnimator;
        [HideInInspector] public Animator nextPanelAnimator;
        [HideInInspector] public Animator nextBGAnimator;

        string panelFadeIn = "Panel In";
        string panelFadeOut = "Panel Out";
        string BGFadeIn = "Panel In";
        string BGFadeOut = "Panel Out";
        string stepFadeIn = "Check";

        [System.Serializable]
        public class StepItem
        {
            public string title = "Step";
            public GameObject indicator;
            public GameObject panel;
            public GameObject background;
            public StepContent stepContent;
        }

        public enum StepContent { Default, Information, Privacy }

        void Awake()
        {
            currentPanel = steps[currentPanelIndex].panel;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();

            if (currentPanelAnimator.transform.parent.gameObject.activeSelf == true)
            {
                currentPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = steps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }

                else
                {
                    currentBG = steps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeIn);
                }
            }
        }

        void Update()
        {
            if (userManager == null)
                return;

            if (steps[currentPanelIndex].stepContent == StepContent.Information)
            {
                if (firstNameInput.text.Length >= userManager.minNameCharacter && firstNameInput.text.Length <= userManager.maxNameCharacter)
                {
                    userManager.nameOK = true;

                    if (lastNameInput.text.Length >= userManager.minNameCharacter && lastNameInput.text.Length <= userManager.maxNameCharacter)
                    {
                        userManager.lastNameOK = true;
                        infoContinueButton.interactable = true;

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("Out"))
                            errorMessageObject.Play("Out");
                    }

                    else
                    {
                        userManager.lastNameOK = false;
                        infoContinueButton.interactable = false;
                        errorMessageText.text = lastNameLengthError;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent.GetComponent<RectTransform>());

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                            errorMessageObject.Play("In");
                    }
                }

                else
                {
                    userManager.nameOK = false;
                    infoContinueButton.interactable = false;
                    errorMessageText.text = firstNameLengthError;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent.GetComponent<RectTransform>());

                    if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                        errorMessageObject.Play("In");
                }
            }

            else if (steps[currentPanelIndex].stepContent == StepContent.Privacy)
            {
                if (passwordInput.text.Length >= userManager.minPasswordCharacter && passwordInput.text.Length <= userManager.maxPasswordCharacter || passwordInput.text.Length == 0)
                {
                    userManager.passwordOK = true;

                    if (passwordInput.text != passwordRetypeInput.text)
                    {
                        userManager.passwordRetypeOK = false;
                        privacyContinueButton.interactable = false;
                        errorMessageText.text = passwordRetypeError;
                        LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent.GetComponent<RectTransform>());

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                            errorMessageObject.Play("In");
                    }

                    else if (passwordInput.text == passwordRetypeInput.text)
                    {
                        userManager.passwordRetypeOK = true;
                        privacyContinueButton.interactable = true;

                        if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("Out"))
                            errorMessageObject.Play("Out");
                    }
                }

                else
                {
                    userManager.passwordOK = false;
                    privacyContinueButton.interactable = false;
                    errorMessageText.text = passwordLengthError;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(errorMessageText.transform.parent.GetComponent<RectTransform>());

                    if (!errorMessageObject.GetCurrentAnimatorStateInfo(0).IsName("In"))
                        errorMessageObject.Play("In");
                }
            }
        }

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = steps[currentPanelIndex].panel;
                currentPanelIndex = newPanel;
                nextPanel = steps[currentPanelIndex].panel;

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = steps[currentPanelIndex].background;
                    currentPanelIndex = newPanel;
                    nextBG = steps[currentPanelIndex].background;

                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    nextBGAnimator = nextBG.GetComponent<Animator>();

                    currentBGAnimator.Play(BGFadeOut);
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void NextPage()
        {
            if (currentPanelIndex <= steps.Count - 2)
            {
                currentPanel = steps[currentPanelIndex].panel;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                currentStep = steps[currentPanelIndex].indicator;
                currentStepAnimator = currentStep.GetComponent<Animator>();
                currentStepAnimator.Play(stepFadeIn);

                if (enableBackgroundAnim == true)
                {
                    currentBG = steps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeOut);
                }

                currentPanelIndex += 1;
                nextPanel = steps[currentPanelIndex].panel;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    nextBG = steps[currentPanelIndex].background;
                    nextBGAnimator = nextBG.GetComponent<Animator>();
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void PrevPage()
        {
            if (currentPanelIndex >= 1)
            {
                currentPanel = steps[currentPanelIndex].panel;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                if (enableBackgroundAnim == true)
                {
                    currentBG = steps[currentPanelIndex].background;
                    currentBGAnimator = currentBG.GetComponent<Animator>();
                    currentBGAnimator.Play(BGFadeOut);
                }

                currentPanelIndex -= 1;
                nextPanel = steps[currentPanelIndex].panel;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBackgroundAnim == true)
                {
                    nextBG = steps[currentPanelIndex].background;
                    nextBGAnimator = nextBG.GetComponent<Animator>();
                    nextBGAnimator.Play(BGFadeIn);
                }
            }
        }

        public void PlayLastStepAnim()
        {
            currentStep = steps[steps.Count].indicator;
            currentStepAnimator = currentStep.GetComponent<Animator>();
            currentStepAnimator.Play(stepFadeIn);
        }
    }
}
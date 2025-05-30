using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalWindowManager : MonoBehaviour
    {
        // Content
        public Sprite windowIcon;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        // Resources
        public Animator mwAnimator;
        public Image iconImage;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public Button confirmButton;
        public Button cancelButton;
        public BlurManager blurManager;

        // Settings
        public bool useBlur = true;
        public bool useCustomValues = false;
        public bool isOn = false;
        public StartBehaviour startBehaviour = StartBehaviour.Disable;
        public CloseBehaviour closeBehaviour = CloseBehaviour.Disable;

        // Events
        public UnityEvent onConfirm;
        public UnityEvent onCancel;

        public enum StartBehaviour { None, Disable }
        public enum CloseBehaviour { None, Disable, Destroy }

        void Awake()
        {
            isOn = false;

            if (mwAnimator == null) { mwAnimator = gameObject.GetComponent<Animator>(); }
            if (confirmButton != null) { confirmButton.onClick.AddListener(onConfirm.Invoke); }
            if (cancelButton != null) { cancelButton.onClick.AddListener(onCancel.Invoke); }
            if (useCustomValues == false) { UpdateUI(); }
            if (startBehaviour == StartBehaviour.Disable) { gameObject.SetActive(false); }
        }

        public void UpdateUI()
        {
            try
            {
                iconImage.sprite = windowIcon;
                windowTitle.text = titleText;
                windowDescription.text = descriptionText;
            }

            catch { Debug.LogWarning("<b>[Modal Window]</b> Cannot update the content due to missing variables.", this); }
        }

        public void OpenWindow()
        {       
            if (isOn == false)
            {
                StopCoroutine("DisableObject");

                gameObject.SetActive(true);
                isOn = true;
                mwAnimator.Play("Fade in");

                if (useBlur == true && blurManager != null) { blurManager.BlurInAnim(); }
            }
        }

        public void CloseWindow()
        {
            if (isOn == true)
            {
                StartCoroutine("DisableObject");
               
                isOn = false;
                mwAnimator.Play("Fade out");

                if (useBlur == true && blurManager != null) { blurManager.BlurOutAnim(); }
            }
        }

        public void AnimateWindow()
        {
            if (isOn == false)
            {
                StopCoroutine("DisableObject");

                gameObject.SetActive(true);
                mwAnimator.Play("Fade in");
                isOn = true;

                if (useBlur == true && blurManager != null) { blurManager.BlurInAnim(); }
            }

            else
            {
                StartCoroutine("DisableObject");

                mwAnimator.Play("Fade out");
                isOn = false;

                if (useBlur == true && blurManager != null) { blurManager.BlurOutAnim(); }
            }
        }

        IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(0.5f);
            if (closeBehaviour == CloseBehaviour.Disable) { gameObject.SetActive(false); }
            else if (closeBehaviour == CloseBehaviour.Destroy) { Destroy(gameObject); }
        }
    }
}
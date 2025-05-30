using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Animator))]
    public class TaskBarButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
    {
        // Resources
        public List<AppElement> appElements = new List<AppElement>();
        public Animator buttonAnimator;
        [HideInInspector] public WindowManager windowManager;

        // Settings
        public string buttonTitle = "App Window";
        [SerializeField] private DefaultPinState defaultPinState = DefaultPinState.Pinned;

        // Context Menu
        public Animator contextMenu;
        public BlurManager contextBlur;
        public ButtonManager headerButton;
        public ButtonManager closeButton;
        public ButtonManager pinButton;
        public ButtonManager unpinButton;
        bool isContextOn;

        // Events
        public UnityEvent onClick;

        [HideInInspector] public bool isPinned;

        bool initialized = false;
        bool isDragging = false;

        [SerializeField] private enum DefaultPinState { Pinned, Unpinned }

        void OnEnable()
        {
            // if (windowManager == null) { Debug.LogError("<b>[Taskbar Button]</b> The button is not linked with any Window Manager.", this); }
            if (buttonAnimator == null) { buttonAnimator = gameObject.GetComponent<Animator>(); }

            if (initialized == true && isPinned == true) { buttonAnimator.Play("Draw"); }
            else if (initialized == true && isPinned == false) { buttonAnimator.Play("Hide"); }

            StartCoroutine("DisableAnimator");
        }

        public void InitializeButton()
        {
            if (buttonAnimator == null) { buttonAnimator = gameObject.GetComponent<Animator>(); }
            if (defaultPinState == DefaultPinState.Pinned && !PlayerPrefs.HasKey(buttonTitle + "_TaskbarShortcut")) { PlayerPrefs.SetString(buttonTitle + "_TaskbarShortcut", "true"); }

            if (PlayerPrefs.GetString(buttonTitle + "_TaskbarShortcut") == "true") { isPinned = true; }
            else { isPinned = false; }

            if (isPinned == true) { buttonAnimator.Play("Draw"); }
            else { buttonAnimator.Play("Hide"); }

            for (int i = 0; i < appElements.Count; i++)
            {
                appElements[i].appID = buttonTitle;
                appElements[i].UpdateLibrary();
                appElements[i].UpdateElement();
            }

            if (headerButton != null)
            {
                headerButton.onClick.AddListener(delegate
                {
                    windowManager.OpenWindow();
                    AnimateContextMenu();
                });
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
                closeButton.onClick.AddListener(delegate
                {
                    windowManager.CloseWindow();
                    AnimateContextMenu();
                });
            }

            if (pinButton != null && unpinButton != null)
            {
                if (isPinned == false) { pinButton.gameObject.SetActive(true); unpinButton.gameObject.SetActive(false); }
                else { pinButton.gameObject.SetActive(false); unpinButton.gameObject.SetActive(true); }

                pinButton.onClick.AddListener(() => PinTaskBarButton());
                unpinButton.onClick.AddListener(() => PinTaskBarButton());
            }

            onClick.AddListener(() => windowManager.OpenWindow());

            initialized = true;
        }

        public void OnBeginDrag(PointerEventData eventData) { isDragging = true; }
        public void OnEndDrag(PointerEventData eventData) { isDragging = false; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging == true || windowManager == null) { return; }
            if (eventData.button == PointerEventData.InputButton.Right) { AnimateContextMenu(); }
            else
            {
                if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Highlighted")
                    && windowManager.transform.GetSiblingIndex() != windowManager.transform.parent.childCount - 1)
                {
                    windowManager.FocusToWindow();
                    buttonAnimator.Play("Inactive to Active");
                }

                else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Highlighted")
                    || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed to Active")
                    || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Active")
                    || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Active")
                    && windowManager.transform.GetSiblingIndex() == windowManager.transform.parent.childCount - 1)
                {
                    windowManager.MinimizeWindow();
                    buttonAnimator.Play("Active to Inactive");
                }

                else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Highlighted")
                    || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Inactive")
                    || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Inactive"))
                {
                    onClick.Invoke();
                    windowManager.FocusToWindow();
                    buttonAnimator.Play("Highlighted to Active");
                }

                else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
                {
                    onClick.Invoke();
                    windowManager.FocusToWindow();
                    buttonAnimator.Play("Hide to Active");
                }

                else
                {
                    onClick.Invoke();
                    windowManager.FocusToWindow();
                    buttonAnimator.Play("Closed to Active");
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StopCoroutine("DisableAnimator");
            buttonAnimator.enabled = true;

            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Closed")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Closed"))
                buttonAnimator.Play("Closed to Highlighted");

            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Active")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed to Active")
               || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Active")
                     || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide to Active"))
                buttonAnimator.Play("Active to Highlighted");

            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Inactive")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Inactive"))
                buttonAnimator.Play("Inactive to Highlighted");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed to Highlighted")) { buttonAnimator.Play("Highlighted to Closed"); }
            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Highlighted")) { buttonAnimator.Play("Highlighted to Active"); }
            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Highlighted")) { buttonAnimator.Play("Highlighted to Inactive"); }

            StartCoroutine("DisableAnimator");
        }

        public void SetOpen()
        {
            buttonAnimator.enabled = true;

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Highlighted")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Highlighted"))
                buttonAnimator.Play("Highlighted to Active");
            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
                buttonAnimator.Play("Hide to Active");
            else
                buttonAnimator.Play("Closed to Active");
        }

        public void SetClosed()
        {
            buttonAnimator.enabled = true;

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");

            if (PlayerPrefs.GetString(buttonTitle + "_TaskbarShortcut") == "true") { buttonAnimator.Play("Active to Closed"); }
            else { buttonAnimator.Play("Hide"); }
        }

        public void SetMinimized()
        {
            buttonAnimator.enabled = true;
            buttonAnimator.Play("Active to Inactive");

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");
        }

        public void PinTaskBarButton()
        {
            if (isPinned == false)
            {
                PlayerPrefs.SetString(buttonTitle + "_TaskbarShortcut", "true");
                isPinned = true;

                if (pinButton != null && unpinButton != null) { pinButton.gameObject.SetActive(false); unpinButton.gameObject.SetActive(true); }
            }

            else
            {
                if (windowManager.isOn == false) { buttonAnimator.Play("Hide"); }
                if (pinButton != null && unpinButton != null) { pinButton.gameObject.SetActive(true); unpinButton.gameObject.SetActive(false); }

                PlayerPrefs.SetString(buttonTitle + "_TaskbarShortcut", "false");
                isPinned = false;
            }

            AnimateContextMenu();
        }

        public void AnimateContextMenu()
        {
            if (isContextOn == true)
            {
                isContextOn = false;
                contextMenu.Play("Menu Out");

                StopCoroutine("DisableContextMenu");
                StartCoroutine("DisableContextMenu");

                if (contextBlur != null) { contextBlur.BlurOutAnim(); }
            }

            else if (isContextOn == false)
            {
                contextMenu.gameObject.SetActive(true);
                isContextOn = true;
                contextMenu.Play("Menu In");

                StopCoroutine("DisableContextMenu");

                if (contextBlur != null) { contextBlur.BlurInAnim(); }
                if (windowManager.isOn == true && closeButton != null) { closeButton.gameObject.SetActive(true); }
                else if (closeButton != null) { closeButton.gameObject.SetActive(false); }
            }
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(0.5f);
            buttonAnimator.enabled = false;
        }

        IEnumerator DisableContextMenu()
        {
            yield return new WaitForSeconds(0.5f);
            contextMenu.gameObject.SetActive(false);
        }
    }
}
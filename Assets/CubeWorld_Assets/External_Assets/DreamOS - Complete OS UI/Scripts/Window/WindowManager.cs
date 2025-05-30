using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WindowManager : MonoBehaviour, IPointerClickHandler
    {
        // Resources
        public Animator windowAnimator;
        public RectTransform windowContent;
        public RectTransform navbarRect;
        public TaskBarButton taskbarButton;
        public WindowDragger windowDragger;
        public WindowResize windowResize;

        // Fullscreen & minimize
        public GameObject fullscreenImage;
        public GameObject normalizeImage;

        // Settings
        public bool enableBackgroundBlur = true;
        public bool enableMobileMode = false;
        public bool hasNavDrawer = true;
        public float minNavbarWidth = 75;
        public float maxNavbarWidth = 300;
        [Range(1, 25)] public float smoothness = 10;
        public DefaultState defaultNavbarState = DefaultState.Minimized;

        // Events
        public UnityEvent onEnableEvents;
        public UnityEvent onLaunchEvents;
        public UnityEvent onQuitEvents;

        public enum DefaultState { Minimized, Expanded }

        BlurManager windowBGBlur;
        RectTransform windowRect;

        float left;
        float right;
        float top;
        float bottom;
        bool isNavDrawerOpen = true;
        bool isInTransition = false;

        [HideInInspector] public bool isOn;
        [HideInInspector] public bool isNormalized;
        [HideInInspector] public bool isFullscreen;
        [HideInInspector] public bool disableAtStart = true;

        void Awake()
        {
            SetupWindow();
        }

        void OnEnable()
        {
            onEnableEvents.Invoke();

            if (enableMobileMode == true)
                return;

            if (hasNavDrawer == true)
            {
                if (navbarRect == null || windowContent == null)
                {
                    Debug.LogError("<b>[Window Manager]</b> Navbar is enabled but its resources are missing!");
                    return;
                }

                if (PlayerPrefs.GetString(gameObject.name + "NavDrawer") == "true")
                {
                    defaultNavbarState = DefaultState.Expanded;
                    isNavDrawerOpen = true;
                }

                else if (PlayerPrefs.GetString(gameObject.name + "NavDrawer") == "false"
                    || !PlayerPrefs.HasKey(gameObject.name + "NavDrawer"))
                {
                    defaultNavbarState = DefaultState.Minimized;
                    isNavDrawerOpen = false;
                }

                if (defaultNavbarState == DefaultState.Minimized)
                {
                    navbarRect.sizeDelta = new Vector2(minNavbarWidth, navbarRect.sizeDelta.y);
                    windowContent.offsetMin = new Vector2(minNavbarWidth, windowContent.offsetMin.y);
                }

                else if (defaultNavbarState == DefaultState.Expanded)
                {
                    navbarRect.sizeDelta = new Vector2(maxNavbarWidth, navbarRect.sizeDelta.y);
                    windowContent.offsetMin = new Vector2(maxNavbarWidth, windowContent.offsetMin.y);
                }
            }
        }

        void Update()
        {
            if (isInTransition == false)
                return;

            if (defaultNavbarState == DefaultState.Minimized)
            {
                navbarRect.sizeDelta = Vector2.Lerp(navbarRect.sizeDelta, new Vector2(minNavbarWidth, navbarRect.sizeDelta.y), Time.deltaTime * smoothness);
                windowContent.offsetMin = Vector2.Lerp(windowContent.offsetMin, new Vector2(minNavbarWidth, windowContent.offsetMin.y), Time.deltaTime * smoothness);

                if (navbarRect.sizeDelta.x <= minNavbarWidth + 0.1f)
                    isInTransition = false;
            }

            else if (defaultNavbarState == DefaultState.Expanded)
            {
                navbarRect.sizeDelta = Vector2.Lerp(navbarRect.sizeDelta, new Vector2(maxNavbarWidth, navbarRect.sizeDelta.y), Time.deltaTime * smoothness);
                windowContent.offsetMin = Vector2.Lerp(windowContent.offsetMin, new Vector2(maxNavbarWidth, windowContent.offsetMin.y), Time.deltaTime * smoothness);

                if (navbarRect.sizeDelta.x >= maxNavbarWidth - 0.1f)
                    isInTransition = false;
            }
        }

        public void SetupWindow()
        {
            // Disable the object for optimization purposes
            //if (disableAtStart == true) { gameObject.SetActive(false); }

            // Don't process further events if mobile mode is enabled
            if (enableMobileMode == true) { return; }

            // Get the animator
            if (windowAnimator == null) { windowAnimator = gameObject.GetComponent<Animator>(); }

            // Assign taskar variables
            if (taskbarButton != null) { taskbarButton.windowManager = this; taskbarButton.InitializeButton(); }
            else { Debug.Log("<b>[Window Manager]</b> Taskbar Button cannot be initialized due to the missing variable.", this); }

            // Assign blur variables
            if (enableBackgroundBlur == true)
            {
                try { windowBGBlur = gameObject.GetComponent<BlurManager>(); }
                catch { Debug.Log("<b>[Window Manager]</b> No Blur Manager attached to Game Object. Background Blur won't be working.", this); }
            }

            // Assign window dragger variables
            if (windowDragger != null) { windowDragger.wManager = this; }

            // Get RectTransform and set offset values
            windowRect = gameObject.GetComponent<RectTransform>();
            left = windowRect.offsetMin.x;
            right = -windowRect.offsetMax.x;
            top = -windowRect.offsetMax.y;
            bottom = windowRect.offsetMin.y;

            // Change fullscreen image state
            if (fullscreenImage != null && normalizeImage != null)
            {
                fullscreenImage.SetActive(true);
                normalizeImage.SetActive(false);
            }
        }

        public void NavDrawerAnimate()
        {
            if (navbarRect == null || windowContent == null)
                return;

            if (isNavDrawerOpen == true)
            {
                PlayerPrefs.SetString(gameObject.name + "NavDrawer", "false");
                defaultNavbarState = DefaultState.Minimized;
                isNavDrawerOpen = false;
            }

            else
            {
                PlayerPrefs.SetString(gameObject.name + "NavDrawer", "true");
                defaultNavbarState = DefaultState.Expanded;
                isNavDrawerOpen = true;
            }

            isInTransition = true;
        }

        public void OpenWindow()
        {
            StopCoroutine("DisableAnimator");
            StopCoroutine("DisableObject");
            FocusToWindow();

            isOn = true;
            gameObject.SetActive(true);
            onLaunchEvents.Invoke();

            if (enableMobileMode == true)
                return;

            windowAnimator.enabled = true;

            if (!windowAnimator.GetCurrentAnimatorStateInfo(0).IsName("Panel Fullscreen")
                && (!windowAnimator.GetCurrentAnimatorStateInfo(0).IsName("Panel Normalize")))
                windowAnimator.Play("Panel In");

            if (taskbarButton != null && enableMobileMode == false) { taskbarButton.SetOpen(); }
            if (windowBGBlur != null) { windowBGBlur.BlurInAnim(); }

            StartCoroutine("DisableAnimator");
        }

        public void CloseWindow()
        {
            StartCoroutine("DisableObject");

            isOn = false;
            windowAnimator.enabled = true;
            onQuitEvents.Invoke();

            if (enableMobileMode == true)
                return;

            if (enableBackgroundBlur == true && windowBGBlur != null) { windowBGBlur.BlurOutAnim(); }
            if (taskbarButton != null) { taskbarButton.SetClosed(); }

            windowAnimator.Play("Panel Out");
        }

        public void MinimizeWindow()
        {
            StopCoroutine("DisableAnimator");

            windowAnimator.enabled = true;
            windowAnimator.Play("Panel Minimize");

            if (taskbarButton != null) { taskbarButton.SetMinimized(); }
            if (enableBackgroundBlur == true && windowBGBlur != null) { windowBGBlur.BlurOutAnim(); }

            StartCoroutine("DisableAnimator");
        }

        public void FullscreenWindow()
        {
            StopCoroutine("DisableAnimator");
            windowAnimator.enabled = true;

            if (isFullscreen == false)
            {
                isFullscreen = true;
                isNormalized = false;

                if (fullscreenImage != null && normalizeImage != null)
                {
                    fullscreenImage.SetActive(false);
                    normalizeImage.SetActive(true);
                }

                StartCoroutine("SetFullscreen");
            }

            else
            {
                isFullscreen = false;
                isNormalized = true;

                if (fullscreenImage != null && normalizeImage != null)
                {
                    fullscreenImage.SetActive(true);
                    normalizeImage.SetActive(false);
                }

                StartCoroutine("SetNormalized");
            }

            StartCoroutine("DisableAnimator");
        }

        public void FocusToWindow() { gameObject.transform.SetAsLastSibling(); }
        public virtual void OnPointerClick(PointerEventData eventData) { FocusToWindow(); }

        IEnumerator SetFullscreen()
        {
            left = windowRect.offsetMin.x;
            right = -windowRect.offsetMax.x;
            top = -windowRect.offsetMax.y;
            bottom = windowRect.offsetMin.y;

            windowAnimator.Play("Panel Fullscreen");

            // Left and bottom
            windowRect.offsetMin = new Vector2(0, 0);

            // Right and top
            windowRect.offsetMax = new Vector2(0, 0);

            isFullscreen = true;
            isNormalized = false;

            if (windowResize != null)
                windowResize.gameObject.SetActive(false);

            yield return null;
        }

        IEnumerator SetNormalized()
        {
            windowAnimator.Play("Panel Normalize");

            // Left and bottom
            windowRect.offsetMin = new Vector2(left, bottom);

            // Right and top
            windowRect.offsetMax = new Vector2(-right, -top);

            isFullscreen = false;
            isNormalized = true;

            if (windowResize != null)
                windowResize.gameObject.SetActive(true);

            yield return null;
        }

        IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(0.5f);
            windowAnimator.enabled = false;
        }
    }
}
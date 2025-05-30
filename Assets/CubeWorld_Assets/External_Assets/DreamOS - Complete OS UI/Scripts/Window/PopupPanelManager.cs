using UnityEngine;

namespace Michsky.DreamOS
{
    public class PopupPanelManager : MonoBehaviour
    {
        [Header("Settings")]
        public bool enableBlurAnim = true;
        public bool useTransition = true;
        public bool disableOnOut = true;

        [Header("Animation")]
        public DefaultState defaultPanelState;
        public AnimationDirection animationDirection;
        [Range(1, 25)] public float transitionSmoothness = 10;
        [Range(1, 25)] public float sizeSmoothness = 15;
        public float closeOn = 25;
        public float panelSize = 100;

        RectTransform objectRect;
        CanvasGroup objectCG;
        BlurManager bManager;
        bool isInTransition = false;
        [HideInInspector] public bool isOn;

        public enum DefaultState { Minimized, Expanded }
        public enum AnimationDirection { Vertical, Horizontal }

        void Awake()
        {
            objectRect = gameObject.GetComponent<RectTransform>();

            if (useTransition == true)
            {
                objectCG = gameObject.GetComponent<CanvasGroup>();
                objectCG.alpha = 0;
                objectCG.interactable = false;
                objectCG.blocksRaycasts = false;
            }

            if (defaultPanelState == DefaultState.Minimized)
            {
                if (animationDirection == AnimationDirection.Vertical) { objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, closeOn); }
                else { objectRect.sizeDelta = new Vector2(closeOn, objectRect.sizeDelta.y); }
            }

            else if (defaultPanelState == DefaultState.Expanded)
            {
                if (animationDirection == AnimationDirection.Vertical) { objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, panelSize); }
                else { objectRect.sizeDelta = new Vector2(panelSize, objectRect.sizeDelta.y); }

                objectCG.alpha = 1;
                objectCG.interactable = true;
                objectCG.blocksRaycasts = true;
            }

            if (enableBlurAnim == true) { bManager = gameObject.GetComponent<BlurManager>(); }
            if (isOn == false && disableOnOut == true && defaultPanelState != DefaultState.Expanded) { gameObject.SetActive(false); }
        }

        void Update()
        {
            if (isInTransition == false)
                return;

            ProcessPopup();
        }

        void ProcessPopup()
        {
            if (animationDirection == AnimationDirection.Vertical)
            {
                if (isOn == true)
                {
                    if (useTransition == true) { objectCG.alpha += Time.deltaTime * transitionSmoothness; }
                    objectRect.sizeDelta = Vector2.Lerp(objectRect.sizeDelta, new Vector2(objectRect.sizeDelta.x, panelSize), Time.deltaTime * sizeSmoothness);

                    if (useTransition == true && objectRect.sizeDelta.y >= panelSize - 0.1f && objectCG.alpha >= 1) { isInTransition = false; }
                    else if (objectRect.sizeDelta.y >= panelSize - 0.1f) { isInTransition = false; }
                }

                else
                {
                    if (useTransition == true) { objectCG.alpha -= Time.deltaTime * transitionSmoothness; }
                    objectRect.sizeDelta = Vector2.Lerp(objectRect.sizeDelta, new Vector2(objectRect.sizeDelta.x, closeOn), Time.deltaTime * sizeSmoothness);

                    if (useTransition == true && objectRect.sizeDelta.y <= closeOn + 0.1f && objectCG.alpha <= 0)
                    {
                        isInTransition = false;
                        if (disableOnOut == true) { gameObject.SetActive(false); }
                    }

                    else if (objectRect.sizeDelta.y <= closeOn + 0.1f)
                    {
                        isInTransition = false;
                        if (disableOnOut == true) { gameObject.SetActive(false); }
                    }
                }
            }

            else
            {
                if (isOn == true)
                {
                    if (useTransition == true) { objectCG.alpha += Time.deltaTime * transitionSmoothness; }
                    objectRect.sizeDelta = Vector2.Lerp(objectRect.sizeDelta, new Vector2(panelSize, objectRect.sizeDelta.y), Time.deltaTime * sizeSmoothness);

                    if (useTransition == true && objectRect.sizeDelta.x >= panelSize - 0.1f && objectCG.alpha >= 1) { isInTransition = false; }
                    else if (objectRect.sizeDelta.x >= panelSize - 0.1f) { isInTransition = false; }
                }

                else
                {
                    if (useTransition == true) { objectCG.alpha -= Time.deltaTime * transitionSmoothness; }
                    objectRect.sizeDelta = Vector2.Lerp(objectRect.sizeDelta, new Vector2(closeOn, objectRect.sizeDelta.y), Time.deltaTime * sizeSmoothness);

                    if (useTransition == true && objectRect.sizeDelta.x <= closeOn + 0.1f && objectCG.alpha <= 0)
                    {
                        isInTransition = false;
                        if (disableOnOut == true) { gameObject.SetActive(false); }
                    }

                    else if (objectRect.sizeDelta.x <= closeOn + 0.1f)
                    {
                        isInTransition = false;
                        if (disableOnOut == true) { gameObject.SetActive(false); }
                    }
                }
            }
        }

        public void AnimatePanel()
        {
            gameObject.SetActive(true);
            isInTransition = true;

            if (isOn == true)
            {
                if (useTransition == true) { objectCG.blocksRaycasts = false; objectCG.interactable = false; }
                if (enableBlurAnim == true) { bManager.BlurOutAnim(); }

                isOn = false;
            }

            else if (isOn == false)
            {
                if (useTransition == true) { objectCG.blocksRaycasts = true; objectCG.interactable = true; }
                if (enableBlurAnim == true) { bManager.BlurInAnim(); }

                isOn = true;
            }
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);

            if (useTransition == true) { objectCG.blocksRaycasts = true; objectCG.interactable = true; }
            if (enableBlurAnim == true) { bManager.BlurInAnim(); }

            isOn = true;
            isInTransition = true;
        }

        public void ClosePanel()
        {
            if (useTransition == true) { objectCG.blocksRaycasts = false; objectCG.interactable = false; }
            if (enableBlurAnim == true) { bManager.BlurOutAnim(); }

            isOn = false;
            isInTransition = true;
        }

        public void InstantMinimized()
        {
            if (objectRect == null || objectCG == null)
                return;

            objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, closeOn);
            objectCG.alpha = 0;
        }

        public void InstantExpanded()
        {
            if (objectRect == null || objectCG == null)
                return;

            objectRect.sizeDelta = new Vector2(objectRect.sizeDelta.x, panelSize);
            objectCG.alpha = 1;
        }
    }
}
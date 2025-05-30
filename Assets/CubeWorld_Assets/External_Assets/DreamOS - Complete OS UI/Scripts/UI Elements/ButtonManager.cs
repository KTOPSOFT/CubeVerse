using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    public class ButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        // Content
        public string buttonText = "Button";
        public Sprite buttonIcon;
        public AudioClip hoverSound;
        public AudioClip clickSound;
        public Button targetButton;

        // Resources
        public TextMeshProUGUI normalText;
        public TextMeshProUGUI highlightedText;
        public TextMeshProUGUI pressedText;
        public Image normalIcon;
        public Image highlightedIcon;
        public Image pressedIcon;
        public AudioSource soundSource;
        public GameObject rippleParent;
        public HoverEffect hoverEffect;
        public ContentSizeFitter contentSizeFitter;
        public HorizontalLayoutGroup normalLayout;
        public HorizontalLayoutGroup highlightedLayout;

        // Settings
        public AnimationSolution animationSolution = AnimationSolution.Animator;
        [Range(1, 30)] public int contentPadding = 15;
        [Range(1, 25)] public int contentSpacing = 10;
        [Range(0.25f, 15)] public float fadingMultiplier = 8;
        public bool useCustomContent = false;
        public bool enableIcon = false;
        public bool enableButtonSounds = false;
        public bool useHoverSound = true;
        public bool useClickSound = true;
        public bool useRipple = true;
        public bool useHoverEffect = false;
        public bool autoFitContent = true;

        // Ripple
        public RippleUpdateMode rippleUpdateMode = RippleUpdateMode.UnscaledTime;
        public Sprite rippleShape;
        [Range(0.1f, 8)] public float speed = 1f;
        [Range(0.5f, 25)] public float maxSize = 4f;
        public Color startColor = new Color(1f, 1f, 1f, 1f);
        public Color transitionColor = new Color(1f, 1f, 1f, 1f);
        public bool renderOnTop = false;
        public bool centered = false;

        // Hover Effect
        [Range(1f, 25)] public float heSpeed = 10f;
        [Range(0.1f, 25)] public float heSize = 2f;
        public Sprite heShape;
        [Range(0, 1)] public float heTransitionAlpha = 0.1f;

        // Events
        public UnityEvent onClick;

        bool isPointerOn;
        float currentNormalValue;
        float currenthighlightedValue;
        CanvasGroup normalCG;
        CanvasGroup highlightedCG;

        public enum AnimationSolution { Animator, Script }
        public enum RippleUpdateMode { Normal, UnscaledTime }

        void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                if (useCustomContent == false) { UpdateUI(); }
                return;
            }
#endif

            CheckForEssentials();
            maxSize = Mathf.Clamp(maxSize, 0.5f, 1000f);

            // If ripple is disabled, destroy it
            if (useRipple == true && rippleParent != null) { rippleParent.SetActive(false); }
            else if (useRipple == false && rippleParent != null) { Destroy(rippleParent); }

            if (useHoverEffect == true && hoverEffect != null) { hoverEffect.gameObject.SetActive(false); }
            if (useCustomContent == false) { UpdateUI(); }
            if (targetButton != null) { StartCoroutine("FixLayout"); }
        }

        void CheckForEssentials()
        {
            if (targetButton == null)
            {
                try { targetButton = gameObject.GetComponent<Button>(); }
                catch { gameObject.AddComponent<Button>(); targetButton = gameObject.GetComponent<Button>(); }
            }

            if (targetButton != null) { targetButton.onClick.AddListener(() => onClick.Invoke()); }

            if (animationSolution == AnimationSolution.Script)
            {
                try
                {
                    if (normalCG == null) { normalCG = transform.Find("Normal").GetComponent<CanvasGroup>(); }
                    if (highlightedCG == null) { highlightedCG = transform.Find("Highlighted").GetComponent<CanvasGroup>(); }

                    normalCG.alpha = 1;
                    highlightedCG.alpha = 0;

                    Animator tempAnimator = this.GetComponent<Animator>();
                    Destroy(tempAnimator);
                }

                catch { animationSolution = AnimationSolution.Animator; }
            }
        }

        public void UpdateUI()
        {
            if (normalText != null) { normalText.text = buttonText; }
            if (highlightedText != null) { highlightedText.text = buttonText; }
            if (pressedText != null) { pressedText.text = buttonText; }
            if (enableIcon == true) 
            {
                if (normalIcon != null) { normalIcon.sprite = buttonIcon; }
                if (highlightedIcon != null) { highlightedIcon.sprite = buttonIcon; }
                if (pressedIcon != null) { pressedIcon.sprite = buttonIcon; }
            }

            if (autoFitContent == false && contentSizeFitter != null) { contentSizeFitter.enabled = false; }
            else if (autoFitContent == true && contentSizeFitter != null) { contentSizeFitter.enabled = true; }

            if (normalLayout != null & highlightedLayout != null)
            {
                normalLayout.padding.left = contentPadding;
                normalLayout.padding.right = contentPadding;
                highlightedLayout.padding.left = contentPadding;
                highlightedLayout.padding.right = contentPadding;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        }

        public void CreateRipple(Vector2 pos)
        {
            // If Ripple Parent is assigned, create the object and get the necessary components
            if (rippleParent != null)
            {
                GameObject rippleObj = new GameObject();
                Image trImage = rippleObj.AddComponent<Image>();
                trImage.raycastTarget = false;
                trImage.sprite = rippleShape;
                rippleObj.name = "Ripple";
                rippleParent.SetActive(true);
                rippleObj.transform.SetParent(rippleParent.transform);

                if (renderOnTop == true) { rippleParent.transform.SetAsLastSibling(); }

                if (centered == true) { rippleObj.transform.localPosition = new Vector2(0f, 0f); }
                else { rippleObj.transform.position = pos; }

                rippleObj.AddComponent<Ripple>();
                Ripple tempRipple = rippleObj.GetComponent<Ripple>();
                tempRipple.speed = speed;
                tempRipple.maxSize = maxSize;
                tempRipple.startColor = startColor;
                tempRipple.transitionColor = transitionColor;

                if (rippleUpdateMode == RippleUpdateMode.Normal) { tempRipple.unscaledTime = false; }
                else { tempRipple.unscaledTime = true; }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (useRipple == true && isPointerOn == true)
#if ENABLE_LEGACY_INPUT_MANAGER
                CreateRipple(Input.mousePosition);
#elif ENABLE_INPUT_SYSTEM
                CreateRipple(Mouse.current.position.ReadValue());
#endif

            if (enableButtonSounds == true && useClickSound == true && targetButton.interactable == true)
                soundSource.PlayOneShot(clickSound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Process On Pointer Enter events
            isPointerOn = true;

            if (enableButtonSounds == true && useHoverSound == true && targetButton.interactable == true)
                soundSource.PlayOneShot(hoverSound);

            if (animationSolution == AnimationSolution.Script && targetButton.interactable == true)
                StartCoroutine("FadeIn");

            if (useHoverEffect == true && hoverEffect != null)
            {
                hoverEffect.speed = heSpeed;
                hoverEffect.targetImage.sprite = heShape;
                hoverEffect.targetImage.transform.localScale = new Vector3(heSize, heSize, heSize);
                hoverEffect.transitionAlpha = heTransitionAlpha;
                hoverEffect.gameObject.SetActive(true);
                hoverEffect.fadeOut = false;
                hoverEffect.fadeIn = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Process On Pointer Exit events
            isPointerOn = false;

            if (animationSolution == AnimationSolution.Script && targetButton.interactable == true)
                StartCoroutine("FadeOut");

            if (useHoverEffect == true && hoverEffect != null)
            {
                hoverEffect.fadeIn = false;
                hoverEffect.fadeOut = true;
            }
        }

        IEnumerator FadeIn()
        {
            StopCoroutine("FadeOut");
            currentNormalValue = normalCG.alpha;
            currenthighlightedValue = highlightedCG.alpha;

            while (currenthighlightedValue <= 1)
            {
                currentNormalValue -= Time.deltaTime * fadingMultiplier;
                normalCG.alpha = currentNormalValue;

                currenthighlightedValue += Time.deltaTime * fadingMultiplier;
                highlightedCG.alpha = currenthighlightedValue;

                if (normalCG.alpha >= 1) { StopCoroutine("FadeIn"); }
                yield return null;
            }
        }

        IEnumerator FadeOut()
        {
            StopCoroutine("FadeIn");
            currentNormalValue = normalCG.alpha;
            currenthighlightedValue = highlightedCG.alpha;

            while (currentNormalValue >= 0)
            {
                currentNormalValue += Time.deltaTime * fadingMultiplier;
                normalCG.alpha = currentNormalValue;

                currenthighlightedValue -= Time.deltaTime * fadingMultiplier;
                highlightedCG.alpha = currenthighlightedValue;

                if (highlightedCG.alpha <= 0) { StopCoroutine("FadeOut"); }
                yield return null;
            }
        }

        IEnumerator FixLayout()
        {
            yield return new WaitForSecondsRealtime(0.05f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetButton.GetComponent<RectTransform>());
        }
    }
}
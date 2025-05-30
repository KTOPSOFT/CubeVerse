using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    public class WorldSpaceManager : MonoBehaviour
    {
        // Resources
        public Camera mainCamera;
        public Camera projectorCam;
        public RawImage rendererImage;
        public Transform enterMount;
        public Canvas osCanvas;
        public FloatingIconManager useFloatingIcon;

        // Settings
        public bool requiresOpening = true;
        public bool autoGetIn = false;
        public bool lockCursorWhenOut = false;
        public bool dynamicRTSize = true;
        public int rtWidth = 1920;
        public int rtHeight = 1080;
        public string playerTag = "Player";
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode getInKey = KeyCode.E;
        public KeyCode getOutKey = KeyCode.Escape;
#elif ENABLE_INPUT_SYSTEM
        public InputAction getInKey;
        public InputAction getOutKey;
#endif
        [Range(0.1f, 50f)] public float transitionSpeed = 10f;
        public float transitionInTimer = 0.7f;
        public float transitionOutTimer = 0.55f;
        public TransitionMode transitionMode;

        // Events
        public UnityEvent onEnter;
        public UnityEvent onEnterEnd;
        public UnityEvent onExit;
        public UnityEvent onExitEnd;

        // Hidden & Helpers
        public int selectedTagIndex = 0;
        public bool isInSystem = false;
        bool isInTrigger = false;
        bool enableCameraIn = false;
        bool enableCameraOut = false;
        bool takenStaticRootPos;

        [HideInInspector] public RenderTexture uiRT;
        CanvasGroup osCG;
        Quaternion camRotHelper;
        Vector3 targetRootPos = new Vector3(0, 0, 0);

        public enum TransitionMode { Static, Dynamic }

        void Awake()
        {
            if (dynamicRTSize == true) { uiRT = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, RenderTextureFormat.RGB111110Float); }
            else { uiRT = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.RGB111110Float); }

            if (projectorCam == null) 
            { 
                Debug.LogError("<b>[DreamOS]</b> Projector Camera is missing but it's essential.");
                return; 
            }

            projectorCam.targetTexture = uiRT;
            projectorCam.enabled = true;

            if (rendererImage != null) { rendererImage.texture = uiRT; }
            else { Debug.LogWarning("<b>[DreamOS]</b> Renderer Image is missing. The system will work but won't be rendered."); }

            osCG = osCanvas.GetComponent<CanvasGroup>();
            osCG.interactable = false;
            osCG.blocksRaycasts = false;

            if (requiresOpening == true) { osCanvas.gameObject.SetActive(false); }
            if (mainCamera == null) { mainCamera = Camera.main; }
        }

        void Update()
        {
            if (isInTrigger == false && isInSystem == true)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(getOutKey)) { TransitionOutHelper(); }
#elif ENABLE_INPUT_SYSTEM
                if (getOutKey.triggered) { TransitionOutHelper(); }
#endif
            }

            else if (isInSystem == false)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(getInKey)) { TransitionInHelper(); }
#elif ENABLE_INPUT_SYSTEM
                if (getInKey.triggered) { TransitionInHelper(); }
#endif
            }

            if (enableCameraIn == true)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, enterMount.position, transitionSpeed * Time.deltaTime);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, enterMount.rotation, transitionSpeed * Time.deltaTime);
            }

            else if (enableCameraOut == true)
            {
                mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, targetRootPos, transitionSpeed * Time.deltaTime);
                mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, camRotHelper, transitionSpeed * Time.deltaTime);
            }
        }

        public void EnableCamera(bool value) { mainCamera.enabled = value; }
        public void GetOut() { TransitionOutHelper(); }
        public void GetIn() { TransitionInHelper(); }

        void TransitionOutHelper()
        {
            onExit.Invoke();
            osCG.interactable = false;
            osCG.blocksRaycasts = false;

            Cursor.visible = false;
            if (lockCursorWhenOut == true) { Cursor.lockState = CursorLockMode.Locked; }

            osCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            projectorCam.targetTexture = uiRT;
            projectorCam.enabled = true;
            enableCameraOut = true;

            StopCoroutine("TransitionIn");
            StopCoroutine("TransitionOut");
            StartCoroutine("TransitionOut");
        }

        IEnumerator TransitionOut()
        {
            yield return new WaitForSeconds(transitionOutTimer);

            enableCameraIn = false;
            enableCameraOut = false;
            isInSystem = false;
            isInTrigger = true;

            onExitEnd.Invoke();
        }

        void TransitionInHelper()
        {
            if (isInTrigger == false || isInSystem == true)
                return;

            onEnter.Invoke();

            osCG.interactable = true;
            osCG.blocksRaycasts = true;
            osCanvas.gameObject.SetActive(true);

            if (transitionMode == TransitionMode.Dynamic) { targetRootPos = mainCamera.transform.position; }
            else if (transitionMode == TransitionMode.Static && takenStaticRootPos == false)
            {
                targetRootPos = mainCamera.transform.localPosition;
                takenStaticRootPos = true;
            }

            camRotHelper = mainCamera.transform.localRotation;

            enableCameraOut = false;
            enableCameraIn = true;
            isInTrigger = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            StopCoroutine("TransitionIn");
            StopCoroutine("TransitionOut");
            StartCoroutine("TransitionIn");
        }

        IEnumerator TransitionIn()
        {
            yield return new WaitForSeconds(transitionInTimer);

            osCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            projectorCam.enabled = false;

            enableCameraIn = false;
            enableCameraOut = false;
            isInSystem = true;

            onEnterEnd.Invoke();
        }


        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == playerTag)
            {
                isInTrigger = true;
                if (autoGetIn == true) { TransitionInHelper(); }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == playerTag) { isInTrigger = false; }
        }
    }
}
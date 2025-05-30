using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(AudioSource))]
    public class KeystrokeManager : MonoBehaviour
    {
        [Header("Strokes")]
        public List<AudioClip> keyboardStrokes = new List<AudioClip>();
        public List<AudioClip> mouseStrokes = new List<AudioClip>();

        [Header("Settings")]
        public AudioSource strokeSource;
        public bool enableStrokes = true;
        public bool enableKeyboard = true;
        public bool enableMouse = true;

        void Awake()
        {
            if (strokeSource == null)
                strokeSource = gameObject.GetComponent<AudioSource>();
        }

        void Update()
        {
            if (enableStrokes == false || Time.timeScale == 0)
                return;

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2))
                PlayMouseStroke();
            else if (Input.anyKeyDown)
                PlayKeyboardStroke();
#elif ENABLE_INPUT_SYSTEM
            if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                PlayMouseStroke();
            else if (Keyboard.current.anyKey.wasPressedThisFrame)
                PlayKeyboardStroke();
#endif
        }

        public void PlayKeyboardStroke()
        {
            if (enableKeyboard == true)
                strokeSource.PlayOneShot(keyboardStrokes[Random.Range(0, keyboardStrokes.Count)]);
        }

        public void PlayMouseStroke()
        {
            if (enableMouse == true)
                strokeSource.PlayOneShot(mouseStrokes[Random.Range(0, mouseStrokes.Count)]);
        }

        public void EnableStrokes(bool bValue)
        {
            if (bValue == true) { enableStrokes = true; this.enabled = true; }
            else { enableStrokes = false; this.enabled = false; }
        }
    }
}
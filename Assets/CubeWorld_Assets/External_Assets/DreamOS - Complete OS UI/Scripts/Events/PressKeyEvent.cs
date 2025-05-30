using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Events/Press Key")]
    public class PressKeyEvent : MonoBehaviour
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode hotkey;
#elif ENABLE_INPUT_SYSTEM
        public InputAction hotkey;
#endif
        public bool pressAnyKey;

        // Events
        public UnityEvent pressAction;

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER

            if (pressAnyKey == true && Input.anyKeyDown)
                pressAction.Invoke();
            else if (pressAnyKey == false && Input.GetKeyDown(hotkey))
                pressAction.Invoke();

#elif ENABLE_INPUT_SYSTEM

            if (pressAnyKey == true && Keyboard.current.anyKey.wasPressedThisFrame)
                pressAction.Invoke();
            else if (pressAnyKey == false && hotkey.triggered)
                pressAction.Invoke();

#endif
        }
    }
}
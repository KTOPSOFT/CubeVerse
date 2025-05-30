using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Events/Hold Key Event")]
    public class HoldKeyEvent : MonoBehaviour
    {
        // Settings
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode hotkey;
#elif ENABLE_INPUT_SYSTEM
        public InputAction hotkey;
#endif
        // Events
        public UnityEvent holdAction;
        public UnityEvent releaseAction;

        // Events
        [HideInInspector] public bool isOn = false;
        [HideInInspector] public bool isHolding = false;

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER

            if (Input.GetKey(hotkey))
            {
                isHolding = true;
                isOn = false;
            }

            else
            {
                isHolding = false;
                isOn = true;
            }

            if (isOn == true && isHolding == false)
            {
                releaseAction.Invoke();
                isHolding = false;
                isOn = false;
            }

            else if (isOn == false && isHolding == true)
            {
                holdAction.Invoke();
                isHolding = true;
            }

#elif ENABLE_INPUT_SYSTEM

            if (hotkey.triggered)
            {
                isHolding = true;
                isOn = false;
            }

            else
            {
                isHolding = false;
                isOn = true;
            }

            if (isOn == true && isHolding == false)
            {
                releaseAction.Invoke();
                isHolding = false;
                isOn = false;
            }

            else if (isOn == false && isHolding == true)
            {
                holdAction.Invoke();
                isHolding = true;
            }

#endif
        }
    }
}
using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MalbersAnimations.InputSystem
{
#if ENABLE_INPUT_SYSTEM
    [System.Serializable]
    public struct FastInput
    {
        public string name;
        public bool debug;
        public InputAction input;
        [Space]
        public BoolEvent OnInputPressed;
        public UnityEvent OnInputDown;
        public UnityEvent OnInputUp;

        public readonly void InputAction(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                OnInputDown.Invoke();
                OnInputPressed.Invoke(true);

                if (debug) Debug.Log($"Input:{name} Pressed");

            }
            else if (context.canceled)
            {
                OnInputUp.Invoke();
                OnInputPressed.Invoke(false);

                if (debug) Debug.Log($"Input:{name} Released");
            }
        }
    }

    [AddComponentMenu("Malbers/Input/Fast Input")]
    public class MFastInput : MonoBehaviour
    {
        [NonReorderable]
        public FastInput[] inputs;

        private void OnEnable()
        {
            if (inputs != null || inputs.Length > 0)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    inputs[i].input.Enable();
                    inputs[i].input.started += inputs[i].InputAction;
                    inputs[i].input.canceled += inputs[i].InputAction;

                    if (inputs[i].OnInputPressed == null) inputs[i].OnInputPressed = new();
                    if (inputs[i].OnInputDown == null) inputs[i].OnInputDown = new();
                    if (inputs[i].OnInputUp == null) inputs[i].OnInputUp = new();
                }
            }
        }

        private void OnDisable()
        {
            if (inputs != null || inputs.Length > 0)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    inputs[i].input.started -= inputs[i].InputAction;
                    inputs[i].input.canceled -= inputs[i].InputAction;
                    inputs[i].input.Disable();
                }
            }
        }
    }
#endif
}

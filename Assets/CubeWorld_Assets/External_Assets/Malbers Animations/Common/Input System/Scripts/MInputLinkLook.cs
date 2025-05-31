using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
#endif


namespace MalbersAnimations.InputSystem
{
    // [HelpURL("https://malbersanimations.gitbook.io/animal-controller/annex/integrations/unity-input-system-new#input-link-ui")]
    [AddComponentMenu("Malbers/Input/MInput Look")]
    public class MInputLinkLook : MonoBehaviour//, AxisState.IInputAxisProvider
    {
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Leave this at -1 for single-player games.
        /// For multi-player games, set this to be the player index, and the actions will
        /// be read from that player's controls
        /// </summary>
        [Tooltip("Leave this at -1 for single-player games.  "
            + "For multi-player games, set this to be the player index, and the actions will "
            + "be read from that player's controls")]
        public int PlayerIndex = -1;

        /// <summary>If set, Input Actions will be auto-enabled at start</summary>
        [Tooltip("If set, Input Actions will be auto-enabled at start")]
        public bool AutoEnableInputs = true;

        /// <summary>Vector2 action for XY movement</summary>
        [Tooltip("Vector2 action for XY movement")]
        public InputActionReference LookAxis;
        /// <summary>Float action for Z movement</summary>
        [Tooltip("Float action for Z movement")]
        public InputActionReference Zoom;

        [Tooltip("Camera Input Values (Look X:Horizontal, Look Y: Vertical)")]
        public Vector2Reference look = new();

        [Tooltip("Camera Input Values (Look X:Horizontal, Look Y: Vertical)")]
        public BoolReference IgnoreOnPause = new();

        public Vector2Event OnLookValue = new();

        public FloatEvent OnZoomValue = new();

        //[Tooltip("Name of the Mouse Scheme to check if the player is using the Mouse and Keyboard")]
        //public string MouseScheme = "Mouse";
        //public BoolEvent OnMouseScheme = new();

        private InputAction m_cachedLook;
        private InputAction m_cachedZoom;

        /// <summary>
        /// In a multi-player context, actions are associated with specific players
        /// This resolves the appropriate action reference for the specified player.
        /// Because the resolution involves a search, we also cache the returned 
        /// action to make future resolutions faster.
        /// </summary>
        /// <param name="axis">Which input axis (0, 1, or 2)</param>
        /// <param name="actionRef">Which action reference to resolve</param>
        /// <returns>The cached action for the player specified in PlayerIndex</returns>
        protected InputAction ResolveForPlayer(InputAction cache, InputActionReference actionRef)
        {
            if (actionRef == null || actionRef.action == null)
                return null;

            if (cache != null && actionRef.action.id != cache.id)
                cache = null;

            if (cache == null)
            {
                cache = actionRef.action;

                // Debug.Log($"Player Index {PlayerIndex}");

                if (PlayerIndex != -1 && InputUser.all.Count > 0)
                    cache = GetFirstMatch(InputUser.all[PlayerIndex], actionRef);

                if (AutoEnableInputs && actionRef != null && actionRef.action != null)
                    actionRef.action.Enable();
            }

            // Update enabled status
            if (cache != null && cache.enabled != actionRef.action.enabled)
            {
                if (actionRef.action.enabled)
                    cache.Enable();
                else
                    cache.Disable();
            }

            return cache;


            // local function to wrap the lambda which otherwise causes a tiny gc
            InputAction GetFirstMatch(in InputUser user, InputActionReference aRef)
            {
                foreach (var x in user.actions)
                {
                    if (x.id == aRef.action.id)
                        return x;
                }

                Debug.LogWarning($"Action Reference [{aRef.action.name}] Not Found. Make sure the Player is Using the Same Action MAP", this);
                return null;
            }
        }

        private InputAction lookXY;
        private InputAction zoom;

        public void OnEnable()
        {
            //Find firt Player Input
            var PlayerInput = GetComponentInParent<PlayerInput>();
            if (PlayerInput != null)
            {
                PlayerIndex = PlayerInput.playerIndex;
            }
            else if (PlayerIndex == -1)
            {
                PlayerIndex = 0;
            }

            lookXY = ResolveForPlayer(m_cachedLook, LookAxis);
            zoom = ResolveForPlayer(m_cachedZoom, Zoom);


            if (lookXY != null)
            {
                lookXY.performed += ReadLook;
                lookXY.canceled += ReadLook;
            }

            if (zoom != null)
            {
                zoom.performed += ReadZoom;
                // zoom.canceled += ReadZoom;
            }

            // InputUser.onChange += OnUserChange;
        }

        //private void OnUserChange(InputUser user, InputUserChange change, InputDevice device)
        //{
        //    if (change == InputUserChange.ControlsChanged && user.index == PlayerIndex)
        //    {
        //        // Debug.Log(user.controlScheme.Value.name);
        //        OnMouseScheme.Invoke(user.controlScheme.Value.name.ToLower().Contains(MouseScheme.ToLower()));
        //    }
        //}

        protected virtual void OnDisable()
        {
            if (lookXY != null)
            {
                lookXY.performed -= ReadLook;
                lookXY.canceled -= ReadLook;
            }

            if (zoom != null)
            {
                zoom.performed -= ReadZoom;
                //  zoom.canceled -= ReadZoom;
            }


            m_cachedLook = null;
            m_cachedZoom = null;

            //InputUser.onChange -= OnUserChange;
        }


        private void ReadLook(InputAction.CallbackContext context)
        {
            look.Value = Vector2.zero;

            if (!IgnoreOnPause || Time.timeScale != 0)
            {
                look.Value = context.ReadValue<Vector2>();
            }
            OnLookValue.Invoke(look.Value);
        }


        private void ReadZoom(InputAction.CallbackContext context)
        {
            var LookValue = 0f;

            if (!IgnoreOnPause || Time.timeScale != 0 && context.valueType == typeof(Vector2))
            {
                LookValue = context.ReadValue<Vector2>().y;
            }

            if (LookValue != 0)
                OnZoomValue.Invoke(LookValue);
        }

        public void SetPlayerIndex(int index)
        {
            PlayerIndex = index;

            //Reset the Player Connection with the Camera
            OnDisable();
            OnEnable();
        }


#if UNITY_EDITOR
        private void Reset()
        {
            look.Variable = MTools.GetInstance<Scriptables.Vector2Var>("Look Camera Input");

            if (look.Variable != null) look.UseConstant = false;

            var SetLook = this.GetUnityAction<Vector2>("ThirdPersonFollowTarget", "SetLook");
            if (SetLook != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnLookValue, SetLook);

            //var SetMouse = this.GetUnityAction<bool>("ThirdPersonFollowTarget", "SetMouse");
            //if (SetMouse != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnMouseScheme, SetMouse);

            var SeZoom = this.GetUnityAction<float>("ThirdPersonFollowZoom", "SetZoom");
            if (SeZoom != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnZoomValue, SeZoom);
        }
#endif

#endif
    }
}

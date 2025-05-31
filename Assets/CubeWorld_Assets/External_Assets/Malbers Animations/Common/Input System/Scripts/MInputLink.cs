using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

#if ENABLE_INPUT_SYSTEM
using static UnityEngine.InputSystem.PlayerInputManager;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Users;
#endif

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.InputSystem
{
    [AddComponentMenu("Input/MInput Link [New Input System]")]
    [DisallowMultipleComponent]
    public partial class MInputLink : MonoBehaviour, IInputSource
    {
#if ENABLE_INPUT_SYSTEM
        ///// <summary> Current Active Action Map Index</summary>
        public static List<MInputLink> MInputLinks { get; protected set; }

        public readonly string versionInput = "Link between the New Unity Input System and any component using Unity Events";
        public Action<Vector3> OnMoveAxis { get; set; } = delegate { };

        [RequiredField]
        public PlayerInput playerInput;

        [Tooltip("If the Input is disabled, clear the Player Input Reference (Useful for Mounts like the Horse)")]
        public bool clearPlayerInput = false;

        /// <summary> Check for a moveable character</summary>
        protected ICharacterMove character;
        public InputActionAsset InputActions;

        [Tooltip("Current Active Map to Activate on Enable\n if there are more Input links on the scene the last Input Link will set its Action Map as Active")]
        [SerializeField] internal int ActiveActionMapIndex;
        [HideInInspector][SerializeField] private int ShowMapIndex;


        public bool debug;
        /// <summary> Current Active Action Map Index</summary>
        public int ActiveMapIndex => ActiveActionMapIndex - 1;

        /// <summary> Current Active Action Map name</summary>
        public string ActiveMap { get; set; }

        /// <summary> Current Active Malbers Button Map (This have the List of buttons)</summary>
        public MInputActionMap ActiveMActionMap;
        public MInputActionMap DefaultMap { get; protected set; }

        /// <summary>  Used to check if the Link is already connected to the Input Actions  </summary>
        public bool Connected { get; protected set; }


        /// <summary>Current Stored movement Axis</summary>
        public Vector3 MoveAxis { get; set; }

        // public void SetMoveAxis(Vector3 move) => MoveAxis = move;
        public bool MoveCharacter { set; get; }

        /// <summary> List of Malbers Input Row Buttons</summary>
        public List<MInputActionMap> m_MapButtons;


        [SerializeField, HideInInspector] private int Editor_Tabs1;

        public bool showInputEvents = false;
        public IntEvent OnInputEnabled = new();
        public IntEvent OnInputDisabled = new();

        public StringEvent OnActionMapChanged = new();
        public StringEvent CurrentControlScheme = new();

        public PlayerJoinedEvent OnControlsChanged = new();
        public PlayerJoinedEvent OnDeviceLost = new();
        public PlayerJoinedEvent OnDeviceRegained = new();


        [Tooltip("All Inputs will be ignored on Time.Scale = 0")]
        public BoolReference IgnoreOnPause = new(true);

        private void OnUserChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (user.index == playerInput.playerIndex)
            {
#if UNITY_EDITOR
                if (debug)
                    Debug.Log($" <color=cyan><B>{playerInput.name}</B> </color> - On User Changed Index: [{user.index}]");
#endif
                //??

                switch (change)
                {
                    case InputUserChange.DeviceLost:
                        //Do stuff when the Device is lost
                        break;
                    case InputUserChange.DeviceRegained:
                        //Do stuff when the Device is Regained
                        break;
                    case InputUserChange.ControlsChanged:
                        /*
                        CurrentControlScheme.Invoke(user.controlScheme.Value.name);
                        //   CurrentControlSchemeIndex = InputActions.FindControlSchemeIndex(user.controlScheme.Value.name);
                        */
                        if (user.controlScheme != null)
                            CurrentControlScheme.Invoke(user.controlScheme.Value.name);

                        break;
                }
            }
        }


        private void ControlsChanged(PlayerInput input)
        {
#if UNITY_EDITOR
            if (debug)
                Debug.Log($" <color=cyan><B>{playerInput.name}</B> </color> - Control Changed: <B>[{input.currentControlScheme}]</B>");
#endif
            OnControlsChanged.Invoke(input);
        }

        private void DeviceLost(PlayerInput input) => OnDeviceLost.Invoke(input);

        private void DeviceRegained(PlayerInput input) => OnDeviceRegained.Invoke(input);

        private void ValidateInputActions()
        {
            if (playerInput == null) return; //Do nothing if there's no Player Inp
            if (InputActions == null) return;

            //The Map Button is different or empty
            if (m_MapButtons == null || m_MapButtons.Count == 0 && InputActions != null)
            {
                m_MapButtons = new List<MInputActionMap>();

                for (int i = 0; i < InputActions.actionMaps.Count; i++)
                {
                    m_MapButtons.Add(new MInputActionMap(InputActions.actionMaps[i], i));
                }
                MTools.SetDirty(this);
            }

            // UpdateActionMaps();

            if (!Application.isPlaying)
            {
                //Set the Active BtMap as the Default one IMPORTANT
                if (ActiveMapIndex >= 0 && m_MapButtons.Count < ActiveMapIndex)
                    ActiveMActionMap = m_MapButtons[ActiveMapIndex];
            }
        }


        protected virtual void Awake()
        {
            character = GetComponent<ICharacterMove>();     //Get the Animal Controller
            ActiveMActionMap = m_MapButtons[ActiveMapIndex];    //Set the Active BtMap as the Default one IMPORTANT
            DefaultMap = m_MapButtons[ActiveMapIndex];      //Set the Active BtMap as the Default one IMPORTANT
        }

        /// <summary>Remove the Player Input Component</summary>
        public virtual void ClearPlayerInput() => playerInput = null;

        public void PlayerInput(IInputSource player)
        {
            playerInput = player.transform.GetComponent<PlayerInput>(); //Get the Player Input from the Rider (Horse)
        }

        /// <summary>Enable Disable the Input Script</summary>
        public virtual void Enable(bool val) => enabled = val;

        // Start is called before the first frame update
        void OnEnable()
        {
            if (MInputLinks == null) MInputLinks = new();
            MInputLinks.Add(this);                                              //Save the the Animal on the current List

            if (playerInput == null) { playerInput = this.FindComponent<PlayerInput>(); } //Find Player Input inside the hierarchy (Rider Find)

            if (playerInput == null) //Second Try to find the Player Input
            {
                var allLinks = this.GetComponentsInChildren<MInputLink>();

                foreach (var link in allLinks)
                {
                    if (link.playerInput != null)
                    {
                        playerInput = link.playerInput;
                        break;
                    }
                }
            }


            if (playerInput != null && playerInput.enabled)
            {
                ConnectActionMap();
            }
            else
            {
                Debug.Log($"[{name}]. Player Input not found. MInputLink component disabled.", this);

                //Disconnect because there's no Player Input
                enabled = false;
                return;
            }

            InputUser.onChange += OnUserChange;

            if (playerInput != null)
            {
                //IMPORTANT IT NEEDS TO BE INVOKE CSHARP
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                playerInput.onControlsChanged += ControlsChanged;
                playerInput.onDeviceLost += DeviceLost;
                playerInput.onDeviceRegained += DeviceRegained;
                ControlsChanged(playerInput); //Do the first call
            }

            UpdateActiveMap();


            OnInputEnabled.Invoke(playerInput.playerIndex);



            //verify every Input to their correct expected control

            foreach (var map in m_MapButtons)
            {
                foreach (var button in map.buttons)
                {
                    if (button == null || button.action == null || button.reference == null) continue; //Make sure is not null

                    //Debug.Log($"BUTTON: {button.action.name}, Expected: {button.action.expectedControlType}");
                    if (button.interaction != MInputInteraction.Vector2 && button.action.expectedControlType == "Vector2")
                    {
                        Debug.LogWarning($"Button <{button.name}> has an action control Type of [{button.action.expectedControlType}]. 'Vector2' is expected. Please change the Interaction Type to Vector2. Disabling Button");
                        button.Active = false;
                    }
                    //else if (button.interaction == MInputInteraction.Float && button.action.expectedControlType != "")
                    //{
                    //    Debug.LogWarning($"Button <{button.name}> has an action control Type of [{button.action.expectedControlType}]. 'Float' is expected. Please change the Interaction Type to Float. Disabling Button");
                    //    button.Active = false;
                    //}
                }
            }

            MoveAxis = Vector3.zero;

            //Invoke the first time the current control scheme

            if (playerInput.user.valid && playerInput.user.controlScheme.HasValue)
                CurrentControlScheme.Invoke(playerInput.user.controlScheme.Value.name);
        }


        private void OnDisable()
        {
            //Remove all Links from the list
            MInputLinks.Remove(this);
            //I NEED TO CHECK IF ANY OTHER MLINK IS USING THE SAME ACTION MAP
            DisconnectActionMap();

            MoveAxis = Vector3.zero;
            InputUser.onChange -= OnUserChange; //Check User changes

            if (playerInput != null)
            {
                playerInput.onControlsChanged -= ControlsChanged;
                playerInput.onDeviceLost += DeviceLost;
                playerInput.onDeviceRegained += DeviceRegained;
                OnInputDisabled.Invoke(playerInput.playerIndex);
            }
            ActiveMActionMap = null;

            if (clearPlayerInput) playerInput = null; //Clear the Player Input Reference
        }

        /// <summary> Activate the Current Action Map! </summary>
        public virtual void ConnectActionMap()
        {
            if (!Connected)
            {
                UpdateActiveMap();

                //Connect all MAPS!!! no matter which one is the Active one!! 
                foreach (var map in m_MapButtons)
                {
                    if (map != null) //NULL CHECK FIRST!!
                    {
                        ConnectMove(map);
                        ConnectUpDown(map);
                        ConnectButtons(map);
                    }
                }
                Connected = true;//Update the Connection to true; This avoid to connect twice, which is bad!
            }
        }

        private void UpdateActiveMap()
        {
            ActiveMActionMap = m_MapButtons.Find(x => x.ActionMap.id == playerInput.currentActionMap.id);
            OnActionMapChanged.Invoke(ActiveMActionMap.ActionMap.name); //Invoke the Active Action Map
        }

        public virtual void SwitchActionMap(string map)
        {
            if (string.IsNullOrEmpty(map)) return;

            string debugval = "";

            //Switch the Map in the Player Input if is not already switched
            if (playerInput != null && playerInput.currentActionMap.name != map)
            {
                playerInput.SwitchCurrentActionMap(map);
                PlayerMap = playerInput.currentActionMap;
                playerInput.defaultActionMap = PlayerMap.id.ToString();
                OnActionMapChanged.Invoke(map);
                UpdateActiveMap();
                Debug.Log($"Action Map Switched <B>[{map}]</B>");
            }
            #region Old

            ////First we need to Disconnect the old Active Input MAP if there's any
            //if (!string.IsNullOrEmpty(ActiveMap))
            //{
            //    if (map == ActiveMap) return; //Means we are trying to activate the same Input link, so there's no need to Swap

            //    //Find the Old Action Map
            //    var oldMap = InputActions.FindActionMap(ActiveMap);

            //    if (oldMap != null)
            //    {
            //        oldMap.Disable();
            //        debugval = $"<B>[{ActiveMap}] Map</B> Disabled.";
            //        string linksC = "";


            //        //Disconnect all the Input Links that are using this Input Action
            //        foreach (var link in MInputLinks)
            //        {
            //            //Update all the MInput Links that have the same Input Link
            //            if (link.isActiveAndEnabled && link.InputActions == InputActions)      
            //            {
            //                link.DisconnectActionMap();
            //                linksC += $"[{link.name}]";
            //            }
            //        }
            //        Debuggin($"Disconnect <B>[{ActiveMap}] Map</B> from <B> {linksC} </B> buttons");
            //    }
            //}

            ////Next we need to connect the new Active Input MAP


            //var newMap = InputActions.FindActionMap(map);

            //if (newMap != null)
            //{
            //    ActiveMap = newMap.name; //Store the Active Action MAP

            //    var ActiveActionMapIndex = InputActions.actionMaps.IndexOf(act => act.id == newMap.id) + 1; //Find the Index +1 because 0 is None

            //    newMap.Enable(); //Enable the New Map
            //    debugval += $"<B>[{ActiveMap}] Map</B> Enabled";

            //    string linksC = "";

            //    if (MInputLinks != null)
            //    {
            //        foreach (var link in MInputLinks)                              //Connect all Maps
            //        {
            //            if (link.isActiveAndEnabled && link.InputActions == InputActions)     //Update all the MInput Links that have the same Input Link
            //            {

            //                link.ActiveMap = ActiveMap;
            //                link.ActiveActionMapIndex = ActiveActionMapIndex;   //IMPORTANT!! this will link all MInput Links
            //                link.ConnectActionMap();
            //                linksC += $"[{link.name}]";
            //            }
            //        }
            //        Debuggin($"Connect <B>[{ActiveMap}] Map</B> to <B> {linksC} </B> buttons");
            //    }

            //    OnActionMapChanged.Invoke(map);

            //}
            #endregion

            Debuggin(debugval);
        }

        public virtual void DisconnectActionMap()
        {
            if (/*ActiveMActionMap != null && */Connected)
            {
                foreach (var map in m_MapButtons)
                {
                    DisconnectButtons(map);
                    DisconnectMove(map);
                    DisconnectUpDown(map);
                }
                Connected = false; //Update the Connection to false;
            }
        }


        #region Connect/Disconnect Buttons
        private void ConnectButtons(MInputActionMap map)
        {
            foreach (var btn in map.buttons)
            {
                if (btn.reference != null) //Check that there's a valid input
                {
                    btn.action = ResolveForPlayer(btn.reference, playerInput.playerIndex);
                    ConnectAction(btn.action, btn);
                    btn.MCoroutine = this;                      //Set the Monobehaviour so I can use Coroutines
                }
            }
        }

        private void DisconnectButtons(MInputActionMap map)
        {
            foreach (var btn in map.buttons)
            {
                if (btn.reference != null) //Check that there's a valid input
                {
                    DisconnectAction(btn.action, btn);
                    btn.MCoroutine = null; //remove the Monobehaviour for coroutines

                    if (btn.ResetOnDisable.Value)
                        btn.OnInputChanged.Invoke(btn.InputValue = false);  //Sent false to all Input listeners 
                }
            }
        }


        public void ConnectInput(string name, UnityAction<bool> action)
        {
            foreach (var maps in m_MapButtons)
            {
                var button = maps.buttons.Find(x => x.name == name);

                button?.OnInputChanged.AddListener(action);
            }
        }

        public void DisconnectInput(string name, UnityAction<bool> action)
        {
            foreach (var maps in m_MapButtons)
            {
                var button = maps.buttons.Find(x => x.name == name);

                if (button != null)
                {
                    button.OnInputChanged.RemoveListener(action);
                }
            }
        }

        public void ConnectAction(InputAction action, MInputAction btn)
        {
            action.started += btn.TranslateInput;
            action.performed += btn.TranslateInput;
            action.canceled += btn.TranslateInput;
        }

        public void DisconnectAction(InputAction action, MInputAction btn)
        {
            action.started -= btn.TranslateInput;
            action.performed -= btn.TranslateInput;
            action.canceled -= btn.TranslateInput;
        }
        #endregion

        #region Connect/Disconnect movement

        /// <summary> Connects the Move Action from the character  </summary>
        private void ConnectMove(MInputActionMap map)
        {
            if (map.Move != null)
            {
                //Conect only for player Index!!!
                map.MoveAction = ResolveForPlayer(map.Move, playerInput.playerIndex);
                //  map.MoveAction.started += OnMove;
                map.MoveAction.performed += OnMove;
                map.MoveAction.canceled += OnMove;
            }
        }
        private void DisconnectMove(MInputActionMap map)
        {
            if (map.Move != null)
            {
                // map.MoveAction.started -= OnMove;
                map.MoveAction.performed -= OnMove;
                map.MoveAction.canceled -= OnMove;
            }

            character?.Move(Vector3.zero);       //When the Input is Disable make sure the character/animal is not moving.
        }

        /// <summary> Connects the Up Action to the character  </summary>
        private void ConnectUpDown(MInputActionMap map)
        {
            if (map.UpDown != null)
            {
                map.UpDownAction = ResolveForPlayer(map.UpDown, playerInput.playerIndex);
                //  map.UpDownAction.started += OnUpDown;
                map.UpDownAction.performed += OnUpDown;
                map.UpDownAction.canceled += OnUpDown;
            }
        }

        private void DisconnectUpDown(MInputActionMap map)
        {
            if (map.UpDown != null)
            {
                // map.UpDownAction.started -= OnUpDown;
                map.UpDownAction.performed -= OnUpDown;
                map.UpDownAction.canceled -= OnUpDown;
            }

            //When the Input is Disable make sure the character/animal is not moving.
            character?.Move(Vector3.zero);
        }

        /// <summary>
        /// In a multi-player context, actions are associated with specific players
        /// This resolves the appropriate action reference for the specified player.
        /// 
        /// Because the resolution involves a search, we also cache the returned 
        /// action to make future resolutions faster.
        /// </summary>
        /// <param name="axis">Which input axis (0, 1, or 2)</param>
        /// <param name="actionRef">Which action reference to resolve</param>
        /// <returns>The cached action for the player specified in PlayerIndex</returns>
        protected InputAction ResolveForPlayer(InputActionReference actionRef, int PlayerIndex)
        {
            if (actionRef == null || actionRef.action == null)
                return null;

            //var m_cachedActions = new InputAction();


            var m_cachedActions = actionRef.action;

            if (PlayerIndex != -1)
            {
                PlayerIndex = Math.Clamp(PlayerIndex, 0, InputUser.all.Count - 1); //Make sure the Input Users does not exceed the list length
                m_cachedActions = GetFirstMatch(InputUser.all[PlayerIndex], actionRef);
            }
            //if (AutoEnableInputs && actionRef != null && actionRef.action != null)
            //    actionRef.action.Enable();

            // Update enabled status
            if (m_cachedActions != null && m_cachedActions.enabled != actionRef.action.enabled)
            {
                if (actionRef.action.enabled)
                    m_cachedActions.Enable();
                else
                    m_cachedActions.Disable();
            }

            return m_cachedActions;

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


        public void OnMove(InputAction.CallbackContext context)
        {
            var move2D = context.ReadValue<Vector2>();

            Vector3 MoveAxis = this.MoveAxis;

            MoveAxis.x = move2D.x * ActiveMActionMap.MoveMult.x; //Catch the Horizontal Axis
            MoveAxis.z = move2D.y * ActiveMActionMap.MoveMult.z; //Catch the Forward Axis

            this.MoveAxis = MoveAxis;

            character?.SetInputAxis(this.MoveAxis);

            OnMoveAxis(MoveAxis);
        }

        public void OnUpDown(InputAction.CallbackContext context)
        {
            if (context.valueType != typeof(float))
            {
                Debug.LogWarning("Up Down Input is not type float, Please set the correct type to the UpDown Input Action Reference");
                return;
            }

            var UpDown = context.ReadValue<float>();

            Vector3 MoveAxis = this.MoveAxis;
            MoveAxis.y = ActiveMActionMap.MoveMult.y * UpDown; //Catch the Up Down Axis
            this.MoveAxis = MoveAxis;

            character?.SetInputAxis(MoveAxis);

            OnMoveAxis(MoveAxis);
        }

        #endregion

        #region Player Input Code I can use  easier for me to do the Local MultiPlayer thingy (Not Implemented yet)

        /// <summary>  Player Input methods I can use too  
        /// IT SEEMS FOR MULTIPLE LOCAL PLAYERS THE ACTION ASSET NEEDS TO BE DUPLICATED.... Tricky guys!!!
        /// 
        /// </summary>


        //[NonSerialized] private Action<InputDevice, InputDeviceChange> m_DeviceChangeDelegate;
        //[NonSerialized] private bool m_OnDeviceChangeHooked;
        //[NonSerialized] private InputUser m_InputUser;
        //internal static int s_AllActivePlayersCount;
        //public static bool isSinglePlayer =>
        //  s_AllActivePlayersCount <= 1 &&
        //  (PlayerInputManager.instance == null || !PlayerInputManager.instance.joiningEnabled);


        //private void StartListeningForDeviceChanges()
        //{
        //    if (m_OnDeviceChangeHooked)
        //        return;
        //    if (m_DeviceChangeDelegate == null)
        //        m_DeviceChangeDelegate = OnDeviceChange;
        //    InputSystem.onDeviceChange += m_DeviceChangeDelegate;
        //    m_OnDeviceChangeHooked = true;
        //}

        //private void StopListeningForDeviceChanges()
        //{
        //    if (!m_OnDeviceChangeHooked)
        //        return;
        //    InputSystem.onDeviceChange -= m_DeviceChangeDelegate;
        //    m_OnDeviceChangeHooked = false;  
        //} 

        //private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        //{
        //    // If a device was added and we have no control schemes in the actions and we're in
        //    // single-player mode, pair the device to the player if it works with the bindings we have.
        //    if (change == InputDeviceChange.Added && isSinglePlayer &&
        //        InputActions != null && InputActions.controlSchemes.Count == 0 &&
        //        HaveBindingForDevice(device) && m_InputUser.valid)
        //    {
        //        InputUser.PerformPairingWithDevice(device, user: m_InputUser);
        //    }
        //}

        //private bool HaveBindingForDevice(InputDevice device)
        //{
        //    if (InputActions == null)
        //        return false;

        //    var actionMaps = InputActions.actionMaps;
        //    for (var i = 0; i < actionMaps.Count; ++i)
        //    {
        //        var actionMap = actionMaps[i];
        //        if (actionMap.IsUsableWithDevice(device))
        //            return true;
        //    }

        //    return false;
        //}
        #endregion
        private void Update()
        {
            if (playerInput == null || !playerInput.enabled)
            {
                Debug.Log("Player Input is Null or Disabled. Disabling MInputLink.", this);
                enabled = false;
                return;
            }



            if (IgnoreOnPause && Time.timeScale == 0) return;

            //Update player Map
            if (PlayerMap != playerInput.currentActionMap)
            {
                UpdateActiveMap();
                PlayerMap = playerInput.currentActionMap;
            }


            character?.SetInputAxis(MoveAxis);
        }

        private InputActionMap PlayerMap;

        public IInputAction GetInput(string name) => DefaultMap.buttons.Find(x => x.Name == name);

        #region Public Methods IInputSource Interface


        /// <summary>Enable an Input Row</summary>
        public virtual void EnableInput(string name) => EnableInput(name, true);


        /// <summary> Disable an Input Row </summary>
        public virtual void DisableInput(string name) => EnableInput(name, false);


        /// <summary>Enable/Disable an Input Row</summary>
        public virtual void EnableInput(string input_name, bool value)
        {
            if (ActiveMActionMap == null) return;

            string[] inputsName = input_name.Split(',');

            foreach (var inp in inputsName)
            {
                var inputs = ActiveMActionMap.buttons.FindAll(x => inp == x.Name);

                if (inputs != null)
                    foreach (var item in inputs)
                        item.active.Value = value;
            }
        }


        /// <summary>Set a Value of an Input internally without calling the Events</summary>
        public virtual void SetInput(string input_name, bool value)
        {
            if (ActiveMActionMap == null || ActiveMActionMap.buttons == null) return;

            var inputs = ActiveMActionMap.buttons.FindAll(x => input_name.Contains(x.Name));

            if (inputs != null)
                foreach (var item in inputs)
                    item.InputValue = value;
        }

        public void ResetInput(string name) => SetInput(name, false);

        internal void ResetButtonMap()
        {
            m_MapButtons = null;
            ActiveActionMapIndex = 0;
        }

        public virtual void PlayerInput_Set(GameObject gameObject) => playerInput = gameObject.GetComponent<PlayerInput>();

        public virtual void PlayerInput_Set(Component component) => playerInput = component.GetComponent<PlayerInput>();

        public virtual void PlayerInput_Set(TransformVar var) => playerInput = var.Value.GetComponent<PlayerInput>();

        public virtual void PlayerInput_Set(GameObjectVar var) => playerInput = var.Value.GetComponent<PlayerInput>();

        public virtual void PlayerInput_Set(PlayerInput player) => playerInput = player;
        #endregion

        void Debuggin(string val)
        {
            if (debug) MDebug.Log(val);
        }

#if UNITY_EDITOR 
        private void CheckActionMaps()
        {
            if (InputActions != null)
            {
                m_MapButtons = new(); //Make sure is not null

                //Check when a new Input Map has been added
                if (m_MapButtons.Count < InputActions.actionMaps.Count)
                {
                    for (int i = m_MapButtons.Count; i < InputActions.actionMaps.Count; i++)
                    {
                        m_MapButtons.Add(new MInputActionMap(InputActions.actionMaps[i], i) { MoveMult = new(Vector3.one) });
                    }
                }
                EditorUtility.SetDirty(this);
            }
        }


        private void Reset()
        {
            playerInput = GetComponent<PlayerInput>();

            if (playerInput == null) playerInput = gameObject.AddComponent<PlayerInput>();

            var inputactions = MTools.GetInstance<InputActionAsset>("Malbers Inputs");
            playerInput.actions = inputactions;
            InputActions = inputactions;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            ValidateInputActions();

            SetDevicesEvents();

            CheckActionMaps();

            ActiveActionMapIndex = 1;
            FindButtons();
        }

        [ContextMenu("Set Device Sprint Events")]

        private void SetDevicesEvents()
        {
            var currentDevice = MTools.GetInstance<StringVar>("Current Device");
            if (currentDevice != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(CurrentControlScheme, currentDevice.SetValue);
            }

            var ActionMapChanged = MTools.GetInstance<StringVar>("ActionMap Changed");
            if (ActionMapChanged != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnActionMapChanged, ActionMapChanged.SetValue);
            }

            MTools.SetDirty(this);
        }

        private void OnValidate()
        {
            ValidateInputActions();

            //IMPORTANT IT NEEDS TO BE INVOKE CSHARP
            if (playerInput != null && playerInput.notificationBehavior != PlayerNotifications.InvokeCSharpEvents)
            {

                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                MTools.SetDirty(playerInput);
            }
        }

        public virtual void FindButtons()
        {
            if (InputActions == null) { m_MapButtons = null; { Debug.Log("NO INPUT ACTION"); } return; }

            m_MapButtons ??= new(InputActions.actionMaps.Count);


            if (ActiveActionMapIndex - 1 < 0) return; //skip

            //Find All the Action Input References
            var AllActionRef = GetAllActionsFromAsset(InputActions);

            var ActiveActionMap = InputActions.actionMaps[ActiveActionMapIndex - 1];

            var buttonMap = m_MapButtons[ActiveActionMapIndex - 1];

            //Find 
            if (buttonMap.Move == null)// || string.Compare( CurrentButtonMap.Move.name,"Move") != 0)
                buttonMap.Move = GetActionReferenceFromAssets(AllActionRef, ActiveActionMap, "Move");
            if (buttonMap.UpDown == null)
                buttonMap.UpDown = GetActionReferenceFromAssets(AllActionRef, ActiveActionMap, "UpDown");


            //Find Missing Buttons first
            for (int i = buttonMap.buttons.Count - 1; i >= 0; i--)
            {
                var button = buttonMap.buttons[i];
                //meaning the button is empty
                if (button != null && button.reference == null)
                {
                    var actionRef = GetActionReferenceFromAssets(AllActionRef, ActiveActionMap, button.name);
                    button.reference = actionRef;
                }

                if (button.reference == null)
                {
                    //Remove button??
                    buttonMap.buttons.Remove(button);
                }
            }

            //Add new buttons
            foreach (var action in ActiveActionMap)
            {
                if (action.type == InputActionType.Button
                    && !buttonMap.buttons.Exists(x => x.reference != null && x.reference.action.id == action.id))
                {
                    var actionRef = GetActionReferenceFromAssets(AllActionRef, ActiveActionMap, action.name);

                    //Do not add it if the name already exist
                    if (buttonMap.buttons.Exists(x => x.name == action.name) == false)
                    {
                        buttonMap.buttons.Add(new MInputAction(action.name, actionRef, MInputInteraction.Press));
                    }
                }
            }

            MTools.SetDirty(this); //Make it Dirty (Ctrl+z)
        }

        private static InputActionReference[] GetAllActionsFromAsset(InputActionAsset actions)
        {
            if (actions != null)
            {
                var path = AssetDatabase.GetAssetPath(actions);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                return assets.Where(asset => asset is InputActionReference).Cast<InputActionReference>().OrderBy(x => x.name).ToArray();
            }
            return null;
        }

        private static InputActionReference GetActionReferenceFromAssets(InputActionReference[] actions, InputActionMap map, params string[] actionNames)
        {
            foreach (var actionName in actionNames)
            {
                foreach (var action in actions)
                {
                    if (action.action != null && action.action.actionMap == map)
                        if (string.Compare(action.action.name, actionName, StringComparison.InvariantCultureIgnoreCase) == 0)
                            return action;
                }
            }
            return null;
        }
#endif
#endif
    }

    public enum MInputInteraction { Press = 0, Down = 1, Up = 2, LongPress = 3, DoubleTap = 4, Toggle = 5, Float = 6, Vector2 = 7 }
    //public enum InputStatus { started, performed, canceled }
    //public enum MInputActionType { Value, Button }
    //public enum MInputActionValueType { Double, Vector2 }


    /// <summary>  Wrapper to separate the Unity System Action Maps</summary>
    [System.Serializable]
    public class MInputActionMap
    {
        public int Index;
        public string Name => ActionMap.name;
        [Tooltip("Action for Moving the Character\n(X:Horizontal; Y:Forward)")]
        public InputActionReference Move;
        [Tooltip("Action for Moving Up and Down the Character")]
        public InputActionReference UpDown;

        [Tooltip("Multiplier for the Move value (X: Horizontal, Y: UpDown, Z:Forward/Vertical")]
        public Vector3Reference MoveMult = new(Vector3.one);

        internal InputAction MoveAction;
        internal InputAction UpDownAction;
        public InputActionMap ActionMap;

        // public string name;
        public List<MInputAction> buttons;

        public MInputActionMap(InputActionMap map, int index)
        {

            Index = index;
            buttons = new List<MInputAction>();
            ActionMap = map;
            // Name = map.name;
        }
    }


    /// <summary> Input Class. Translate Unity Input System Callback to Malbers Input System -> Bool values </summary>
    [System.Serializable]
    public class MInputAction : IInputAction
    {
        public InputActionReference reference;

        public InputAction action { get; set; }

        //public InputActionMap ActionMap => reference.action.actionMap;

        public BoolReference active = new(true);

        [Tooltip("If the Input Component gets disable the Input will be set to false and it will send false to all its listeners")]
        public BoolReference ResetOnDisable = new(true);


        [Tooltip("Input will not work on Time.Scale = 0")]
        public BoolReference ignoreOnPause = new();

        /// <summary>Type of iteraction the button has</summary>
        public MInputInteraction interaction = MInputInteraction.Press;


        public MonoBehaviour MCoroutine { get; set; }
        private IEnumerator C_Press;
        private IEnumerator C_LongPress;

        // public bool ShowEvents = false;

        #region LONG PRESS and Double Tap
        public FloatReference DoubleTapTime = new(0.3f);                          //Double Tap Time
        public FloatReference LongPressTime = new(0.5f);
        public FloatReference PressThreshold = new(0.5f);
        public FloatReference Vector2Mult = new(1f);

        private bool FirstInputPress = false;
        private bool InputCompleted = false;
        private float InputStartTime;



        public bool debug;

        #endregion

        /// <summary>  Name of the Action  </summary>
        public string name = "InputName";
        public string Name => name;

        /// <summary> Enable Disable the Input Action </summary>
        public bool Active
        {
            get => active.Value;
            set
            {
                active.Value = value;
                if (Application.isPlaying)
                {

                    if (value)
                    {
                        action.Enable();
                        OnInputEnabled.Invoke();
                    }
                    else
                    {
                        action.Disable();
                        OnInputDisabled.Invoke();
                    }
                }
            }
        }




        /// <summary>Current Input Value</summary>
        private bool inputValue = false;
        public bool InputValue
        {
            get => inputValue;
            set
            {
                if (inputValue != value)
                {
                    inputValue = value;
                    DebbugInput(value);
                }
            }
        }

        private void DebbugInput(bool value)
        {
#if UNITY_EDITOR

            if (debug)
            {
                Debug.Log($"<color=yellow> <B>[Input {name} - {interaction} : {value}]</B>. Map [{reference.action.actionMap.name}]  </color>", reference);
            }
#endif
        }

        public virtual bool GetValue { get => inputValue; set => inputValue = value; }


        public UnityEvent OnInputDown = new();

        public UnityEvent OnInputEnabled = new();
        public UnityEvent OnInputDisabled = new();

        public UnityEvent OnInputUp = new();
        public UnityEvent OnLongPress = new();
        public UnityEvent OnDoubleTap = new();
        public BoolEvent OnInputChanged = new();
        public UnityEvent OnInputPressed = new();
        public FloatEvent OnInputFloatValue = new();
        public Vector2Event OnInputV2Value = new();


        public UnityEvent InputDown => this.OnInputDown;
        public UnityEvent InputUp => this.OnInputUp;
        public BoolEvent InputChanged => this.OnInputChanged;



        public void TranslateInput(InputAction.CallbackContext context)
        {
            if (!Active) return; //Do nothing if the Local Active is false;
            if (ignoreOnPause && Time.timeScale == 0) return; //Do nothing if TimeScale = 0;

            bool OldValue = InputValue;            //Store the Old Value first
            bool NewValue = context.performed || context.started;

            switch (interaction)
            {
                #region Press Interation
                case MInputInteraction.Press:

                    InputValue = NewValue; //Update the value for LongPress IMPORTANT!

                    if (OldValue != InputValue)
                    {
                        if (InputValue)
                        {
                            OnInputDown.Invoke();
                            DoPress();
                        }
                        else
                        {
                            OnInputUp.Invoke();
                        }
                        OnInputChanged.Invoke(InputValue);
                    }
                    break;
                #endregion

                #region Down Interaction
                //-------------------------------------------------------------------------------------------------------
                case MInputInteraction.Down:

                    if (context.phase == InputActionPhase.Started)
                    {
                        OnInputDown.Invoke();
                        OnInputChanged.Invoke(InputValue = true);
                    }
                    else if (context.phase == InputActionPhase.Performed)
                    {
                        OnInputChanged.Invoke(InputValue = false);
                    }

                    //InputValue = context.started; //Update the value for Down IMPORTANT! OVEWRITE!!!

                    //if (OldValue != InputValue)
                    //{
                    //    if (InputValue) OnInputDown.Invoke();

                    //     OnInputChanged.Invoke(InputValue);
                    //}
                    break;
                #endregion

                #region Up Interaction
                //-------------------------------------------------------------------------------------------------------
                case MInputInteraction.Up:

                    // InputValue = NewValue; //Update the value for UP IMPORTANT!

                    if (context.phase == InputActionPhase.Canceled)
                    {
                        OnInputUp.Invoke();
                        OnInputChanged.Invoke(InputValue = true);
                        MCoroutine.StartCoroutine(IEnum_UpRelease());
                    }
                    break;
                #endregion

                #region Long Press
                //-------------------------------------------------------------------------------------------------------
                case MInputInteraction.LongPress:

                    if (context.phase == InputActionPhase.Performed)
                    {
                        DoLongPressed();
                    }
                    else if (context.phase == InputActionPhase.Canceled)
                    {
                        //If the Input was released before the LongPress was completed ... take it as Interrupted 
                        //(ON INPUT UP serves as Interrupted)
                        if (!InputCompleted)
                        {
                            if (C_LongPress != null) MCoroutine.StopCoroutine(C_LongPress); //Call Interruption
                            //OnInputUp.Invoke();
                        }

                        OnInputUp.Invoke(); //Invoke no mater what

                        InputCompleted = false;  //Reset the Long Press
                        OnInputChanged.Invoke(InputValue = false);
                    }
                    break;
                #endregion

                #region Double Tap
                //-------------------------------------------------------------------------------------------------------
                case MInputInteraction.DoubleTap:
                    InputValue = NewValue; //Update the value for LongPress IMPORTANT!


                    if (OldValue != InputValue)
                    {
                        OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                        if (InputValue)
                        {
                            if (InputStartTime != 0 && MTools.ElapsedTime(InputStartTime, DoubleTapTime))
                            {
                                FirstInputPress = false;    //This is in case it was just one Click/Tap this will reset it
                            }

                            if (!FirstInputPress)
                            {
                                OnInputDown.Invoke();
                                InputStartTime = Time.time;
                                FirstInputPress = true;
                            }
                            else
                            {
                                if ((Time.time - InputStartTime) <= DoubleTapTime)
                                {
                                    FirstInputPress = false;
                                    InputStartTime = 0;
                                    OnDoubleTap.Invoke();       //Sucesfull Double tap
                                }
                                else
                                {
                                    FirstInputPress = false;
                                }
                            }
                        }
                    }

                    break;

                #endregion

                #region Toggle
                //-------------------------------------------------------------------------------------------------------
                case MInputInteraction.Toggle:

                    if (context.phase == InputActionPhase.Started)
                    {
                        InputValue = !InputValue;

                        OnInputChanged.Invoke(InputValue);

                        if (InputValue)
                            OnInputDown.Invoke();
                        else
                            OnInputUp.Invoke();

                    }
                    break;

                #endregion

                case MInputInteraction.Float:
                    {
                        var value = context.action.ReadValue<float>();

                        InputValue = value != 0;

                        if (OldValue != InputValue)
                        {
                            OnInputChanged.Invoke(InputValue);

                            if (InputValue) OnInputDown.Invoke();
                            else OnInputUp.Invoke();
                        }


                        if (value > PressThreshold.Value)
                        {
                            OnInputPressed.Invoke();
                        }


                        if (debug)
                        {
                            Debug.Log($"<color=cyan><B>[Input {name} : {value}]</B></color>");
                        }

                        OnInputFloatValue.Invoke(value);
                    }
                    break;


                case MInputInteraction.Vector2:

                    var v2 = context.action.ReadValue<Vector2>();

                    InputValue = v2 != Vector2.zero;

                    if (OldValue != InputValue)
                    {
                        OnInputChanged.Invoke(InputValue);

                        if (InputValue) OnInputDown.Invoke();
                        else OnInputUp.Invoke();
                    }

                    if (InputValue)
                    {
                        OnInputPressed.Invoke();
                    }

                    if (debug)
                    {
                        Debug.Log($"<color=cyan><B>[Input {name} : {v2}]</B></color>");
                    }



                    OnInputV2Value.Invoke(v2 * Vector2Mult);
                    break;

                default: break;
            }
        }

        private void DoPress()
        {
            if (C_Press != null)
                MCoroutine.StopCoroutine(C_Press);

            C_Press = IEnum_Press();
            MCoroutine.StartCoroutine(C_Press);
        }


        private void DoLongPressed()
        {
            if (C_LongPress != null)
                MCoroutine.StopCoroutine(C_LongPress);

            C_LongPress = IEnum_LongPress();
            MCoroutine.StartCoroutine(C_LongPress);
        }

        #region Coroutines

        IEnumerator IEnum_Press()
        {
            while (InputValue)
            {
                OnInputPressed.Invoke();
                yield return null;
            }
        }

        IEnumerator IEnum_UpRelease()
        {
            yield return null;
            OnInputChanged.Invoke(InputValue = false);
        }


        IEnumerator IEnum_LongPress()
        {
            InputStartTime = Time.time;
            InputCompleted = false;
            OnInputDown.Invoke();
            OnInputChanged.Invoke(InputValue = true);

            float elapsed;

            while (!InputCompleted)
            {
                elapsed = (Time.time - InputStartTime) / LongPressTime;
                OnInputFloatValue.Invoke(elapsed);

                if (elapsed >= 1f)
                {
                    OnInputFloatValue.Invoke(1);
                    OnLongPress.Invoke();
                    InputCompleted = true;                     //This will avoid the longpressed being pressed just one time
                    InputValue = true;
                    break;
                }
                yield return null;
            }
        }

        #endregion

        #region Constructors


        public MInputAction(string name)
        {
            active.Value = true;
            this.name = name;
            interaction = MInputInteraction.Down;
            reference = null;
            action = null;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }

        public MInputAction(string name, MInputInteraction pressed)
        {
            this.name = name;
            active.Value = true;
            interaction = pressed;
            reference = null;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }


        public MInputAction(string name, InputActionReference reference)
        {
            this.name = name;
            active.Value = true;
            interaction = MInputInteraction.Down;
            this.reference = reference;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }

        public MInputAction(string name, InputActionReference reference, MInputInteraction pressed)
        {
            this.name = name;
            active.Value = true;
            interaction = pressed;
            this.reference = reference;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }

        public MInputAction(bool active, string name, MInputInteraction pressed)
        {
            this.name = name;
            this.active.Value = active;
            interaction = pressed;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }

        public MInputAction()
        {
            active.Value = true;
            name = "InputName";
            interaction = MInputInteraction.Press;
            reference = null;
            DoubleTapTime = new FloatReference(0.3f);                          //Double Tap Time
            LongPressTime = new FloatReference(0.5f);
        }
        #endregion
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MInputLink)), CanEditMultipleObjects]
    public class MInputLinkEditor : Editor
    {


        //protected ReorderableList list;
        protected SerializedProperty
            m_Buttons, InputActions, m_MapButtons, showInputEvents, playerInput, clearPlayerInput, Editor_Tabs1,
            UpDown, Move, r_Move, PlayerIndex, DefaultScheme, ActiveActionMap, ShowMapIndex,
            IgnoreOnPause, OnInputEnabled, OnInputDisabled, OnActionMapChanged, CurrentControlScheme,
            OnControlsChanged, OnDeviceLost, OnDeviceRegained,
            debug;
        private MInputLink M;
        protected MonoScript script;


        private readonly Dictionary<string, ReorderableList> innerListDict = new();

        string[] ActionMapsNames;


        protected virtual void OnEnable()
        {
            M = ((MInputLink)target);
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            playerInput = serializedObject.FindProperty("playerInput");
            clearPlayerInput = serializedObject.FindProperty("clearPlayerInput");
            m_Buttons = serializedObject.FindProperty("m_Buttons");
            m_MapButtons = serializedObject.FindProperty("m_MapButtons");

            OnInputEnabled = serializedObject.FindProperty("OnInputEnabled");
            OnInputDisabled = serializedObject.FindProperty("OnInputDisabled");
            OnActionMapChanged = serializedObject.FindProperty("OnActionMapChanged");

            OnControlsChanged = serializedObject.FindProperty("OnControlsChanged");
            OnDeviceLost = serializedObject.FindProperty("OnDeviceLost");
            OnDeviceRegained = serializedObject.FindProperty("OnDeviceRegained");



            CurrentControlScheme = serializedObject.FindProperty("CurrentControlScheme");
            showInputEvents = serializedObject.FindProperty("showInputEvents");

            IgnoreOnPause = serializedObject.FindProperty("IgnoreOnPause");
            debug = serializedObject.FindProperty("debug");

            Move = serializedObject.FindProperty("Move");
            UpDown = serializedObject.FindProperty("UpDown");
            InputActions = serializedObject.FindProperty("InputActions");

            DefaultScheme = serializedObject.FindProperty("DefaultScheme");
            ActiveActionMap = serializedObject.FindProperty("ActiveActionMapIndex");
            ShowMapIndex = serializedObject.FindProperty("ShowMapIndex");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");



            if (InputActions.objectReferenceValue == null)
            {
                M.ResetButtonMap();
                EditorUtility.SetDirty(target);
            }
        }

        private void CheckActionMaps(bool DifferentMapButton)
        {
            if (M.InputActions != null)
            {
                var count = M.InputActions.actionMaps.Count; //Store the Amount of Action Maps

                ActionMapsNames = new string[count + 1]; // Set the first one as NONE  
                ActionMapsNames[0] = "<None>";

                for (int i = 0; i < count; i++)
                {
                    var nme = M.InputActions.actionMaps[i].name;
                    ActionMapsNames[i + 1] = nme;
                    // Debug.Log($"MAP Name = <{MInp.InputActions.actionMaps[i].name}>");
                }


                M.m_MapButtons ??= new(); //Make sure is not null

                //Check when a new Input Map has been added
                if (M.m_MapButtons.Count < count)
                {
                    for (int i = M.m_MapButtons.Count; i < count; i++)
                    {
                        M.m_MapButtons.Add(new MInputActionMap(M.InputActions.actionMaps[i], i));
                    }
                }

                if (M.m_MapButtons == null || DifferentMapButton) //The Map Button is different or empty
                {
                    M.m_MapButtons = new List<MInputActionMap>();
                    if (debug.boolValue)
                        Debug.Log($"{target.name} <MapButton list reset>");


                    for (int i = 0; i < count; i++)
                    {
                        M.m_MapButtons.Add(new MInputActionMap(M.InputActions.actionMaps[i], i));
                    }
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                ActionMapsNames = null;
            }
        }

        private readonly string[] tabs = new string[] { "Inputs", "Events" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var ActiveActionMapName = Application.isPlaying ? $" Active Map: [{M.ActiveMActionMap.Name}]" : "";

            MalbersEditor.DrawDescription(M.versionInput + ActiveActionMapName);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var CA = new EditorGUI.ChangeCheckScope())
                {
                    var OldInpSource = InputActions.objectReferenceValue;
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(playerInput);
                        MalbersEditor.DrawDebugIcon(debug);
                    }

                    if (CA.changed)
                    {
                        //Check if the Player Input is found
                        if (playerInput.objectReferenceValue != null)
                        {
                            var pI = playerInput.objectReferenceValue as PlayerInput;

                            if (pI != null)
                            {
                                InputActions.objectReferenceValue = pI.actions;
                                InputActions.serializedObject.ApplyModifiedProperties();
                                serializedObject.ApplyModifiedProperties(); //Update the new Input Source
                                                                            //  serializedObject.Update();
                            }
                        }

                        var DifferentINPSource = OldInpSource != InputActions.objectReferenceValue;


                        //IMPORTANT clean everything!
                        if (OldInpSource != InputActions.objectReferenceValue)
                            ActiveActionMap.intValue = 0;


                        serializedObject.ApplyModifiedProperties(); //Update the new Input Source


                        CheckActionMaps(DifferentINPSource);
                    }
                }

                using (new EditorGUI.DisabledGroupScope(playerInput.objectReferenceValue != null))
                    EditorGUILayout.PropertyField(InputActions);

                if (M.playerInput != null && M.playerInput.actions != M.InputActions)
                {
                    // EditorGUILayout.HelpBox("[Input Actions] does not match with the Player Input -> [Input Actions].\nUse the same asset", MessageType.Error);
                    InputActions.objectReferenceValue = M.playerInput.actions;
                    serializedObject.ApplyModifiedProperties(); //Update the new Input Source
                    M.ResetButtonMap();
                    //Debug.Log("Button Cleared and Disconnected. The Input Action Asset has changed");
                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.PropertyField(clearPlayerInput);
                EditorGUILayout.PropertyField(IgnoreOnPause);

                if (InputActions.objectReferenceValue != null)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (ActionMapsNames == null)
                            CheckActionMaps(false);

                        //LINK the Player Input with the Map in this comonent! TODO 
                        if (ActiveActionMap != null && ActionMapsNames != null)
                        {
                            ActiveActionMap.intValue =
                            EditorGUILayout.Popup(new GUIContent("Connect Maps (" + (ActiveActionMap.intValue - 1) + ")",
                            "Action Map connected to the Animal Controller at start"), ActiveActionMap.intValue, ActionMapsNames);
                        }



                        using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                        {
                            if (GUILayout.Button(new GUIContent("Find Buttons",
                                "Search for Actions set as buttons on the Action Map. New Buttons will be added automatically to the button list"), GUILayout.MaxWidth(100), GUILayout.MinWidth(50)))
                            {
                                M.FindButtons();
                                EditorUtility.SetDirty(target);
                            }
                        }
                    }

                    if (Application.isPlaying)
                        EditorGUILayout.HelpBox("To Change Action maps use SwitchActionMap(string actionMap)", MessageType.Info);
                }
                //else
                //{
                //    M.ResetButtonMap();
                //    //  EditorUtility.SetDirty(target);
                //}
            }

            // EditorGUILayout.PropertyField(serializedObject.FindProperty("ActiveActionMapIndex"));

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, tabs);

            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawButtons(); break;
                case 1: DrawEvents(); break;
                default: break;

            }

            serializedObject.ApplyModifiedProperties();
        }
        private void DrawButtons()
        {
            if (InputActions.objectReferenceValue != null)
            {
                ReorderableList Reo_AbilityList;

                var index = ActiveActionMap.intValue - 1;

                if (ActiveActionMap.intValue <= 0) return;

                if (M.m_MapButtons == null || m_MapButtons == null || M.m_MapButtons.Count <= index) return; //Null Checking IMPORTANT!

                // Debug.Log(index);
                SerializedProperty actionMap = m_MapButtons.GetArrayElementAtIndex(index);

                if (actionMap == null) return;


                var ButtonList = actionMap.FindPropertyRelative("buttons");
                var Move = actionMap.FindPropertyRelative("Move");
                var UpDown = actionMap.FindPropertyRelative("UpDown");
                var id = actionMap.FindPropertyRelative("id");
                var map = actionMap.FindPropertyRelative("ActionMap");
                var m_Name = map.FindPropertyRelative("m_Name");
                var MoveMult = actionMap.FindPropertyRelative("MoveMult");


                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Action Map: [{m_Name.stringValue}]", EditorStyles.boldLabel);
                        //if (GUILayout.Button("Clear Buttons", GUILayout.Width(100)))
                        //{
                        //    actionMap.SetValue(null);

                        //}
                    }
                    EditorGUILayout.PropertyField(Move);
                    EditorGUILayout.PropertyField(UpDown);
                    EditorGUILayout.PropertyField(MoveMult);

                    string listKey = actionMap.propertyPath;

                    if (innerListDict.ContainsKey(listKey))
                    {
                        // fetch the reorderable list in dict
                        Reo_AbilityList = innerListDict[listKey];
                    }
                    else
                    {
                        Reo_AbilityList = new ReorderableList(actionMap.serializedObject, ButtonList, true, true, true, true)
                        {
                            drawElementCallback = (rect, ele_index, isActive, isFocused) =>
                            {
                                if (M.m_MapButtons == null || M.m_MapButtons.Count <= index) return; //Nul Check!! IMPORTANT
                                if (M.m_MapButtons[index].buttons == null || M.m_MapButtons[index].buttons.Count <= ele_index) return; //Nul Check!! IMPORTANT

                                var element = M.m_MapButtons[index].buttons[ele_index];

                                // if (element.action.action.actionMap != MInp.InputActions.actionMaps[MInp.ActiveActionMap]) return;

                                var elementSer = actionMap.FindPropertyRelative("buttons").GetArrayElementAtIndex(ele_index);

                                rect.y += 2;


                                var spliter = (rect.width - 20) / 3;
                                var height = EditorGUIUtility.singleLineHeight;

                                Rect R_0 = new(rect.x, rect.y, 20, height);
                                Rect R_1 = new(rect.x + 20, rect.y, spliter - 75, height);
                                Rect R_2 = new(rect.x + 20 + spliter - 70, rect.y, spliter + 75, height);
                                Rect R_4 = new(rect.x + (spliter * 2) + 30, rect.y, spliter - 5, height);



                                var name = elementSer.FindPropertyRelative("name");
                                var GetPressed = elementSer.FindPropertyRelative("interaction");
                                var action = elementSer.FindPropertyRelative("reference");
                                var active = elementSer.FindPropertyRelative("active");

                                var dbC = GUI.backgroundColor;
                                GUI.backgroundColor = isActive ? MTools.MBlue : dbC;

                                var result = EditorGUI.Toggle(R_0, element.active.Value);

                                if (result != element.active.Value)
                                {
                                    element.active.Value = result;
                                    EditorUtility.SetDirty(target); //UPDATE THE VALUE ON THE ACTIVE INPUT
                                }


                                // EditorGUI.PropertyField(R_0, active, GUIContent.none);
                                EditorGUI.PropertyField(R_1, name, GUIContent.none);
                                EditorGUI.PropertyField(R_2, action, GUIContent.none);
                                EditorGUI.PropertyField(R_4, GetPressed, GUIContent.none);
                                GUI.backgroundColor = dbC;
                            },

                            drawHeaderCallback = HeaderCallbackDelegate,
                        };


                        innerListDict.Add(listKey, Reo_AbilityList);  //Store it on the Editor
                    }

                    //using (var X = new GUILayout.ScrollViewScope(Scroll, GUILayout.MaxHeight(200)))
                    //{
                    //  Scroll = X.scrollPosition;
                    Reo_AbilityList.DoLayoutList();
                    //  }

                    var buttonIndex = Reo_AbilityList.index;

                    if (buttonIndex != -1 && Reo_AbilityList.count > buttonIndex)
                    {
                        var elem = ButtonList.GetArrayElementAtIndex(buttonIndex);
                        DrawInputEvents(elem, buttonIndex);
                    }
                }
            }
        }
        //private Vector2 Scroll;

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnInputEnabled);
            EditorGUILayout.PropertyField(OnInputDisabled);
            EditorGUILayout.PropertyField(OnActionMapChanged);
            EditorGUILayout.PropertyField(CurrentControlScheme);

            EditorGUILayout.PropertyField(OnControlsChanged);
            EditorGUILayout.PropertyField(OnDeviceLost);
            EditorGUILayout.PropertyField(OnDeviceRegained);
        }

        protected void DrawInputEvents(SerializedProperty Element, int index)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;

                var InputName = Element.FindPropertyRelative("name").stringValue;



                Element.isExpanded = EditorGUILayout.Foldout(Element.isExpanded,
                    new GUIContent($"[{InputName} Properties]"));
                EditorGUI.indentLevel--;
                if (Element.isExpanded)
                {
                    var active = Element.FindPropertyRelative("active");
                    var debug = Element.FindPropertyRelative("debug");
                    var ignoreOnPause = Element.FindPropertyRelative("ignoreOnPause");
                    var ResetOnDisable = Element.FindPropertyRelative("ResetOnDisable");


                    var OnInputChanged = Element.FindPropertyRelative("OnInputChanged");
                    var OnInputDown = Element.FindPropertyRelative("OnInputDown");
                    var OnInputUp = Element.FindPropertyRelative("OnInputUp");


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(active);
                        MalbersEditor.DrawDebugIcon(debug);
                    }

                    EditorGUILayout.PropertyField(ResetOnDisable);
                    EditorGUILayout.PropertyField(ignoreOnPause);


                    using (new GUILayout.HorizontalScope())
                    {
                        var reference = Element.FindPropertyRelative("reference").objectReferenceValue as InputActionReference;
                        var style = EditorStyles.toolbarButton;


                        using (new GUILayout.VerticalScope())
                            for (int i = 0; i < reference.action.bindings.Count; i += 2)
                            {
                                EditorGUILayout.LabelField($"{reference.action.bindings[i].path}", style, GUILayout.MinWidth(50));
                            }

                        using (new GUILayout.VerticalScope())
                            for (int i = 1; i < reference.action.bindings.Count; i += 2)
                            {
                                EditorGUILayout.LabelField($"{reference.action.bindings[i].path}", style, GUILayout.MinWidth(50));
                            }
                    }


                    MalbersEditor.DrawSplitter();
                    EditorGUILayout.Space();


                    EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                    MInputInteraction interaction = (MInputInteraction)Element.FindPropertyRelative("interaction").enumValueIndex;


                    switch (interaction)
                    {
                        case MInputInteraction.Press:
                            EditorGUILayout.PropertyField(OnInputChanged);
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputPressed"));
                            break;
                        case MInputInteraction.Down:
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case MInputInteraction.Up:
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case MInputInteraction.LongPress:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("LongPressTime"), new GUIContent("Long Press Time", "Time the Input Should be Pressed"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnLongPress"), new GUIContent("On Long Press Completed"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputFloatValue"), new GUIContent("On Pressed Time Normalized"));
                            EditorGUILayout.PropertyField(OnInputDown, new GUIContent("On Input Down"));
                            EditorGUILayout.PropertyField(OnInputUp, new GUIContent("On Pressed Interrupted (On Input Up)"));
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case MInputInteraction.DoubleTap:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("DoubleTapTime"));
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(OnInputDown, new GUIContent("On First Tap"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnDoubleTap"));
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        case MInputInteraction.Toggle:
                            EditorGUILayout.PropertyField(OnInputChanged, new GUIContent("On Input Toggle"));
                            EditorGUILayout.PropertyField(OnInputDown, new GUIContent("On Toggle On"));
                            EditorGUILayout.PropertyField(OnInputUp, new GUIContent("On Toggle Off"));
                            break;
                        case MInputInteraction.Float:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("PressThreshold"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputFloatValue"), new GUIContent("On Input Float"));
                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;

                        case MInputInteraction.Vector2:
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("Vector2Mult"), new GUIContent("Multiplier", "Multiply the Vector2 value by this value"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnInputV2Value"), new GUIContent("On Vector2 Value"));


                            EditorGUILayout.PropertyField(OnInputDown);
                            EditorGUILayout.PropertyField(OnInputUp);
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                        default:
                            EditorGUILayout.PropertyField(OnInputChanged);
                            break;
                    }

                    var OnInputEnabled = Element.FindPropertyRelative("OnInputEnabled");
                    var OnInputDisabled = Element.FindPropertyRelative("OnInputDisabled");
                    EditorGUILayout.Space();
                    MalbersEditor.DrawSplitter();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(OnInputEnabled, new GUIContent($"On [{InputName}] Enabled"));
                    EditorGUILayout.PropertyField(OnInputDisabled, new GUIContent($"On [{InputName}] Disabled"));
                }
            }
        }

        protected void HeaderCallbackDelegate(Rect rect)
        {
            var spliter = (rect.width - 20) / 3;
            Rect R_1 = new(rect.x + 20, rect.y, spliter - 75, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new(rect.x + 20 + spliter - 70, rect.y, spliter + 75, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new(rect.x + (spliter * 2) + 30, rect.y, spliter - 5, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "   Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_2, "  Input Action Reference", EditorStyles.boldLabel);
            EditorGUI.LabelField(R_4, "  Interaction", EditorStyles.boldLabel);
        }
    }
#endif
}
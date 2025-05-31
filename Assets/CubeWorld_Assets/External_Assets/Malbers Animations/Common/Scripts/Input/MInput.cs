using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    [AddComponentMenu("Malbers/Input/MInput")]
    public class MInput : MonoBehaviour, IInputSource, IAnimatorListener
    {
        #region Variables
        public IInputSystem Input_System;
        // public Dictionary<string, InputRow> DInputs = new Dictionary<string, InputRow>();        //Shame it cannot be Serialided :(
        /// <summary>Default Input Row Values </summary>
        public List<InputRow> inputs = new();                                     //Used to convert them to dictionary
        public List<InputRow> AllInputs = new();
        public List<MInputMap> actionMaps = new();                                     //Used to convert them to dictionary
        public int ActiveMapIndex;
        public MInputMap DefaultMap;
        public MInputMap ActiveMap;

        [Tooltip("Reset All inputs if the Component is Disabled.")]
        public bool ResetAllInputsOnDisable = true;

        public bool showInputEvents = false;

        [Tooltip("It will reset the Inputs to False if the Game Window Loses Focus")]
        public bool ResetOnFocusLost = false;
        public UnityEvent OnInputEnabled = new();
        public UnityEvent OnInputDisabled = new();

        public BoolEvent OnUsingGamePad = new();

        [Tooltip("Inputs won't work on Time.Scale = 0")]
        public BoolReference IgnoreOnPause = new(true);

        public string PlayerID = "Player0"; //This is use for Rewired Asset

        /// <summary>Send to the Character to Move using the interface ICharacterMove</summary>
        public bool MoveCharacter { set; get; }

        public Action<Vector3> OnMoveAxis { get; set; } = delegate { };

        public Vector3 MoveAxis { get; set; }
        // public void SetMoveAxis(Vector3 move) => MoveAxis = move;

        #endregion

        //public void PlayerInput(IInputSource player) { }

        void Awake()
        {
            Initialize();
        }

        public void SetMap(int map)
        {
            if (map == 0)
            {
                ActiveMap = DefaultMap;
                ActiveMapIndex = 0;
            }
            else
            {
                var index = Mathf.Clamp(map - 1, 0, actionMaps.Count);
                ActiveMap = actionMaps[index];
                ActiveMapIndex = index + 1;
            }
        }

        /// <summary>
        /// Remap an Input using KeyCode
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newKeyCode"></param>
        public virtual void RemapInput(string name, KeyCode newKeyCode)
        {
            var foundInput = ActiveMap.inputs.Find(inputs => inputs.name == name);

            if (foundInput != null && foundInput.type == InputType.Key)
            {
                foundInput.key = newKeyCode;
            }
        }

        /// <summary>
        /// Remap an Input if is using a Input type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newInput"></param>
        public virtual void RemapInput(string name, string newInput)
        {
            var foundInput = ActiveMap.inputs.Find(inputs => inputs.name == name);

            if (foundInput != null && foundInput.type == InputType.Input)
            {
                foundInput.input = newInput;
            }
        }


        public virtual void SetMap(string map)
        {
            if (DefaultMap.name == map)
            {
                ResetMap();
            }
            else
            {
                var nextMap = actionMaps.FindIndex(x => x.name == map);

                if (nextMap != -1)
                {
                    ActiveMap = actionMaps[nextMap];
                    ActiveMapIndex = nextMap + 1;
                }
                else
                {
                    Debug.Log("No Action Map was found with the name: " + map);
                }
            }
        }

        public virtual void ResetMap()
        {
            ActiveMap = DefaultMap;
            ActiveMapIndex = 0;
        }

        protected virtual void Initialize()
        {
            InitializeDefaultMap();


            //Store all inputs from all MAPS in a new list
            AllInputs = new List<InputRow>(inputs);
            if (actionMaps.Count > 0)
                foreach (var item in actionMaps)
                    AllInputs = AllInputs.Concat(item.inputs).ToList();

            Input_System = DefaultInput.GetInputSystem(PlayerID);      //Get Which Input System is being used

            //Update to all the Inputs the Input System
            foreach (var i in AllInputs)
                i.InputSystem = Input_System;

            //  List_to_Dictionary();
        }

        public virtual void InitializeDefaultMap() => DefaultMap = new MInputMap() { name = new StringReference("Default"), inputs = this.inputs };


        // /// <summary>Convert the List of Inputs into a Dictionary</summary>
        //protected void List_to_Dictionary()
        // {
        //     DInputs = new Dictionary<string, InputRow>();
        //     foreach (var item in inputs)
        //         DInputs.Add(item.name, item);
        // }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && ResetOnFocusLost) ResetInputs();
        }


        /// <summary>Enable Disable the Input Script</summary>
        public virtual void Enable(bool val) => enabled = val;

        protected virtual void OnEnable()
        {
            OnInputEnabled.Invoke();
            SetMap(ActiveMapIndex);
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                OnInputDisabled.Invoke();

                if (ResetAllInputsOnDisable)
                    ResetInputs();
            }
        }

        public virtual void ResetInputs()
        {
            foreach (var input in inputs)
            {
                if (input.ResetOnDisable && input.Active) input.OnInputChanged.Invoke(input.InputValue = false);  //Sent false to all Input listeners 
            }



            foreach (var item in actionMaps)
            {
                foreach (var input in item.inputs)
                {
                    if (input.ResetOnDisable && input.Active) input.OnInputChanged.Invoke(input.InputValue = false);  //Sent false to all Input listeners 
                }
            }
        }

        private void Update() => SetInput();

        /// <summary>Send all the Inputs to the Animal</summary>
        protected virtual void SetInput()
        {
            if (IgnoreOnPause.Value && Time.timeScale == 0) return;

            //   Debug.Log($"activemap [{ActiveMap.name.Value}] [{ActiveMapIndex}]");

            foreach (var item in ActiveMap.inputs)
                _ = item.GetValue;  //This will set the Current Input value to the inputs and Invoke the Values


            CheckDevice();
        }

        protected virtual void CheckDevice()
        {
            if (IsJoystickInput())
            {
                if (!usingGamePad)
                {
                    usingGamePad = true;
                    OnUsingGamePad.Invoke(true);
                }
            }
            else if (IsMouseAndKeyboard())
            {
                if (usingGamePad)
                {
                    usingGamePad = false;
                    OnUsingGamePad.Invoke(false);
                }
            }

            currentMousePosition = Input.mousePosition;
        }

        protected bool usingGamePad;
        protected Vector3 currentMousePosition;

        protected virtual bool IsJoystickInput()
        {
            // joystick buttons
            if (Input.GetKey(KeyCode.Joystick1Button0) ||
                Input.GetKey(KeyCode.Joystick1Button1) ||
                Input.GetKey(KeyCode.Joystick1Button2) ||
                Input.GetKey(KeyCode.Joystick1Button3) ||
                Input.GetKey(KeyCode.Joystick1Button4) ||
                Input.GetKey(KeyCode.Joystick1Button5) ||
                Input.GetKey(KeyCode.Joystick1Button6) ||
                Input.GetKey(KeyCode.Joystick1Button7) ||
                Input.GetKey(KeyCode.Joystick1Button8) ||
                Input.GetKey(KeyCode.Joystick1Button9) ||
                Input.GetKey(KeyCode.Joystick1Button10) ||
                Input.GetKey(KeyCode.Joystick1Button11) ||
                Input.GetKey(KeyCode.Joystick1Button12) ||
                Input.GetKey(KeyCode.Joystick1Button13) ||
                Input.GetKey(KeyCode.Joystick1Button14) ||
                Input.GetKey(KeyCode.Joystick1Button15) ||
                Input.GetKey(KeyCode.Joystick1Button16) ||
                Input.GetKey(KeyCode.Joystick1Button17) ||
                Input.GetKey(KeyCode.Joystick1Button18) ||
                Input.GetKey(KeyCode.Joystick1Button19))
            {
                return true;
            }


            return false;
        }

        protected virtual bool IsMouseAndKeyboard()
        {
            // mouse & keyboard buttons
            if (Input.anyKey || Input.GetMouseButton(0))
                return true;
            // mouse movement
            if ((Input.mousePosition - currentMousePosition).sqrMagnitude > 0.01f)
                return true;

            return false;
        }



        /// <summary>Enable/Disable an Input Row</summary>
        public virtual void EnableInput(string name, bool value)
        {
            string[] inputsName = name.Split(',');

            foreach (var inp in inputsName)
            {
                for (int i = 0; i < AllInputs.Count; i++)
                {
                    if (AllInputs[i].name == inp) AllInputs[i].Active = value;
                }
            }
        }



        /// <summary>Set the value of  Reset on Disable in a Input Element</summary>
        public virtual void ResetOnDisableInput(string name, bool value)
        {
            string[] inputsName = name.Split(',');

            foreach (var inp in inputsName)
            {
                for (int i = 0; i < AllInputs.Count; i++)
                {
                    if (AllInputs[i].name == inp) AllInputs[i].ResetOnDisable = value;
                }
            }
        }

        /// <summary>Set the value of  Reset on Disable in a Input Element</summary>
        public virtual void IgnoreOnPauseInput(string name, bool value)
        {
            string[] inputsName = name.Split(',');

            foreach (var inp in inputsName)
            {
                for (int i = 0; i < AllInputs.Count; i++)
                {
                    if (AllInputs[i].name == inp) AllInputs[i].ignoreOnPause.Value = value;
                }
            }
        }

        public virtual void SetInput(string name, bool value)
        {
            for (int i = 0; i < AllInputs.Count; i++)
            {
                if (AllInputs[i].name == name)
                {
                    AllInputs[i].InputValue = value;
                }
            }
        }


        /// <summary>  Resets the value and toggle of an Input to False </summary>
        public virtual void ResetInput(string name)
        {
            for (int i = 0; i < AllInputs.Count; i++)
            {
                if (AllInputs[i].name == name)
                {
                    AllInputs[i].InputValue = false;
                }
            }
        }

        /// <summary>Enable an Input Row</summary>
        public virtual void EnableInput(string name) => EnableInput(name, true);

        /// <summary> Disable an Input Row </summary>
        public virtual void DisableInput(string name) => EnableInput(name, false);

        /// <summary>Check if an Input Row  is active</summary>
        public virtual bool IsActive(string name)
        {
            var i = GetInput(name);
            if (i != null) return i.Active;
            return false;
        }

        /// <summary>Check if an Input Row  exist  and returns it</summary>
        public virtual InputRow FindInput(string name)
        {
            if (ActiveMap == null) return null;
            return ActiveMap.inputs.Find(item => item.name == name);
        }

        public IInputAction GetInput(string name)
        {
            return AllInputs.Find(item => item.name == name);
        }


        public void ConnectInput(string name, UnityAction<bool> action)
        {
            if (string.IsNullOrEmpty(name)) return;

            var inputs = AllInputs.FindAll(item => item.name == name);

            foreach (var item in inputs)
                item.InputChanged.AddListener(action);
        }

        public void DisconnectInput(string name, UnityAction<bool> action)
        {
            if (string.IsNullOrEmpty(name)) return;
            var inputs = AllInputs.FindAll(item => item.name == name);

            foreach (var item in inputs)
                item.InputChanged.RemoveListener(action);
        }

        public void PlayerInput(IInputSource player) {/* Do nothing this is for the new input link*/}
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        #region Create Inputs
#if UNITY_EDITOR

        [ContextMenu("Disable All", false, 2000000)]
        private void DisableAllInputs()
        {
            UnityEditor.Undo.RecordObject(this, "DisableAllInputs");

            if (ActiveMapIndex == 0)
            {
                foreach (var inp in inputs)
                    inp.active.Value = false;
            }
            else
            {
                foreach (var inp in actionMaps[ActiveMapIndex - 1].inputs)
                    inp.active.Value = false;
            }

            MTools.SetDirty(this);
        }

        [ContextMenu("Enable All", false, 2000000)]
        private void EnableAllInputs()
        {
            UnityEditor.Undo.RecordObject(this, "EnableAllInputs");

            if (ActiveMapIndex == 0)
            {
                foreach (var inp in inputs)
                    inp.active.Value = true;
            }
            else
            {
                foreach (var inp in actionMaps[ActiveMapIndex - 1].inputs)
                    inp.active.Value = true;
            }

            MTools.SetDirty(this);
        }


        [ContextMenu("All Types = [Input]", false, 2000000)]
        private void ChangeToInputs()
        {
            UnityEditor.Undo.RecordObject(this, "Input Type: Input");


            foreach (var item in actionMaps)
            {
                foreach (var inp in item.inputs)
                    inp.type = InputType.Input;
            }


            foreach (var inp in inputs)
                inp.type = InputType.Input;

            MTools.SetDirty(this);
        }

        private List<InputRow> TrueInput => ActiveMapIndex == 0 ? this.inputs : actionMaps[ActiveMapIndex - 1].inputs;



        [ContextMenu("All Types = [Keys]", false, 2000000)]
        private void ChangeToKeys()
        {
            UnityEditor.Undo.RecordObject(this, "Input Type: Keys");

            foreach (var item in actionMaps)
            {
                foreach (var inp in item.inputs)
                    inp.type = InputType.Key;
            }

            foreach (var inp in inputs)
                inp.type = InputType.Key;

            MTools.SetDirty(this);
        }


        [ContextMenu("Create/Jump")]
        private void CreateJumpInput()
        {
            TrueInput.Add(new InputRow(true, "Jump", "Jump", KeyCode.Space, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Fly")]
        private void CreateFlyInput()
        {
            TrueInput.Add(new InputRow(true, "Fly", "Fly", KeyCode.Q, InputButton.Toggle, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Sprint")]
        private void CreateSprintInput()
        {
            var sprint = new InputRow(true, "Sprint", "Sprint", KeyCode.LeftShift, InputButton.Press, InputType.Key);

            TrueInput.Add(sprint);

            var method = this.GetUnityAction<bool>("MAnimal", "Sprint");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(sprint.OnInputChanged, method);
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Activate Zone")]
        private void CreateZone()
        {
            var sprint = new InputRow(true, "Zone", "Zone", KeyCode.E, InputButton.Down, InputType.Key);

            TrueInput.Add(sprint);

            var method = this.GetUnityAction("MAnimal", "Zone_Activate");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(sprint.OnInputDown, method);
            MTools.SetDirty(this);
        }


        [ContextMenu("Create/Main Attack")]
        private void CreateMainAttackInput()
        {
            TrueInput.Add(new InputRow(true, "MainAttack", "Fire1", KeyCode.Mouse0, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Secondary Attack")]
        private void Create2ndAttackInput()
        {
            TrueInput.Add(new InputRow(true, "SecondAttack", "Fire2", KeyCode.Mouse1, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Action")]
        private void CreateInteraction()
        {
            TrueInput.Add(new InputRow(true, "Action", "Action", KeyCode.E, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Dodge")]
        private void CreateDodge()
        {
            TrueInput.Add(new InputRow(true, "Dodge", "Dodge", KeyCode.Q, InputButton.Press, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Speed Up")]
        private void CreateSpeedUP()
        {
            var inputUp = new InputRow(true, "Speed Up", "Speed Up", KeyCode.Alpha2, InputButton.Down, InputType.Key);

            TrueInput.Add(inputUp);

            var method = this.GetUnityAction("MAnimal", "SpeedUp");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(inputUp.OnInputDown, method);

            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Speed Down")]
        private void CreateSpeedDown()
        {
            var SpeedDown = new InputRow(true, "Speed Down", "Speed Down", KeyCode.Alpha1, InputButton.Down, InputType.Key);
            TrueInput.Add(SpeedDown);

            var method = this.GetUnityAction("MAnimal", "SpeedDown");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(SpeedDown.OnInputDown, method);

            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Damage")]
        private void CreateDamage()
        {
            TrueInput.Add(new InputRow(true, "Damage", "Damage", KeyCode.J, InputButton.Down, InputType.Key));
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Strafe")]
        private void CreateStrafeInput()
        {
            var strafe = new InputRow(true, "Strafe", "Strafe", KeyCode.Tab, InputButton.Toggle, InputType.Key);

            TrueInput.Add(strafe);

            var method = this.GetUnityAction<bool>("MAnimal", "Strafe");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(strafe.OnInputChanged, method);
            MTools.SetDirty(this);
        }

        [ContextMenu("Create/Sneak")]
        private void CreateSneakInput()
        {
            var sne = new InputRow(true, "Sneak", "Sneak", KeyCode.C, InputButton.Down, InputType.Key);
            TrueInput.Add(sne);
            MTools.SetDirty(this);
        }



        [ContextMenu("Create/Lock On Target")]
        private void CreateLockOnTarget()
        {
            var key = new InputRow(true, "LockTarget", "LockTarget", KeyCode.Mouse2, InputButton.Down, InputType.Key);
            TrueInput.Add(key);

            var method = this.GetUnityAction("MalbersAnimations.Utilities.LockOnTarget", "LockTargetToggle");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(key.OnInputDown, method);

            MTools.SetDirty(this);
        }





#endif
        #endregion
    }

    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    #region InputRow and Input Axis 
    /// <summary>Input Class to change directly between Keys and Unity Inputs </summary>
    [System.Serializable]
    public class InputRow : IInputAction
    {
        public string name = "InputName";
        public BoolReference active = new(true);
        public BoolReference ignoreOnPause = new();
        public InputType type = InputType.Input;
        public string input = "Value";
        [SearcheableEnum]
        public KeyCode key = KeyCode.A;
        public bool debug;
        /// <summary>Type of Button of the Row System</summary>
        public InputButton GetPressed = InputButton.Press;
        /// <summary>Current Input Value</summary>

        // public bool InputValue = false;
        public bool InputValue
        {
            get => m_Input;
            set
            {
                if (m_Input != value)
                {
                    m_Input = value;
                    if (debug) Debug.Log($"<color=cyan><B>[Input {name} : {m_Input}]</B></color>");
                }
            }
        }
        private bool m_Input = false;

        //  public bool ToggleValue = false;
        [Tooltip("When the Input is disabled the input value will set to false and it will send that value to all possible connections")]
        public bool ResetOnDisable = true;


        public UnityEvent OnInputDown = new();
        public UnityEvent OnInputUp = new();
        public UnityEvent OnLongPress = new();
        public UnityEvent OnLongPressReleased = new();
        public UnityEvent OnDoubleTap = new();
        public BoolEvent OnInputChanged = new();
        public BoolEvent OnInputToggle => OnInputChanged;
        public UnityEvent OnInputEnable = new();
        public UnityEvent OnInputDisable = new();

        protected IInputSystem inputSystem = new DefaultInput();

        #region LONG PRESS and Double Tap
        public float DoubleTapTime = 0.3f;                          //Double Tap Time
        [Tooltip("Time the Input Should be Pressed")]
        public float LongPressTime = 0.5f;
        [Tooltip("Smooth decrese the acumulated pressed time")]
        public bool SmoothDecrease;
        //public FloatReference LongPressTime = new FloatReference(0.5f);
        private bool FirstInputPress = false;
        private bool InputCompleted = false;
        private float InputStartTime;
        public UnityEvent OnInputPressed = new UnityEvent();
        public FloatEvent OnInputFloat = new FloatEvent();

        #endregion

        /// <summary>Return True or False to the Selected type of Input of choice</summary>
        public virtual bool GetValue
        {
            get
            {
                if (!active) return false;
                if (ignoreOnPause) return false;
                if (inputSystem == null) return false;

                var oldValue = InputValue;

                switch (GetPressed)
                {
                    case InputButton.Press:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue)
                                OnInputDown.Invoke();
                            else
                                OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }

                        if (InputValue) OnInputPressed.Invoke();

                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Down:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                        if (oldValue != InputValue)
                        {
                            if (InputValue) OnInputDown.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }
                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.Up:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButtonUp(input) : Input.GetKeyUp(key);

                        if (oldValue != InputValue)
                        {
                            if (!InputValue) OnInputUp.Invoke();

                            OnInputChanged.Invoke(InputValue);
                        }
                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.LongPress:

                        InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                        if (oldValue != InputValue) OnInputChanged.Invoke(InputValue); //Just to make sure the Input is Pressed

                        //Debug.Log($"FirstInputPress = {FirstInputPress} | InputCompleted {InputCompleted}");

                        if (InputValue)
                        {
                            if (!FirstInputPress && !InputCompleted)
                            {
                                FirstInputPress = true;
                                InputStartTime = 0;
                                OnInputFloat.Invoke(0);
                                OnInputDown.Invoke();
                            }
                            else
                            {
                                if (!InputCompleted)
                                {
                                    if (InputStartTime >= LongPressTime)
                                    {
                                        OnInputFloat.Invoke(1);
                                        OnLongPress.Invoke(); //Complete the long press
                                        FirstInputPress = false;
                                        InputCompleted = true;
                                        //  return (InputValue = true);
                                    }
                                    else
                                    {
                                        InputStartTime += Time.deltaTime;
                                        OnInputFloat.Invoke(Mathf.Clamp01(InputStartTime / LongPressTime));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (InputCompleted) OnLongPressReleased.Invoke(); //Invoke when the Input Long press is completed and released


                            //If the Input was released before the LongPress was completed  
                            if (FirstInputPress)
                            {
                                if (SmoothDecrease)
                                {
                                    InputStartTime -= Time.deltaTime;

                                    if (InputStartTime > 0)
                                    {
                                        OnInputFloat.Invoke(Mathf.Clamp01(InputStartTime / LongPressTime));
                                    }
                                    else
                                    {
                                        ResetLongPress();
                                    }
                                }
                                else
                                {
                                    ResetLongPress();
                                }
                            }
                            else
                            {
                                InputCompleted = false;
                            }
                        }
                        break;
                    //-------------------------------------------------------------------------------------------------------
                    case InputButton.DoubleTap:
                        {
                            InputValue = (type == InputType.Input) ? InputSystem.GetButton(input) : Input.GetKey(key);

                            if (oldValue != InputValue)
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
                        }
                    case InputButton.Toggle:
                        {
                            var toggle = (type == InputType.Input) ? InputSystem.GetButtonDown(input) : Input.GetKeyDown(key);

                            if (toggle)
                            {
                                InputValue ^= true;
                                OnInputToggle.Invoke(InputValue);

                                if (InputValue) OnInputDown.Invoke();
                                else OnInputUp.Invoke();
                            }
                            break;
                        }
                    case InputButton.Axis:
                        {
                            var axisValue = InputSystem.GetAxis(input);
                            InputValue = Mathf.Abs(axisValue) > 0;

                            if (oldValue != InputValue)
                            {
                                if (InputValue)
                                {
                                    OnInputDown.Invoke();
                                }
                                else
                                {
                                    OnInputUp.Invoke();
                                    OnInputFloat.Invoke(0);
                                }

                                OnInputChanged.Invoke(InputValue);
                            }

                            if (InputValue)
                            {
                                OnInputPressed.Invoke();
                                OnInputFloat.Invoke(axisValue);
                            }
                            break;
                        }
                    default: break;
                }
                return InputValue;

                void ResetLongPress()
                {
                    InputStartTime = 0;
                    OnInputUp.Invoke();         //Set it as interrupted
                    FirstInputPress = false;    //This will reset the Long Press
                    InputCompleted = false;
                }
            }
        }

        public IInputSystem InputSystem { get => inputSystem; set => inputSystem = value; }
        public string Name { get => name; set => name = value; }
        public bool Active
        {
            get => active.Value;
            set
            {
                //Debug.Log($"EnableInput {name} - {value}"); ;
                active.Value = value;
                if (value)
                    OnInputEnable.Invoke();
                else
                    OnInputDisable.Invoke();
            }
        }
        public InputButton Button => GetPressed;

        public UnityEvent InputDown => this.OnInputDown;

        public UnityEvent InputUp => this.OnInputUp;

        public BoolEvent InputChanged => this.OnInputChanged;


        #region Constructors

        public InputRow(KeyCode k)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string input, KeyCode key)
        {
            active.Value = true;
            type = InputType.Key;
            this.key = key;
            this.input = input;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string unityInput, KeyCode k, InputButton pressed)
        {
            active.Value = true;
            type = InputType.Key;
            key = k;
            input = unityInput;
            GetPressed = InputButton.Down;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            active.Value = true;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow(bool active, string name, string unityInput, KeyCode k, InputButton pressed, InputType itype)
        {
            this.name = name;
            this.active.Value = active;
            type = itype;
            key = k;
            input = unityInput;
            GetPressed = pressed;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        public InputRow()
        {
            active.Value = true;
            name = "InputName";
            type = InputType.Input;
            input = "Value";
            key = KeyCode.A;
            GetPressed = InputButton.Press;
            inputSystem = new DefaultInput();
            ResetOnDisable = true;
        }

        #endregion
    }
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    [System.Serializable]
    public class InputAxis
    {
        public bool active = true;
        public string name = "NewAxis";
        public bool raw = true;
        public string input = "Value";
        IInputSystem inputSystem = new DefaultInput();
        public FloatEvent OnAxisValueChanged = new();
        float currentAxisValue = 0;

        /// <summary>Returns the Axis Value</summary>
        public float GetAxis
        {
            get
            {
                if (inputSystem == null || !active) return 0f;
                currentAxisValue = raw ? inputSystem.GetAxisRaw(input) : inputSystem.GetAxis(input);
                return currentAxisValue;
            }
        }

        /// <summary> Set/Get which Input System this Axis is using by Default is set to use the Unity Input System </summary>
        public IInputSystem InputSystem { get => inputSystem; set => inputSystem = value; }

        public InputAxis()
        {
            active = true;
            raw = true;
            input = "Value";
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string value)
        {
            active = true;
            raw = false;
            input = value;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string InputValue, bool active, bool isRaw)
        {
            this.active = active;
            this.raw = isRaw;
            input = InputValue;
            name = "NewAxis";
            inputSystem = new DefaultInput();
        }

        public InputAxis(string name, string InputValue, bool active, bool raw)
        {
            this.active = active;
            this.raw = raw;
            input = InputValue;
            this.name = name;
            inputSystem = new DefaultInput();
        }

    }
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    [System.Serializable]
    public class MInputMap
    {
        public StringReference name = new("New Map");
        public List<InputRow> inputs;
        public int selectedIndex;
    }
    #endregion
}
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using System;
using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Conditions;


#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    public abstract class State : ScriptableObject
    {
        /// <summary>Animal Transform.position</summary>
        public Vector3 Position
        {
            get => animal.Position;
            set => animal.Position = value;
        }

        /// <summary>Animal Transform.rotation</summary>
        public Quaternion Rotation
        {
            get => animal.Rotation;
            set => animal.Rotation = value;
        }

        /// <summary>Enter Status used while when the state was active</summary>
        public int EnterStatus { get; set; }

        /// <summary>Debug all gizmos of the state</summary>
        public bool GizmoDebug => m_debug && animal.debugGizmos;

        /// <summary>  Name that will be represented on the creation State List</summary>
        public virtual string StateName { get; }

        /// <summary>  Name that will be represented the ID Value for the state</summary>
        public abstract string StateIDName { get; }

        /// <summary>You can enable/disable temporarly  the State</summary>
        [HideInInspector]
        public bool Active = true;

        /// <summary>True if this state is the Animal Active State</summary>
        public bool IsActiveState => animal.ActiveState == this;

        /// <summary>Reference for the Animal that Holds this State</summary>
        [HideInInspector] public MAnimal animal;

        /// <summary>Height from the ground to the hip multiplied for the Scale Factor</summary>
        protected float Height => animal.Height;

        #region Animal Shortcuts
        /// <summary>(Z), horizontal (X) and Vertical (Y) Raw Movement Input</summary>
        internal Vector3 MovementRaw => animal.MovementAxisRaw;

        /// <summary>Forward (Z), horizontal (X) and Vertical (Y) Smoothed Movement Input AFTER aplied Speeds Multipliers (THIS GOES TO THE ANIMATOR)</summary>
        internal Vector3 MovementSmooth => animal.MovementAxisSmoothed;

        /// <summary>  Gravity Direction: Vector3.Down</summary>
        protected Vector3 Gravity => animal.Gravity;
        /// <summary>Reference for the Animal Transform</summary>
        protected Transform transform;
        /// <summary> Layers the Animal considers ground</summary>
        protected LayerMask GroundLayer => animal.GroundLayer;

        /// <summary> Up Vector is the Opposite direction of the Gravity dir</summary>
        protected Vector3 UpVector => animal.UpVector;
        /// <summary>Animal Transform.Forward (Stored) </summary>
        protected Vector3 Forward => animal.Forward;
        /// <summary>Animal Transform.UP (Stored)</summary>
        protected Vector3 Up => animal.Up;

        /// <summary>Animal Transform.Right (Stored)</summary>
        protected Vector3 Right => animal.Right;
        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        protected Vector3 DeltaPos => animal.DeltaPos;
        /// <summary>The Scale Factor of the Animal.. if the animal has being scaled this is the multiplier for the raycasting things </summary>
        protected float ScaleFactor => animal.ScaleFactor;
        #endregion

        [Space, Tooltip("Input to Activate the State, leave empty for automatic states")]
        /// <summary>Input to Activate the State</summary>
        public string Input;
        [Tooltip("Input to Exit the State, leave empty for automatic states")]
        /// <summary>Input to Activate the State</summary>
        public StringReference ExitInput;


        public BoolReference enterInputValue = new();
        public BoolReference exitInputValue = new();

        [Tooltip("Profiles works as profiles to activate different ways of using a State. E.g. You can fly as Ironman or with a Broom")]
        public IntReference m_StateProfile = new();


        [Tooltip("Forces the State to move always forward. E.g. when Flying the animal will not stay idling in one place")]
        public BoolReference AlwaysForward = new();

        [Tooltip("Priority of the State. Higher value -> more priority to be activated")]
        /// <summary>Priority of the State.  Higher value more priority</summary>
        public int Priority;

        [Tooltip("If the State is trying to be activated by an Input, and it failed... then Reset the Input Value")]
        public bool resetInputOnFailed = false;


        [Tooltip("When Entering this state the last state Animator Parameter will be be reset to -1")]
        public bool ResetLastState = false;


        [Tooltip("Main/Core Modifier. When the Animal enters the Main Animation, it will change the core parameters of the Animal")]
        public AnimalModifier General;
        [Tooltip("Main/Core Animation Messages. When the Animal enters the Main Animation,It will send messages to the Animal Components")]
        public List<MesssageItem> GeneralMessage;
        public List<TagModifier> TagModifiers = new();
        [Tooltip("When Sending messages, it will use Unity: SendMessage, instead of the IAnimatorListener Interface")]
        public bool UseSendMessage = false;
        [Tooltip("When Sending messages, it will send the messages to all the Animal Children gameobjects")]
        public bool IncludeChildren = true;

        /// <summary> Override to multiply on the Movement Axis when this state is active (By default is 1,1,1)</summary>
        internal Vector3 MovementAxisMult;

        [Tooltip(" To Allow to Exit the state, the Animations need to use the [Allow Exit Behaviour] on the Animator.")]
        public bool AllowExitFromAnim = false;

        [Tooltip("The State can be Activated even when it's already the Current Active state. Usefull for Double Jumps")]
        public bool CanTransitionToItself = false;

        [Tooltip("Sleep from state check if the Active State is on this list. Set this value to false to invert the list")]
        public bool IncludeSleepState = true;

        // [Space]
        [Tooltip("If the Active State is one of one on the List, the state cannot be activated")]
        public List<StateID> SleepFromState = new();

        [Tooltip(" If A mode is Enabled and is one of one on the List ...the state cannot be activated")]
        public List<ModeID> SleepFromMode = new();

        [Tooltip("When the State is active, Disable these modes. Modes will be internally disabled")]
        public List<ModeID> DisableModes = new();

        [Tooltip("Do not allow any modes when using this State. Modes will be internally disabled")]
        public BoolReference noModes = new();
        public bool NoModes { get => noModes.Value; }

        //[Tooltip("Which Modes are allowed during this State. Leave empty to include all")]
        //public List<ModeID> modes = new List<ModeID>();

        [Tooltip("If The state is trying to be active but the active State is on this list, " +
            "the State will be queued until the Active State is not inlcuded on the queue list")]
        public List<StateID> QueueFrom = new();

        [Tooltip("If the State exit, it cannot be used again until one of these states on this list gets activated.\n" +
            "E.g. You can disable fly and not using it again until the animal uses Idle or Locomotion.")]
        public List<StateID> ResetFrom = new();

        /// <summary>If True, the State will be on hold until some of the ResetFrom states are activated</summary>
        public bool OnHoldByReset { get; set; }

        /// <summary>  Has some Reset From States  </summary>
        public bool HasResetFrom => ResetFrom.Count > 0;


        [Tooltip(" If A Stance is active, and is one of one on the List ...the state cannot be activated")]
        public List<StanceID> SleepFromStance = new();

        [Tooltip("Which stances are allowed during this State. Leave empty to include all")]
        public List<StanceID> stances = new();



        [Tooltip("When the state is playing ,it will override the main capsule collider to fit better the stance")]
        public bool OverrideCapsule = false;
        public OverrideCapsuleCollider newCapsule = new();

        /// <summary>The State can play only in stances </summary>
        public bool HasStances => stances != null && stances.Count > 0;

        ///// <summary>The Modes that will be disabled during this state</summary>
        //public bool HasModes => DisableModes != null && DisableModes.Count > 0;

        //[Space]
        [Tooltip("Try States will try to activate every X frames")]
        public IntReference TryLoop = new(1);
        ///// <summary>Try Loop Used on the States</summary>
        //public int TryLoop
        //{
        //    get => tryLoop.Value; 
        //    set
        //    {
        //        tryLoop.Value = value;
        //        //Debug.Log($"tryLoop {name}: {value}");
        //    }
        //}

        [Tooltip("Keeps the state enabled for x seconds. It executes internally the AllowExit() state method. If is set to zero this will be ignored.")]
        public FloatReference Duration = new();

        //[Space]
        [Tooltip("Tag to Identify Entering Animations on a State.\nE.g. (TakeOff) in Fly, EnterWater on Swim")]
        public StringReference EnterTag = new();
        [Tooltip("Tag to Identify Exiting Animations on a State.\nE.g. (Land) in Fall, or SwimClimb in Swim")]
        public StringReference ExitTag = new();

        //[Tooltip("if True, the state will execute another frame of logic while entering the other state ")]
        //public bool ExitFrame = true;


        [Tooltip("Try Exit State on Main State Animation. E.g. The Fall Animation can try to exit only when is on the Fall Animation")]
        public bool ExitOnMain = true;
        [Tooltip("Time needed to activate this state again after exit")]
        public FloatReference EnterCooldown = new(0);
        [Tooltip("Time needed to exit this state after being activated")]
        public FloatReference ExitCooldown = new(0);


        [Tooltip("Can straffing be used with this State?")]
        public bool CanStrafe;
        [Tooltip("This state has new  strafe animations. If is set to false, then it will not update the Animator with the [StateOn] Paramter")]
        public bool StrafeAnimations = true;
        [Tooltip("Strafe Multiplier when movement is detected. This will make the Character be aligned to the Strafe Direction Quickly")]
        [Range(0, 1)]
        public float MovementStrafe = 1f;
        [Tooltip("Strafe Multiplier when there's no movement. This will make the Character be aligned to the Strafe Direction Quickly")]
        [Range(0, 1)]
        public float IdleStrafe = 1f;


        /// <summary> Use this for state that require moving forward all the time like Slide, Glide Forward Fly Forward...
        /// etc Fall will inherit too  the last state   </summary>
        public virtual bool KeepForwardMovement { get; set; } = false;

        internal bool ValidStance(StanceID currentStance)
        {
            if (!HasStances) return true;
            return stances.Contains(currentStance);
        }





        public bool m_debug = true;

        [HideInInspector] public int Editor_Tabs1;


        #region Properties
        protected QueryTriggerInteraction IgnoreTrigger => QueryTriggerInteraction.Ignore;

        /// <summary>Unique ID used on performance</summary>
        public int UniqueID { get; private set; }


        /// <summary>Reference for the Animal Animator</summary>
        protected Animator Anim => animal.Anim;

        /// <summary> Store the OnEnterOnExit Event</summary>
        internal OnEnterExitState EnterExitEvent;

        /// <summary>Check all the Rules to see if the state can be activated</summary>
        public bool CanBeActivated
        {
            get
            {
                var colorDeb = "orange";

                if (ActiveState == null)
                { DebugingState($"Failed <b>[{name}]</b> There's no active State [First Creation]", colorDeb); return false; }
                if (animal.JustActivateState)
                { DebugingState($"Failed <b>[{name}]</b> Another state was just activated", colorDeb); return false; }
                if (!Active || IsSleep)
                { DebugingState($"Failed <b>[{name}]</b> The State is disabled or Animal is set to Sleep", colorDeb); return false; }
                if (ActiveState.Priority > Priority && ActiveState.IgnoreLowerStates)
                { DebugingState($"Failed <b>[{name}]</b> Current State [{ActiveState.name}] has High Priority and [Ignore Lower States: ON]", colorDeb); return false; }
                if (IsActiveState && !CanTransitionToItself)
                { DebugingState($"Failed <b>[{name}]</b> State is already active and [Can transition to Self: OFF]", colorDeb); return false; }
                if (OnEnterCoolDown)
                { DebugingState($"Failed <b>[{name}]</b> State is still in on Enter Cooldown", colorDeb); return false; }
                if (OnHoldByReset)
                { DebugingState($"Failed <b>[{name}]</b> State [On Hold by Reset]. It needs other states to Reset it", colorDeb); return false; }

                if (ActiveState.IsPending)
                {
                    if (ActiveState.Priority < Priority)
                    {
                        DebugingState($"Pending <b>[{name}]</b> Override. Current State Priority [{ActiveState.Priority}] Animation", colorDeb);

                        return true;
                    }

                    DebugingState($"Failed <b>[{name}]</b> The Current State is Pending.. it has not enter its Main Animation", colorDeb);
                    return false;
                }
                return true;
            }
        }

        private void DebugingState(string value, string color1 = "white")
        {
#if UNITY_EDITOR
            if (animal.debugStates) Debug.Log($"<B>[{animal.name}]</B> → <color={color1}>{value}</color>", animal);
#endif
        }

        /// <summary>Has completed the Exit Cooldown so it can be activated again  </summary>
        public bool OnEnterCoolDown => EnterCooldown > 0 && !MTools.ElapsedTime(CurrentExitTime, EnterCooldown.Value/* + 0.01f*/);

        /// <summary>Main Tag of the Animation State which it will rule the State the ID name Converted to Hash</summary>
        public int MainTagHash { get; private set; }


        /// <summary> Hash of the Exit Tag Animation</summary>
        protected int ExitTagHash { get; private set; }

        /// <summary> Hash of the Tag of an Enter Animation</summary>
        protected int EnterTagHash { get; private set; }

        /// <summary>The State is on an Exit Animation</summary>
        public bool InExitAnimation => ExitTagHash != 0 && ExitTagHash == CurrentAnimTag;

        /// <summary>The State is on an Enter Animation(TAG)</summary>
        public bool InEnterAnimation => EnterTagHash != 0 && EnterTagHash == CurrentAnimTag;

        /// <summary>Current Time the state exited</summary>
        internal float CurrentExitTime { get; set; }

        /// <summary>Current Time the state was Activated</summary>
        internal float CurrentEnterTime { get; set; }

        /// <summary>Profiles works as profiles to activate different ways of using a State. E.g. You can fly as Ironman or with a Broomvated</summary>
        public int StateProfile { get => m_StateProfile.Value; set => m_StateProfile.Value = value; }

        /// <summary>Returns the Active Animation State tag Hash on the Base Layer</summary>
        protected int CurrentAnimTag => animal.AnimStateTag;

        /// <summary>Animal Current Active State. animal.ActiveState</summary>
        protected State ActiveState => animal.ActiveState;
        //Old
        protected State CurrentActiveState => ActiveState;

        /// <summary>Can the State use the TryExitMethod</summary>
        public bool CanExit { get; internal set; }
        //{
        //    get => canExit;
        //    internal set
        //    {
        //        canExit = value;
        //        Debug.Log($"{name}: canExit {canExit}");
        //    }
        //}
        //private bool canExit;

        //public bool AllowingExit => !IgnoreLowerStates && !IsPersistent;

        /// <summary>Input Value for a State (Some states can by activated by inputs</summary>
        public virtual bool InputValue //{ get; set; }

        {
            get => enterInputValue.Value;
            set
            {
                enterInputValue.Value = value;
                //  Debug.Log($"[STATE {name}] InputValue: {enterInputValue.Value} ");
            }
        }

        /// <summary>Exit Input Value for a State</summary>
        public virtual bool ExitInputValue //{ get; set; } 
        {
            get => exitInputValue.Value;
            set
            {
                exitInputValue.Value = value;
                //  Debug.Log($"[STATE {name}] InputValue: {enterInputValue.Value} ");
            }
        }

        /// <summary>Put a state to sleep it works with the Avoid States list</summary>
        public virtual bool IsSleepFromState { get; internal set; }
        //{
        //    get => isSleepFromState;
        //    set
        //    { 
        //        isSleepFromState = value;
        //        if ( m_debug)  Debug.Log($"<color=orange>[STATE {name}] SleepFromState: {isSleepFromState}</color>");
        //    }
        //}
        //bool isSleepFromState;

        /// <summary>Put a state to sleep When Certaing Mode is Enable</summary>
        public virtual bool IsSleepFromMode { get; internal set; }

        /// <summary>Put a state to sleep When Certaing Mode is Enable</summary>
        public virtual bool IsSleepFromStance { get; internal set; }

        /// <summary>The State is Sleep (From Mode, State or Stance)</summary>
        public virtual bool IsSleep => IsSleepFromMode || IsSleepFromState || IsSleepFromStance;

        /// <summary>is this state on queue?</summary>
        public virtual bool OnQueue { get; internal set; }

        /// <summary>The State wants to be activated but is on QUEUE!</summary>
        public bool OnActiveQueue { get; internal set; }

        /// <summary>The State is on the Main State Animation</summary>
        public bool InCoreAnimation { get; internal set; }

        /// <summary>Quick Access to Animal.currentSpeedModifier.position</summary>
        public float CurrentSpeedPos
        {
            get => animal.CurrentSpeedModifier.position;
            set => animal.currentSpeedModifier.position = value;
        }

        public MSpeed CurrentSpeed => animal.CurrentSpeedModifier;


        /// <summary>If True this state cannot be interrupted by other States</summary>
        public bool IsPersistent { get; set; }
        //{
        //    get => isPersistent;
        //    set
        //    {
        //        isPersistent = value;
        //        Debug.Log($"IsPersistent: {isPersistent} ");
        //    }
        //}
        //bool isPersistent;

        /// <summary>If true the states below it will not try to Activate themselves</summary>
        public bool IgnoreLowerStates { get; set; }
        //{
        //    get => ignoreLowerStates;
        //    set
        //    {
        //        ignoreLowerStates = value;
        //        Debug.Log($"ignoreLowerStates: {ignoreLowerStates} ");
        //    }
        //}
        //bool ignoreLowerStates;


        /// <summary>Means that is already activated but is Still exiting the Last State and it does not have entered any of the Active State Animations</summary>
        public bool IsPending { get; set; }
        //{
        //    get => isPending;
        //    set
        //    {
        //        isPending = value;
        //        if (animal.debugStates) 
        //            Debug.Log($" [{animal.name}] [{ID.name}] isPending: {isPending} ");
        //    }
        //}
        //bool isPending;

        /// <summary>The Last State still has animations to exit</summary>
       // public bool PendingExit { get; set; }


        /// <summary>Speed Sets this State may has... Locomotion, Sneak etc</summary>
        public List<MSpeedSet> SpeedSets = new();

        /// <summary>Gravity Power Multiplier for each state</summary>
        public virtual float GravityMultiplier => 1f;
        #endregion



        private IAnimatorListener[] listeners;

        //-----
        [Tooltip("ID to Identify the State. The name of the ID is the Core Tag used on the Animator")]
        /// <summary>ID Asset Reference</summary>
        public StateID ID;
        [Tooltip("Extra Enter Conditions needed to activate the State (Base Target: MAnimal Component)")]
        public Conditions2 EnterConditions;
        [Tooltip("If Conditions are true then Persistant and Allow Lower States will be set to false. Allowing other states to be activated")]
        public Conditions2 ExitConditions;



        #region Methods
        /// <summary> Return if this state have a current Tag used on the animal</summary>
        protected bool StateAnimationTags(int MainTag)
        {
            if (MainTagHash == MainTag) return true;

            var Foundit = TagModifiers.Find(tag => tag.TagHash == MainTag);

            return Foundit != null;
        }

        /// <summary>Set all the values for all the States on Awake</summary>
        public void AwakeState(MAnimal mAnimal)
        {
            animal = mAnimal;
            transform = animal.transform;

            AwakeState();

            TryOverride = true;
        }


        /// <summary>Calculates the Input Update IMPORTANT </summary>
        public virtual void InputAxisUpdate() => animal.InputAxisUpdate();

        /// <summary>Called on Awake</summary>
        public virtual void AwakeState()
        {
            if (ID == null) Debug.LogError($"State {name} is missing its ID", this);

            MainTagHash = Animator.StringToHash(ID.name);                       //Store the Main Tag at Awake
            ExitTagHash = Animator.StringToHash(ExitTag.Value);                       //Store the Main Tag at Awake
            EnterTagHash = Animator.StringToHash(EnterTag.Value);                       //Store the Main Tag at Awake

            foreach (var mod in TagModifiers)
                mod.TagHash = Animator.StringToHash(mod.AnimationTag);          //Convert all the Tags to HashTags


            //Find Speed Sets if the State does not have any
            if (SpeedSets == null || SpeedSets.Count == 0)
            {
                SpeedSets = new List<MSpeedSet>();
                foreach (var set in animal.speedSets) //Find if this state has a Speed Set
                    if (set.states.Contains(ID)) SpeedSets.Add(set);

            }


            foreach (var set in SpeedSets) //set the current Index
            {
                set.CurrentIndex = set.StartVerticalIndex;
            }

            if (SpeedSets.Count > 0) SpeedSets.Sort(); //IMPORTANT!

            EnterExitEvent = animal.OnEnterExitStates.Find(st => st.ID == ID);

            InputValue = false;
            ExitInputValue = false;
            OnHoldByReset = false;

            //ResetState();
            //ResetStateValues();



            //DirectionalVelocity = transform.forward; //As default the Directional is the Transform.forward

            if (TryLoop < 1) TryLoop = 1;

            UniqueID = UnityEngine.Random.Range(0, 99999);

            //Fin all the IAnimator Listener
            if (!UseSendMessage)
            {
                if (IncludeChildren)
                    listeners = animal.GetComponentsInChildren<IAnimatorListener>();
                else
                    listeners = animal.GetComponents<IAnimatorListener>();
            }
        }

        /// <summary>Current Direction Speed Applied to the Additional Speed, by default is the Animal Forward Direction
        /// This is after INPUT UPDATE</summary>
        public virtual Vector3 Speed_Direction() => animal.Forward * Mathf.Abs(animal.VerticalSmooth);


        /// <summary>Check if the State is Queued</summary>
        public bool CheckQueuedState()
        {
            // Debug.Log("Queued = " + OnQueue, this);

            if (OnQueue)
            {
                OnActiveQueue = true; //meaning is waiting for bee activated
                Debugging($"<color=green>[Active*Queued]</color>. Allow Exit to Active State: [{ActiveState.ID.name}]");

                ActiveState.AllowExit(); //Force Allow Exit
                animal.QueueState = this;
                return true;
            }
            return false;
        }

        /// <summary>Connects the State with the External Inputs Source</summary>
        internal void ConnectInput(IInputSource InputSource, bool connect)
        {
            //EntryInput
            if (connect)
                InputSource.ConnectInput(Input, ActivatebyInput);
            else
                InputSource.DisconnectInput(Input, ActivatebyInput);


            //ExitInput
            if (connect)
                InputSource.ConnectInput(ExitInput, ExitByInput);
            else
                InputSource.DisconnectInput(ExitInput, ExitByInput);

            ExtraInputs(InputSource, connect);
        }

        #region Extra Inputs and example
        /// <summary> Use this to connect extra inputs the State may have</summary>
        public virtual void ExtraInputs(IInputSource InputSource, bool connect)
        {
            //---------------This is an example of what you need to do on the override version this method on your custom state
            //if (connect)
            //{
            //    InputSource.ConnectInput(ExtraInputA, ExtraInputA_Logic);
            //    InputSource.ConnectInput(ExtraInputB, ExtraInputB_Logic);
            //}
            //else
            //{
            //    InputSource.DisconnectInput(ExtraInputA, ExtraInputA_Logic);
            //    InputSource.DisconnectInput(ExtraInputB, ExtraInputB_Logic);
            //}
        }

        ////---------------------ADD THIS IN YOUR CUSTOM STATE ------------------------------------------

        //public StringReference ExtraInputA = new("Action A");
        //public StringReference ExtraInputB = new("Action B");

        //public void ExtraInputA_Logic(bool InputValue)
        //{
        //    if (InputValue)
        //    {
        //        //Do your True Input Logic
        //    }
        //    else
        //    {
        //        //Do your True Input Stuff
        //    }
        //}

        //public void ExtraInputB_Logic(bool InputValue)
        //{
        //    if (InputValue)
        //    {
        //        //Do your True Input Logic
        //    }
        //    else
        //    {
        //        //Do your True Input Stuff
        //    }
        //}

        #endregion

        public virtual void Activate(int StateStatus)
        {
            EnterStatus = StateStatus; //Store the Enter Status
            animal.State_SetEnterStatus(EnterStatus);
            Activate();
        }


        /// <summary>Activate the State. Code is Applied on base.Activate()</summary>
        public virtual void Activate()
        {
            if (CheckQueuedState()) { return; }

            //CHECK IF CURRENT STATE is PENDING AND IS it HAS LOWER PRIORITY
            if (ActiveState.IsPending)
            {
                // Debug.Log("IS PENDING");

                ActiveState.IsPending = false;
            }

            animal.LastState = animal.ActiveState;    //Set a new Last State Release the Old States

            animal.Check_Queue_States(ID); //Check if a queue State was released

            if (animal.LastState != this)
                DisableModes_Temp(false, animal.LastState.DisableModes); //Release the modes 

            //Wake UP the State that is no longer on QUEUE and it was activated! (PRIORITY FOR THE QUEDED STATES)!
            if (animal.QueueReleased)
            {
                animal.QueueState.ActivateQueued();
                return;
            }

            if (animal.JustActivateState) { return; } //Do not activate any state if a new state has being activated in the same frame.

            Debugging($"Activated");

            animal.ActiveState = this;                  //Update to the Current State
            animal.LastState?.PostExitState();           //Exectute the Exit State code on the Last State.
            SetSpeed(); //Set the Speed on the New State
            MovementAxisMult = Vector3.one;

            if (animal.LastState != this)
                DisableModes_Temp(true, DisableModes);

            //IsActiveState = true;                       //Set this state as the Active State
            CanExit = false;
            CurrentEnterTime = Time.time;


            if (animal.LastState != ActiveState)
            {
                IsPending = true;
                //PendingExit = true;
            }
            else
            {
                //Clear the Anim Tag so it can enter again to the same Animation Again
                animal.AnimStateTag = -1;
                //  Debug.Log("ENTERING FROM THE SAME STATE ");
            }

            //FIRST WE PLAY THE EXIT EVENT!!
            if (animal.LastState != this)
                animal.LastState.EnterExitEvent?.OnExit.Invoke();

            //THEN THE ENTER EVENT! IMPORTANT!!!
            EnterExitEvent?.OnEnter.Invoke();

            //Execute the Allow Exit Time Feature here if the State has Duration
            if (Duration > 0)
            {
                //if (C_Duration != null) { animal.StopCoroutine(C_Duration); C_Duration = null; }//Make sure the Duration is interrutped
                C_Duration = IDuration();
                animal.StartCoroutine(C_Duration);
            }
        }

        public void DisableModes_Temp(bool disable, List<ModeID> modelist)
        {
            //Disable Temporarily the modes the States
            if (modelist != null && modelist.Count > 0)
            {
                foreach (var ID in modelist)
                {
                    if (animal.modes_Dict.TryGetValue(ID, out Mode mode))
                    {
                        if (disable) mode.Disable_Temporal(); else mode.Enable_Temporal();
                    }
                }
            }
        }

        System.Collections.IEnumerator C_Duration;
        private System.Collections.IEnumerator IDuration()
        {
            yield return new WaitForSeconds(Duration.Value);
            Debugging($"[Allow Exit] by Duration [{Duration.Value:F2} seg]");
            AllowExit();
        }

        public virtual void ForceActivate() => ForceActivate(-1);

        public virtual void ForceActivate(int enterStatus)
        {
            Debugging($"<Color=cyan>[Force Activated {enterStatus}] </color>");

            //EXIT EVENT FORCED! 
            ActiveState?.EnterExitEvent?.OnExit.Invoke();

            animal.LastState = ActiveState;             //Set a new Last State
            animal.ActiveState = this;                  //Update to the Current State ?????
            Activate(enterStatus);
            SetSpeed();                                 //Set the Speed on the New State

            //  IsActiveState = true;                       //Set this state as the Active State
            CanExit = false;
            // CurrentEnterTime = Time.time;

            animal.AnimStateTag = -1;

            if (animal.LastState != ActiveState)
            {
                //We need to set is as pending since we have not enter this states animations yet IMPORTANT IF we are not activating outselves
                IsPending = true;
                // PendingExit = true;

                // animal.LastState.EnterExitEvent?.OnExit.Invoke();
            }
            else
            {
                animal.AnimStateTag = -1;
                IsPending = false;
                //Clear the Anim Tag so it can enter again to the same Animation Again
                //  Debug.Log("ENTERING FROM THE SAME STATE ");
            }
            EnterExitEvent?.OnEnter.Invoke();
        }

        /// <summary>Search on the Internal Speed Set which one it can be used</summary>
        internal virtual void SetSpeed()
        {
            animal.CustomSpeed = false;

            foreach (var set in SpeedSets)
            {
                if (animal.Stance == 0 && !set.HasStances ||
                    animal.Stance != 0 && set.HasStance(animal.Stance))
                {
                    animal.CurrentSpeedSet = set;                   //Set a new Speed Set 
                    animal.CurrentSpeedIndex = set.CurrentIndex;                   //Set a new Speed Set 
                                                                                   //  Debug.Log($"Animal.CurrentSpeedSet: {animal.CurrentSpeedSet.name}");
                    return;
                }
            }

            var speedSet = new MSpeedSet()
            { name = this.name, Speeds = new List<MSpeed>(1) { new(this.name, animal.CurrentSpeedModifier.Vertical.Value, 4, 4) } };
            animal.CustomSpeed = true;
            animal.CurrentSpeedSet = speedSet; //Use Default instead
            animal.CurrentSpeedModifier = speedSet[0]; //Use Default instead
        }

        /// <summary> Reset a State values to its first Awakening </summary>
        public virtual void ResetState()
        {
            IgnoreLowerStates = false;
            InCoreAnimation = false;
            IsPersistent = false;
            IsPending = false;
            CanExit = false;
            IsSleepFromMode = false;
            IsSleepFromState = false;
            IsSleepFromStance = false;
            OnQueue = false;
            OnActiveQueue = false;
            CurrentExitTime = Time.time; //Set the Current Exit Time to the Current Time when a state exits 

            MovementAxisMult = Vector3.one;

            EnterStatus = -1; //Reset Enter Status
                              // IsActiveState = false;

            ResetInputOnFailed();

            foreach (var tags in TagModifiers) tags.Entered = false; //Teset the enter tag

        }

        protected virtual void ResetInputOnFailed()
        {
            if (resetInputOnFailed)
            {
                InputValue = false;
                //THIS IS CAUSING AN ISSUE WHEN MULTIPLE STATES ARE SUBSCRIBED TO THE SAME STATE
                animal.InputSource?.ResetInput(Input);
            }
        }

        /// <summary>Restore some of the Animal Parameters when the State exits</summary>
        public virtual void RestoreAnimalOnExit() { }


        /// <summary>Execute the Final Code After the new active state is played</summary>
        public virtual void PostExitState() { }

        /// <summary> Called when the Animal Component is enabled </summary>
        public virtual void OnAnimalEnabled() { }
        /// <summary> Called when the Animal Component is disabled </summary>
        public virtual void OnAnimalDisabled() { }

        /// <summary> Called when the Animal enters a zone</summary>
        public virtual void OnAnimalEnterZone(IZone zone)
        {
            // if (zone != null) MDebug.Log($"OnAnimalEnterZone {zone.transform.name}, State {name}");
        }

        /// <summary> Called when the Animal exit a zone</summary>
        public virtual void OnAnimalExitZone(IZone zone)
        {
            // MDebug.Log($"OnAnimalExitZone {zone.transform.name}");
        }

        /// <summary> Called on Disable on the Animal Controller (Use this to disconnect events from the states)  </summary>
        public virtual void ExitState()
        {
            ResetStateValues();
            ResetState();
            RestoreAnimalOnExit();

            if (C_Duration != null) { animal.StopCoroutine(C_Duration); C_Duration = null; }//Make sure the Duration is interrutped

            //Check that if it has reset from. then activate it
            if (HasResetFrom)
            {
                OnHoldByReset = true;
                // Debugging("OnHoldByReset {Until any state of the Reset list gets active}");
            }
        }

        /// <summary>Status Value of the State</summary>
        public void SetEnterStatus(int value) => animal.State_SetStatus(value);
        public void SetStatus(int value) => SetEnterStatus(value);
        public void SetFloat(float value) => animal.State_SetFloat(value);

        public void SetFloatSmooth(float value, float time)
        {
            if (animal.State_Float != 0f)
                animal.State_SetFloat(Mathf.MoveTowards(animal.State_Float, value, time));
        }

        /// <summary>Exit Status Value of the State</summary>
        public virtual void SetExitStatus(int value) => animal.State_SetExitStatus(value);

        public virtual void ActivateQueued()
        {
            OnQueue = false;
            OnActiveQueue = false;
            animal.QueueState = null;
            // animal.lastState = animal.activeState;
            Debugging("[No Longer on Queue]");
            Activate();
        }

        /// <summary> Send Messages to the Animal when entering Animations </summary>
        private void SendMessagesTags(List<MesssageItem> msgs)
        {
            if (msgs != null && msgs.Count > 0)
            {
                if (UseSendMessage)
                {
                    foreach (var item in msgs)
                        item.DeliverMessage(animal, IncludeChildren, animal.debugStates && m_debug);
                }
                else
                {
                    if (listeners != null && listeners.Length > 0)
                    {
                        foreach (var animListeners in listeners)
                        {
                            foreach (var item in msgs)
                                item.DeliverAnimListener(animListeners, animal.debugStates && m_debug);
                        }
                    }
                }
            }
        }

        /// <summary>When a Tag Changes apply this modifications</summary>
        public void AnimationTagEnter(int animTagHash)
        {
            if (!IsActiveState) return;// this need to be ignored if the State has not Started yet

            //Debug.Log($" <B>[State:{name}. Main: {MainTagHash}]  [New: {animTagHash}]  [In Core Anim: {InCoreAnimation}]</B>");

            //Check Tags on the State Animations
            if (MainTagHash == animTagHash || animTagHash == 0)
            {
                if (animTagHash == 0)
                    Debug.Log($"<b>[{name}]</b> The Current Animation State does not have any animation Tag.\n" +
                        $"The Animation State needs at least the animation Tag [{ID.name}]." +
                        $"\n See: <b>https://malbersanimations.gitbook.io/animal-controller/quickstart/common-issues#states-are-not-getting-active</b>");

                General.Modify(animal);

                if (!InCoreAnimation)
                {
                    Debugging($"<b>[{name}]</b> Entering Core Animation");
                    InternalCoreAnimation();
                }
            }
            //Try to find the Animation in the Tag Modifiers
            else
            {
                var AnimTag = TagModifiers.Find(tag => tag.TagHash == animTagHash);

                if (AnimTag != null)
                {
                    AnimTag.modifier.Modify(animal); //Modify it always (not just once)

                    InCoreAnimation = false;

                    if (!AnimTag.Entered)
                    {
                        AnimTag.Entered = true;
                        //CheckPendingExit();
                        animal.SprintUpdate();
                        SendMessagesTags(AnimTag.tagMessages); //Send the Messages to the Animal Controller when entering tags
                        InvokeEnterPendingFalse();
                        if (ResetLastState) animal.LastState_Reset();


                        Debugging($"<b>[{name}]</b> Entering Tag Animation  <B>[{AnimTag.AnimationTag}] </B>");
                        EnterTagAnimation();
                    }
                }
                //Means no Tag was found so Use the Default one
                //else if (animTagHash != -1 && !InCoreAnimation)
                //{
                //    Debugging($"No Tag Found, Check Your Tags! Using Core Tag [({ID.name}) ({animTagHash})]");
                //    EnteringCoreAnimation();
                //}
            }
        }

        private void InternalCoreAnimation()
        {
            InCoreAnimation = true;

            //Reset the Exit Enter Status on Main! But do it only the first time!!!
            SetExitStatus(0);
            SetEnterStatus(0);

            animal.SprintUpdate();

            SendMessagesTags(GeneralMessage); //Send the Messages to the Animal Controller

            InvokeEnterPendingFalse();

            if (ResetLastState) animal.LastState_Reset();

            EnterCoreAnimation();
        }

        private void InvokeEnterPendingFalse()
        {
            if (IsPending)
            {
                IsPending = false;
                animal.OnStateChange.Invoke(ID);//Invoke the Event only when the State is no longer Pending
                animal.OnState?.Invoke(ID);
            }
        }

        ///// <summary> Used on Pending States from the Last State exiting </summary>
        //private void CheckPendingExit()
        //{
        //    if (IsPending && PendingExit)
        //    {
        //        animal.LastState?.PendingAnimationState();
        //        PendingExit = false;
        //    }
        //}

        public void SetInput(bool value) => InputValue = value;

        /// <summary>Receive messages from the Animator Controller</summary>
        public void ReceiveMessages(string message, object value)
        {
            this.Invoke(message, value);
        }


        /// <summary>Enable the State using an Input. Example :Fly, Jump </summary>
        internal void ActivatebyInput(bool value)
        {
            // Debug.Log("BY INPUT");
            InputValue = value;
            if (!Active) return; //Do not reactivate if we are already active

            //Skip if the Input to Exit is the same as the input to enter and we are already on this state
            if (ExitInput == Input && IsActiveState) { return; }


            if (value && IsSleep                //If the State is sleep 
               || OnHoldByReset                 //Or is hold by reset
               || animal.LockInput             //Oth the animal has locked the inputs
               || animal.JustActivateState       //Or the Animal has just activated a state and this state is also trying to activate
            )
            {
                //Reset the input //Needed if the Input  was toggle (Glide)... but causes issues with  
                if (resetInputOnFailed)
                {
                    InputValue = false;

                    //THIS IS CAUSING AN ISSUE WHEN MULTIPLE STATES ARE SUBSCRIBED TO THE SAME STATE
                    animal.InputSource?.ResetInput(Input);
                }
            }
            else if (value && CanBeActivated && EnterConditions.Evaluate(animal))
            {
                StatebyInput();
            }
        }

        internal void ExitByInput(bool exitValue)
        {
            ExitInputValue = exitValue;

            //Only Exit if we are the active staet
            if (IsActiveState && CanExit)
            {
                StateExitByInput();
            }
        }


        internal void SetCanExit()
        {
            if (!CanExit && !IsPending && !animal.InTransition)
            {
                if (MTools.ElapsedTime(CurrentEnterTime, ExitCooldown))
                {
                    if (ExitOnMain)
                    {
                        if (InCoreAnimation) CanExit = true;
                    }
                    else
                    {
                        CanExit = true;
                    }
                }
            }
        }

        /// <summary> A new State has been activated</summary>
        internal void NewStateActivated(StateID stateID)
        {
            //Release the State from the Reset From list
            if (OnHoldByReset && ResetFrom.Contains(stateID))
            {
                OnHoldByReset = false;

                Debugging("Reseted from ResetFrom List");
            }

            NewActiveState(stateID);
        }

        /// <summary> Notifies all the  States that a new state has started</summary>
        public virtual void NewActiveState(StateID newState) { }


        /// <summary> Notifies all the  States the Speed Have Changed</summary>
        public virtual void SpeedModifierChanged(MSpeed speed, int SpeedIndex) { }


        /// <summary>Allow the State to be Replaced by lower States</summary>
        public bool AllowExit()
        {
            // if (!AllowExitFromAnim && CanExit)
            if (CanExit)
            {
                IgnoreLowerStates = false;
                IsPersistent = false;
                AllowStateExit();
            }
            return CanExit;
        }

        /// <summary>Internal Allow Exit Logic for the State This is called inside the Allow Exit methods </summary>
        public virtual void AllowStateExit() { }


        /// <summary>Allow the State to Exit. It forces the Next state to be activated. Set a value for the Exit Status </summary>
        public void AllowExit(int nextState, int StateExitStatus = 0)
        {
            SetExitStatus(StateExitStatus);

            if (!AllowExitFromAnim && AllowExit())
            {
                if (nextState != -1) animal.State_Activate(nextState);
            }
        }

        public void Debugging(string value, string color = "white")
        {
#if UNITY_EDITOR
            if (m_debug && animal.debugStates)
                MDebug.Log($"<B>[{animal.name}]</B> → <B>[{this.GetType().Name}]</B> → <color={color}>{value}</color>", this);
#endif
        }
        #endregion 

        #region Empty Methods

        /// <summary> Reset a State values to its first Awakening </summary>
        public void Enable(bool value)
        {
            Active = value;

            if (IsActiveState && !Active)
            {
                AllowExit();
            }
        }

        ///// <summary>This will be called on the Last State before the Active state enters Core animations</summary>
        //public virtual void PendingAnimationState() { }

        /// <summary>Set all the values for all the States on Start of the Animal... NOT THE START(ACTIVATION OF THE STATE) OF THE STATE</summary>
        public virtual void InitializeState() { }


        /// <summary>When Entering Core Animation of the State (the one tagged like the State) </summary>
        public virtual void EnterCoreAnimation() { }


        /// <summary>When Entering a new animation State do this</summary>
        public virtual void EnterTagAnimation() { }

        /// <summary>Logic to Try exiting to Lower Priority States</summary>
        public virtual void TryExitState(float DeltaTime) { }

        public virtual void AutoExitConditions()
        {
            if (CanExit && ExitConditions.Valid && ExitConditions.Evaluate(animal))
            {
                AllowExit();
            }
        }


        /// <summary>When creating the states for the first time, it will add to the Animal a new Speed Set</summary>
        public virtual void SetSpeedSets(MAnimal animal) { }


        public bool InternalTryActivate() => EnterConditions.Evaluate(animal) && TryActivate(); //First evaluate the enter conditions


        /// <summary>Logic Needed to Try to Activate the State, By Default is the Input Value for the State </summary>
        public virtual bool TryActivate() => InputValue && CanBeActivated;

        /// <summary>Try Override is an extra Value you can change outside the Animal Controller 
        /// to have an extra filter to set the Automatic Try Activate state
        /// Default value is true </summary>
        public virtual bool TryOverride { get; set; }

        public virtual void StatebyInput()
        {
            //Reset input if the State is sleep
            if (IsSleep) return;
            if (animal.LockInput) return;

            if (InternalTryActivate() && TryOverride)
            {
                Activate();      //Enable the State if is not already active
            }
        }


        /// <summary>Check if the state Exit Input is valid </summary>
        public virtual void StateExitByInput()
        {
            if (ExitInputValue) AllowExit();
        }

        /// <summary> Restore Internal values on the State (DO NOT INLCUDE Animal Methods or Parameter calls here</summary>
        public virtual void ResetStateValues() { }

        /// <summary> Restore Internal values on the State (DO NOT INLCUDE Animal Methods or Parameter calls here</summary>
        public virtual void OnStateMove(float deltatime) { }

        /// <summary>Called before Adding Additive Position and Rotation</summary>
        public virtual void OnStatePreMove(float deltatime) { }

        /// <summary>Called when a Mode Start Playing and This is the Active State</summary>
        public virtual void OnModeStart(Mode mode) { }

        /// <summary>Called when a Mode Ends Playing and This is the Active State</summary>
        public virtual void OnModeEnd(Mode mode) { }

        /// <summary>Called the Animal Grounded Plataform has changed</summary>
        public virtual void OnPlataformChanged(Transform newPlatform) { }

        public virtual void StateGizmos(MAnimal animal) { }

        /// <summary> Use this method to draw a custom inspector on the States</summary>
        public virtual bool CustomStateInspector() => false;
        #endregion

        internal virtual void Reset()
        {
            ID = MTools.GetInstance<StateID>(StateIDName); //Auto Fill the State ID Value
        }
    }


    /// <summary>When an new Animation State Enters and it have a tag = "AnimationTag"</summary>
    [System.Serializable]
    public class TagModifier
    {
        /// <summary>Animation State with the Tag  to apply the modifiers</summary>
        public string AnimationTag;
        public AnimalModifier modifier;

        /// <summary>"Animation Tag" Converted to TagHash</summary>
        public int TagHash { get; set; }

        public List<MesssageItem> tagMessages;                                     //Store messages to send it when Enter the animation State

        /// <summary>  Does the Tag has been activated already </summary>
        public bool Entered { get; internal set; }

    }

    /// <summary>Modifier for the Animals</summary>
    [System.Serializable]
    public struct AnimalModifier
    {
        ///// <summary>Animation State with the Tag  to apply the modifiers</summary>
        //public string AnimationTag;

        [Utilities.Flag]
        public modifier modify;

        /// <summary>Enable/Disable the Root Motion on the Animator</summary>
        public bool RootMotion;
        /// <summary>Enable/Disable the Root Motion Rotation Values on the Animator</summary>
        public bool RootMotionRotation;
        /// <summary>Enable/Disable the Sprint on the Animal </summary>
        public bool Sprint;
        /// <summary>Enable/Disable the Gravity on the Animal, only used when the animal is on the air, falling, jumping ..etc</summary>
        public bool Gravity;
        /// <summary>Enable/Disable the if the Animal is Grounded (Align|Snap to ground position) </summary>
        public bool Grounded;
        /// <summary>Enable/Disable the Rotation Alignment while grounded </summary>
        public bool OrientToGround;
        /// <summary>Enable/Disable the Custom Rotations ( Climb, Swimming, etc)</summary>
        public bool CustomRotation;
        /// <summary>Enable/Disable the Free Movement... This allow to Use the Pitch direction vector</summary>
        public bool FreeMovement;
        /// <summary>Enable/Disable Additive Position use on the Speed Modifiers</summary>
        public bool AdditivePosition;
        /// <summary>Enable/Disable Additive Rotation use on the Speed Modifiers</summary>
        public bool AdditiveRotation;
        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool Persistent;

        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool IgnoreLowerStates;

        /// <summary>Enable/Disable the movement on the Animal</summary>
        public bool LockMovement;

        /// <summary>Enable/Disable is AllInputs on the Animal</summary>
        public bool LockInput;


        public void Modify(MAnimal animal)
        {
            if ((int)modify == 0) return; //Means that the animal have no modification
            if (animal == null) return; //Animal is missing

            if (Modify(modifier.IgnoreLowerStates)) { animal.ActiveState.IgnoreLowerStates = IgnoreLowerStates; }
            if (Modify(modifier.AdditivePositionSpeed)) { animal.UseAdditivePos = AdditivePosition; }

            if (Modify(modifier.AdditiveRotationSpeed)) { animal.UseAdditiveRot = AdditiveRotation; }
            if (Modify(modifier.RootMotion)) { animal.RootMotion = RootMotion; }
            if (Modify(modifier.RootMotionRotation)) { animal.RootMotionRotation = RootMotionRotation; }

            if (Modify(modifier.Gravity)) { animal.UseGravity = Gravity; }
            if (Modify(modifier.Sprint)) { animal.UseSprintState = Sprint; }

            if (Modify(modifier.Grounded)) { animal.Grounded = Grounded; }
            if (Modify(modifier.OrientToGround)) { animal.UseOrientToGround = OrientToGround; }
            if (Modify(modifier.CustomRotation)) { animal.UseCustomRotation = CustomRotation; }
            if (Modify(modifier.Persistent)) { animal.ActiveState.IsPersistent = Persistent; /*Debug.Log($"{animal.ActiveState.name} + Pers{Persistent}");*/}
            if (Modify(modifier.LockInput)) { animal.LockInput = LockInput; }
            if (Modify(modifier.LockMovement)) { animal.LockMovement = LockMovement; }
            if (Modify(modifier.FreeMovement)) { animal.FreeMovement = FreeMovement; }
        }

        private readonly bool Modify(modifier modifier) => ((modify & modifier) == modifier);
    }
    public enum modifier
    {
        RootMotion = 1,
        Sprint = 2,
        Gravity = 4,
        Grounded = 8,
        OrientToGround = 16,
        CustomRotation = 32,
        IgnoreLowerStates = 64,
        Persistent = 128,
        LockMovement = 256,
        LockInput = 512,
        AdditiveRotationSpeed = 1024,
        AdditivePositionSpeed = 2048,
        FreeMovement = 4096,
        RootMotionRotation = 8192,
    }

    #region Inspector


#if UNITY_EDITOR

    [CustomEditor(typeof(State), true)]
    public class StateEd : Editor
    {
        SerializedProperty
           ID, Input, ExitInput, enterInputValue, exitInputValue,
            General, GeneralMessage, TryLoop, EnterTag, ExitTag, /*ExitFrame,*/ ExitOnMain, ExitCooldown, EnterCooldown,
            // Priority,
            CanStrafe, MovementStrafe, IdleStrafe, StrafeAnimations, ResetInputOnFailed, AlwaysForward,
            //  EnterConditions, AllowExitConditions,
            m_StateProfile, ResetLastState,
            debug, UseSendMessage, CanTransitionToItself,
            IncludeChildren, AllowExitAnimation, IncludeSleepState, Duration, S_Speed_List,
            OverrideCapsule, newCapsule,
           SleepFromState, SleepFromMode, noModes, DisableModes,
            TagModifiers, QueueFrom, ResetFrom, Editor_Tabs1, stances, SleepFromStance;

        State M;




        ReorderableList Reo_List_Speeds;

        readonly string[] Tabs = new string[] { "General", "Tags", "Limits", "Speeds", "" };

        GUIStyle GreatLabel;


        private static GUIContent editicon;
        public static GUIContent EditIcon
        {
            get
            {
                editicon ??= new GUIContent(EditorGUIUtility.IconContent("d_CapsuleCollider Icon"))
                {
                    tooltip = "Edit Main Capsule Collider"
                };
                return editicon;
            }
        }

        private void OnEnable()
        {
            if (target is not State) return;

            M = (State)target;
            Tabs[4] = M.ID ? M.ID.name : "Missing ID***";

            //Priority = serializedObject.FindProperty("Priority");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            noModes = serializedObject.FindProperty("noModes");
            DisableModes = serializedObject.FindProperty("DisableModes");
            ID = serializedObject.FindProperty("ID");
            S_Speed_List = serializedObject.FindProperty("SpeedSets");
            ResetLastState = serializedObject.FindProperty("ResetLastState");
            m_StateProfile = serializedObject.FindProperty("m_StateProfile");

            // EnterConditions = serializedObject.FindProperty("EnterConditions");
            //  AllowExitConditions = serializedObject.FindProperty("AllowExitConditions");


            Input = serializedObject.FindProperty("Input");
            enterInputValue = serializedObject.FindProperty("enterInputValue");

            ExitInput = serializedObject.FindProperty("ExitInput");
            exitInputValue = serializedObject.FindProperty("exitInputValue");


            TryLoop = serializedObject.FindProperty("TryLoop");
            AllowExitAnimation = serializedObject.FindProperty("AllowExitFromAnim");
            CanTransitionToItself = serializedObject.FindProperty("CanTransitionToItself");


            EnterTag = serializedObject.FindProperty("EnterTag");
            ExitTag = serializedObject.FindProperty("ExitTag");
            TagModifiers = serializedObject.FindProperty("TagModifiers");
            ResetInputOnFailed = serializedObject.FindProperty("resetInputOnFailed");

            General = serializedObject.FindProperty("General");
            GeneralMessage = serializedObject.FindProperty("GeneralMessage");
            UseSendMessage = serializedObject.FindProperty("UseSendMessage");
            IncludeChildren = serializedObject.FindProperty("IncludeChildren");


            //  ExitFrame = serializedObject.FindProperty("ExitFrame");
            ExitOnMain = serializedObject.FindProperty("ExitOnMain");
            ExitCooldown = serializedObject.FindProperty("ExitCooldown");
            EnterCooldown = serializedObject.FindProperty("EnterCooldown");

            CanStrafe = serializedObject.FindProperty("CanStrafe");
            MovementStrafe = serializedObject.FindProperty("MovementStrafe");
            IdleStrafe = serializedObject.FindProperty("IdleStrafe");
            StrafeAnimations = serializedObject.FindProperty("StrafeAnimations");
            AlwaysForward = serializedObject.FindProperty("AlwaysForward");


            debug = serializedObject.FindProperty("m_debug");


            IncludeSleepState = serializedObject.FindProperty("IncludeSleepState");
            SleepFromState = serializedObject.FindProperty("SleepFromState");
            SleepFromMode = serializedObject.FindProperty("SleepFromMode");
            QueueFrom = serializedObject.FindProperty("QueueFrom");
            ResetFrom = serializedObject.FindProperty("ResetFrom");
            stances = serializedObject.FindProperty("stances");
            SleepFromStance = serializedObject.FindProperty("SleepFromStance");
            Duration = serializedObject.FindProperty("Duration");
            OverrideCapsule = serializedObject.FindProperty("OverrideCapsule");
            newCapsule = serializedObject.FindProperty("newCapsule");

            Reo_List_Speeds = new ReorderableList(serializedObject, S_Speed_List, true, true, true, true)
            {
                drawElementCallback = Draw_Element_Speed,
                drawHeaderCallback = Draw_Header_Speed,
                onAddCallback = OnAddCallback_Speeds,
                onRemoveCallback = OnRemoveCallback_Speeds,

                onSelectCallback = (list) =>
                {
                    SelectedSpeed = list.index;
                }

            };

        }
        private int SpeedTabs = 0;
        private int SelectedSpeed = -1;

        public GUIContent Deb;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            if (GreatLabel == null)
                GreatLabel = new GUIStyle(EditorStyles.largeLabel) { fontStyle = FontStyle.Bold, fontSize = 14 };

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs);

            switch (Editor_Tabs1.intValue)
            {
                case 0: ShowGeneral(); break;
                case 1: ShowTags(); break;
                case 2: ShowLimits(); break;
                case 3: ShowSpeeds(); break;
                case 4: ShowState(); break;
                default: break;
            }

            serializedObject.ApplyModifiedProperties();

            Deb = new GUIContent((Texture)(AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Scripts/Editor/Icons/Debug_Icon.png", typeof(Texture))), "Debug");

            // base.OnInspectorGUI();
        }


        private void ShowSpeeds()
        {
            MalbersEditor.DrawDescription($"State Speeds");
            MSpeedEditor.ShowSpeeds(Reo_List_Speeds, M.SpeedSets, SelectedSpeed, ref SpeedTabs);
        }

        #region Draw Speeds

        private void Draw_Header_Speed(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var nameRec = new Rect(rect.x + 30, rect.y, rect.width / 2, height);

            EditorGUI.LabelField(nameRec, "Speed Sets");

            Rect R_1 = new Rect(rect.width + 5, rect.y, 20, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(R_1, "?"))
                Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/speeds");

        }

        private void OnRemoveCallback_Speeds(ReorderableList list)
        {
            S_Speed_List.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;
            SelectedSpeed = -1;

            if (list.index == -1 && S_Speed_List.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            EditorUtility.SetDirty(M);
        }

        private void OnAddCallback_Speeds(ReorderableList reo_List_Speeds)
        {
            if (M.SpeedSets == null) M.SpeedSets = new List<MSpeedSet>();

            M.SpeedSets.Add(new MSpeedSet());

            EditorUtility.SetDirty(M);
        }

        private void Draw_Element_Speed(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (S_Speed_List.arraySize <= index) return;

            var nameRect = new Rect(rect);
            var IndexRect = new Rect(rect);

            nameRect.y += 1;
            nameRect.x += 25;
            nameRect.height = EditorGUIUtility.singleLineHeight;
            IndexRect.height = EditorGUIUtility.singleLineHeight;
            IndexRect.width = 20f;

            Rect activeRect = new Rect(nameRect);

            var speedSet = S_Speed_List.GetArrayElementAtIndex(index);
            var nameSpeedSet = speedSet.FindPropertyRelative("name");
            nameRect.width /= 2;
            nameRect.width += 10;
            EditorGUI.LabelField(IndexRect, $"[{index}]");
            EditorGUI.PropertyField(nameRect, nameSpeedSet, GUIContent.none);

            activeRect.x = rect.width / 2 + 80;
            activeRect.width = rect.width / 2 - 40;
        }
        #endregion

        private void ShowGeneral()
        {
            MalbersEditor.DrawDescription($"Global State parameters");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(ID);
                    MalbersEditor.DrawDebugIcon(debug);
                }


                // EditorGUILayout.PropertyField(Priority);
                EditorGUILayout.PropertyField(m_StateProfile);
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                EditorGUILayout.PropertyField(Input, new GUIContent("Enter Input"));
                EditorGUILayout.PropertyField(ExitInput);
                EditorGUILayout.PropertyField(enterInputValue);
                EditorGUILayout.PropertyField(exitInputValue);
                EditorGUILayout.PropertyField(ResetInputOnFailed);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(OverrideCapsule);
                if (OverrideCapsule.boolValue)
                {
                    EditorGUILayout.PropertyField(newCapsule);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(AlwaysForward);
                EditorGUILayout.PropertyField(ResetLastState);
                EditorGUILayout.PropertyField(AllowExitAnimation);
                EditorGUILayout.PropertyField(CanTransitionToItself);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //  EditorGUILayout.PropertyField(ExitFrame);
                EditorGUILayout.PropertyField(ExitOnMain);
                EditorGUILayout.PropertyField(EnterCooldown);
                EditorGUILayout.PropertyField(ExitCooldown);
                EditorGUILayout.PropertyField(TryLoop);
                EditorGUILayout.PropertyField(Duration);
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(CanStrafe);
                if (M.CanStrafe)
                {
                    EditorGUILayout.PropertyField(StrafeAnimations);
                    EditorGUILayout.PropertyField(MovementStrafe);
                    EditorGUILayout.PropertyField(IdleStrafe);
                }
            }

            //using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            //{
            //    EditorGUILayout.PropertyField(EnterConditions);
            //    EditorGUILayout.PropertyField(AllowExitConditions);
            //}

            ShowDebug();
        }
        private void ShowTags()
        {
            MalbersEditor.DrawDescription($"Animator Tags will modify the core parameters on the Animal.\nThe core tag value is the name of the ID - [{Tabs[3]}]");


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(EnterTag);
                EditorGUILayout.PropertyField(ExitTag);
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(General, new GUIContent("Tag [" + Tabs[4] + "]"), true);

                var st = new GUIStyle(EditorStyles.boldLabel);
                st.fontSize += 2;

                // EditorGUILayout.Space();
                EditorGUILayout.LabelField("Messages", st);

                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {

                    EditorGUIUtility.labelWidth = 85;
                    EditorGUILayout.PropertyField(UseSendMessage, new GUIContent("Use SendMsg"));
                    EditorGUIUtility.labelWidth = 55;
                    EditorGUILayout.PropertyField(IncludeChildren, new GUIContent("Children"));
                    EditorGUIUtility.labelWidth = 0;
                }


                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(GeneralMessage, new GUIContent("Messages [" + Tabs[4] + "]"), true);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animation Tags", st);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(TagModifiers, new GUIContent(TagModifiers.displayName + " [" + TagModifiers.arraySize + "]"), true);
                EditorGUI.indentLevel--;
            }
        }
        private void ShowDebug()
        {
            if (M.m_debug && Application.isPlaying && M.animal)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.FloatField("Priority", M.Priority);
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUIUtility.labelWidth = 50;
                        using (new GUILayout.VerticalScope())
                        {
                            EditorGUILayout.ToggleLeft("Enabled", M.Active);
                            EditorGUILayout.ToggleLeft("Is Active State", M.IsActiveState);
                            EditorGUILayout.ToggleLeft("Can Exit", M.CanExit);
                            EditorGUILayout.ToggleLeft("OnQueue", M.OnQueue);
                            EditorGUILayout.ToggleLeft("On Active Queue", M.OnActiveQueue);
                            EditorGUILayout.ToggleLeft("Pending", M.IsPending);
                            // EditorGUILayout.ToggleLeft("Pending Exit", M.PendingExit);
                            EditorGUILayout.ToggleLeft("Sleep From State", M.IsSleepFromState);
                            EditorGUILayout.ToggleLeft("Sleep", M.IsSleep);
                        }
                        using (new GUILayout.VerticalScope())
                        {
                            EditorGUILayout.ToggleLeft("Sleep From Mode", M.IsSleepFromMode);
                            EditorGUILayout.ToggleLeft("Sleep From Stance", M.IsSleepFromStance);
                            EditorGUILayout.ToggleLeft("In Core Animation", M.InCoreAnimation);
                            EditorGUILayout.ToggleLeft("Ignore Lower States", M.IgnoreLowerStates);
                            EditorGUILayout.ToggleLeft("Is Persistent", M.IsPersistent);
                            EditorGUILayout.ToggleLeft("On Hold by Reset", M.OnHoldByReset);
                            EditorGUILayout.ToggleLeft("Enter Input Value", M.InputValue);
                            EditorGUILayout.ToggleLeft("Exit Input Value", M.ExitInputValue);
                        }
                        EditorGUIUtility.labelWidth = 0;

                        Repaint(); //Repaint the Editor to show the changes
                    }
                }
            }
        }
        private void ShowLimits()
        {
            MalbersEditor.DrawDescription($"Set Limitations to the States when another State, Mode or Stance is playing");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var incl = IncludeSleepState.boolValue;
                var AcSleep = incl ? "INCLUDE" : "EXCLUDE";
                var AcSleepList = incl ? "" : "EXCEPT";


                var dC = GUI.color;

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("States", GreatLabel);

                    GUI.color = incl ? GUI.color : Color.red;
                    IncludeSleepState.boolValue = GUILayout.Toggle(incl, new GUIContent(AcSleep), EditorStyles.miniButton, GUILayout.Width(90));
                }

                GUI.color = dC;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(SleepFromState, new GUIContent($"Sleep from States {AcSleepList}"), true);
                EditorGUILayout.PropertyField(QueueFrom, true);
                EditorGUILayout.PropertyField(ResetFrom, true);
                EditorGUI.indentLevel--;
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Modes", GreatLabel);
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(SleepFromMode, true);
                EditorGUILayout.PropertyField(noModes);

                if (!M.noModes.Value)
                {
                    EditorGUILayout.PropertyField(DisableModes);
                }
                EditorGUI.indentLevel--;
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Stances", GreatLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(SleepFromStance, true);
                EditorGUILayout.PropertyField(stances, new GUIContent("Allowed Stances", stances.tooltip), true);
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void ShowState()
        {
            //var st = new GUIStyle(EditorStyles.boldLabel);
            //st.fontSize += 2;

            MalbersEditor.DrawDescription($"{Tabs[4]} Parameters");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))

            {
                if (!M.CustomStateInspector())
                {
                    var skip = 41;
                    var property = serializedObject.GetIterator();
                    property.NextVisible(true);

                    for (int i = 0; i < skip; i++)
                        property.NextVisible(false);


                    do EditorGUILayout.PropertyField(property, true);
                    while (property.NextVisible(false));
                }
            }
        }
    }


    [UnityEditor.CustomPropertyDrawer(typeof(AnimalModifier))]
    public class AnimalModifierDrawer : UnityEditor.PropertyDrawer
    {

        private float Division;
        int activeProperties;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none, UnityEditor.EditorStyles.helpBox);

            position.x += 2;
            position.width -= 2;

            position.y += 2;
            position.height -= 2;


            var indent = UnityEditor.EditorGUI.indentLevel;
            UnityEditor.EditorGUI.indentLevel = 0;

            var height = UnityEditor.EditorGUIUtility.singleLineHeight;

            #region Serialized Properties
            var modify = property.FindPropertyRelative("modify");
            // var Colliders = property.FindPropertyRelative("Colliders");
            var RootMotion = property.FindPropertyRelative("RootMotion");
            var RootMotionRot = property.FindPropertyRelative("RootMotionRotation");
            var Sprint = property.FindPropertyRelative("Sprint");
            var Gravity = property.FindPropertyRelative("Gravity");
            var OrientToGround = property.FindPropertyRelative("OrientToGround");
            var CustomRotation = property.FindPropertyRelative("CustomRotation");
            var IgnoreLowerStates = property.FindPropertyRelative("IgnoreLowerStates");
            var AdditivePositionSpeed = property.FindPropertyRelative("AdditivePosition");
            var AdditiveRotation = property.FindPropertyRelative("AdditiveRotation");
            var Grounded = property.FindPropertyRelative("Grounded");
            var FreeMovement = property.FindPropertyRelative("FreeMovement");
            var Persistent = property.FindPropertyRelative("Persistent");
            var LockInput = property.FindPropertyRelative("LockInput");
            var LockMovement = property.FindPropertyRelative("LockMovement");
            #endregion

            var line = position;
            var lineLabel = line;
            line.height = height;

            var foldout = lineLabel;
            foldout.width = 10;
            foldout.x += 10;

            EditorGUIUtility.labelWidth = 16;
            EditorGUIUtility.labelWidth = 0;

            modify.intValue = (int)(modifier)EditorGUI.EnumFlagsField(line, label, (modifier)(modify.intValue));

            line.y += height + 2;
            Division = line.width / 3;

            activeProperties = 0;
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion))
                DrawProperty(ref line, RootMotion, new GUIContent("RootMotion", "Root Motion:\nEnable/Disable the Root Motion (Position and Rotation) on the Animator"));

            if (Modify(ModifyValue, modifier.RootMotionRotation))
                DrawProperty(ref line, RootMotionRot, new GUIContent("Root Motion Rot", "Root MotionRotation:\nEnable/Disable the Root Motion (Only Rotation) the Animator"));

            if (Modify(ModifyValue, modifier.Sprint))
                DrawProperty(ref line, Sprint, new GUIContent("Sprint", "Sprint:\nEnable/Disable Sprinting on the Animal"));

            if (Modify(ModifyValue, modifier.Gravity))
                DrawProperty(ref line, Gravity, new GUIContent("Gravity", "Gravity:\nEnable/Disable the Gravity on the Animal. Used when is falling or jumping"));

            if (Modify(ModifyValue, modifier.Grounded))
                DrawProperty(ref line, Grounded, new GUIContent("Grounded", "Grounded\nEnable/Disable if the Animal is Grounded (If True it will  calculate  the Alignment for Position with the ground ). If False:  Orient to Ground is also disabled."));

            if (Modify(ModifyValue, modifier.CustomRotation))
                DrawProperty(ref line, CustomRotation, new GUIContent("Custom Rot", "Custom Rotation: \nEnable/Disable the Custom Rotations (Used in Climb, Swim), This will disable Orient to Ground"));

            UnityEditor.EditorGUI.BeginDisabledGroup(CustomRotation.boolValue || !Grounded.boolValue);
            if (Modify(ModifyValue, modifier.OrientToGround))
                DrawProperty(ref line, OrientToGround, new GUIContent("Orient Ground", "Orient to Ground:\nEnable/Disable the Rotation Alignment while grounded. (If False the Animal will be aligned with the Up Vector)"));
            UnityEditor.EditorGUI.EndDisabledGroup();

            if (Modify(ModifyValue, modifier.IgnoreLowerStates))
                DrawProperty(ref line, IgnoreLowerStates, new GUIContent("Ignore Lower States", "States below will not be able to try to activate themselves"));

            if (Modify(ModifyValue, modifier.Persistent))
                DrawProperty(ref line, Persistent, new GUIContent("Persistent", "Persistent:\nEnable/Disable is Persistent on the Active State ... meaning the Animal will not Try to activate any States"));

            if (Modify(ModifyValue, modifier.LockMovement))
                DrawProperty(ref line, LockMovement, new GUIContent("Lock Move", "Lock Movement:\nLock the Movement on the Animal, does not include Action Inputs for Attack, Jump, Action, etc"));

            if (Modify(ModifyValue, modifier.LockInput))
                DrawProperty(ref line, LockInput, new GUIContent("Lock Input", "Lock Input:\nLock the Inputs, (Jump, Attack, etc) does not include Movement Input (WASD or Axis Inputs)"));

            if (Modify(ModifyValue, modifier.AdditiveRotationSpeed))
                DrawProperty(ref line, AdditiveRotation, new GUIContent("+ Rot Speed", "Additive Rotation Speed:\nEnable/Disable Additive Rotation used on the Speed Modifier"));

            if (Modify(ModifyValue, modifier.AdditivePositionSpeed))
                DrawProperty(ref line, AdditivePositionSpeed, new GUIContent("+ Pos Speed", "Additive Position Speed:\nEnable/Disable Additive Position used on the Speed Modifiers"));


            if (Modify(ModifyValue, modifier.FreeMovement))
                DrawProperty(ref line, FreeMovement, new GUIContent("Free Move", "Free Movement:\nEnable/Disable the Free Movement... This allow to Use the Pitch direction vector and the Rotator Transform"));

            UnityEditor.EditorGUI.indentLevel = indent;
            UnityEditor.EditorGUI.EndProperty();
        }

        private void DrawProperty(ref Rect rect, UnityEditor.SerializedProperty property, GUIContent content)
        {
            Rect splittedLine = rect;
            splittedLine.width = Division - 1;

            splittedLine.x += (Division * (activeProperties % 3)) + 1;

            // property.boolValue = GUI.Toggle(splittedLine, property.boolValue, content, EditorStyles.miniButton);
            property.boolValue = UnityEditor.EditorGUI.ToggleLeft(splittedLine, content, property.boolValue);

            activeProperties++;
            if (activeProperties % 3 == 0)
            {
                rect.y += UnityEditor.EditorGUIUtility.singleLineHeight + 2;
            }
        }


        private bool Modify(int modify, modifier modifier)
        {
            return ((modify & (int)modifier) == (int)modifier);
        }

        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            int activeProperties = 0;

            var modify = property.FindPropertyRelative("modify");
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion)) activeProperties++;
            if (Modify(ModifyValue, modifier.Sprint)) activeProperties++;
            if (Modify(ModifyValue, modifier.Gravity)) activeProperties++;
            if (Modify(ModifyValue, modifier.Grounded)) activeProperties++;
            if (Modify(ModifyValue, modifier.CustomRotation)) activeProperties++;
            if (Modify(ModifyValue, modifier.OrientToGround)) activeProperties++;
            if (Modify(ModifyValue, modifier.IgnoreLowerStates)) activeProperties++;
            if (Modify(ModifyValue, modifier.AdditivePositionSpeed)) activeProperties++;
            if (Modify(ModifyValue, modifier.AdditiveRotationSpeed)) activeProperties++;
            if (Modify(ModifyValue, modifier.Persistent)) activeProperties++;
            if (Modify(ModifyValue, modifier.FreeMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockInput)) activeProperties++;
            //  if (Modify(ModifyValue, modifier.Colliders)) activeProperties++;

            float lines = (int)((activeProperties + 2) / 3) + 1;

            return base.GetPropertyHeight(property, label) * lines + (2 * lines);
        }
    }
#endif
    #endregion
}

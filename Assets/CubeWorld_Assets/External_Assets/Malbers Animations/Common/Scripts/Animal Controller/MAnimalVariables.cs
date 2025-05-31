using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Controller
{
    /// Variables
    public partial class MAnimal
    {
        /// <summary>Used to have local Priority and Active values
        /// </summary>
        [System.Serializable]
        public class StateCache
        {
            public bool active = true;
            public State state;
            public int priority;
        }

        /// <summary>  By default the character will use the Main Camera Direction to calculate the relative movement of the Input source.  </summary>
        public BoolReference UseMainCameraDirection = new(true);

        ///<summary> List of States for this animal Cached and used in Runtime Stuff </summary>
        public List<StateCache> states_C = new();

        ///<summary> List of States for this animal  </summary>
        public List<State> states = new();
        /// <summary>List of Stances Available on the animals</summary>
        public List<Stance> Stances;
        ///<summary> List of States for this animal  </summary>
        public List<Mode> modes = new();

        ///<summary> List of Modes for this animal  [Converted to dictionary to be faster to find]</summary>
        internal Dictionary<int, Mode> modes_Dict;

        /// <summary>Sets a Bool Parameter on the Animator using the parameter Hash</summary>
        public System.Action<int, bool> SetBoolParameter { get; set; }
        /// <summary>Sets a float Parameter on the Animator using the parameter Hash</summary>
        public System.Action<int, float> SetFloatParameter { get; set; }

        /// <summary>Sets a Integer Parameter on the Animator using the parameter Hash</summary> 
        public System.Action<int, int> SetIntParameter { get; set; }

        /// <summary>Sets a Trigger Parameter on the Animator using the parameter Hash</summary> 
        public System.Action<int> SetTriggerParameter { get; set; }

        /// <summary>Check when a Animation State is Starting</summary>
        public System.Action<int> StateCycle { get; set; }

        /// <summary>Invoked before the Input logic is calculated. Use this to override the input coming from player input</summary>
        public System.Action<MAnimal> PreInput { get; set; }

        public System.Action<MAnimal> PreStateMovement { get; set; }

        /// <summary>Invoked after the ActiveState execute its code</summary>
        public System.Action<MAnimal> PostStateMovement { get; set; }

        /// <summary>Get all the Animator Parameters the Animal Controller has</summary>
        private List<int> animatorHashParams;
        //private Hashtable animatorParams;

        #region Static Properties
        /// <summary>List of all the animals on the scene</summary>
        public static List<MAnimal> Animals;
        /// <summary>Main Animal used as the player controlled by Input</summary>
        public static MAnimal MainAnimal;
        #endregion

        #region States
        /// <summary>NECESARY WHEN YOU ARE USING MULTIPLE ANIMALS</summary>
        public bool CloneStates = true;

        //[Tooltip("The Animal gameObject will have no Parent on the Hierarchy. (Recommended)")]
        //public bool NoParent = true;

        ///<summary>On Which State the animal should Start on Enable</summary>
        public StateID OverrideStartState;

        /// <summary> Current State. Changing this variable wil not exectute the Active State logic</summary> 
        public State activeState;
        /// <summary> Store the Last State, This will not change the Last State parameter on the animator</summary> 
        public State lastState;
        /// <summary> Store the current queue State </summary> 
        public State queueState;

        /// <summary> Check if a State was queued and if it can be released because it was activated </summary> 
        public bool QueueReleased => QueueState != null && QueueState.OnActiveQueue && !QueueState.OnQueue;

        /// <summary> Store the current queue State </summary> 
        public State QueueState { get => queueState; internal set => queueState = value; }


        /// <summary> Store the Last State </summary> 
        public State LastState
        {
            get => lastState;
            internal set
            {
                if (value == null) return;

                lastState = value;

                LastState.ExitState();                               //Exectute the Exit State code on the Last State.
                var LastStateID = (QueueState == null) ? lastState.ID.ID : QueueState.ID.ID;
                SetIntParameter(hash_LastState, LastStateID);   //Sent to the Animator the previews Active State
                //    Debug.Log("LastStateID = " + LastStateID);
            }
        }

        ///<summary> Store a State (PreState) that can be used later</summary>
        protected State Pin_State;

        /// <summary>Used to call the Last State one more time before it changes to the new state </summary>
        public bool JustActivateState { get; internal set; }
        //{
        //    get => justActivateState;
        //    set { justActivateState = value; Debug.Log("justActivateState = " + value); }
        //}
        //private bool justActivateState;
        /// <summary> ID of the Current Active State</summary>
        public StateID ActiveStateID { get; private set; }


        /// <summary>State Float Value</summary>
        public float State_Float { get; private set; }

        /// <summary> Current Active State Profile</summary>
        public int ActiveState_Profile { get; private set; }

        /// <summary>Set/Get the Active State</summary>
        public State ActiveState
        {
            get => activeState;
            internal set
            {
                //Are we Activating itself the same state?
                if (activeState == value)
                {
                    currentAnimTag = 0; //Reset the AnimaTag so it can enter again to the Animations Tag
                }

                var OldStrafe = Strafe; //Cache the old strafe

                activeState = value;

                if (value == null) return;

                JustActivateState = true;
                // if (LastState != null && LastState.ExitFrame) LastState.OnStateMove(DeltaTime);           //Play One Last Time the Last State
                this.Delay_Action(() => { JustActivateState = false; });            //Reset Just Activate State The next Frame
                ActiveStateID = activeState.ID;

                OnStateActivate.Invoke(activeState.ID);

                SetIntParameter(hash_State, activeState.ID.ID);                     //Sent to the Animator the value to Apply  


                Sprint = sprint; //Update the Sprint value with the latest input

                //Debug.Log($"<B>{name}] STATE INT {activeState.ID.ID} STATE ON </B>");
                TryAnimParameter(hash_StateOn);                                     //Enable State On

                Update_StateProfile();

                // Execute the code inside in case has changed

                //Update the Strafe Logic
                if (OldStrafe != Strafe)
                    StrafeLogic();

                if (ActiveState.OverrideCapsule)
                {
                    ActiveState.newCapsule.Modify(MainCollider);
                }
                else if (LastState.OverrideCapsule) //If the Last State Overrided the Capsule then reset it
                {
                    Reset_MainCollider();
                }


                if (HasStances)
                {
                    //Check if the Active Stance cannot be used on the New State
                    if (ActiveStance != null && !ActiveStance.CanBeUsedOnState(ActiveStateID))
                    {
                        ActiveStance.SetPersistent(false); //Force to exit if it was persistent

                        if (ActiveStance.OnQueueState(ActiveStateID))
                            ActiveStance.Queued = true;

                        Stance_Reset();
                    }
                }
                ////Old Way
                //else if (!activeState.ValidStance(currentStance))
                //    Stance_Reset();

                foreach (var st in states) st.NewStateActivated(activeState.ID); //Notify all states that a new state has been activated
                foreach (var sn in Stances) sn.NewStateActivated(activeState.ID); //Notify all stances that a new state has been activated

                Set_Sleep_FromStates(activeState);
                Check_Queue_States(activeState.ID);    //Check if a queue State was released


                if (IsPlayingMode && ActiveMode.StateCanInterrupt(ActiveStateID))//If a mode is playing check a State Change
                {
                    Mode_Interrupt();
                }
                else
                {
                    CheckCacheModeInput();
                }
            }
        }

        protected virtual void Update_StateProfile()
        {
            ActiveState_Profile = activeState.StateProfile;                //Store the Active State Profile
            TryAnimParameter(hash_StateProfile, ActiveState_Profile);      //Use trigger in case the Animal is using Triggers
            OnStateProfile.Invoke(ActiveState_Profile);
        }


        protected virtual void CheckCacheModeInput()
        {
            var HasInputCache = false;
            foreach (var MQ in ModeQueueInput)
            {
                if (MQ.Active && MQ.TryActivate())
                {
                    MQ.Debugging("<color=cyan> <B>[ModeQueueInput]</B>Try Activate Succesfull </color>");
                    HasInputCache = true;
                    break;
                }
            }

            //If there's no Input Cache then check the Ability Queue
            if (!HasInputCache)
            {
                foreach (var MQ in AbilityQueueInput)
                {
                    if (MQ.mode.TryActivate(MQ))
                    {
                        MQ.mode.Debugging("<color=cyan> <B>[AbilityQueueInput]</B>Try Activate Succesfull </color>");

                        break;
                    }
                }
            }
        }

        /// <summary> Store if a Mode tried to use its Input and Failed </summary>
        public HashSet<Mode> ModeQueueInput;
        /// <summary> Store if a Ability tried to use its Input and Failed </summary>
        public HashSet<Ability> AbilityQueueInput;

        /// <summary>When a new State is Activated, Make sure the other States are sent to sleep</summary>
        internal void Set_Sleep_FromStates(State state)
        {
            // Debug.Log("Check SleepFrom State = " + state.ID.name);

            foreach (var st in states)
            {
                if (state == st) continue;                          //Do not check yourself
                var sleep = st.SleepFromState.Contains(state.ID);   //Sent to sleep states that has some Avoid State
                sleep ^= !st.IncludeSleepState;
                st.IsSleepFromState = sleep;
            }
        }

        /// <summary>Check if there's a State that cannot be enabled when playing a mode </summary>
        internal virtual void Set_State_Sleep_FromMode(bool playingMode)
        {
            foreach (var state in states)
                state.IsSleepFromMode = playingMode && state.SleepFromMode.Contains(ActiveMode.ID);
        }


        /// <summary>Check if there's a State that cannot be enabled when playing a mode </summary>
        internal virtual void Set_State_Sleep_FromStance()
        {
            foreach (var state in states)
            {
                state.IsSleepFromStance = state.SleepFromStance.Contains(Stance);
            }
        }

        /// <summary>When a new State is Activated, Make sure the other States are sent to sleep</summary>
        internal virtual void Check_Queue_States(StateID ID)
        {
            foreach (var st in states)
            {
                st.OnQueue = st.QueueFrom.Contains(ID);        //Sent to sleep states that has some Avoid State
            }
        }

        #endregion

        #region General  
        /// <summary> Layers the Animal considers ground</summary>
        public LayerReference groundLayer = new(1);

        /// <summary> Layers the Animal considers ground</summary>
        public LayerMask GroundLayer => groundLayer.Value;

        /// <summary>Distance from the Pivots to the ground </summary>
        [Tooltip("Distance from Animal Hip to the ground. It is Recomended to use the Y value of the Hip Pivot")]
        public float height = 1f;

        /// <summary>Height from the ground to the hip multiplied for the Scale Factor</summary>
        public float Height
        {
            get => (height) * ScaleFactor;
            set => height = value;
        }
        /// <summary>The Scale Factor of the Animal.. if the animal has being scaled this is the multiplier for the raycasting things </summary>
        public float ScaleFactor => transform.localScale.y;

        /// <summary>Does this Animal have an InputSource </summary>
        public IInputSource InputSource;

        [SerializeField] private Vector3 center;
        /// <summary>Center of the Animal to be used for AI and Targeting  based on World Position</summary>
        public Vector3 Center
        {
            private set => center = value;
            get => transform.TransformPoint(center);
        }
        #endregion

        #region Stance
        [SerializeField] private StanceID currentStance;
        [SerializeField] private StanceID defaultStance;

        /// <summary>Does the Animal has the new List of Stances</summary>
        public bool HasStances { get; private set; }
        public Stance ActiveStance { get; set; }
        public Stance LastActiveStance { get; set; }

        /// <summary>Last Stance the Animal was</summary>
        public int LastStanceID { get; private set; }

        public StanceID DefaultStanceID { get => defaultStance; set => defaultStance = value; }

        private StanceID StartingStance;

        /// <summary>Current Active Stance</summary>
        public StanceID Stance
        {
            get => currentStance;
            set
            {
                if (value == currentStance) return;     //Do nothing if we are already on the same Stance
                if (value == null) return;              //Do nothing with empty IDs

                //if (!enabled) return;                   //Do nothing if is not active
                //if (Sleep) return;                      //Do nothing if is not active

                if (value == currentStance) return;     //Change only when the values are different

                SetAdvancedStance(value);
            }
        }

        private void SetAdvancedStance(StanceID value)
        {
            var new_stance = Stance_Get(value);

            if (new_stance != null)
            {
                if (new_stance.CanActivate())
                {
                    var OldStrafe = Strafe; //Check Old Strafe

                    LastActiveStance = ActiveStance;
                    ActiveStance = new_stance;
                    LastStanceID = currentStance;
                    currentStance = value;

                    ActiveStance.Activate();

                    //Deactivate last Stance Only when is different
                    if (ActiveStance != LastActiveStance) LastActiveStance.Exit();

                    OnStanceChange.Invoke(value);
                    OnStance?.Invoke(value.ID);

                    //Enable and Disable Temporarly the Stances
                    foreach (var _st in Stances)
                    {
                        if (new_stance.DisableStances.Count > 0 &&
                            new_stance.DisableStances.Contains(_st.ID)) _st.Disable_Temp();

                        if (LastActiveStance.DisableStances.Count > 0 &&
                            LastActiveStance.DisableStances.Contains(_st.ID)) _st.Disable_Temp_Restore();
                    }

                    Set_State_Sleep_FromStance();

                    if (debugStances)
                        Debug.Log($"<B>[{name}] → Set: <color=yellow>[Stance - {value.name} - {value.ID}]</color></B>", gameObject);

                    TryAnimParameter(hash_Stance, currentStance.ID);      //Set on the Animator the Current Stance
                    TryAnimParameter(hash_LastStance, LastStanceID);      //Set on the Animator the Last Stance

                    //Sent to the Animator the previews Active State  (BUG)
                    if (!JustActivateState)
                        TryAnimParameter(hash_LastState, ActiveStateID.ID);

                    TryAnimParameter(hash_StateOn);                    //Set on the Animator the Trigger Stance
                                                                       //  Debug.Log($"<B>  STATE ON  [[[[STANCE]]]]</B>");

                    //Updae the Strafe Logic
                    if (OldStrafe != Strafe)
                        StrafeLogic();

                    ActiveState.SetSpeed(); //Check if the speed modifier has changed when you have a new Stance


                    //If a mode is playing check a State Change
                    if (IsPlayingMode && ActiveMode.StanceCanInterrupt(currentStance))
                    {
                        Mode_Interrupt();
                    }
                    else
                    {
                        CheckCacheModeInput();
                    }

                    if (!Sleep) //It may cause issues with the Stances changes while riding
                    {
                        if (ActiveStance.OverrideCapsule)
                        {
                            ActiveStance.newCapsule.Modify(MainCollider);
                        }
                        else if (LastActiveStance.OverrideCapsule) //If the Last Stance Overrided the Capsule then reset it
                        {
                            Reset_MainCollider();
                        }
                    }
                }
            }
            else
            {
                if (debugStances && new_stance == null)
                {
                    Debug.Log($"<B>[{name}]</B> - <B> <color=yellow>[Stance: {value.name}]</color> - Fail to Activate. [NOT Found]</B>", gameObject);
                }
            }

        }
        #endregion

        #region Movement


        [Tooltip("Global multiplier for the Animator Speed")]
        public FloatReference AnimatorSpeed = new(1);

        [Tooltip("Local Time Multiplier for the Animal. Cool Slowmo Stuffs")]
        public FloatReference m_TimeMultiplier = new(1);

        public float TimeMultiplier { get => m_TimeMultiplier.Value; set => m_TimeMultiplier.Value = value; }

        //public FloatReference MovementDeathValue = new FloatReference(0.05f);
        [SerializeField] private BoolReference alwaysForward = new(false);

        /// <summary>Sets to Zero the Z on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the Z on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockForwardMovement = new(false);
        /// <summary>Sets to Zero the X on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the X on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockHorizontalMovement = new(false);
        /// <summary>Sets to Zero the Y on the Movement Axis when this is set to true</summary>
        [Tooltip("Sets to Zero the Y on the Movement Axis when this is set to true")]
        [SerializeField] private BoolReference lockUpDownMovement = new(false);

        ////public bool AdditiveX;
        ////public bool AdditiveY;
        //[Tooltip("The Up Down Input is interpreted as Additive (Spyro Underwater Movement Style)")]
        //public bool AdditiveUp;

        /// <summary>Multiplier to Add or Remove Forward Movement to the Animal, Used when the Animal Rotates in Place</summary>
       // public float ForwardMultiplier { get; set; }

        /// <summary>(Z), horizontal (X) and Vertical (Y) Movement Input</summary>
        public Vector3 MovementAxis; // { get; set; }
        //{
        //    get => movementAxis;
        //    set
        //    {
        //        movementAxis = value;
        //        Debug.Log($"MovementAxis: {movementAxis} ");
        //    }
        //}
        //internal Vector3 movementAxis; 

        /// <summary>(Z), horizontal (X) and Vertical (Y) Raw Movement Input</summary>
        public Vector3 MovementAxisRaw;  // { get; set; }
        //{
        //    get => movementAxisRaw;
        //    set
        //    {
        //        movementAxisRaw = value;
        //        Debug.Log($"movementAxisRaw: {movementAxisRaw} ");
        //    }
        //}
        //internal Vector3 movementAxisRaw;

        /// <summary>Current Raw Input Axis gotted from an Input Entry </summary>
        public Vector3 RawInputAxis;//{ get; set; }
        //{
        //    get => rawInputAxis;
        //    set
        //    {
        //        rawInputAxis = value;
        //        Debug.Log($"RawInputAxis: {rawInputAxis} ");
        //    }
        //}
        //internal Vector3 rawInputAxis;


        /// <summary>Current Raw Input Axis gotted from an Input Entry </summary>
        public Vector3 RawRotateDirAxis { get; set; }

        ///// <summary>Input Added on Top of the Raw Input Axis, usually comes from Mobile Input</summary>
        //public Vector3 AdditiveRawInputAxis;//{ get; set; }

        /// <summary>The Animal is using Input instead of a Direction to move</summary>
        public bool UseRawInput { get; set; }
        //{
        //    get => useRawInput;
        //    set
        //    {
        //        if (useRawInput != value)
        //        {
        //            useRawInput = value;
        //            Debug.Log($"{name}: useRawInput: {useRawInput} ");
        //        }
        //    }
        //}
        //bool useRawInput;

        /// <summary>Forward (Z), horizontal (X) and Vertical (Y) Smoothed Movement Input AFTER aplied Speeds Multipliers (THIS GOES TO THE ANIMATOR)</summary>
        public Vector3 MovementAxisSmoothed;


        /// <summary>The animal will always move forward</summary>
        public bool AlwaysForward
        {
            get => alwaysForward.Value;
            set
            {
                alwaysForward.Value = value;
                MovementAxis.z = alwaysForward.Value ? 1 : 0;
                MovementDetected = AlwaysForward;

                //Debug.Log($"{name}: AlwaysForward: {value} ");
            }
        }

        /// <summary>[Raw Direction the Character will go Using Inputs or a Move Direction</summary>
        public Vector3 Move_Direction;
        // public Vector3 MoveDirection => Move_Direction;


        private bool movementDetected;

        /// <summary>Checking if the movement input was activated</summary>
        public bool MovementDetected
        {
            get => movementDetected;
            internal set
            {
                if (movementDetected != value)
                {
                    movementDetected = value;
                    OnMovementDetected.Invoke(value);
                    SetBoolParameter?.Invoke(hash_Movement, MovementDetected);

                    //Debug.Log($"Movement Detected {value}");
                    //if (!value) Sprint = sprint;
                }
            }
        }

        /// <summary>The Animal uses the Camera Forward Diretion to Move</summary>
        public BoolReference useCameraInput = new(true);

        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public BoolReference useCameraUp = new();

        /// <summary>Store the Starting Use camera Input Value</summary>
        public bool DefaultCameraInput { get; private set; }

        /// <summary>Restore to the default camera input on the Animal</summary>
        public void ResetCameraInput() => UseCameraInput = DefaultCameraInput;

        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public bool UseCameraUp { get => useCameraUp.Value; set => useCameraUp.Value = value; }

        /// <summary>The Animal uses the Camera Forward Direction to Move</summary>
        public bool UseCameraInput
        {
            get => useCameraInput.Value;
            set
            {
                useCameraInput.Value = UsingMoveWithDirection = value;
            }
        }

        public bool DefaulCameraInput { get; set; }

        /// <summary> Is the animal using a Direction Vector for moving(True) or a World Axis Input (False)</summary>
        public bool UsingMoveWithDirection //  { set; get; }
        {
            get => usingMoveWithDirection;
            set
            {
                if (usingMoveWithDirection != value)
                {
                    usingMoveWithDirection = value;
                    //  Debug.Log($"{name}: UsingMoveWithDirection = " + value);
                }
            }
        }
        private bool usingMoveWithDirection;

        /// <summary> Is the animal using a Direction Vector for rotate in place(true?)</summary>
        public bool Rotate_at_Direction { set; get; }

        /// <summary>Main Camera on the Game</summary>
        public TransformReference m_MainCamera = new();

        /// <summary> Main Forward Direction to calculate relative Movement  </summary>
        public Transform MainCamera => m_MainCamera.Value;




        [SerializeField] private bool additivePosLog;
        [SerializeField] private bool additiveRotLog;
        private void DebLogAdditivePos()
        {
#if UNITY_EDITOR
            additivePosLog ^= true;
            MTools.SetDirty(this);
            Debug.Log($"Additive Pos Log : {additivePosLog} ", this);
#endif
        }

        private void DebLogAdditiveRot() => additiveRotLog ^= true;

        [ContextMenuItem("Debug AdditivePos", nameof(DebLogAdditivePos))]
        //  [ContextMenuItem("Debug AdditiveRot", nameof(DebLogAdditiveRot))]
        /// <summary>Is this animal is the main Player?</summary>
        public BoolReference isPlayer = new(true);



        /// <summary>Inertia Speed to smoothly change the Speed Modifiers </summary>
        public Vector3 InertiaPositionSpeed// { get; internal set; }
        {
            get => InertiaPPS;
            set
            {
                InertiaPPS = value;

                //if (additivePosLog)
                //    Debug.Log($"InertiaPositionSpeed:  {(InertiaPPS.magnitude):F3} ");
            }
        }
        Vector3 InertiaPPS;


        /// <summary> Additive Position Modifications for the  animal (Terrian Snapping, Speed Modifiers Positions, etc)</summary>
        public Vector3 AdditivePosition//   ; 
        {
            get => additivePosition;
            set
            {
                additivePosition = value;
#if UNITY_EDITOR  

                if (additivePosLog)
                    Debug.Log($"Additive Pos:  {(additivePosition / DeltaTime)}", this);
#endif
            }
        }
        internal Vector3 additivePosition;


        /// <summary> Additive Rotation Modifications for the  animal (Terrian Aligment, Speed Modifiers Rotations, etc)</summary>
        public Quaternion AdditiveRotation//;
        {
            get => additiveRotation;
            set
            {
                additiveRotation = value;
                // if (additiveRotLog) 
                // Debug.Log($"Additive ROT:  {(additiveRotation):F3} ");
            }
        }
        Quaternion additiveRotation;

        /// <summary>Animal Transform.position</summary>
        public Vector3 Position
        {
            get => t.position;
            set
            {
                var lastValue = t.position;

                t.position = value;

#if UNITY_EDITOR
                if (additivePosLog)
                {
                    Debug.Log($"Current Pos Changes: {(value - lastValue).Round(3)}");
                    //Debug.Log($"Current Pos: {value.Round(3)} Last Pos:{LastPosition.Round(3)}");
                }
#endif
            }
        }

        /// <summary>World Position on the last Frame</summary>
        public Vector3 LastPosition { get; internal set; }
        //{
        //    set
        //    {
        //        m_LastPosition = value;
        //        //  Debug.Log($"LP: {m_LastPosition} ");
        //    }
        //    get => m_LastPosition;
        //}
        //Vector3 m_LastPosition;


        /// <summary>Animal Transform.rotation</summary>
        public Quaternion Rotation
        {
            get => t.rotation;
            set => t.rotation = value;
        }



        [SerializeField] private BoolReference SmoothVertical = new(true);

        [Tooltip("Global turn multiplier to increase rotation on the animal")]
        public FloatReference TurnMultiplier = new(0f);

        [Tooltip("Smooth Damp Value to Turn in place, when using LookAt Direction Instead of Move()")]
        public FloatReference inPlaceDamp = new(2f);


        /// <summary>Global Additive multiplier. I'm Using this to remove Rotation from Animations  and leaving only RootMotion stuff</summary>
        public float AdditiveRotationMultiplier { get; set; } = 1f;


        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        public Vector3 DeltaPos { get; internal set; }
        //{
        //    set
        //    {
        //        m_DeltaPos = value;
        //        Debug.Log($"DeltaPos POS: {m_DeltaPos}  {(m_DeltaPos / DeltaTime):F3} ");
        //    }
        //    get => m_DeltaPos;
        //}
        //Vector3 m_DeltaPos;




        /// <summary>Velocity acumulated from the last Frame</summary>
        public Vector3 Inertia => DeltaPos / DeltaTime;

        /// <summary>UpIntertia from the Fall and Jump States</summary>
        public Vector3 UpInertia { get; internal set; }


        /// <summary>Difference between the Current Rotation and the desire Input Rotation </summary>
        public float DeltaAngle { get; internal set; }
        //{
        //    set
        //    {
        //        deltaAngle = value;
        //        Debug.Log($"DeltaAngle: {deltaAngle:F2} ... {RawInputAxis}");
        //    }
        //    get => deltaAngle;
        //}
        //float deltaAngle;


        /// <summary>Pitch direction used when Free Movement is Enable (Direction of the Move Input) </summary>
        public Vector3 PitchDirection { get; internal set; }
        /// <summary>Pitch Angle </summary>
        public float PitchAngle { get; internal set; }
        /// <summary>Bank</summary>
        public float Bank { get; internal set; }

        /// <summary>Speed from the Vertical input multiplied by the speeds inputs(Walk Trot Run) this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float VerticalSmooth { get => MovementAxisSmoothed.z; internal set => MovementAxisSmoothed.z = value; }

        /// <summary>Direction from the Horizontal input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float HorizontalSmooth { get => MovementAxisSmoothed.x; internal set => MovementAxisSmoothed.x = value; }

        /// <summary>Direction from the Up Down input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float UpDownSmooth
        {
            get => MovementAxisSmoothed.y;
            internal set
            {
                MovementAxisSmoothed.y = value;
                // Debug.Log("UD" + value);
            }
        }


        /// <summary> Vertical (Y) Difference between Target and Current UpDown</summary>
        public float DeltaUpDown { get; internal set; }


        /// <summary> If true it will keep the Controller smooth push of the movement stick</summary>
        public bool UseSmoothVertical { get => SmoothVertical.Value; set => SmoothVertical.Value = value; }


        /// <summary> The current value of the Delta time the animal is using (Fixed or not)</summary>
        public float DeltaTime { get; private set; }

        #endregion

        #region Alignment Ground
        /// <summary>Smoothness value to Snap to ground </summary>
        public FloatReference AlignPosLerp = new(15f);
        /// <summary>Smoothness Position value when Entering from Non Grounded States</summary>
        public FloatReference AlignPosDelta = new(2.5f);
        /// <summary>Smoothness Rotation value when Entering from Non Grounded States</summary>
        public FloatReference AlignRotDelta = new(2.5f);

        /// <summary>Smoothness Position value when Entering from Non Grounded States </summary>
        public float AlignPosLerpDelta { get; internal set; }

        /// <summary>Smoothness Rotation value when Entering from non Grounded States </summary>
        public float AlignRotLerpDelta { get; internal set; }


        /// <summary>Smoothness value to Snap to ground  </summary>
        public FloatReference AlignRotLerp = new(15f);

        [Tooltip("When the Animal is grounded the Controller will check every X frame for the Ground... Higher values: better performance -> less acurancy")]
        public IntReference AlignCycle = new(1);

        [Tooltip("Tag your small rocks, debris,steps and stair objects  with this Tag. It will help the animal to recognize better the Terrain")]
        public StringReference DebrisTag = new("Stair");

        ///// <summary>Maximun angle on the terrain the animal can walk </summary>
        //[Range(1f, 90f), Tooltip("Maximun angle on the terrain the animal can walk. If the Terrain Angle is greater than this the animal will stop")]
        //public float maxAngleSlope = 45f;
        //[Tooltip("Additional Angle to calculate Down Slope Limits (MaxAngleSlope-DeepSlope), If the Terrain Angle is lower than this the animal will fall")]
        //public float m_deepSlope = 5f;

        #region Slope


        [Tooltip("Maximun and Minimun Angle on the terrain the animal can walk. If the Terrain Angle is higher than the Max value: the animal will stop moving, if is lower than the Min Value: the animal will fall")]
        [MinMaxRange(-90, 90)]
        public RangedFloat TerrainSlopeLimit = new(-50, 45);

        [Range(10, 90)]
        [Tooltip("Maximun and Minimun Angle on the terrain the animal can walk. If the Terrain Angle is higher than the Max value: the animal will slideDown")]
        public float SlopeLimit = 50;

        [Tooltip("Angle on the terrain to start Sliding Down")]
        [Min(0f)]
        public float slideThreshold = 10;
        [Tooltip("When the Animal gets to a Slide Because the Slope, This is the amount of pushing down")]
        public float slideAmount = 0.5f;
        [Tooltip("Damp Value to activate the sliding effect, Lower Value Faster to achieve the sliding")]
        public float slideDamp = 15f;


        /// <summary>Main Pivot Slope Angle</summary>
        public float MainPivotSlope { get; private set; }

        /// <summary>Direction Vector of the Terrain</summary>
        public Vector3 SlopeDirection { get; private set; }

        /// <summary>Slope Normal from the ground</summary>
        public Vector3 SlopeNormal { get; internal set; }

        /// <summary>Calculate slope Angle and normalize it with the Max Angle Slope</summary>
        public float SlopeNormalized => TerrainSlope / SlopeLimit;

        /// <summary>Angle value from the Vector Up to the ground</summary>
        public float SlopeDirectionAngle { get; internal set; }


        /// <summary>Angle value When using aGround Changer Component</summary>
        public float SlopeAngleDifference { get; internal set; }


        /// <summary>Smooth Lerp Value of Direction Vector of the Terrain</summary>
        public Vector3 SlopeDirectionSmooth { get; set; }

        /// <summary>Calculation of the Average Surface Normal</summary>
        public Vector3 SurfaceNormal { get; internal set; }


        //public float SlopeNormalized => TerrainSlope / InclineMaxSlope;

        /// <summary>Slope Calculate from the Surface Normal. Positive = Higher Slope, Negative = Lower Slope </summary>
        public float TerrainSlope { get; private set; }
        //{
        //    get => terrainSlope;
        //    internal set
        //    {
        //        terrainSlope = value;
        //        Debug.Log("terrainSlope = " + terrainSlope);
        //    }
        //}
        //private float terrainSlope;


        /// <summary>Check if can Fall on slope while on the ground "Decline Slope"</summary>
        public bool DeepSlope => SlopeDirectionAngle > SlopeLimit;

        #endregion

        /// <summary>Used to add extra Rotations to the Animal</summary>
        public Transform Rotator;
        public Transform RootBone;
        /// <summary>Offset Bone in between the Rotator and the RootBone</summary>
        private GameObject RotatorOffset;


        /// <summary>Speed of the Animal used on the Rigid Body On the Horizontal Plane</summary>
        public float HorizontalSpeed { get; internal set; }

        /// <summary>Velocity of the Animal used on the RIgid Body (Useful for Speed Modifiers)</summary>
        public Vector3 HorizontalVelocity { get; internal set; }




        [SerializeField] private BoolReference grounded = new(false);
        /// <summary> Is the Animal on a surface, when True the Raycasting for the Ground is Applied</summary>
        public bool Grounded
        {
            get => grounded.Value;
            set
            {
                if (grounded.Value != value)
                {
                    grounded.Value = value;

                    if (!value)
                    {
                        SetPlatform(null); //If groundes is false remove the stored Platform 
                        SlopeNormal = UpVector; //Reset the Slope Normal when the animal is not grounded
                    }
                    else
                    {
                        Gravity_ResetValues();
                        UpInertia_Clear();
                        GravityExtraPower = 1;
                        Force_Reset();
                        UpDownAdditive = 0; //Reset UpDown Additive 
                        UsingUpDownExternal = false; //Reset UpDown Additive 
                        GravityMultiplier = 1;
                        ExternalForceAirControl = true; //Reset the External Force Air Control
                        UseGravity = false;
                    }

                    SetBoolParameter(hash_Grounded, grounded.Value);
                    OnGrounded.Invoke(value);
                }
            }
        }
        #endregion

        #region External Force

        /// <summary>Add an External Force to the Animal</summary>
        public Vector3 ExternalForce { get; set; }




        /// <summary>Current External Force the animal current has</summary>
        public Vector3 CurrentExternalForce { get; set; }
        //  public bool LocalForce { get; set; }
        //{
        //    set
        //    {
        //        m_CurrentExternalForce = value;
        //        Debug.Log($"CurrentExternalForce:  {m_CurrentExternalForce} ");
        //    }
        //    get => m_CurrentExternalForce;
        //}
        //Vector3 m_CurrentExternalForce;

        /// <summary>External Force Aceleration /summary>
        public float ExternalForceAcel { get; set; }

        /// <summary>External Force Air Control, Can it be controlled while on the air?? </summary>
        public bool ExternalForceAirControl { get; set; }

        public bool HasExternalForce => ExternalForce != Vector3.zero;
        #endregion

        #region References
        /// <summary>Returns the Animator Component of the Animal </summary>

        [RequiredField] public Animator Anim;
        [RequiredField] public Rigidbody RB;                   //Reference for the RigidBody

        private float rb_angularDrag;
        private float rb_Drag;

        /// <summary>Transform.UP (Stored)</summary>
        public Vector3 Up => t.up;
        /// <summary>Transform.Right (Stored)</summary>
        public Vector3 Right => t.right;
        /// <summary>Transform.Forward (Stored) </summary>
        public Vector3 Forward => t.forward;


        #endregion

        #region Modes
        /// <summary>Allows the Animal Start Playing a Mode</summary>
        public IntReference StartWithMode = new(0);

        /// <summary>Status value for the Mode (-1: Loop, -2: Interrupted, 0: Available</summary>
        public int ModeStatus { get; private set; }

        /// <summary>Float value for the Mode</summary>
        public float ModePower { get; set; }

        // private int modeStatus;
        internal Mode activeMode;


        /// <summary>Is Playing a mode on the Animator</summary>
        public bool IsPlayingMode => activeMode != null;

        /// <summary>A mode is Set, but the Animation has not started yet</summary>
        public bool IsPreparingMode //{ get; set; }
        {
            get => m_IsPreparingMode;
            internal set
            {
                m_IsPreparingMode = value;
                // Debug.Log($"[{name}] - <color=orange><b>[☼☼☼☼☼☼ ☼☼☼  IsPreparingMode::{value}]</b></color>");
            }
        }
        bool m_IsPreparingMode;

        /// <summary>Current Moment in time the Mode was activated</summary>
        public double ModeActivationTime { get; set; }

        /// <summary>Store if the Animal is on a Zone</summary>
        public bool InZone => Zone != null;


        private IZone zone;

        /// <summary>Store the Zone the animal is</summary>
        public IZone Zone
        {
            get { return zone; }

            set
            {
                if (zone != value)
                {
                    foreach (var state in states)
                    {
                        if (state != null) //The animal is entering a zone
                            state.OnAnimalEnterZone(value);
                        else
                            state.OnAnimalExitZone(zone);
                    }

                    zone = value;
                }
            }
        }

        ///// <summary>ID Value for the Last Mode Played </summary>
        //public int LastModeID { get; set; }

        /// <summary>ID Value for the Last Ablity Played </summary>
        public int LastAbilityIndex { get; set; }

        /// <summary>Store if the Mode allows using Gravity, Any playing ablity will update this value</summary>
        public bool Mode_IgnoreGravity { get; protected set; }

        /// <summary>If the Ability is Set to Persistent there will be no State Changes. Any playing ablity will update this value</summary>
        public bool Mode_PersistentState { get; protected set; }

        /// <summary>Store if the Mode allows using Grounded Align Logic. Any playing ablity will update this value </summary>
        public bool Mode_IgnoreGrounded { get; protected set; }

        /// <summary>Store if the Mode Current Multiplier Position. Default 1 when is not playing a Mode</summary>
        public float Mode_Multiplier_Pos { get; protected set; } = 1;

        /// <summary>Store if the Mode Current Multiplier Rotation. Default 1 when is not playing a Mode</summary>
        public float Mode_Multiplier_Rot { get; protected set; } = 1;

        /// <summary>Store if the Mode Current Additive Position. Default 1 when is not playing a Mode</summary>
        public float Mode_Additive_Pos { get; protected set; }

        /// <summary>Store if the Mode Current Additive Rotation. Default 1 when is not playing a Mode</summary>
        public float Mode_Additive_Rot { get; protected set; }

        public Mode LastMode { get; protected set; }



        /// <summary>Set/Get the Active Mode, Prepare the values for the Animator... Does not mean the Mode is Playing</summary>
        public Mode ActiveMode
        {
            get => activeMode;
            internal set
            {
                LastMode = activeMode;

                activeMode = value;
                ModeTime = 0; //Reset the Time of the Mode Activation

                if (activeMode != null)
                {
                    ActiveModeID = activeMode.ID;
                    OnModeStart.Invoke(ActiveModeID, activeMode.ActiveAbility.Index);
                    ModeStart?.Invoke(activeMode.ID, activeMode.ActiveAbility.Index);

                    ActiveState.OnModeStart(activeMode);

                    Mode_IgnoreGravity = value.ActiveAbility.IgnoreGravity;
                    Mode_IgnoreGrounded = value.ActiveAbility.IgnoreGrounded;
                    Mode_PersistentState = value.ActiveAbility.Persistent;

                    Mode_Additive_Pos = value.ActiveAbility.AdditivePosition;
                    Mode_Additive_Rot = value.ActiveAbility.AdditiveRotation;
                    Mode_Multiplier_Pos = value.ActiveAbility.MultiplierPosition;
                    Mode_Multiplier_Rot = value.ActiveAbility.MultiplierRotation;

                }
                else
                {
                    ActiveModeID = 0;

                    //Remember to reset the trigger on the Mode ON. Just in case
                    ResetModeOn();

                    //Reset Ignore Values
                    Mode_IgnoreGravity = false;
                    Mode_IgnoreGrounded = false;
                    Mode_PersistentState = false;

                    Mode_Multiplier_Rot = 1;
                    Mode_Multiplier_Pos = 1;
                    Mode_Additive_Pos = 0;
                    Mode_Additive_Rot = 0;


                    //if (InZone && Zone.IsMode)
                    //    Zone.RemoveAnimal(this);
                }

                if (LastMode != null)
                {
                    var LastAbilityIndex = LastMode.AbilityIndex;
                    OnModeEnd.Invoke(LastMode.ID, LastAbilityIndex);
                    ModeEnd?.Invoke(LastMode.ID, LastAbilityIndex);
                    ActiveState.OnModeEnd(LastMode);
                    // Stance = Stance; //Updates the Stance Code ??
                }
            }
        }

        /// <summary>Set the Values to the Animator to Enable a mode... Does not mean that the mode is enabled</summary>
        internal virtual void SetModeParameters(Mode value, int status)
        {
            if (value != null)
            {
                var ability = (value.ActiveAbility != null ? (int)value.ActiveAbility.Index : 0);

                int mode = Mathf.Abs(value.ID * 1000) + Mathf.Abs(ability);      //Convert it into a 4 int value Ex: Attack 1001

                //If the Mode is negative or the Ability is negative then Set the Animator Parameter negative too. (Right Left Abilities)
                ModeAbility = (value.ID < 0 || ability < 0) ? -mode : mode;

                TryAnimParameter(hash_ModeOn); //Activate the Optional Trigger

                // Debug.Log($"<color=orange><b>****SetModeParameters:  MODE ON IsPlayingMode [{IsPlayingMode}] </b></color>");
                SetModeStatus(status);

                IsPreparingMode = true;
                ModeActivationTime = Time.time;
                ModeTime = 0;
            }
            else
            {
                SetModeStatus(Int_ID.Available);
                ModeAbility = 0;
            }
        }

        /// <summary>Current Mode ID and Ability Append Together (ModeID*100+AbilityIndex)</summary>
        public int ModeAbility
        {
            get => m_ModeIDAbility;
            set
            {
                //if (m_ModeIDAbility != value)
                {
                    m_ModeIDAbility = value;
                    //if (debugModes)
                    //Debug.Log($"◘○◘○******[{name}] → <color=orange><b>Mode: [{m_ModeIDAbility}]</b></color>");
                    SetIntParameter?.Invoke(hash_Mode, m_ModeIDAbility);
                }
            }
        }
        private int m_ModeIDAbility;
        /// <summary> Set the Parameter Int ID to a value and pass it also to the Animator </summary>
        public void SetModeStatus(int value)
        {
            //if (debugModes)
            //  Debug.Log($"[{name}] → <color=orange><b>Mode Status: [{value}]</b></color>");

            SetIntParameter?.Invoke(hash_ModeStatus, ModeStatus = value);
        }

        /// <summary>Current Animation Time of the Mode,used in combos</summary>
        public float ModeTime { get; internal set; }

        /// <summary>Active Mode ID</summary>
        public int ActiveModeID { get; private set; }

        public Mode Pin_Mode { get; private set; }

        #endregion

        #region Sleep
        [SerializeField] private BoolReference sleep = new BoolReference(false);

        /// <summary>Put the Controller to sleep, is like disalbling the script but internally</summary>
        public bool Sleep
        {
            get => sleep.Value;
            set
            {
                var oldSleep = Sleep;
                sleep.Value = value;

                //Means is out of sleep
                if (!value && oldSleep)
                {
                    //Set All Float values to their defaut (For all the Float Values on the Controller  while is not riding)
                    MTools.ResetFloatParameters(Anim);
                    ResetController();
                }


                //  Debug.Log("Sleep" + Sleep);

                LockInput = LockMovement = value;                       //Also Set to sleep the Movement and Input

                if (Sleep)
                {
                    Reset_Movement();
                    TryAnimParameter(hash_Random, 0);    //Set Random to 0

                    if (Rotator) Rotator.localRotation = Quaternion.identity;
                    Bank = 0;
                    PitchAngle = 0;
                    PitchDirection = Vector3.forward;
                }
            }
        }

        private bool inTimeline;

        [Tooltip("Set the Animal to Kinematic when is in a Timeline")]
        public bool kinematicTimeline = true;
        public bool InTimeline
        {
            get => inTimeline;
            set
            {
                if (value && InTimeline) //Check if we are exiting a Timeline
                {
                    TryActivateState();
                    Gravity_ResetValues();

                    if (RB) RB.isKinematic = defaultKinematic; //Make sure is not set to kinematic

                    UpdateInputSource(true); //Connect all the inputs
                }

                inTimeline = value;

                if (inTimeline && kinematicTimeline) //if we are entering a Timeline
                {
                    if (RB) RB.isKinematic = true; //Set it to kinematic
                    UpdateInputSource(false); //Disconnect all the inputs
                }
            }
        }

        #endregion

        #region Strafe
        public BoolEvent OnStrafe = new();

        [SerializeField] private BoolReference m_strafe = new(false);
        [SerializeField] private BoolReference m_CanStrafe = new(false);
        [SerializeField] private BoolReference m_StrafeNormalize = new(false);
        [SerializeField] private FloatReference m_StrafeLerp = new(5f);


        public bool StrafeNormalize => m_StrafeNormalize.Value;

        /// <summary> Is the Animal on the Strafe Mode</summary>
        public bool Strafe
        {
            get => m_CanStrafe.Value && m_strafe.Value && ActiveStance.CanStrafe && ActiveState.CanStrafe;
            set
            {
                if (value != m_strafe.Value)
                {
                    m_strafe.Value = value;
                    StrafeLogic();
                }
            }
        }

        private void StrafeLogic()
        {
            if (sleep) return;

            if (debugStates) Debuging($"Strafe: <B>[{Strafe}]</B>", "yellow");

            OnStrafe.Invoke(Strafe);
            TryAnimParameter(hash_Strafe, Strafe);

            // Check again that the But only if the State has Strafe Animations!!!
            // Important so the State On is only called when is true
            if (ActiveState.CanStrafe && ActiveState.StrafeAnimations)
            {
                TryAnimParameter(hash_StateOn);
                //Debug.Log($"<B>  STATE ON  [[[STRAFE[[ </B>");
            }


            if (!JustActivateState)
                SetIntParameter?.Invoke(hash_LastState, ActiveStateID);   //Sent to the Animator the previews Active State  (BUG)


            if (!Strafe) //false
            {
                ResetCameraInput();
            }
            else
            {
                Aimer?.SetEnable(true); //Enable the Aimer just in case
            }

        }

        public bool CanStrafe { get => m_CanStrafe.Value; set => m_CanStrafe.Value = value; }

        public float StrafeDeltaValue { get; internal set; }
        //private float HorizontalAimAngle_Raw;

        public Aim Aimer;


        #endregion

        #region Pivots

        internal RaycastHit hit_Hip;            //Hip and Chest Ray Cast Information
        internal RaycastHit hit_Chest;            //Hip and Chest Ray Cast Information

        public List<MPivots> pivots = new();

        [HideInInspector] public MPivots Pivot_Hip;
        [HideInInspector] public MPivots Pivot_Chest;

        public int AlignUniqueID { get; private set; }

        /// <summary>Does it have a Hip Pivot?</summary>
        public bool Has_Pivot_Hip;

        /// <summary>Does it have a Hip Pivot?</summary>
        public bool Has_Pivot_Chest;

        /// <summary> Do the Main (Hip Ray) found ground </summary>
        public bool MainRay { get; private set; }
        /// <summary> Do the Fron (Chest Ray) found ground </summary>
        public bool FrontRay { get; private set; }

        /// <summary>Main pivot Point is the Pivot Chest Position, if not the Pivot Hip Position one</summary>
        public Vector3 Main_Pivot_Point
        {
            get
            {
                Vector3 pivotPoint;
                if (Has_Pivot_Chest)
                {
                    pivotPoint = Pivot_Chest.World(t);
                }
                else if (Has_Pivot_Hip)
                {
                    pivotPoint = Pivot_Hip.World(t);
                }
                else
                {
                    pivotPoint = t.TransformPoint(new Vector3(0, Height, 0));
                }

                //  return pivotPoint + DeltaPos;
                return pivotPoint  // + DeltaVelocity
                    ;
                //  return pivotPoint; 
            }
        }



        /// <summary> Does the Animal Had a Pivot Chest at the beggining?</summary>
        private bool Starting_PivotChest;

        /// <summary> Disable Temporally the Pivot Chest in case the animal is on 2 legs </summary>
        public void DisablePivotChest() => Has_Pivot_Chest = false;

        /// <summary> Used for when the Animal is on 2 feet instead of 4</summary>
        public void ResetPivotChest() => Has_Pivot_Chest = Starting_PivotChest;
        public void UsePivotChest(bool value) => Has_Pivot_Chest = value;


        /// <summary>Check if there's no Pivot Active </summary>
        public bool NoPivot => !Has_Pivot_Chest && !Has_Pivot_Hip;

        /// <summary> Gets the the Main Pivot Multiplier * Scale factor (Main Pivot is the Chest, if not then theHip Pivot) </summary>
        public float Pivot_Multiplier { get; private set; }
        //{
        //    get
        //    {
        //        float multiplier = Has_Pivot_Chest ? Pivot_Chest.multiplier : (Has_Pivot_Hip ? Pivot_Hip.multiplier : 1f);
        //        return multiplier * ScaleFactor * (NoPivot ? 1.5f : 1f);
        //    }
        //}
        #endregion

        #region RigidBody

        /// <summary>What is the Rigid Body velocity the animal should have...</summary>
        public Vector3 DesiredRBVelocity { get; internal set; }

        /// <summary> Delta Animal Velocity  </summary>
        public Vector3 DeltaVelocity { get; internal set; }
        #endregion

        #region Speed Modifiers  


        /// <summary>True if the Current Speed is Locked</summary>
        public bool CurrentSpeedSetIsLocked => CurrentSpeedSet.LockSpeed;

        /// <summary>Speed Set for Stances</summary>
        public List<MSpeedSet> speedSets;
        /// <summary>Active Speed Set</summary>
        private MSpeedSet currentSpeedSet = new();
        internal MSpeedSet defaultSpeedSet = new()
        { name = "Default Set", Speeds = new(1) { new("Default", 1, 4, 4) } }; //Create a Default Speed at Awake

        /// <summary>True if the State is modifing the current Speed Modifier</summary>
        public bool CustomSpeed;

        public MSpeed currentSpeedModifier = MSpeed.Default;
        public MSpeed SprintSpeed = MSpeed.Default;
        //public List<MSpeed> speedModifiers = new List<MSpeed>();

        protected int speedIndex;

        /// <summary>What is the Speed modifier the Animal is current using (Walk? trot? Run?)</summary>
        public MSpeed CurrentSpeedModifier
        {
            get
            {
                if (CurrentSpeedSetIsLocked) return CurrentSpeedSet.LockedSpeedModifier; //Return the Locked
                if (Sprint && !CustomSpeed) return SprintSpeed;
                return currentSpeedModifier;
            }
            internal set
            {
                //Debug.Log("******value = " + value.name);

                // if (currentSpeedModifier.name != value.name)
                {
                    currentSpeedModifier = value;
                    OnSpeedChange.Invoke(CurrentSpeedModifier);
                    EnterSpeedEvent(CurrentSpeedIndex);
                    ActiveState?.SpeedModifierChanged(CurrentSpeedModifier, CurrentSpeedIndex);

                    TryAnimParameter(hash_VerticalRaw, CurrentSpeedModifier.Vertical); //Send to the animator the Vertical Speed Value

                    //  Debug.Log("******CurrentSpeedModifier = " + currentSpeedModifier.name);
                }
            }
        }


        /// <summary>Current Speed Index used of the Current Speed Set E.G. (1 for Walk, 2 for trot)</summary>
        public int CurrentSpeedIndex
        {
            get
            {
                if (CurrentSpeedSetIsLocked) return CurrentSpeedSet.LockIndex; //Return the Locked Index
                if (Sprint && !CustomSpeed) return CurrentSpeedSet.SprintIndex; //Return the Sprint Index
                return speedIndex; //Return default index
            }


            //get => CurrentSpeedSet.LockSpeed ? CurrentSpeedSet.LockIndex : speedIndex; //Return the LockSpeed Index in case the speed is locked

            internal set
            {
                //if (debugStates) Debug.Log($"SpeedChanges: {value}", this);

                if (CustomSpeed || CurrentSpeedSet == null) return;

                var speedModifiers = CurrentSpeedSet.Speeds;

                var newValue = Mathf.Clamp(value, 1, speedModifiers.Count); //Clamp the Speed Index
                if (newValue > CurrentSpeedSet.TopIndex) newValue = CurrentSpeedSet.TopIndex;

                newValue = Mathf.Clamp(value, 1, newValue); // TOP INDEX CANNOT BE SET OT ZERO



                //   if (speedIndex != newValue)
                {
                    speedIndex = newValue;

                    var sprintSpeed = Mathf.Clamp(CurrentSpeedSet.SprintIndex, 1, speedModifiers.Count);

                    CurrentSpeedModifier = speedModifiers[speedIndex - 1];

                    SprintSpeed = speedModifiers[sprintSpeed - 1];

                    if (CurrentSpeedSet != null)
                        CurrentSpeedSet.CurrentIndex = speedIndex; //Keep the Speed saved on the state too in case the active speed was changed
                }
            }
        }

        /// <summary>Current Speed Set used on the Animal</summary>
        public MSpeedSet CurrentSpeedSet
        {
            get => currentSpeedSet;
            internal set
            {
                if (value.name != currentSpeedSet.name) //Calculate this only when the Set changes
                {
                    //Debug.Log("SpeedSet = " + currentSpeedSet.name);
                    //Debug.Log("currentSpeedSet = " + currentSpeedSet.CurrentIndex);


                    //Try to keep the Position Speed from the last state
                    SetTargetSpeed(); //Important needs to calculate the Target Speed again
                                      //InertiaPositionSpeed = TargetSpeed; //Set the Target speed to the Fall Speed so there's no Lerping when the speed changes


                    currentSpeedSet = value;
                    speedIndex = -1; //Reset the speed Index
                    JustChangedSpeedSet = true;
                    CurrentSpeedIndex = currentSpeedSet.CurrentIndex;
                    JustChangedSpeedSet = false;

                    EnterSpeedEvent(CurrentSpeedIndex);

                }
            }
        }

        bool JustChangedSpeedSet;

        private void EnterSpeedEvent(int index)
        {
            if (JustChangedSpeedSet) return;

            if (OnEnterExitSpeeds != null)
            {
                var SpeedEnterEvent = OnEnterExitSpeeds.Find(s => s.SpeedIndex == index && s.SpeedSet == CurrentSpeedSet.name);

                if (OldEnterExitSpeed != null && SpeedEnterEvent != OldEnterExitSpeed)
                {
                    OldEnterExitSpeed.OnExit.Invoke();
                    OldEnterExitSpeed = null;
                }


                if (SpeedEnterEvent != null)
                {
                    SpeedEnterEvent.OnEnter.Invoke();
                    OldEnterExitSpeed = SpeedEnterEvent;
                }
            }
        }

        private OnEnterExitSpeed OldEnterExitSpeed;

        /// <summary> Use Default SpeedSet 0 everything </summary>
        public void ResetSpeedSet() => CurrentSpeedSet = defaultSpeedSet;


        /// <summary> Value for the State Speed Global Multiplier Parameter on the Animator (Used on the States Speed Parameter)</summary>
        internal float SpeedMultiplier { get; set; }

        internal bool sprint;
        internal bool realSprint;

        /// <summary>Sprint Input</summary>
        public bool Sprint
        {
            get =>
                UseSprintState && sprint && UseSprint && !CurrentSpeedSetIsLocked
                && MovementDetected
                ;
            set
            {
                //Check if the animal has movement
                var newRealSprint = UseSprintState && value && UseSprint && !CurrentSpeedSetIsLocked
                     && MovementDetected
                    ; //Check if the animal has movement

                //Debug.Log($"UseSprintState {UseSprintState} && value{value} && " +
                //    $"UseSprint{UseSprint} && MovementDetected{MovementDetected} && !SpeedChangeLocked {SpeedChangeLocked}");

                sprint = value; //Store only the Input value for the sprint I think it works

                if (realSprint != newRealSprint)
                {
                    realSprint = newRealSprint;

                    OnSprintEnabled.Invoke(realSprint);
                    TryAnimParameter(hash_Sprint, realSprint);        //Set on the Animator Sprint Value

                    int currentPI = CurrentSpeedIndex;
                    var speed = CurrentSpeedModifier;

                    if (realSprint)
                    {
                        speed = SprintSpeed;
                        currentPI = CurrentSpeedSet.SprintIndex;
                    }

                    OnSpeedChange.Invoke(SprintSpeed);       //Invoke the Speed again
                    EnterSpeedEvent(currentPI);
                    ActiveState?.SpeedModifierChanged(speed, currentPI);
                    TryAnimParameter(hash_VerticalRaw, speed.Vertical); //Send to the animator the Vertical Speed Value
                }
            }
        }

        public void SetSprint(bool value) => Sprint = value;

        /// <summary> Try State current Cycle. This is used to skip frames on the states if the Try Cycle is greater than 1 </summary>
        internal int CurrentCycle { get; private set; }

        #endregion 

        #region Gravity
        [SerializeField] private Vector3Reference m_gravityDir = new(Vector3.down);

        [SerializeField] private FloatReference m_gravityPower = new(9.8f);
        private float defaultGravityPower;

        [SerializeField] private IntReference m_gravityTime = new(10);
        [Tooltip("Clamp Gravity Speed. Zero will ignore this")]
        [SerializeField] private FloatReference m_clampGravitySpeed = new(20f);


        public int StartGravityTime { get => m_gravityTime.Value; internal set => m_gravityTime.Value = value; }
        public float ClampGravitySpeed { get => m_clampGravitySpeed.Value; internal set => m_clampGravitySpeed.Value = value; }

        /// <summary>Multiplier Added to the  Gravity Direction</summary>
        public float GravityMultiplier { get; internal set; }

        public float GravityTime { get; internal set; }
        //{
        //    get => m_GravityTime;
        //    set
        //    {
        //        m_GravityTime = value;
        //        Debug.Log("m_GravityTime    " + m_GravityTime);
        //    }
        //}
        //int m_GravityTime;

        public float GravityPower { get => m_gravityPower.Value * (GravityMultiplier * ActiveState.GravityMultiplier); set => m_gravityPower.Value = value; }


        /// <summary>Stored Gravity Velocity when the animal is using Gravity</summary>
        public Vector3 GravityStoredVelocity { get; internal set; }

        /// <summary>Value of Gravity Offset acumulation. (From Fake Gravity stuff) E.g. Jump</summary>
        public Vector3 GravityOffset { get; internal set; }

        /// <summary>Gravity ExtraPower (From Fake Gravity stuff) E.g. Jump</summary>
        public float GravityExtraPower { get; internal set; }

        /// <summary> Direction of the Gravity </summary>
        public Vector3 Gravity { get => m_gravityDir.Value; set => m_gravityDir.Value = value; }

        /// <summary> Up Vector is the Opposite direction of the Gravity dir</summary>
        public Vector3 UpVector => -m_gravityDir.Value;

        /// <summary>if True the gravity will be the Negative Ground Normal Value</summary>
        public BoolReference ground_Changes_Gravity = new(false);

        #endregion

        #region Advanced Parameters 
        [Range(0, 180), Tooltip("Slow the Animal when the Turn Angle is ouside this limit")]
        public float TurnLimit = 120;

        private bool rootMotion = true;
        private bool rootMotionRotation = true;


        [Tooltip("Enable Disable the Rootmotion completely on th controller")]
        public BoolReference GlobalRootMotion = new(true);

        /// <summary> Raudius for the Sphere Cast</summary>
        public FloatReference rayCastRadius = new(0.05f);

        /// <summary>RayCast Radius for the Alignment Raycasting</summary>
        public float RayCastRadius => rayCastRadius.Value + 0.001f;
        /// <summary>This parameter exist to Add Additive pose to correct the animal</summary>
        public IntReference animalType = new(0);
        #endregion

        #region Use Stuff Properties  
        /// <summary>Does the Active State uses Additive Position Speed?</summary>
        public bool UseAdditivePos// { get; internal set; } 
        {
            get => useAdditivePos;
            set
            {
                useAdditivePos = value;
                if (!useAdditivePos) ResetInertiaSpeed();
            }
        }
        private bool useAdditivePos;

        /// <summary>Does the Active State uses Additive Position Speed?</summary>
        public bool UseAdditiveRot { get; internal set; }

        /// <summary>Does the Active State uses Sprint?</summary>
        public bool UseSprintState { get; internal set; }

        /// <summary>Custom Alignment done by some States.. (E.g. Swim) </summary>
        public bool UseCustomRotation { get; set; }
        //{
        //    get => m_UseCustomRotation;
        //    set
        //    {
        //        m_UseCustomRotation = value;
        //        Debug.Log($"UseCustomRotation : {value}");
        //    }
        //}
        //bool m_UseCustomRotation;



        private bool freemovement;
        /// <summary>The Animal is on Free Movement... which means is flying or swiming underwater</summary>
        public bool FreeMovement
        {
            get => freemovement;
            set
            {
                if (freemovement != value)
                {
                    freemovement = value;
                    OnFreeMovement.Invoke(value);
                    //Debug.Log($"Free Move: {value}");
                }
            }
        }
        /// <summary>Enable Disable the Global Sprint</summary>
        public bool UseSprint
        {
            get => useSprintGlobal;
            set
            {
                useSprintGlobal.Value = value;
                Sprint = sprint; //Update the Sprint value  IMPORTANT
            }
        }
        /// <summary>Enable Disable the Global Sprint (SAME AS USE SPRINT)</summary>
        public bool CanSprint { get => UseSprint; set => UseSprint = value; }

        /// <summary>Locks Input on the Animal, Ingore inputs like Jumps, Attacks , Actions etc</summary>
        public bool LockInput
        {
            get => lockInput.Value;
            set
            {
                lockInput.Value = value;
                OnInputLocked.Invoke(lockInput);
            }
        }

        /// <summary>Enable/Disable RootMotion on the Animator</summary>
        public bool RootMotion
        {
            get => rootMotion && GlobalRootMotion.Value;
            set => rootMotion = value;
        }

        /// <summary>Enable/Disable RootMotion Rotation Values on the Animator</summary>
        public bool RootMotionRotation
        {
            get => rootMotionRotation && RootMotion;
            set => rootMotionRotation = value;
        }

        /// <summary>  This store the DeltaRootMotion everytime its Deactivated/Activated  </summary>
        public Vector3 DeltaRootMotion { get; set; }
        //{
        //    get => m_DeltaRootMotion;
        //    set
        //    {
        //        m_DeltaRootMotion = value;
        //        Debug.Log($"DeltaRootMotion : {value}");
        //    }
        //}
        //Vector3 m_DeltaRootMotion;



        private bool useGravity;
        /// <summary>Does it use Gravity or not? </summary>
        public bool UseGravity
        {
            get => useGravity;
            set
            {
                useGravity = value;

                if (!useGravity) Gravity_ResetValues();//Reset Gravity Logic when Use gravity is false

                // Debug.Log("useGravity = " + useGravity);
            }
        }

        /// <summary>Locks the Movement on the Animal</summary>
        public bool LockMovement
        {
            get => lockMovement;
            set
            {
                lockMovement.Value = value;
                OnMovementLocked.Invoke(lockMovement);

                //Clear all residual movement if value is true
                if (lockMovement.Value) Reset_Movement();
            }
        }


        /// <summary>Sets to Zero the Z on the Movement Axis when this is set to true</summary>
        public bool LockForwardMovement
        {
            get => lockForwardMovement;
            set
            {
                lockForwardMovement.Value = value;
                LockMovementAxis.z = value ? 0 : 1;
            }
        }

        /// <summary>Sets to Zero the X on the Movement Axis when this is set to true</summary>
        public bool LockHorizontalMovement
        {
            get => lockHorizontalMovement;
            set
            {
                lockHorizontalMovement.Value = value;
                LockMovementAxis.x = value ? 0 : 1;
            }
        }

        /// <summary>Sets to Zero the Y on the Movement Axis when this is set to true</summary>
        public bool LockUpDownMovement
        {
            get => lockUpDownMovement;
            set
            {
                lockUpDownMovement.Value = value;
                LockMovementAxis.y = value ? 0 : 1;
            }
        }

        private Vector3 LockMovementAxis;
        private bool useOrientToGround;

        /// <summary>if True It will Aling it to the ground rotation depending the Front and Back Pivots</summary>
        public bool UseOrientToGround
        {
            get => useOrientToGround && m_OrientToGround.Value;
            set => useOrientToGround = value;
        }

        public bool GlobalOrientToGround
        {
            get => m_OrientToGround.Value;
            set
            {
                m_OrientToGround.Value = value;

                //Hide the Pivot Chest IMPORTANT
                if (Has_Pivot_Hip)
                    Has_Pivot_Chest = value && Pivot_Chest != null;
            }
        }

        [SerializeField, Tooltip("Global Orient to ground. Disable This for Humanoids")]
        private BoolReference m_OrientToGround = new(true);


        [SerializeField, Tooltip("Locks Input on the Animal, Ignore inputs like Jumps, Attacks, Actions etc")]
        private BoolReference lockInput = new(false);

        [SerializeField, Tooltip("Locks the Movement entries on the animal. (Horizontal, Vertical,Up Down)")]
        private BoolReference lockMovement = new(false);

        [SerializeField]
        private BoolReference useSprintGlobal = new(true);
        #endregion

        #region Animator States Info
        internal AnimatorStateInfo m_CurrentState;             // Information about the base layer of the animator cached.
        internal AnimatorStateInfo m_NextState;

        /// <summary> Meaning its transitioning from one animation to another Layer 0</summary>
        public bool InTransition => m_NextState.fullPathHash != 0;

        /// <summary>Returns the Current Animation State Tag of animal, if is in transition it will return the NextState Tag</summary>
        public AnimatorStateInfo AnimState { get; private set; }

        public int currentAnimTag;
        /// <summary>Current Active Animation Hash Tag </summary>
        public int AnimStateTag
        {
            get => currentAnimTag;
            internal set
            {
                if (value != currentAnimTag)
                {
                    // Debug.Log($" currentAnimTag {currentAnimTag}   value {value}");
                    currentAnimTag = value;
                    activeState?.AnimationTagEnter(value);
                }
            }
        }
        #endregion

        #region Platform
        public Transform platform;
        public Vector3 Last_Platform_Pos { get; set; }
        public Quaternion Last_Platform_Rot { get; set; }
        #endregion  

        #region Extras
        /// <summary>Used for Disabling Additive Position and Additive Rotation on the ANimal (The Pulling Wagons on the Horse Car  take care of it)</summary>?????
        public bool DisablePosition { get; set; } = false;
        ///// <summary>Used for Disabling Additive Position and Additive Rotation on the ANimal (The Pulling Wagons on the Horse Car  take care of it)</summary>?????
        public bool DisableRotation { get; set; } = false;

        //[Tooltip("When Falling and the animal get stuck falling, the animal will be force to move forward.")]
        //public FloatReference FallForward = new FloatReference(2);

        /// <summary> Internal Attack Trigers inside the Animal Controller </summary>
        public List<IMDamager> Attack_Triggers;

        #region Colliders
        [Tooltip("Main Collider of the Animal Controller (Usually attached to the Root GameObject)")]
        [ContextMenuItem("Find Main Collider", nameof(FindMainCollider))]
        public CapsuleCollider MainCollider;



        private OverrideCapsuleCollider MainCapsuleDefault;

        /// <summary>All Colliders Inside the Animals> summary>
        [Tooltip("Internal Colliders included in the Character (usually head, spine and limbs colliders)")]
        [ContextMenuItem("Find Internal Colliders", nameof(FindInternalColliders))]
        public List<Collider> colliders = new();
        #endregion

        /// <summary>Animator Normalized State Time for the Base Layer  </summary>
        public float StateTime { get; private set; }

        ///// <summary>Store from where the damage came from</summary>
        //public Vector3 HitDirection { set; get; }

        #endregion

        #region Events
        public IntEvent OnAnimationChange;

        /// <summary> Invoked when Input Locked value changes</summary>
        public BoolEvent OnInputLocked = new();
        /// <summary> Invoked when Movement Locked value changes</summary>
        public BoolEvent OnMovementLocked = new();
        /// <summary> Invoked when Sprint value changes</summary>
        public BoolEvent OnSprintEnabled = new();
        /// <summary> Invoked when Grounded value changes</summary>
        public BoolEvent OnGrounded = new();
        /// <summary> Invoked when Movement value changes</summary>
        public BoolEvent OnMovementDetected = new();
        /// <summary> Invoked when Free movement value changes</summary>
        public BoolEvent OnFreeMovement = new();

        #region ICharacter Implementation
        public System.Action<int> OnState { get; set; }

        public System.Action<int> OnStance { get; set; }
        public System.Action<int, int> ModeStart { get; set; }
        public System.Action<int, int> ModeEnd { get; set; }
        public bool PlayAction(int Set, int Index) => Mode_TryActivate(Set, Index);
        public bool ForceAction(int Set, int Index) => Mode_ForceActivate(Set, Index);
        public bool IsPlayingAction => IsPlayingMode;

        #endregion

        /// <summary> Invoked when a new State changes its profile</summary>
        public IntEvent OnStateProfile = new();

        /// <summary> Invoked when a new Mode start</summary>
        public Int2Event OnModeStart = new();
        /// <summary> Invoked when a new Mode ends</summary>
        public Int2Event OnModeEnd = new();

        /// <summary> Invoked when a new Stance is Activated</summary>
        public IntEvent OnStanceChange = new();                 //Invoked when is Changed to a new Stance
        public SpeedModifierEvent OnSpeedChange = new();        //Invoked when a new Speed is changed
        public Vector3Event OnTeleport = new();                 //Invoked when a new Speed is changed
        public Vector3Event OnPreTeleport = new();                 //Invoked when a new Speed is changed
        public BoolEvent OnGroundChangesGravity = new();                 //Invoked when a new Speed is changed


        /// <summary> Invoked when a new State is Activated</summary>
        public IntEvent OnStateActivate = new();
        /// <summary> Invoked when a new State has entered any of its Animations</summary>
        public IntEvent OnStateChange = new();

        ///<summary>List of Events to Use on the States</summary>
        public List<OnEnterExitState> OnEnterExitStates;
        ///<summary>List of Events to Use on the Stances</summary>
        public List<OnEnterExitStance> OnEnterExitStances;
        ///<summary>List of Events to Use on the Speeds</summary>
        public List<OnEnterExitSpeed> OnEnterExitSpeeds;
        #endregion

        #region Random
        public int RandomID { get; private set; }
        public int RandomPriority { get; private set; }

        /// <summary>Let States have Random Animations</summary>
        public bool Randomizer { get; set; }

        #endregion

        #region Animator Parameters

        [SerializeField, Tooltip("Forward (Z) Movement for the Animator")] private string m_Vertical = "Vertical";

        [SerializeField, Tooltip("Forward (Z) Movement for the Animator No Smoothing")] private string m_VerticalRaw = "VerticalRaw";

        [SerializeField, Tooltip("Horizontal (X) Movement for the Animator")] private string m_Horizontal = "Horizontal";
        [SerializeField, Tooltip("Vertical (Y) Movement for the Animator")] private string m_UpDown = "UpDown";
        [SerializeField, Tooltip("Vertical (Y) Difference between Target and Current UpDown")] private string m_DeltaUpDown = "DeltaUpDown";

        [SerializeField, Tooltip("Is the animal on the Ground? ")] private string m_Grounded = "Grounded";
        [SerializeField, Tooltip("Is the animal moving?")] private string m_Movement = "Movement";

        [SerializeField, Tooltip("Active/Current State the animal is")]
        private string m_State = "State";

        [SerializeField, Tooltip("Trigger to Notify the Activation of a State")]
        private string m_StateOn = "StateOn";
        [SerializeField, Tooltip("State profile to have multiple ways of playing a State")]
        private string m_StateProfile = "StateProfile";

        [SerializeField, Tooltip("Trigger to Notify the Activation of a Mode")]
        private string m_ModeOn = "ModeOn";

        [SerializeField, Tooltip("The Active State can have multiple status to change inside the State itself")]
        private string m_StateStatus = "StateEnterStatus";
        [SerializeField, Tooltip("The Active State can use this parameter to activate exiting animations")]
        private string m_StateExitStatus = "StateExitStatus";
        [SerializeField, Tooltip("Float value for the States to be used when needed")]
        private string m_StateFloat = "StateFloat";
        [SerializeField, Tooltip("Last State the animal was")]
        private string m_LastState = "LastState";

        [SerializeField, Tooltip("Active State Time for the States Animations")]
        private string m_StateTime = "StateTime";

        [SerializeField, Tooltip("Speed Multiplier for the Animations")]
        private string m_SpeedMultiplier = "SpeedMultiplier";

        [SerializeField, Tooltip("Active Mode the animal is... The Value is the Mode ID plus the Ability Index. Example Action Eat = 4002")]
        private string m_Mode = "Mode";

        [SerializeField, Tooltip("Store the Modes Status (Available=0  Started=1  Looping=-1 Interrupted=-2)")]
        private string m_ModeStatus = "ModeStatus";
        [SerializeField, Tooltip("Mode Float Value, Used to have a float Value for the modes to be used when needed")]
        private string m_ModePower = "ModePower";

        [SerializeField, Tooltip("Sprint Value")]
        private string m_Sprint = "Sprint";

        [SerializeField, Tooltip("Active/Current stance of the animal")] private string m_Stance = "Stance";
        [SerializeField, Tooltip("Previus/Last stance of the animal")] private string m_LastStance = "LastStance";
        [SerializeField, Tooltip("Normalized value of the Slope of the Terrain")] private string m_Slope = "Slope";
        [SerializeField, Tooltip("Type of animal for the Additive corrective pose")] private string m_Type = "Type";

        [SerializeField, Tooltip("Random Value for Animations States with multiple animations")] private string m_Random = "Random";
        [SerializeField, Tooltip("Target Angle calculated from the current forward  direction to the desired direction")] private string m_DeltaAngle = "DeltaAngle";
        [SerializeField, Tooltip("Does the Animal Uses Strafe")] private string m_Strafe = "Strafe";

        //[SerializeField, Tooltip("Horizontal Angle For the Target or Camera. Old [strafeAngle]")]
        //    [FormerlySerializedAs("m_strafeAngle")] 
        //private string m_TargetHorizontal = "TargetHorizontal";

        internal int hash_Vertical, hash_VerticalRaw;
        internal int hash_Horizontal;
        internal int hash_UpDown;

        internal int hash_DeltaUpDown;

        internal int hash_Movement;
        internal int hash_Grounded;
        internal int hash_SpeedMultiplier;

        internal int hash_DeltaAngle;

        internal int hash_State;
        internal int hash_StateOn;
        internal int hash_StateProfile;
        internal int hash_StateEnterStatus;
        internal int hash_StateExitStatus;
        internal int hash_StateFloat;
        internal int hash_StateTime;
        internal int hash_LastState;

        internal int hash_Mode;
        internal int hash_ModeOn;
        internal int hash_ModeStatus;
        internal int hash_ModePower;

        internal int hash_Stance;

        internal int hash_LastStance;

        internal int hash_Slope;
        internal int hash_Sprint;
        internal int hash_Random;
        internal int hash_Strafe;

        #endregion
    }


    [System.Serializable]
    public class OnEnterExitSpeed
    {
        [Tooltip("Which is the Speed Set (By its Name) changed. Case Sensitive")]
        public string SpeedSet;
        [Tooltip("Which is the Speed Modifier (By its Name) changed. This is Ignored if is set to 1. Case Sensitive")]
        public int SpeedIndex;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class OnEnterExitState
    {
        public StateID ID;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class OnEnterExitStance
    {
        public StanceID ID;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class SpeedModifierEvent : UnityEvent<MSpeed> { }
}

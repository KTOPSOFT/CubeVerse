using MalbersAnimations.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public partial class MAnimal
    {
        /// <summary> stored transform shorcut </summary>
        public Transform t;

        private Vector3 GizmoDeltaPos = Vector3.zero;

        void ChechUnscaledParent(Transform character)
        {
            if (character.parent == null) return;

            if (character.parent.transform.localScale != Vector3.one)
            {
                MDebug.LogWarning("The Character is parented to an Object with an Uneven Scale. Unparenting");
                character.parent = null;
            }
            else
            {
                ChechUnscaledParent(character.parent);
            }
        }


        private void UpdateCacheState()
        {
            //Use the same that is already on the states
            if (states_C == null || states_C.Count == 0 || (states_C.Count != states.Count))
            {
                states_C = new();

                foreach (var st in states)
                {
                    states_C.Add(new() { active = st.Active, priority = st.Priority, state = st });
                }
            }
        }


        /// <summary> Reparent the RootBone and Rotator so it works perfeclty with the Free Movement </summary>
        public void UpdateRotatorParent()
        {
            var CurrentScale = t.localScale; //IMPORTANT ROTATOR Animals needs to set the Rotator Bone with no scale first.
            t.localScale = Vector3.one;

            if (Rotator != null)
            {
                if (RootBone == null)
                {
                    if (Anim.avatar && Anim.avatar.isHuman)
                        RootBone = Anim.GetBoneTransform(HumanBodyBones.Hips).parent; //Get the RootBone from
                    else
                        RootBone = Anim.avatarRoot;
                    //Rotator.GetChild(0);           //Find the First Rotator Child  THIS CAUSE ISSUES WITH TIMELINE!!!!!!!!!!!!

                    if (RootBone == null)
                        MDebug.LogWarning("Make sure the Root Bone is Set on the Advanced Tab -> Misc -> RootBone. This is the Character's Avatar root bone");
                }

                if (RootBone != null && !RootBone.SameHierarchy(Rotator)) //If the rootbone is not grandchild Parent it
                {
                    //If the Rotator and the RootBone does not have the same position then create one
                    if (Rotator.position != RootBone.position)
                    {
                        RotatorOffset = new GameObject("Offset");
                        RotatorOffset.transform.SetPositionAndRotation(Position, Rotation);
                        RotatorOffset.layer = gameObject.layer; //Set the same layer as the Animal

                        RotatorOffset.transform.SetParent(Rotator);
                        RootBone.SetParent(RotatorOffset.transform);

                        RotatorOffset.transform.localScale = Vector3.one;
                        //RootBone.localScale = Vector3.one;
                    }
                    else
                    {
                        RootBone.parent = Rotator;
                    }
                }

                Rotator.gameObject.layer = gameObject.layer; //Set the same layer as the Animal
            }

            t.localScale = CurrentScale;

            //  Anim.Rebind(); //Necesary to complete the new Rotator Bone in the middle
        }


        public void Awake()
        {
            if (Anim == null) Anim = this.FindComponent<Animator>();   //Cache the Animator
            if (RB == null) RB = this.FindComponent<Rigidbody>();      //Cache the Rigid Body  
            if (Aimer == null) Aimer = this.FindComponent<Aim>();  //Cache the Aim Component 
            InputSource ??= this.FindInterface<IInputSource>(); //Find if we have a InputSource

            if (!CloneStates)
                MDebug.LogWarning
                    (
                        $"[{name}] has [ClonesStates] disabled. " +
                        $"If multiple characters use the same states, it will cause issues." +
                        $" Use this only for runtime changes on a single character"
                    , this);


            DefaultCameraInput = UseCameraInput;

            t = transform;

            AdditivePosition = Vector3.zero;
            AdditiveRotation = Quaternion.identity; //IMPORTANT!!!

            defaultGravityPower = m_gravityPower; //Store the Default Gravity Power in a private value
            // Anim.updateMode = AnimatorUpdateMode.AnimatePhysics; //Set the Animator to Update in the Physics Update

            //Clear the ModeQuee and Ability Input
            ModeQueueInput = new();
            AbilityQueueInput = new();

            GroundRootPosition = true;

            ChechUnscaledParent(t); //IMPORTANT

            UpdateRotatorParent();

            GetHashIDs();

            SetPivots();
            CalculateCenter();

            //Initialize all SpeedModifiers
            foreach (var set in speedSets) set.CurrentIndex = set.StartVerticalIndex;

            if (Anim)
            {
                // Anim.Rebind(); //Reset the Animator Controller
                Anim.speed = AnimatorSpeed * TimeMultiplier;                         //Set the Global Animator Speed

                var AllModeBehaviours = Anim.GetBehaviours<ModeBehaviour>();

                if (AllModeBehaviours != null)
                {
                    foreach (var ModeB in AllModeBehaviours)
                        ModeB.InitializeBehaviour(this);
                }
                else
                {
                    if (modes != null && modes.Count > 0)
                    {
                        MDebug.LogWarning("Please check your Animator Controller. There's no Mode Behaviors Attached to it. Re-import the Animator again");
                    }
                }
            }


            //Initialize The Default Stance
            if (defaultStance == null)
            {
                defaultStance = ScriptableObject.CreateInstance<StanceID>();
                defaultStance.name = "Default";
                defaultStance.ID = 0;
            }


            StartingStance = defaultStance; //Store the starting Stance

            //if (currentStance == null) currentStance = defaultStance; //Set the current Stance

            FindInternalColliders();
            SetDefaultMainColliderValues();

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == null) continue; //Skip Null States

                if (CloneStates)
                {
                    //Create a clone from the Original Scriptable Objects! IMPORTANT
                    var instance = ScriptableObject.Instantiate(states[i]);
                    instance.name = instance.name.Replace("(Clone)", "(Runtime)");
                    states[i] = instance;

                    //NEW LOCAL PRIORITY AND ACTIVE VALUES
                    states[i].Active = states_C[i].active;
                    states[i].Priority = states_C[i].priority;

                }

                states[i].AwakeState(this);

                if (states[i].Priority == 0) MDebug.LogWarning($"State [{states[i].name}] has priotity [0]. Please set a proper priority value", states[i]);
            }

            AwakeAllModes();

            Stances ??= new List<Stance>(); //Check if stances is null

            HasStances = Stances.Count > 0;
            if (HasStances)
            {
                foreach (var stance in Stances) stance.AwakeStance(this); //Awake all Stances

                LastActiveStance = Stance_Get(DefaultStanceID);
                ActiveStance = LastActiveStance;
            }


            currentSpeedSet = defaultSpeedSet;
            AlignUniqueID = UnityEngine.Random.Range(0, 99999);



            if (CanStrafe && !Aimer) MDebug.LogWarning("This character can strafe but there's no Aim component. Please add the Aim component");

            //Editor Checking (Make sure the Animator has an Avatar)
            if (Anim.avatar == null)
                MDebug.LogWarning("There's no Avatar on the Animator", Anim);

            if (RB) defaultKinematic = RB.isKinematic; //Make use the Rigibody is not kinematic

            DefaulCameraInput = UseCameraInput; //Cache the Default Camera Input


            //CHECK THE NEW STATE PRIORITY AND ACTIVE VALUES After the clone!
            UpdateCacheState();


            if (height == 1) CalculateCenter(true); //Update the height if is 1 (Default Value)



            //Fix to have on Awake all the states and stances not Null
            // activeState = states[^1];
            ActiveStance = Stance_Get(DefaultStanceID);

            //Reseting = true;
            JustActivateState = true;
            //  ActiveStateID = states[^1].ID;
        }


        private void AwakeAllModes()
        {
            modes_Dict = new();

            for (int i = 0; i < modes.Count; i++)
            {
                modes[i].Priority = modes.Count - i;
                modes[i].AwakeMode(this);

                modes_Dict.Add(modes[i].ID.ID, modes[i]); //Save the modes into a dictionary so they are easier to find.
            }
        }
        private bool defaultKinematic;
        private void CacheAllModes()
        {
            modes_Dict = new();

            for (int i = 0; i < modes.Count; i++)
            {
                modes_Dict.Add(modes[i].ID.ID, modes[i]); //Save the modes into a dictionary so they are easy to find.

                modes[i].CacheAbilities(); //Save all abilities to find it faster.
            }
        }

        // private bool JustReseted;

        public virtual void ResetController()
        {
            FindCamera();
            UpdateDamagerSet();

            if (MainCollider != null) MainCollider.enabled = true; //make sure the main collider is on

            GravityExtraPower = 1;

            //Clear the ModeQuee and Ability Input
            ModeQueueInput = new();
            AbilityQueueInput = new();

            LockMovement = false;
            LockInput = false;

            foreach (var state in states)
            {
                state.InitializeState();
                state.InputValue = false;
                state.ResetState();

                //Make sure the states are not in cooldown when the controller is reset 
                state.CurrentExitTime = -state.ExitCooldown.Value * 5;
                state.CurrentEnterTime = -state.EnterCooldown.Value * 5;


                state.OnAnimalEnabled();
            }

            foreach (var stance in Stances) stance.Reset(); //Reset All Stances!!

            //Reset RB Properties
            if (RB)
            {
                RB.useGravity = false;
                RB.constraints = RigidbodyConstraints.FreezeRotation;
                RB.linearDamping = 0;
                RB.angularDamping = 0;
                RB.isKinematic = defaultKinematic; //Make use the Rigibody is not kinematic
            }

            //  CacheAnimatorState(); //Find all Animator Tags

            EnableColliders(true); //Make sure to enable all colliders

            CheckIfGrounded(); //Make the first Alignment 
                               // CalculateCenter();

            lastState = null;

            //  TryActivateState();

            if (states == null || states.Count == 0)
            { Debug.LogError("The Animal must have at least one State added", this); return; }


            var StartStateIndex = states.Count - 1; //Find Idle


            if (OverrideStartState != null)
            {
                StartStateIndex = states.FindIndex(item => item.ID == OverrideStartState); //Find the Index of the Override State
            }

            CleanStateStart(StartStateIndex);

            //Reset Just Activate State The next Frame
            JustActivateState = true;
            this.Delay_Action(() => { JustActivateState = false; });

            var stan = currentStance;
            currentStance = null; //CLEAR STANCE
            Stance_Set(stan);
            State_SetFloat(0);
            UsingMoveWithDirection = (UseCameraInput); //IMPORTANT

            // Debug.Log("activeMode = " +(activeMode != null ? activeMode.Name: "NUKL"));

            activeMode = null;


            if (IsPlayingMode) Mode_Stop();

            //Set Start with Mode
            if (StartWithMode.Value != 0)
            {
                if (StartWithMode.Value / 1000 == 0)
                {
                    Mode_Activate(StartWithMode.Value);
                }
                else
                {
                    var mode = StartWithMode.Value / 1000;
                    var modeAb = StartWithMode.Value % 1000;
                    if (modeAb == 0) modeAb = -99;
                    Mode_Activate(mode, modeAb);
                }
            }

            LastPosition = Position; //Store Last Animal Position

            //  ForwardMultiplier = 1f; //Initialize the Forward Multiplier
            GravityMultiplier = 1f;

            MovementAxis =
            MovementAxisRaw =
            AdditivePosition =
            InertiaPositionSpeed =
            SlopeDirectionSmooth =
            MovementAxisSmoothed = Vector3.zero; //Reset Vector Values

            LockMovementAxis = (new Vector3(LockHorizontalMovement ? 0 : 1, LockUpDownMovement ? 0 : 1, LockForwardMovement ? 0 : 1));

            UseRawInput = true; //Set the Raw Input as default.
            UseAdditiveRot = true;
            UseAdditivePos = true;
            Grounded = true;
            Randomizer = true;
            AlwaysForward = AlwaysForward;         // Execute the code inside Always Forward .... Why??? Don't know ..something to do with the Input stuff

            StrafeLogic();

            GlobalOrientToGround = GlobalOrientToGround; // Execute the code inside Global Orient
            SpeedMultiplier = 1;
            CurrentCycle = 0;
            Gravity_ResetValues();



            var TypeHash = TryOptionalParameter(m_Type);
            TryAnimParameter(TypeHash, animalType); //This is only done once!

            //Reset FreeMovement.
            if (Rotator) Rotator.localRotation = Quaternion.identity;

            Bank = 0;
            PitchAngle = 0;
            PitchDirection = Vector3.forward;


            if (!GlobalOrientToGround) DisablePivotChest();

            void CleanStateStart(int ID)
            {
                activeState = states[ID];
                ActiveStateID = activeState.ID;         //Set the New ActivateID
                activeState.Activate();
                lastState = activeState;                //Do not use the Properties....
                activeState.IsPending = false;          //Force the active state to start without entering the animation.
                activeState.CanExit = true;             //Force that it can exit... so another can activate it
                activeState.General.Modify(this);       //Force the active state to Modify all the Animal Settings
                activeState.InCoreAnimation = true;
                ActiveState = activeState;
                activeState.DisableModes_Temp(true, activeState.DisableModes);      //Make sure the modes are disabled on start

                OnStateActivate.Invoke(activeState.ID);                             //Play the correc animations
                SetIntParameter(hash_State, activeState.ID.ID);                     //Sent to the Animator the value to Apply  
                TryAnimParameter(hash_StateOn);                                     //Enable State On
            }

            //Debug.Log("Animal Reseted");
            //Reseting = false;
        }

        /// <summary> Find the Main Camera if Main CameraDirection is true </summary>
        public virtual void FindCamera()
        {
            if (MainCamera == null) //Find the Camera if is not already set
            {
                if (UseMainCameraDirection.Value)
                {
                    m_MainCamera.UseConstant = true;

                    var mainCam = MTools.FindMainCamera();

                    if
                        (mainCam) m_MainCamera.Value = mainCam.transform;
                }
                else
                {
                    var DefaulDirection = new GameObject("World Direction");
                    m_MainCamera.Value = DefaulDirection.transform;
                    DefaulDirection.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }

        [ContextMenu("Set Pivots")]
        public void SetPivots()
        {
            Pivot_Hip = pivots.Find(item => item.name.ToUpper() == "HIP");
            Pivot_Chest = pivots.Find(item => item.name.ToUpper() == "CHEST");
            Has_Pivot_Hip = Pivot_Hip != null;
            Has_Pivot_Chest = Pivot_Chest != null;
            Starting_PivotChest = Has_Pivot_Chest;

            if (Has_Pivot_Hip) Pivot_Multiplier = Pivot_Hip.multiplier;
            if (Has_Pivot_Chest) Pivot_Multiplier = Mathf.Max(Pivot_Multiplier, Pivot_Chest.multiplier);
            if (NoPivot) Pivot_Multiplier = Height;

            if (!Application.isPlaying) MTools.SetDirty(this);
        }



        public void OnEnable()
        {
            Animals ??= new List<MAnimal>();
            Animals.Add(this);                                              //Save the the Animal on the current List

            ResetInputSource(); //Connect the Inputs

            if (isPlayer) SetMainPlayer();

            SetBoolParameter += SetAnimParameter;
            SetIntParameter += SetAnimParameter;
            SetFloatParameter += SetAnimParameter;
            SetTriggerParameter += SetAnimParameter;

            if (!alwaysForward.UseConstant && alwaysForward.Variable != null)
                alwaysForward.Variable.OnValueChanged += Always_Forward;


            //Reset the Controller the next frame (Weird Bug with override state)
            // this.Delay_Action(() =>
            //  { 
            ResetController();
            //});

            if (Grounded) this.Delay_Action(() => SetAnimParameter(hash_Grounded, Grounded)); //Set the Grounded Hash on the next frame (Important for the Grounded Logic

            Sleep = false;
        }

        public void OnDisable()
        {
            Animals?.Remove(this);       //Remove all this animal from the Overall AnimalList

            UpdateInputSource(false); //Disconnect the inputs
            DisableMainPlayer();

            MTools.ResetFloatParameters(Anim); //Reset all Anim Floats!!
            if (RB && !RB.isKinematic) RB.linearVelocity = Vector3.zero;

            if (!alwaysForward.UseConstant && alwaysForward.Variable != null) //??????
                alwaysForward.Variable.OnValueChanged -= Always_Forward;

            if (states != null)
            {
                foreach (var st in states)
                {
                    if (st != null)
                    {
                        st.ExitState();
                        st.OnAnimalDisabled();
                    }
                }
            }

            if (IsPlayingMode)
            {
                ActiveMode?.ResetMode();
                Mode_Stop();
            }

            OverrideStartState = ActiveStateID; //Save the current State to start with it next time
            ActiveState?.EnterExitEvent?.OnExit.Invoke();

            //This needs to be at the end of the Disable stuff
            SetBoolParameter -= SetAnimParameter;
            SetIntParameter -= SetAnimParameter;
            SetFloatParameter -= SetAnimParameter;
            SetTriggerParameter -= SetAnimParameter;

            StopAllCoroutines();

        }

        /// <summary> Calculates the center and Height of the Animal </summary>
        public void CalculateCenter(bool updateHeight = false)
        {
            if (Has_Pivot_Hip)
            {
                if (updateHeight) height = Pivot_Hip.position.y;
                Center = Pivot_Hip.position; //Set the Center to be the Pivot Hip Position
            }
            else if (Has_Pivot_Chest)
            {
                if (updateHeight) height = Pivot_Chest.position.y;
                Center = Pivot_Chest.position;
            }

            if (Has_Pivot_Chest && Has_Pivot_Hip)
            {
                Center = (Pivot_Chest.position + Pivot_Hip.position) / 2;
            }

            center.y = 0; //Remove Y since that is calculated by the Height of the Animal

            if (!Application.isPlaying) MTools.SetDirty(this);
        }

        /// <summary>Update all the Attack Triggers Inside the Animal...In case there are more or less triggers</summary>
        public void UpdateDamagerSet()
        {
            Attack_Triggers = GetComponentsInChildren<IMDamager>(true).ToList();        //Save all Attack Triggers.

            foreach (var at in Attack_Triggers)
            {
                at.Owner = (gameObject);                 //Tell to avery Damager that this Animal is the Owner
                                                         // at.Enabled = false;
            }
        }

        #region Animator Stuff
        protected virtual void GetHashIDs()
        {
            if (Anim == null) return;

            //Store all the Animator Parameter in a Dictionary
            //animatorParams = new Hashtable();
            animatorHashParams = new();

            foreach (var parameter in Anim.parameters)
            {
                animatorHashParams.Add(parameter.nameHash);
            }

            #region Main Animator Parameters
            //Movement
            hash_Vertical = Animator.StringToHash(m_Vertical);

            hash_Horizontal = Animator.StringToHash(m_Horizontal);
            hash_SpeedMultiplier = Animator.StringToHash(m_SpeedMultiplier);

            hash_Movement = Animator.StringToHash(m_Movement);
            hash_Grounded = Animator.StringToHash(m_Grounded);

            //States
            hash_State = Animator.StringToHash(m_State);
            hash_StateEnterStatus = Animator.StringToHash(m_StateStatus);


            hash_LastState = Animator.StringToHash(m_LastState);
            hash_StateFloat = Animator.StringToHash(m_StateFloat);

            //Modes
            hash_Mode = Animator.StringToHash(m_Mode);

            hash_ModeStatus = Animator.StringToHash(m_ModeStatus);


            //Triggers
            hash_ModeOn = Animator.StringToHash(m_ModeOn);
            hash_StateOn = Animator.StringToHash(m_StateOn);


            #endregion

            #region Optional Parameters

            //Movement 
            hash_StateExitStatus = TryOptionalParameter(m_StateExitStatus);
            //hash_StateEnterStatus = TryOptionalParameter(m_StateStatus);
            hash_SpeedMultiplier = TryOptionalParameter(m_SpeedMultiplier);

            hash_VerticalRaw = TryOptionalParameter(m_VerticalRaw);

            hash_UpDown = TryOptionalParameter(m_UpDown);
            hash_DeltaUpDown = TryOptionalParameter(m_DeltaUpDown);

            hash_Slope = TryOptionalParameter(m_Slope);


            hash_DeltaAngle = TryOptionalParameter(m_DeltaAngle);
            hash_Sprint = TryOptionalParameter(m_Sprint);

            //States
            hash_StateTime = TryOptionalParameter(m_StateTime);


            hash_Strafe = TryOptionalParameter(m_Strafe);
            //hash_TargetHorizontal = TryOptionalParameter(m_TargetHorizontal);

            //Stance
            hash_Stance = TryOptionalParameter(m_Stance);

            hash_LastStance = TryOptionalParameter(m_LastStance);

            //Misc
            hash_Random = TryOptionalParameter(m_Random);
            hash_ModePower = TryOptionalParameter(m_ModePower);


            hash_StateProfile = TryOptionalParameter(m_StateProfile);
            // hash_StanceOn = TryOptionalParameter(m_StanceOn);
            #endregion
        }


        //Send 0 if the Animator does not contain
        private int TryOptionalParameter(string param)
        {
            var AnimHash = Animator.StringToHash(param);

            if (!animatorHashParams.Contains(AnimHash))
                return 0;
            return AnimHash;
        }

        private bool sameAnimTag;

        protected virtual void CacheAnimatorState()
        {
            // m_PreviousCurrentState = m_CurrentState;
            //  m_PreviousNextState = m_NextState;

            m_CurrentState = Anim.GetCurrentAnimatorStateInfo(0);
            m_NextState = Anim.GetNextAnimatorStateInfo(0);

            //If the animator is in transition (Next state has full path )
            if (m_NextState.fullPathHash != 0)
            {
                //If the animations are different but the tags are the same
                if (m_CurrentState.fullPathHash != AnimState.fullPathHash
                    && m_CurrentState.tagHash == m_NextState.tagHash)
                {
                    if (!sameAnimTag)
                    {
                        sameAnimTag = true;
                        currentAnimTag = -1; //Reset the current animtag so the method can be called again
                    }
                }
                else
                {
                    sameAnimTag = false;
                }

                AnimStateTag = m_NextState.tagHash;
                AnimState = m_NextState;
            }

            else
            {
                if (m_CurrentState.fullPathHash != AnimState.fullPathHash)
                {
                    AnimStateTag = m_CurrentState.tagHash;
                }
                AnimState = m_CurrentState;
            }

            var lastStateTime = StateTime;
            StateTime = Mathf.Repeat(AnimState.normalizedTime, 1);

            //Check if the Animation Started again.
            if (lastStateTime > StateTime)
                StateCycle?.Invoke(ActiveStateID);
        }

        /// <summary>Link all Parameters to the animator</summary>
        internal virtual void UpdateAnimatorParameters()
        {
            SetFloatParameter?.Invoke(hash_Vertical, VerticalSmooth);
            SetFloatParameter?.Invoke(hash_Horizontal, HorizontalSmooth);

            TryAnimParameter(hash_UpDown, UpDownSmooth);
            TryAnimParameter(hash_DeltaUpDown, DeltaUpDown);


            TryAnimParameter(hash_DeltaAngle, DeltaAngle);
            TryAnimParameter(hash_Slope, SlopeNormalized);
            TryAnimParameter(hash_SpeedMultiplier, SpeedMultiplier);
            TryAnimParameter(hash_StateTime, StateTime);
        }
        #endregion

        #region Additional Speeds (Movement, Turn) 

        public bool ModeNotAllowMovement => IsPlayingMode && !ActiveMode.AllowMovement;


        /// <summary>Multiplier added to the Additive position when the mode is playing.
        /// This will fix the issue Additive Speeds to mess with RootMotion Modes  </summary>
        public float Mode_Multiplier => IsPlayingMode ? ActiveMode.PositionMultiplier : 1;
        private void MoveRotator()
        {
            if (!FreeMovement && Rotator)
            {
                if (PitchAngle != 0 || Bank != 0)
                {
                    float limit = 0.005f;
                    var lerp = DeltaTime * (CurrentSpeedSet.PitchLerpOff);

                    Rotator.localRotation = Quaternion.Slerp(Rotator.localRotation, Quaternion.identity, lerp);

                    PitchAngle = Mathf.Lerp(PitchAngle, 0, lerp); //Lerp to zero the Pitch Angle when goind Down
                    Bank = Mathf.Lerp(Bank, 0, lerp);

                    if (Mathf.Abs(PitchAngle) < limit && Mathf.Abs(Bank) < limit)
                    {
                        Bank = PitchAngle = 0;
                        Rotator.localRotation = Quaternion.identity;
                    }
                }
            }
            else
            {
                CalculatePitchDirectionVector();
            }
        }

        public virtual void FreeMovementRotator(float Ylimit, float bank)
        {
            CalculatePitch(Ylimit);
            CalculateBank(bank);
            CalculateRotator();
        }

        internal virtual void CalculateRotator()
        {
            if (Rotator) Rotator.localEulerAngles = new Vector3(PitchAngle, 0, Bank); //Angle for the Rotator
        }
        internal virtual void CalculateBank(float bank) =>
            Bank = Mathf.Lerp(Bank, -bank * Mathf.Clamp(HorizontalSmooth, -1, 1), DeltaTime * CurrentSpeedSet.BankLerp);
        internal virtual void CalculatePitch(float Pitch)
        {
            float NewAngle = 0;

            if (MovementAxis != Vector3.zero)             //Rotation PITCH
            {
                NewAngle = 90 - Vector3.Angle(UpVector, PitchDirection);
                NewAngle = Mathf.Clamp(-NewAngle, -Pitch, Pitch);
            }

            var deltatime = DeltaTime * CurrentSpeedSet.PitchLerpOn;

            PitchAngle = Mathf.Lerp(PitchAngle, Strafe ? Pitch * VerticalSmooth : NewAngle, deltatime);
            DeltaUpDown = Mathf.Lerp(DeltaUpDown, -Mathf.DeltaAngle(PitchAngle, NewAngle), deltatime * 2);

            if (Mathf.Abs(DeltaUpDown) < 0.01f) DeltaUpDown = 0;
        }


        /// <summary>Calculates the Pitch direction to Appy to the Rotator Transform</summary>
        internal virtual void CalculatePitchDirectionVector()
        {
            var dir = Move_Direction != Vector3.zero ? Move_Direction : Forward;
            PitchDirection = Vector3.Lerp(PitchDirection, dir, DeltaTime * CurrentSpeedSet.PitchLerpOn * 2);
        }

        public void SetTargetSpeed()
        {
            //var lerp = CurrentSpeedModifier.lerpPosition * DeltaTime;

            if ((!UseAdditivePos) ||      //Do nothing when UseAdditivePos is False
               (ModeNotAllowMovement))  //Do nothing when the Mode Locks the Movement
            {
                //TargetSpeed = Vector3.Lerp(TargetSpeed, Vector3.zero, lerp);
                TargetSpeed = Vector3.zero;
                return;
            }

            Vector3 TargetDir = ActiveState.Speed_Direction();

            // MDebug.Draw_Arrow(Position + GizmoDeltaPos, TargetDir, Color.blue);

            //IMPORTANT USE THE SLOPE IF the Animal uses only one slope
            if (Grounded && Has_Pivot_Chest && !Has_Pivot_Hip)
                TargetDir = Quaternion.FromToRotation(Up, SlopeNormal) * TargetDir;

            float Speed_Modifier = Strafe ? CurrentSpeedModifier.strafeSpeed.Value : CurrentSpeedModifier.position.Value;


            if (InGroundChanger)
            {
                var GroundSpeedRoot = RootMotion ? (Anim.deltaPosition / DeltaTime).magnitude : 0;
                Speed_Modifier = Speed_Modifier + GroundChanger.Position + GroundSpeedRoot;
            }

            if (Strafe)
            {
                TargetDir = (Forward * VerticalSmooth) + (Right * HorizontalSmooth);

                if (FreeMovement)
                    TargetDir += (Up * UpDownSmooth);

            }
            else
            {
                if ((VerticalSmooth < 0) && CurrentSpeedSet != null)//Decrease when going backwards and NOT Strafing
                {
                    TargetDir *= -CurrentSpeedSet.BackSpeedMult.Value;
                    Speed_Modifier = CurrentSpeedSet[0].position; //Get the current speed modifier and the additive mode speed

                    if (InGroundChanger)
                    {
                        var GroundSpeedRoot = RootMotion ? (Anim.deltaPosition / DeltaTime).magnitude : 0;
                        Speed_Modifier = Speed_Modifier + GroundChanger.Position + GroundSpeedRoot;
                    }
                }
                if (FreeMovement)
                {
                    float SmoothZYInput = Mathf.Clamp01(Mathf.Max(Mathf.Abs(UpDownSmooth), Mathf.Abs(VerticalSmooth))); // Get the Average Multiplier of both Z and Y Inputs
                    TargetDir *= SmoothZYInput;
                }
                else
                {
                    TargetDir *= VerticalSmooth; //Use Only the Vertical Smooth while grounded
                }
            }



            if (TargetDir.magnitude > 1) TargetDir.Normalize();


            Speed_Modifier += Mode_Additive_Pos; //Add the Mode Additive Position

            TargetSpeed = DeltaTime * Mode_Multiplier * ScaleFactor * Speed_Modifier * TargetDir;   //Calculate these Once per Cycle Extremely important 

            HorizontalVelocity = Vector3.ProjectOnPlane(Inertia + SlopeDirectionSmooth, SlopeNormal);
            HorizontalSpeed = HorizontalVelocity.magnitude;

            if (debugGizmos)
                MDebug.Draw_Arrow(Position + GizmoDeltaPos, TargetSpeed, Color.green);

            // MDebug.Draw_Arrow(Position + GizmoDeltaPos, TargetDir * 5, Color.cyan);
        }

        /// <summary> Add more Speed to the current Move animations</summary>  
        protected virtual void AdditionalSpeed(float time)
        {
            var Speed = CurrentSpeedModifier;

            var LerpPos = (Strafe) ? Speed.lerpStrafe : Speed.lerpPosition;

            if (InGroundChanger) LerpPos = GroundChanger.Lerp; //USE GROUND CHANGER LERP


            InertiaPositionSpeed = (LerpPos > 0) ?
                Vector3.Lerp(InertiaPositionSpeed, UseAdditivePos ? TargetSpeed : Vector3.zero, time * LerpPos) : TargetSpeed;

            AdditivePosition += InertiaPositionSpeed;


            //Avoids code returning NaN
            if (float.IsNaN(InertiaPositionSpeed.x) || float.IsNaN(InertiaPositionSpeed.y) || float.IsNaN(InertiaPositionSpeed.z))
                InertiaPositionSpeed = TargetSpeed;

            if (debugGizmos)
            {
                MDebug.Draw_Arrow(Position + GizmoDeltaPos + (Vector3.one * 0.02f), 2 * ScaleFactor * InertiaPositionSpeed, new Color(.8f, .5f, 0));  //Draw the Intertia Direction 
            }
            // MDebug.Draw_Arrow(Position, 2 * ScaleFactor * TargetSpeed.normalized, Color.cyan);  //Draw the Intertia Direction 
        }
        /// <summary>The full Velocity we want to without lerping, for the Additional Position NOT INLCUDING ROOTMOTION</summary>
        public Vector3 TargetSpeed { get; internal set; }


        /// <summary>Add more Rotations to the current Turn Animations  </summary>
        protected virtual void AdditionalRotation(float time)
        {
            if (IsPlayingMode && !ActiveMode.AllowRotation) return;          //Do nothing if the Mode Does not allow Rotation

            float SpeedRotation = CurrentSpeedModifier.rotation * AdditiveRotationMultiplier;

            if (VerticalSmooth < 0.01 && !CustomSpeed && CurrentSpeedSet != null)
            {
                SpeedRotation = CurrentSpeedSet[0].rotation; //When not moving ???
            }

            SpeedRotation += Mode_Additive_Rot; //Add the Mode Rotation

            if (SpeedRotation < 0) return;      //Do nothing if the rotation is lower than 0

            if (MovementDetected)
            {
                if (UsingMoveWithDirection)
                {
                    if (DeltaAngle != 0)
                    {
                        var TargetLocalRot = Quaternion.Euler(0, DeltaAngle * Mode_Multiplier_Rot, 0);

                        var targetRotation =
                            Quaternion.Slerp(Quaternion.identity, TargetLocalRot, (SpeedRotation + 1) / 4 * ((TurnMultiplier + 1) * time));

                        AdditiveRotation *= targetRotation;
                    }
                }
                else
                {
                    float Turn = SpeedRotation * 10 * Mode_Multiplier_Rot;           //Add Extra Multiplier

                    //Add +Rotation when going Forward and -Rotation when going backwards
                    float TurnInput = Mathf.Clamp(HorizontalSmooth, -1, 1) * (MovementAxis.z >= 0 ? 1 : -1);

                    AdditiveRotation *= Quaternion.Euler(0, Turn * TurnInput * time /** ModeRotation*/, 0);
                    var TargetGlobal = Quaternion.Euler(0, TurnInput * (TurnMultiplier + 1), 0);
                    var AdditiveGlobal = Quaternion.Slerp(Quaternion.identity, TargetGlobal, time * (SpeedRotation + 1) /** ModeRotation*/);
                    AdditiveRotation *= AdditiveGlobal;
                }
            }
        }


        internal void SetMaxMovementSpeed()
        {
            float maxspeedV = CurrentSpeedModifier.Vertical;
            float maxspeedH = 1;

            if (Strafe)
            {
                maxspeedH = maxspeedV;
            }
            VerticalSmooth = MovementAxis.z * maxspeedV;
            HorizontalSmooth = MovementAxis.x * maxspeedH;
            UpDownSmooth = MovementAxis.y;
        }


        /// <summary> Movement Trot Walk Run (Velocity changes)</summary>
        internal void MovementSystem()
        {
            float maxspeedV = CurrentSpeedModifier.Vertical;
            float maxspeedH = 1;

            var LerpUpDown = DeltaTime * CurrentSpeedSet.PitchLerpOn;
            var LerpVertical = DeltaTime * CurrentSpeedModifier.lerpPosAnim;
            var LerpTurn = DeltaTime * CurrentSpeedModifier.lerpRotAnim;
            var LerpAnimator = DeltaTime * CurrentSpeedModifier.lerpAnimator;

            if (Strafe)
            {
                maxspeedH = maxspeedV;
            }

            if (ModeNotAllowMovement) //Active mode and Is playing Mode is failing!!**************
                MovementAxis = Vector3.zero;

            float Horiz;

            float v = MovementAxis.z;


            if (Rotate_at_Direction /*|| inTurnLimit*/)
            {
                float r = 0;
                v = 0; //Remove the Forward since its
                Horiz = Mathf.SmoothDamp(HorizontalSmooth,/* RawRotateDirAxis.x*/MovementAxis.x, ref r, inPlaceDamp * DeltaTime); //Using properly the smooth  down
            }
            else
            {
                Horiz = Mathf.Lerp(HorizontalSmooth, MovementAxis.x * maxspeedH, LerpTurn);
            }


            //Horiz = Mathf.SmoothDamp(HorizontalSmooth, MovementAxis.x, ref r, inPlaceDamp * DeltaTime); //Using properly the smooth  down

            //float rr = 0;
            //VerticalSmooth = LerpVertical > 0 ?
            // Mathf.SmoothDamp(VerticalSmooth, v * maxspeedV, ref rr, LerpVertical) :
            // MovementAxis.z * maxspeedV;           //smoothly transitions bettwen Speeds


            //Debug.Log($"v {v:F2}, maxspeedV {maxspeedV:F2} , LerpVertical {LerpVertical:F2} MovementAxis.z {MovementAxis.z}");

            VerticalSmooth = LerpVertical > 0 ?
                Mathf.Lerp(VerticalSmooth, v * maxspeedV, LerpVertical) :
                MovementAxis.z * maxspeedV;           //smoothly transitions bettwen Speeds

            HorizontalSmooth = LerpTurn > 0 ? Horiz : MovementAxis.x * maxspeedH;               //smoothly transitions bettwen Directions

            UpDownSmooth = LerpVertical > 0 ?
                Mathf.Lerp(UpDownSmooth, MovementAxis.y, LerpUpDown) :
                MovementAxis.y;                                                //smoothly transitions bettwen Directions


            SpeedMultiplier = (LerpAnimator > 0) ?
                Mathf.Lerp(SpeedMultiplier, CurrentSpeedModifier.animator.Value, LerpAnimator) :
                CurrentSpeedModifier.animator.Value;  //Changue the velocity of the animator

            if (Mathf.Abs(VerticalSmooth) < zero) VerticalSmooth = 0;
            if (Mathf.Abs(HorizontalSmooth) < zero) HorizontalSmooth = 0;
            if (Mathf.Abs(UpDownSmooth) < zero) UpDownSmooth = 0;
        }

        private const float zero = 0.005f;

        #endregion

        #region Platorm movement


        /// <summary>  Reference for the Animal to check if it is on a Ground Changer  </summary>
        public GroundSpeedChanger GroundChanger { get; set; }
        /// <summary> True if GroundChanger is not Null </summary>
        public bool InGroundChanger;

        /// <summary>Check if the Animal can do the Ground RootMotion </summary>
        internal bool GroundRootPosition = true;
        public void SetPlatform(Transform newPlatform)
        {
            if (platform != newPlatform)
            {
                GroundRootPosition = true;
                platform = newPlatform;

                if (platform != null)
                {
                    //Debug.Log($"NEW PLATFORM {platform}");
                    var NewGroundChanger = newPlatform.GetComponent<GroundSpeedChanger>();

                    if (NewGroundChanger)
                    {
                        GroundRootPosition = false; //Important! Calculate RootMotion instead of adding it
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = NewGroundChanger;
                        GroundChanger.OnEnter?.React(this); //set to the ground changer that this has enter 
                    }
                    else
                    {
                        GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                        GroundChanger = null;
                    }

                    Last_Platform_Pos = platform.position;
                    Last_Platform_Rot = platform.rotation;
                }
                else  //No Platform
                {
                    GroundChanger?.OnExit?.React(this); //set to the ground changer that this has enter 
                    GroundChanger = null;

                    DeltaPlatformPos = Vector3.zero;
                    DeltaPlatformRot = Quaternion.identity;

                    // Debug.Log("RESET PLATFORM VALUES");

                    MainPivotSlope = 0;
                    ResetSlopeValues();
                }

                InGroundChanger = GroundChanger != null;


                foreach (var s in states)
                    s.OnPlataformChanged(platform);
            }
        }

        public void PlatformMovement()
        {
            if (platform == null) return;
            if (platform.gameObject.isStatic) return; //means it cannot move so do not calculate anything

            // LastPosition = Position;

            DeltaPlatformPos = platform.position - Last_Platform_Pos;
            Quaternion Inverse_Rot = Quaternion.Inverse(Last_Platform_Rot);
            DeltaPlatformRot = Inverse_Rot * platform.rotation;

            if (DeltaPlatformRot != Quaternion.identity)                                    //No rotation founded.. Skip the code below
            {
                var pos = t.DeltaPositionFromRotate(platform.position, DeltaPlatformRot);
                DeltaPlatformPos += pos;
            }

            Position += DeltaPlatformPos;               //Set it Directly to the Transform.. Additive Position can be reset any time..
            Rotation *= DeltaPlatformRot;  //Set it Directly to the Transform.. Additive Position can be reset any time..

            //Debug.Log($"- {DeltaPlatformPos}");
            //AdditiveRotation *= Delta;

            Last_Platform_Pos = platform.position;
            Last_Platform_Rot = platform.rotation;
        }

        public Vector3 DeltaPlatformPos { get; private set; }
        public Quaternion DeltaPlatformRot { get; private set; }
        #endregion

        #region Terrain Alignment
        /// <summary> Store the GameObjectFront Hit.. This is used to compare the tag and find if it is a debreee or not.  </summary>
        private GameObject MainFronHit;
        private bool isDebrisFront;

        /// <summary>  Raycasting stuff to align and calculate the ground from the animal ****IMPORTANT***  </summary>
        /// <param name="distance">
        /// if is set to zero then Use the PIVOT_MULTIPLIER. Set the Distance when you want to cast from the Animal Height instead.
        /// </param>
        internal virtual void AlignRayCasting(float distance = 0)
        {
            //Debug.Log($"Align RayCasting!!");
            MainRay = FrontRay = false;
            hit_Chest = new RaycastHit() { normal = Vector3.zero };                               //Clean the Raycasts every time 
            hit_Hip = new RaycastHit();                                 //Clean the Raycasts every time 
            hit_Chest.distance = hit_Hip.distance = Height;            //Reset the Distances to the Heigth of the animal

            if (distance == 0) distance = Pivot_Multiplier * ScaleFactor; //IMPORTANT 

            //  Debug.Log($"Main_Pivot_Point: {Main_Pivot_Point} ");

            if (Physics.Raycast(Main_Pivot_Point, -Up, out hit_Chest, distance, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                if (MTools.Layer_in_LayerMask(hit_Chest.collider.gameObject.layer, groundLayer.Value) && hit_Chest.collider.transform.SameHierarchy(transform))
                { MDebug.LogWarning($"The Internal Collider [{hit_Chest.collider.name}] is on the Ground Layer Mask. Please change the Layer of the gameobject", hit_Chest.collider); }

                FrontRay = true;

                //Store if the Front Hit is a Debris so Storing if is a Debree it will be only be done once
                if (MainFronHit != hit_Chest.transform.gameObject)
                {
                    MainFronHit = hit_Chest.transform.gameObject;
                    isDebrisFront = MainFronHit.CompareTag(DebrisTag);
                }

                //If is a debree clean everything like it was a Flat Terrain (CHECK DEBREEEE)
                if (isDebrisFront)
                {
                    MainPivotSlope = 0;
                    hit_Chest.normal = UpVector;
                    ResetSlopeValues();
                }
                else
                {
                    //Store the Downward Slope Direction
                    SlopeNormal = hit_Chest.normal;
                    MainPivotSlope = Vector3.SignedAngle(SlopeNormal, UpVector, Right);
                    SlopeDirection = Vector3.ProjectOnPlane(Gravity, SlopeNormal).normalized;

                    SlopeDirectionAngle = 90 - Vector3.Angle(Gravity, SlopeDirection);
                    if (Mathf.Approximately(SlopeDirectionAngle, 90)) SlopeDirectionAngle = 0;
                }

                if (debugGizmos)
                {
                    MDebug.DrawRay(hit_Chest.point + GizmoDeltaPos, 0.2f * ScaleFactor * SlopeNormal, Color.green);
                    MDebug.DrawWireSphere(Main_Pivot_Point + GizmoDeltaPos + -Up * (hit_Chest.distance - RayCastRadius), Color.green, RayCastRadius * ScaleFactor);
                    MDebug.Draw_Arrow(hit_Chest.point + GizmoDeltaPos, SlopeDirection * 0.5f, Color.black, 0, 0.1f);
                }

                SetPlatform(hit_Chest.transform);

                //Physic Logic (Push RigidBodys Down with the Weight)
                AddForceToGround(hit_Chest.collider, hit_Chest.point);

            }
            else
            {
                SetPlatform(null); //Nothing was touched
            }

            if (Has_Pivot_Hip && Has_Pivot_Chest) //Ray From the Hip to the ground
            {
                var hipPoint = Pivot_Hip.World(t);// + DeltaVelocity;

                MDebug.DrawWireSphere(hipPoint, Color.yellow, RayCastRadius * ScaleFactor);

                if (Physics.Raycast(hipPoint, -Up, out hit_Hip, distance, GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    if (MTools.Layer_in_LayerMask(hit_Hip.collider.gameObject.layer, groundLayer.Value) && hit_Hip.collider.transform.SameHierarchy(transform))
                    { MDebug.LogWarning($"The Internal Collider [{hit_Hip.collider}] is on the Ground Layer Mask. Please change the Layer of the gameobject", hit_Hip.collider); }

                    MainRay = true;

                    if (debugGizmos)
                    {
                        MDebug.DrawRay(hit_Hip.point + GizmoDeltaPos, 0.2f * ScaleFactor * hit_Hip.normal, Color.green);
                        MDebug.DrawWireSphere(hipPoint + GizmoDeltaPos + -Up * (hit_Hip.distance - RayCastRadius), Color.green, RayCastRadius * ScaleFactor);
                    }

                    SetPlatform(hit_Hip.transform);               //Platforming logic

                    AddForceToGround(hit_Hip.collider, hit_Hip.point);


                    //If there's no Front Ray but we did find a Hip Ray, so save the hit chest
                    if (!FrontRay)
                        hit_Chest = hit_Hip;

                }
                else
                {
                    MainRay = false;

                    SetPlatform(null);

                    if (FrontRay)
                    {
                        MovementAxis.z = 1; //Force going forward in case there's no Back Ray (HACK)
                        hit_Hip = hit_Chest;  //In case there's no Hip Ray
                        //MainRay = true; //Fake is Grounded even when the HOP Ray did not Hit .
                    }
                }
            }
            else
            {
                MainRay = FrontRay; //Just in case you dont have HIP RAY IMPORTANT FOR HUMANOID CHARACTERS
                hit_Hip = hit_Chest;  //In case there's no Hip Ray
            }

            //   Debug.Log($"hit_Hip {hit_Hip.distance}: hit_Chest {hit_Chest.distance}");
            if (ground_Changes_Gravity && hit_Hip.normal != Vector3.zero)
                Gravity = -hit_Hip.normal;


            CalculateSurfaceNormal();
        }

        public void ResetSlopeValues()
        {
            SlopeDirection = Vector3.zero;
            SlopeDirectionSmooth = Vector3.ProjectOnPlane(SlopeDirectionSmooth, UpVector);
            SlopeDirectionAngle = 0;
        }

        private void AddForceToGround(Collider collider, Vector3 point)
        {
            collider.attachedRigidbody?.AddForceAtPosition(Gravity * (RB.mass / 2), point, ForceMode.Force);
        }

        internal virtual void CalculateSurfaceNormal()
        {
            if (Has_Pivot_Hip)
            {
                Vector3 TerrainNormal;

                if (Has_Pivot_Chest)
                {
                    Vector3 direction = (hit_Chest.point - hit_Hip.point).normalized;
                    Vector3 Side = Vector3.Cross(UpVector, direction).normalized;
                    SurfaceNormal = Vector3.Cross(direction, Side).normalized;

                    TerrainNormal = SurfaceNormal;
                    SlopeNormal = SurfaceNormal;

                    if (!MainRay && FrontRay)
                    {
                        SurfaceNormal = hit_Chest.normal;
                    }
                }
                else
                {
                    SurfaceNormal = TerrainNormal = hit_Hip.normal;
                }

                TerrainSlope = Vector3.SignedAngle(TerrainNormal, UpVector, Right);
            }
            else
            {
                TerrainSlope = Vector3.SignedAngle(hit_Hip.normal, UpVector, Right);
                SurfaceNormal = UpVector;
            }
        }

        /// <summary>Align the Animal to Terrain</summary>
        /// <param name="align">True: Aling to Surface Normal, False: Align to Up Vector</param>
        public virtual void AlignRotation(bool align, float time, float smoothness)
        {
            AlignRotation(align ? SurfaceNormal : UpVector, time, smoothness);
        }

        /// <summary>Align the Animal to a Custom </summary>
        /// <param name="align">True: Aling to UP, False Align to Terrain</param>
        public virtual void AlignRotation(Vector3 alignNormal, float time, float Smoothness)
        {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, Smoothness, time * AlignRotDelta * 4);

            Quaternion AlignRot = Quaternion.FromToRotation(Up, alignNormal) * Rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(Rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * AlignRotLerpDelta); //Calculate the Delta Align Rotation

            Rotation *= Delta;
            //AdditiveRotation *= Delta;
        }

        public virtual void AlignRotation(Vector3 from, Vector3 to, float time, float Smoothness)
        {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, Smoothness, time * AlignRotDelta * 4);

            Quaternion AlignRot = Quaternion.FromToRotation(from, to) * Rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(Rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * AlignRotLerpDelta); //Calculate the Delta Align Rotation

            Rotation *= Delta;
            //AdditiveRotation *= Delta;
        }

        /// <summary>Snap to Ground with Smoothing</summary>
        internal void AlignPosition(float time)
        {
            if (!MainRay && !FrontRay) return;         //DO NOT ALIGN  IMPORTANT This caused the animals jumping upwards when falling down
            AlignPosition(hit_Hip.distance, time);
        }

        // private float difference;

        internal void AlignPosition(float distance, float time)
        {
            float difference = Height - distance;

            if (!Mathf.Approximately(distance, Height))
            {
                AlignPosLerpDelta = Mathf.Lerp(AlignPosLerpDelta, AlignPosLerp * 2, time * AlignPosDelta);

                var DeltaDiference = Mathf.Lerp(0, difference, time * AlignPosLerpDelta);
                Vector3 align = Rotation * new Vector3(0, DeltaDiference, 0); //Rotates with the Transform to better alignment
                Position += align; //WORKS WITH THIS!! 

                // Debug.Log($"ALIGN : {DeltaDiference:F4} :   hit_Hip.distance {hit_Hip.distance:F2} Height {Height} - Dist {distance:F2} = {difference:F2}");
                hit_Hip.distance += DeltaDiference; //REMOVE the difference (PERFORMANCE!!!!!)

            }
        }

        /// <summary> Slope momevent when the slope is big or small and where there's a ground changer component  </summary>
        private void SlopeMovement()
        {
            SlopeAngleDifference = 0;

            float threshold;
            float slide;
            float slideDamp;

            if (InGroundChanger)
            {
                threshold = GroundChanger.SlideThreshold;
                slide = GroundChanger.SlideAmount;
                slideDamp = GroundChanger.SlideDamp;
            }
            else
            {
                //Restore the values
                threshold = slideThreshold;
                slide = slideAmount;
                slideDamp = this.slideDamp;
            }

            var Min = SlopeLimit - threshold;

            if (SlopeDirectionAngle > Min)
            {
                SlopeAngleDifference = (SlopeDirectionAngle - Min) / (SlopeLimit - Min);
                SlopeAngleDifference = Mathf.Clamp01(SlopeAngleDifference); //Clamp the Slope Movement so in higher angles does not get push that much
            }

            //Move in the direction of the Ground Normal, 
            if (Grounded)
                SlopeDirectionSmooth = Vector3.ProjectOnPlane(SlopeDirectionSmooth, SlopeNormal);

            SlopeDirectionSmooth = Vector3.SmoothDamp(
                    SlopeDirectionSmooth, slide * SlopeAngleDifference * SlopeDirection,
                    ref vectorSmoothDamp, DeltaTime * slideDamp);

            if (debugGizmos) MDebug.Draw_Arrow(Position + GizmoDeltaPos, SlopeDirectionSmooth * 2f, Color.yellow);

            if (SlopeDirectionSmooth != Vector3.zero)
                Position += SlopeDirectionSmooth;
        }

        private Vector3 vectorSmoothDamp = Vector3.zero;

        /// <summary>Snap to Ground with no Smoothing</summary>
        internal virtual void AlignPosition_Distance(float distance)
        {
            float difference = Height - distance;
            AdditivePosition += Rotation * new Vector3(0, difference, 0); //Rotates with the Transform to better alignment
        }


        /// <summary>Snap to Ground with no Smoothing</summary>
        public virtual void AlignPosition()
        {
            float difference = Height - hit_Hip.distance;

            //Debug.Log($"Difference: {difference} - hit_Hip.distance {hit_Hip.distance}");
            Position += Rotation * new Vector3(0, difference, 0); //Rotates with the Transform to better alignment
            InertiaPositionSpeed = Vector3.ProjectOnPlane(RB.linearVelocity * DeltaTime, UpVector);
            ResetUPVector(); //IMPORTANT!
        }
        #endregion

        /// <summary> Try Activate all other states </summary>
        protected virtual void TryActivateState()
        {
            if (ActiveState.IsPersistent) return;        //If the State cannot be interrupted the ignored trying activating any other States
            if (Mode_PersistentState) return;             //The Modes are not allowing the States to Change
            if (JustActivateState) return;               //Do not try to activate a new state since there already a new one on Activation

            foreach (var trySt in states)
            {
                if (trySt == ActiveState) continue;      //Skip Re-Activating yourself

                if (ActiveState.IgnoreLowerStates && ActiveState.Priority > trySt.Priority) continue; //Do not check lower priority states

                if ((trySt.UniqueID + CurrentCycle) % trySt.TryLoop != 0) continue;     //Check the Performance Loop for the  trying state

                // Debug.Log($"trySt.name {trySt.name}");

                if (!ActiveState.IsPending && ActiveState.CanExit)                      //Means a new state can be activated
                {
                    if (trySt.Active &&
                        !trySt.OnEnterCoolDown &&
                        !trySt.IsSleep &&
                        !trySt.OnQueue &&
                        !trySt.OnHoldByReset &&
                         trySt.InternalTryActivate() && trySt.TryOverride
                         )
                    {
                        trySt.Activate();
                        break;
                    }
                }
            }
        }

        /// <summary>Check if the Active State can exit </summary>
        protected virtual void TryExitActiveState()
        {
            if (ActiveState.CanExit && !ActiveState.IsPersistent)
                ActiveState.TryExitState(DeltaTime);     //if is not in transition and is in the Main Tag try to Exit to lower States

            ActiveState.AutoExitConditions(); //Check the Auto Exit Conditions

        }


        //private bool JustAnimatorMove;
        protected virtual void OnAnimatorMove()
        {
            OnAnimalMove();
        }


        protected virtual void OnAnimalMove()
        {
            CurrentCycle = (CurrentCycle + 1) % 999999999;

            DeltaTime = Anim.updateMode == AnimatorUpdateMode.Fixed ?
              Time.fixedDeltaTime
                 : Time.deltaTime
                 ;

            DeltaPos = Position - LastPosition + DeltaPlatformPos;                    //DeltaPosition from the last frame

            // GizmoDeltaPos = DeltaPos;

            if (Sleep || InTimeline)
            {
                Anim.ApplyBuiltinRootMotion();
                return;
            }

            CacheAnimatorState();
            ResetValues();

            if (ActiveState == null) return;

            Anim.speed = AnimatorSpeed * TimeMultiplier;

            DeltaTime = Anim.updateMode == AnimatorUpdateMode.Fixed ?
               Time.fixedDeltaTime
                  : Time.deltaTime
                  ;

            // Debug.Log($"DeltaTime: {DeltaTime:F4}");

            PreInput?.Invoke(this);             //Check the Pre State Movement on External Scripts

            ActiveState.InputAxisUpdate();      //States will calculate the Input State, States can override the default values.
            ActiveState.SetCanExit();           //Check if the Active State can Exit to a new State (Was not Just Activated or is in transition)

            PreStateMovement?.Invoke(this);             //Check the Pre State Movement on External Scripts

            ActiveState.OnStatePreMove(DeltaTime);          //Call before the Target is calculated After the Input

            SetTargetSpeed();

            MoveRotator();

            AdditionalSpeed(DeltaTime);

            if (UseAdditiveRot)
                AdditionalRotation(DeltaTime);


            //Update the State Profile if is different
            if (ActiveState_Profile != ActiveState.StateProfile) Update_StateProfile();


            ActiveState.OnStateMove(DeltaTime);                                                     //UPDATE THE STATE BEHAVIOUR

            ApplyExternalForce();

            if (IsPlayingMode)
                ActiveMode.OnAnimatorMove(DeltaTime); //Do Charged Mode AND MODIFIERS


            var PosBeforePlatform = Position;

            PlatformMovement(); //This needs to be calculated first!!! 

            if (!GroundedLogic())
            {
                MainRay = FrontRay = false;
                SurfaceNormal = UpVector;

                //Use is also if there's a residual Slope movement
                SlopeMovement();

                //Reset the PosLerp
                AlignPosLerpDelta = 0;
                AlignRotLerpDelta = 0;

                if (!UseCustomRotation)
                    AlignRotation(false, DeltaTime, AlignRotLerp); //Align to the Gravity Normal
                TerrainSlope = 0;

                GravityLogic();
            }

            PostStateMovement?.Invoke(this); // Check the Post State Movement on External Scripts

            TryExitActiveState();
            TryActivateState();
            MovementSystem();

            if (float.IsNaN(AdditivePosition.x)) return;

            //Clear Y Movement
            if (ActiveMode != null && ActiveMode.ActiveAbility.NoYMovement)
            {
                AdditivePosition = Vector3.ProjectOnPlane(AdditivePosition, UpVector);
            }

            if (!DisablePosition)
            {
                if (RB)
                {
                    if (Anim.updateMode == AnimatorUpdateMode.Normal)
                    {
                        RB.isKinematic = true;

                        //Position += CollideAndSlide(AdditivePosition * TimeMultiplier, Main_Pivot_Point, 3, AdditivePosition); //COLIDE AND SLIDE AFTER 
                        Position += AdditivePosition * TimeMultiplier;

                    }
                    else if (Anim.updateMode == AnimatorUpdateMode.Fixed)
                    {
                        if (RB.isKinematic)
                        {
                            Position += AdditivePosition * TimeMultiplier;
                        }
                        else
                        {
                            DesiredRBVelocity = (AdditivePosition / DeltaTime) * TimeMultiplier;
                            RB.linearVelocity = DesiredRBVelocity;
                        }
                    }
                }
                else
                {
                    Position += AdditivePosition * TimeMultiplier;
                }
            }

            // if (additivePosLog) Debug.Log($"ADDITIVE POSITION APPLIED {AdditivePosition}");

            if (!DisableRotation)
            {
                Rotation *= AdditiveRotation;
                Strafing_Rotation();
            }

            UpdateAnimatorParameters();              //Set all Animator Parameters

            LastPosition = Position;

            // LastPosition -= Position - PosBeforePlatform;

            additivePosition = Vector3.zero;
            additiveRotation = Quaternion.identity;
        }

        private bool GroundedLogic()
        {
            if (Grounded && !Mode_IgnoreGrounded)
            {
                SlopeMovement(); //Before Raycasting so the Raycast is calculated correclty

                if (AlignCycle.Value <= 1 || (AlignUniqueID + CurrentCycle) % AlignCycle.Value == 0)
                    AlignRayCasting();

                AlignPosition(DeltaTime);

                if (!UseCustomRotation)
                    AlignRotation(UseOrientToGround, DeltaTime, AlignRotLerp);


                return true;
            }

            return false;
        }

        /// <summary> Resets Additive Rotation and Additive Position to their default</summary>
        void ResetValues()
        {
            //The animator might be set to UPDATE(0) due to the IK which breaks the RootMotion
            if (Anim.deltaPosition == Vector3.zero && Anim.deltaRotation == Quaternion.identity)
            {
                return;
            }

            var deltaTime = Anim.updateMode == AnimatorUpdateMode.Normal ? Time.deltaTime : Time.fixedDeltaTime;


            DeltaRootMotion = RootMotion && GroundRootPosition ? (Anim.deltaPosition * CurrentSpeedSet.RootMotionPos) :
                Vector3.Lerp(DeltaRootMotion, Vector3.zero, currentSpeedModifier.lerpAnimator * deltaTime);

            // DeltaRootMotion = Vector3.zero;

            //IMPORTANT USE THE SLOPE IF the Animal uses only one Pivot
            if (Grounded && Has_Pivot_Chest && !Has_Pivot_Hip)
                DeltaRootMotion = Quaternion.FromToRotation(Up, SlopeNormal) * DeltaRootMotion;


            AdditivePosition = DeltaRootMotion * TimeMultiplier;


            // AdditivePosition = RootMotion ? Anim.deltaPosition : Vector3.zero;
            AdditiveRotation = RootMotionRotation ?
                Quaternion.Slerp(Quaternion.identity, Anim.deltaRotation, CurrentSpeedSet.RootMotionRot) :
                Quaternion.identity;

            //  DeltaPos = t.position - LastPos;                    //DeltaPosition from the last frame

            //  Debug.Log($"DeltaPos : {DeltaPos.magnitude/DeltaTime:F3} ");

            //CurrentCycle = (CurrentCycle + 1) % 999999999;

            if (RB)
            {
                var DeltaRB = RB.linearVelocity * DeltaTime;
                DeltaVelocity = DeltaRB; //When is not grounded take the Up Vector this is the one!!!
            }
            else DeltaVelocity = DeltaPos;
        }

        #region Inputs 
        /// <summary> Calculates the Movement Axis from the Input or Direction </summary>
        internal void InputAxisUpdate()
        {
            if (Rotate_at_Direction)
            {
                if (MainCamera && UseCameraInput)
                    MoveFromDirection(RawRotateDirAxis);
            }
            else if (UseRawInput)
            {
                //override the Forward Input if the State or Always Forward is set
                if (AlwaysForward || ActiveState.AlwaysForward.Value)
                    RawInputAxis.z = 1;

                var inputAxis = RawInputAxis;

                inputAxis.Scale(LockMovementAxis);

                if (LockMovement || Sleep)
                {
                    MovementAxis = Vector3.zero;
                    return;
                }

                if (MainCamera && UseCameraInput)
                {
                    MoveWithCameraInput(inputAxis);
                }
                else
                {
                    MoveWorld(inputAxis);
                }
            }
            else //Means that is Using a Direction Instead 
            {
                MoveFromDirection(RawInputAxis);
            }
        }

        /// <summary> Convert the Camera View to Forward Direction </summary>
        private void MoveWithCameraInput(Vector3 inputAxis)
        {
            // if (MovementDone) return; //This was already called

            //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
            var Cam_Forward = Vector3.ProjectOnPlane(MainCamera.forward, UpVector).normalized;
            var Cam_Right = Vector3.ProjectOnPlane(MainCamera.right, UpVector).normalized;

            Vector3 UpInput;

            if (!FreeMovement)
            {
                UpInput = Vector3.zero;            //Reset the UP Input in case is on the Ground
            }
            else
            {
                if (UseCameraUp)
                {
                    var angle = Vector3.SignedAngle(MainCamera.up, Vector3.up, MainCamera.right);

                    angle = Mathf.Clamp((angle / 90) * CurrentSpeedSet.UpDownMult.Value, -1, 1);
                    UpInput = (inputAxis.y * LockMovementAxis.y * UpVector); //Input addition
                    UpInput += angle * inputAxis.z * UpVector;

                }
                else
                {
                    UpInput = (inputAxis.y * LockMovementAxis.y * UpVector);
                }
            }

            var m_Move = (inputAxis.z * Cam_Forward) + (inputAxis.x * Cam_Right) + UpInput;

            MoveFromDirection(m_Move);
        }


        /// <summary>Get the Raw Input Axis from a source</summary>
        public virtual void SetInputAxis(Vector3 inputAxis)
        {
            UseRawInput = true;
            RawInputAxis = inputAxis;// + AdditiveRawInputAxis; // Store the last current use of the Input
            if (UsingUpDownExternal)
                RawInputAxis.y = UpDownAdditive; //Add the UPDown Additive from the Mobile.

            // Debug.Log("HERE");
        }

        public virtual void SetInputAxis(Vector2 inputAxis) => SetInputAxis(new Vector3(inputAxis.x, 0, inputAxis.y));

        public virtual void SetInputAxisXY(Vector2 inputAxis) => SetInputAxis(new Vector3(inputAxis.x, inputAxis.y, 0));

        public virtual void SetInputAxisYZ(Vector2 inputAxis) => SetInputAxis(new Vector3(0, inputAxis.x, inputAxis.y));

        private float UpDownAdditive;

        /// <summary> Up Down External Axis</summary>
        private bool UsingUpDownExternal;

        /// <summary>Use this for Custom UpDown Movement</summary>
        public virtual void SetUpDownAxis(float upDown)
        {
            UpDownAdditive = upDown;
            UsingUpDownExternal = true;
            SetInputAxis(RawInputAxis); //Call the Raw IMPORTANT
        }

        /// <summary>Gets the movement from the World Coordinates</summary>
        /// <param name="move">World Direction Vector</param>
        public virtual void MoveWorld(Vector3 move)
        {
            //  if (MovementDone) return; //This was already called

            UsingMoveWithDirection = false;

            if (!UseSmoothVertical && move.z > 0) move.z = 1;                   //It will remove slowing Stick push when rotating and going Forward

            Move_Direction = t.TransformDirection(move).normalized;    //Convert from world to relative IMPORTANT
            SetMovementAxis(move);
        }

        public virtual void SetMovementAxis(Vector3 move)
        {
            //MovementAxisRaw.z *= ForwardMultiplier;

            MovementAxisRaw = move;

            MovementAxis = MovementAxisRaw;
            MovementDetected = MovementAxisRaw != Vector3.zero;

            //   MovementAxis.Scale(LockMovementAxis);
            MovementAxis.Scale(ActiveState.MovementAxisMult);

            // Debug.Log($"MovementAxis {MovementAxis}");

            //  MovementDone = true;
        }

        // private bool MovementDone;

        /// <summary>Gets the movement values from a Direction</summary>
        /// <param name="move">Direction Vector</param>
        public virtual void MoveFromDirection(Vector3 move)
        {
            if (LockMovement)
            {
                MovementAxis = Vector3.zero;
                return;
            }


            //??
            if (LockForwardMovement) move = Vector3.Project(move, MainCamera.forward);
            if (LockHorizontalMovement) move = Vector3.Project(move, MainCamera.right);


            //If the State use KeepForward then ignore when the movement is Zero. Use the last one
            if (ActiveState.KeepForwardMovement && move == Vector3.zero)
            {
                move = Move_Direction;
            }


            UsingMoveWithDirection = true;

            if (move.magnitude > 1f) move.Normalize();

            var UpDown = FreeMovement ? move.y : 0; //Ignore UP Down Axis when the Animal is not on Free movement

            if (!FreeMovement)
                move = Quaternion.FromToRotation(UpVector, SlopeNormal) * move;    //Rotate with the ground Surface Normal. CORRECT!

            Move_Direction = move;

            if (debugGizmos)
            {
                MDebug.Draw_Arrow(Position + GizmoDeltaPos, Move_Direction.normalized * 2, Color.yellow);


                MDebug.DrawRay(Position, SlopeNormal, Color.black); //REAL TRANSFORM POS
                MDebug.DrawRay(Position + GizmoDeltaPos, SlopeNormal, Color.black);
            }
            move = t.InverseTransformDirection(move);               //Convert the move Input from world to Local  

            float turnAmount = Mathf.Atan2(move.x, move.z);                 //Convert it to Radians
            float forwardAmount = move.z < 0 ? 0 : move.z;

            if (!Strafe)
            {
                DeltaAngle = MovementDetected ? turnAmount * Mathf.Rad2Deg : 0;

                if (Mathf.Approximately(DeltaAngle, float.NaN)) DeltaAngle = 0f; //Remove the NAN Bug

                if (Mathf.Abs(Vector3.Dot(Move_Direction, UpVector)) == 1)//Remove turn Mount when its goinf UP/Down
                {
                    turnAmount = 0;
                    DeltaAngle = 0f;
                }

                inTurnLimit = Mathf.Abs(DeltaAngle) > (TurnLimit); //Calculate if the Animal is in a turn Limit

                if (!UseRawInput && inTurnLimit) //Meaning is using Move from AI and not from Input
                {
                    forwardAmount = 0; //This will make the animal turn in place if it circling around a target
                }
                else if (!UseSmoothVertical) //It will remove slowing Stick push when rotating and going Forward
                {
                    forwardAmount = Mathf.Abs(move.z);
                    forwardAmount = forwardAmount > 0 ? 1 : forwardAmount;
                    inTurnLimit = false;
                }
                else
                {
                    if (!inTurnLimit || VerticalSmooth > 1) //If the animal is not moving allow him to turn around 
                    {
                        forwardAmount = Mathf.Clamp01(Move_Direction.magnitude);
                    }
                    else
                    {

                        if (MovementDetected && UpDownSmooth != 0)
                        {
                            forwardAmount = Mathf.Clamp01(Move_Direction.magnitude);
                        }
                    }
                }

                if (Rotate_at_Direction) forwardAmount = 0;

                var MovAxis = new Vector3(turnAmount, UpDown, forwardAmount);
                SetMovementAxis(MovAxis);
            }
            else
            {
                StrafeWithDirection(UpDown);
            }
        }

        private bool inTurnLimit;
        private void StrafeWithDirection(float UpDown)
        {
            var Dir = Vector3.ProjectOnPlane(Aimer.RawAimDirection.normalized, UpVector);
            var M = Move_Direction;
            var cross = Quaternion.AngleAxis(90, UpVector) * Aimer.RawAimDirection;
            var turnAmount = Vector3.Dot(cross, M);
            var forwardAmount = Vector3.Dot(Dir, M);

            if (debugGizmos)
            {
                MDebug.DrawRay(Position + GizmoDeltaPos, Dir * 2, Color.cyan);
                MDebug.DrawRay(Position + GizmoDeltaPos, cross * 2, Color.green);
            }

            DeltaAngle = Mathf.MoveTowards(DeltaAngle, 0f, DeltaTime * 2);

            var MovAxis = new Vector3(turnAmount, UpDown, forwardAmount).normalized;
            SetMovementAxis(MovAxis);
        }

        /// <summary>Gets the movement from a Direction but it wont fo forward it will only rotate in place</summary>
        public virtual void RotateAtDirection(Vector3 direction)
        {
            if (IsPlayingMode && !ActiveMode.AllowRotation) return;
            RawRotateDirAxis = direction; // Store the last current use of the Input
            UseRawInput = false;
            Rotate_at_Direction = true;
        }
        #endregion

        private void Strafing_Rotation()
        {
            if (Strafe && Aimer)
            {
                if (m_StrafeLerp > 0)
                {
                    StrafeDeltaValue = Mathf.Lerp(StrafeDeltaValue,
                    MovementDetected ? ActiveState.MovementStrafe * ActiveStance.MovementStrafe : ActiveState.IdleStrafe * ActiveStance.IdleStrafe,
                    DeltaTime * m_StrafeLerp);
                    Rotation *= Quaternion.Euler(0, Aimer.HorizontalAngle_Raw * StrafeDeltaValue, 0);
                }
                else
                {
                    Rotation *= Quaternion.Euler(0, Aimer.HorizontalAngle_Raw, 0);
                }
            }
            else
            {
                StrafeDeltaValue = 0; //Reset Strafe Delta value
            }
        }



        /// <summary> Do the Gravity Logic </summary>
        public void GravityLogic()
        {
            if (UseGravity && !Mode_IgnoreGravity && !Grounded)
            {
                GravityStoredVelocity = StoredGravityVelocity();

                if (ClampGravitySpeed > 0 &&
                    (ClampGravitySpeed * ClampGravitySpeed) < GravityStoredVelocity.sqrMagnitude)
                {
                    GravityTime--; //Clamp the Gravity Speed
                    GravityStoredVelocity = GravityStoredVelocity.normalized * ClampGravitySpeed;
                }

                AdditivePosition += (DeltaTime * GravityExtraPower * GravityStoredVelocity) //Add Gravity if is in use
                                     + GravityOffset * DeltaTime;            //Add Gravity Offset JUMP if is in use

                // GravityResult += GravityOffset * DeltaTime;                  
                //AdditivePosition += GravityResult; //Add the Gravity to the Additive Position

                GravityTime++;
            }
        }

        // private Vector3 GravityResult;

        internal Vector3 StoredGravityVelocity()
        {
            var GTime = DeltaTime * GravityTime;
            return (GTime * GTime / 2) * GravityPower * ScaleFactor * TimeMultiplier * Gravity;


        }
        //int maxBounces = 5;
        ////  float skinWidth = 0.015f;
        //float radius = 0.5f;

        //private Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, int depth, Vector3 velInit)
        //{
        //    if (MainCollider == null) return Vector3.zero;

        //    if (depth >= maxBounces)
        //        return Vector3.zero;

        //    float dist = vel.magnitude;// + skinWidth;

        //    //MDebug.DrawWireSphere(pos, Color.green, radius, 0, 72);

        //    var CapsuleDir = MainCollider.direction == 0 ? Vector3.right : MainCollider.direction == 1 ? Vector3.up : Vector3.forward;
        //    var point1 = pos + CapsuleDir * (MainCollider.height / 2);
        //    var point2 = pos - CapsuleDir * (MainCollider.height / 2);

        //    if (Physics.CapsuleCast(point1, point2, MainCollider.radius, vel.normalized, out var hit, dist, GroundLayer, QueryTriggerInteraction.Ignore))
        //    {
        //        //Vector3 snapToSurface = vel.normalized * (hit.distance * skinWidth);
        //        Vector3 snapToSurface = Vector3.zero;
        //        Vector3 leftOver = vel - snapToSurface;
        //        //  float angle = Vector3.Angle(UpVector, hit.normal);

        //        //if (snapToSurface.magnitude <= skinWidth)  snapToSurface = Vector3.zero;


        //        //if (angle <= SlopeLimit || !Grounded)
        //        //{
        //        //    leftOver = ProjectAndScale(hit, leftOver);
        //        //}
        //        // if (Grounded) //Slide smoothly on a wall
        //        {
        //            float scale = 1 - Vector3.Dot(new Vector3(hit.normal.x, 0, hit.normal.z).normalized, -new Vector3(velInit.x, 0, velInit.z).normalized);
        //            leftOver = ProjectAndScale(hit, leftOver) * (Grounded ? scale : 1);
        //        }

        //        return snapToSurface + CollideAndSlide(leftOver, pos + snapToSurface, depth + 1, velInit);
        //    }

        //    return vel;
        //}

        //private static Vector3 ProjectAndScale(RaycastHit hit, Vector3 leftOver)
        //{
        //    float magn = leftOver.magnitude;
        //    leftOver = Vector3.ProjectOnPlane(leftOver, hit.normal).normalized;
        //    return leftOver * magn;
        //}
    }
}
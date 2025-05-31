using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddTypeMenu("Jump/Basic Jump")]
    public class JumpBasic : State
    {
        //public override string StateName => "Jump/Basic Jump";
        public override string StateIDName => "Jump";

        private int JumpEndHash = Animator.StringToHash("JumpEnd");
        private int JumpStartHash = Animator.StringToHash("JumpStart");
        //private int DoubleJumpHash = Animator.StringToHash("DoubleJump");


        [Header("Jump Parameters")]
        //[Tooltip("Can the Animal be Rotated while Jumping?")]
        //public BoolReference AirControl = new(true);
        [Tooltip("How much Rotation the character can do while Jumping")]
        public FloatReference AirRotation = new(10);
        [Tooltip("How much movement is allowed when the character is on the air")]
        public FloatReference AirMovement = new(5);
        [Tooltip("Smooth Value for Changing Speed Movement on the Air")]
        public FloatReference AirSmooth = new(5);

        [Tooltip("Keep the  Up Inertia from platforms or going upwards to keep the height of the jump")]
        public BoolReference KeepUPIntertia = new(true);

        [Tooltip("Amount of jumps the character can do (Double and Triple Jumps)")]
        public IntReference Jumps = new(1);

        [Tooltip("Time needed to use the Double Jump again, so you don't have jumps to close to each other")]
        public FloatReference DoubleJumpTime = new(0.3f);


        [Tooltip("The Jump can be interrupted if a ground is found in the middle of the jump. This is the multiplier to cast the Ray using the Animal Height.")]
        // [Range(0,99)]
        public FloatReference JumpInterruptRay = new(0.9f);

        //[Tooltip("For Multiple Jumps, time needed to activate the next jump logic")]
        //public FloatReference JumpTime = new FloatReference(0.3f);

        ///// <summary>If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing</summary>
        //[Space, Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        //public BoolReference JumpPressed = new(false);
        [Tooltip("Lerp Value for Pressing Jump. THis will smooth out exiting the height of the jump ")]
        public FloatReference JumpPressedLerp = new(5);

        [Space, Tooltip("Which states will reset the Jump Count. Idle and Locomotion are set by default. E.g. Swim can Reset the Jump State and the character can jump while swimming ")]
        public List<StateID> ResetJump;

        public List<JumpBasicProfile> profiles = new();
        public List<JumpDoubleProfile> multipleJumps = new();


        private JumpBasicProfile activeJump;
        protected MSpeed JumpSpeed;

        /// <summary>  This allows the Jump Logic to occur, its activated by the  </summary>
        private bool IsDoubleJump;

        /// <summary> Activate Jump Logic (Done by Animator or by Code)  </summary>
        private bool ActivateJumpLogic;
        //{
        //    get => activateJumpLogic;
        //    set
        //    {
        //        activateJumpLogic = value;
        //       //Debug.Log("activateJumpLogic = " + activateJumpLogic);
        //    }
        //}
        //private bool activateJumpLogic;

        public override bool InputValue
        {
            get => base.InputValue;
            set
            {
                base.InputValue = value;
                if (!value) { CanMultipleJump = true; } //This allows to Double jump after the input is released!
            }
        }

        public Vector3 StartingSpeedDirection { get; private set; }

        public override bool KeepForwardMovement => !activeJump.AirControl.Value;

        public int JumpsPerformanced { get; set; }
        //{
        //    get => jumpsPerformanced;
        //    set
        //    {
        //        jumpsPerformanced = value;
        //         Debug.Log(" BASIC JumpsPerformanced = " + jumpsPerformanced);
        //    }
        //}
        //private int jumpsPerformanced;


        //private int GravityTime = 0;
        /// <summary>  Used on the Pressed feature so it cannot be pressed again on the middle </summary>
        private bool justJumpPressed;

        private float StartedJumpLogicTime;
        private float JumpStartTime;
        private float JumpPressHeight_Value = 1;


        /// <summary>  Only Activate the Multiple jump if the Input is False (meaning the Input was released ) </summary>
        public bool CanMultipleJump { get; internal set; }
        //{
        //    get => canMultipleJump;
        //    set
        //    {
        //        canMultipleJump = value;
        //         Debug.Log(" canMultipleJump = " + canMultipleJump);
        //    }
        //}
        //private bool canMultipleJump;

        public override void AwakeState()
        {
            base.AwakeState();
            activeJump = profiles[0]; //set the first profile

            if (EnterTagHash != 0) JumpStartHash = EnterTagHash;
            if (ExitTagHash != 0) JumpEndHash = ExitTagHash;
        }


        /// <summary> Restore Internal values on the State (DO NOT INLCUDE Animal Methods or Parameter calls here</summary>
        public override void ResetStateValues()
        {
            JumpPressHeight_Value = 1;
            ActivateJumpLogic = false;
            justJumpPressed = false;
            StartedJumpLogicTime = 0;
            // JumpStartTime = Time.time;
            IsDoubleJump = false;
            StartingSpeedDirection = Vector3.zero;
            //Debugging("Reset Jump Values");
        }


        //Do not use the Try Activate Jump is a non Try State 
        // public override bool TryActivate() => false;

        //I use these for pressing and jumping???
        public override bool TryActivate()
        {
            if (animal.LockInput) return false; //Parche
            return InputValue && (JumpsPerformanced < Jumps) && CanMultipleJump;
        }

        public override void StatebyInput()
        {
            if (InputValue && (JumpsPerformanced < Jumps) && CanMultipleJump) Activate();
        }

        public void ActivateJump()
        {
            //ResetStateValues();//??

            ActivateJumpLogic = true;
            justJumpPressed = true;
            animal.Grounded = false;

            // animal.ResetGravityValues();

            StartedJumpLogicTime = 0;
            JumpStartTime = Time.time;
            animal.GravityTime = activeJump.StartGravityTime;
            StartingSpeedDirection = animal.HorizontalVelocity;            //Store the Starting SpeedDirection

            Debugging("[Basic Jump] Activate JumpLogic");

            animal.platform = null;
            LastPlatform = null;
        }

        public override void Activate()
        {
            if (JumpsPerformanced < Jumps && MTools.ElapsedTime(JumpStartTime, DoubleJumpTime))
            {
                CanMultipleJump = false; //It Cannot multiple jump unless the input is released

                base.Activate();
                animal.State_SetFloat(0);

                JumpsPerformanced++;
                JumpStartTime = Time.time;

                General.Gravity = false;
                IsPersistent = true;                                //IMPORTANT!!!!! DO NOT ELIMINATE!!!!!  causing issues

                animal.currentSpeedModifier.animator = 1;
                animal.Gravity_ResetValues();                        //Reset the Gravity
                StartingSpeedDirection = animal.HorizontalVelocity;            //Store the Starting SpeedDirection
                FindJumpProfile();

            }

            LastPlatform = animal.platform;


            animal.UpInertia_Clear(); //Clear the Up Intertia
                                      // if (Vector3.Dot(animal.Inertia, animal.UpVector) > 0)
            if (animal.platform != null)
                animal.UpInertia_Store(); //This is used for Jumping on a platform to get the inertia from the platform
        }


        private Transform LastPlatform;

        /// <summary> Use this to lock jumps profiles and not use the Vertical to check which is the valid profile </summary>
        public void LockJumpProfile(int index)
        {
            index = Mathf.Clamp(index, 0, profiles.Count - 1);
            activeJump = profiles[index];
            animal.VerticalSmooth = activeJump.VerticalSpeed;
            animal.SetAnimParameter(animal.hash_Vertical, animal.VerticalSmooth);
        }

        private void FindJumpProfile()
        {
            activeJump = (profiles != null && profiles.Count > 0) ? profiles[0] : new JumpBasicProfile() { };

            foreach (var j in profiles)                          //Save/Search the Current Jump Profile by the Lowest Speed available
            {
                if (j.LastState == null)
                {
                    if (j.VerticalSpeed <= animal.VerticalSmooth) activeJump = j; //Find the Closest 
                }
                else
                {
                    if (j.VerticalSpeed <= animal.VerticalSmooth &&
                        j.LastState == animal.LastState.ID)
                    {
                        activeJump = j;
                    }
                }
            }


            var JumpNumber = 0;

            //if (!CanMultipleJump) return;

            //Search again if the Character can perform multiple jumps
            if (Jumps > 1 && JumpsPerformanced > 1 && multipleJumps != null && multipleJumps.Count > 0)
            {
                var PosibleJump = new JumpDoubleProfile() { JumpNumber = -1 };



                foreach (var j in multipleJumps)                          //Save/Search the Current Jump Profile by the Lowest Speed available
                {
                    if (JumpsPerformanced == j.JumpNumber) PosibleJump = j;
                }

                if (PosibleJump.JumpNumber != -1) //Means that  it found a Multiple Jump
                {
                    activeJump.name = PosibleJump.name;
                    activeJump.GravityPower = PosibleJump.GravityPower;
                    activeJump.Height = PosibleJump.Height;
                    activeJump.JumpTime = PosibleJump.JumpTime;
                    activeJump.StartGravityTime = PosibleJump.StartGravityTime;
                    activeJump.WaitForAnimation = PosibleJump.WaitForAnimation;
                    activeJump.AirControl = PosibleJump.AirControl;
                    JumpNumber = PosibleJump.JumpNumber;
                    IsDoubleJump = true;
                }
            }
            SetEnterStatus(JumpNumber);
        }


        public override void EnterCoreAnimation()
        {
            Debugging($"Jump Profile: [{activeJump.name}] Jumps <B> <color=orange>Performanced:[{JumpsPerformanced}]</color> </B>");
            JumpPressHeight_Value = 1;

            animal.ResetSlopeValues();

            var Speed = animal.HorizontalSpeed / ScaleFactor;

            if (activeJump.JumpInterruptRay == 0) activeJump.JumpInterruptRay = 1; //Make sure is set to 1 if is not configured

            //Debug.Log($"Speed: {Speed}");
            if (activeJump.ClampForward > 0) Speed = Mathf.Clamp(Speed, 0, activeJump.ClampForward); //Clamp the Speed Forward to avoid Acceleration
                                                                                                     // Debug.Log($"Speed After Clamp: {Speed}");
                                                                                                     //  Debug.Log($"Speed JUMP: {Speed}");
            if (animal.HasExternalForce)
            {
                var HorizontalForce = Vector3.ProjectOnPlane(animal.ExternalForce, animal.UpVector);    //Remove Horizontal Force
                var HorizontalInertia = Vector3.ProjectOnPlane(animal.Inertia, animal.UpVector);        //Remove Horizontal Force

                //Remove the Horizontal FORCE SPEED
                var HorizontalSpeed = HorizontalInertia - HorizontalForce;
                Speed = HorizontalSpeed.magnitude / ScaleFactor; //Remove the scaleFactor since it will be added later 
            }

            //Remove all Speed if the External Force does not allows it
            if (!animal.ExternalForceAirControl) Speed = 0;


            JumpSpeed = new MSpeed(animal.CurrentSpeedModifier) //Inherit the Vertical and the Lerps
            {
                name = $"Jump [{activeJump.name}] Speed",
                position = General.RootMotion ? 0 : Speed, //Inherit the Horizontal Speed you have from the last state
                strafeSpeed = General.RootMotion ? 0 : Speed,
                lerpPosition = AirSmooth.Value,
                lerpStrafe = AirSmooth.Value,
                rotation = AirRotation.Value,
                animator = 1,
            };

            animal.SetCustomSpeed(JumpSpeed, false);                     //Set the Current Speed to the Jump Speed Modifier

            animal.GravityExtraPower = activeJump.GravityPower.Value;    //Send the Gravity ExtraPower value.
            //Debug.Log($"animal.GravityExtraPower {animal.GravityExtraPower}");

            //Mean is doing a double jump!
            if (IsDoubleJump)
            {
                ActivateJump();
                animal.UpInertia_Clear(); //Clear the Up Intertia on Double Jump
            }
            //if it does not require to Wait for the Animator to call
            else if (!activeJump.WaitForAnimation)
            {
                ActivateJump();
            }
            else
            {
                var PlatLastPos = animal.Last_Platform_Pos;
                animal.SetPlatform(LastPlatform); //No Code Involved
                animal.Last_Platform_Pos = PlatLastPos;
            }
        }

        public override Vector3 Speed_Direction()
        {
            if (GizmoDebug)
                MDebug.Draw_Arrow(Position, StartingSpeedDirection * 2, Color.red);
            return activeJump.AirControl.Value ? base.Speed_Direction() : StartingSpeedDirection;
        }

        public override void EnterTagAnimation()
        {
            if (CurrentAnimTag == JumpStartHash && !animal.RootMotion)
            {
                var Speed = animal.HorizontalSpeed / ScaleFactor; //Remove the scaleFactor since it will be added later 
                if (animal.HasExternalForce)
                {
                    var HorizontalForce = Vector3.ProjectOnPlane(animal.ExternalForce, animal.UpVector);    //Remove Horizontal Force
                    var HorizontalInertia = Vector3.ProjectOnPlane(animal.Inertia, animal.UpVector);        //Remove Horizontal Force

                    var HorizontalSpeed = HorizontalInertia - HorizontalForce;
                    Speed = HorizontalSpeed.magnitude / ScaleFactor; //Remove the scaleFactor since it will be added later 
                }

                var JumpStartSpeed = new MSpeed(animal.CurrentSpeedModifier)
                {
                    name = "JumpStartSpeed",
                    position = Speed,
                    Vertical = animal.CurrentSpeedModifier.Vertical,
                    animator = 1,
                    rotation = activeJump.AirControl.Value ? (!animal.UseCameraInput ? AirRotation.Value : AirRotation.Value / 10f) : 0f,
                    strafeSpeed = Speed,
                    lerpStrafe = AirSmooth
                };

                Debugging("[EnterTag-JumpStart]");

                animal.SetCustomSpeed(JumpStartSpeed, false);       //Set the Current Speed to the Jump Speed Modifier
                if (animal.TerrainSlope > 0) animal.UseCustomRotation = true; //Means we are jumping uphill

            }
            else if (CurrentAnimTag == JumpEndHash)
            {
                Debugging("[EnterTag-JumpEnd]");
                AllowExit();
            }
        }

        public override void OnStateMove(float deltaTime)
        {
            if (InCoreAnimation)
            {
                StartedJumpLogicTime += deltaTime;

                if (ActivateJumpLogic)
                {
                    //Keep the Up Momentum
                    if (KeepUPIntertia.Value/* && Vector3.Dot(animal.UpInertia, animal.UpVector) > 0*/)
                    {
                        animal.UpInertia_Apply();
                    }

                    if (activeJump.JumpPressed.Value)
                    {
                        if (!InputValue) justJumpPressed = false;

                        JumpPressHeight_Value = Mathf.Lerp(JumpPressHeight_Value, (InputValue && justJumpPressed) ? 1 : 0, deltaTime * JumpPressedLerp);
                    }

                    if (activeJump.ForwardPush > 0)
                        animal.AdditivePosition += activeJump.ForwardPush * deltaTime * ScaleFactor * Forward;     //Forward Movement

                    if (AirMovement > CurrentSpeedPos && activeJump.AirControl)
                    {
                        if (!animal.ExternalForceAirControl) return;
                        CurrentSpeedPos = Mathf.Lerp(CurrentSpeedPos, AirMovement, (AirSmooth != 0 ? (deltaTime * AirSmooth) : 1));
                    }

                    //Height of the Jump
                    Vector3 ExtraJumpHeight = (UpVector * activeJump.Height.Value);
                    animal.AdditivePosition += deltaTime * JumpPressHeight_Value * ScaleFactor * ExtraJumpHeight;

                    //If we still can apply movement
                    if (StartedJumpLogicTime < activeJump.JumpTime)
                        IsPersistent = false; //Let other High Priority States to play

                    //Apply Fake Gravity
                    if (!animal.Mode_IgnoreGravity)
                    {

                        animal.AdditivePosition += activeJump.GravityPower.Value * deltaTime * animal.StoredGravityVelocity();
                        animal.GravityOffset = (JumpPressHeight_Value * ScaleFactor * ExtraJumpHeight);     //Send the Values of the Height to stored 
                        animal.GravityTime++;
                    }
                }
                else //Keep RootMotion
                {
                    if (!General.RootMotion)
                    {
                        animal.AdditivePosition += Vector3.Project(Anim.deltaPosition, Up);
                    }
                }
            }
        }

        //Use this to remove completely the Jump Height when the Jump is interrupted by forces.
        public virtual void NoJumpHeight() => JumpPressHeight_Value = 0;


        public override void TryExitState(float deltaTime)
        {
            if (!ActivateJumpLogic) return; //The Jump logic has not being activated yet

            // var GoingDown = Vector3.Dot(DeltaPos, Gravity) > 0; //Check if is falling down

            //  Debug.Log($"TryExitState  StartedJumpLogicTime: {StartedJumpLogicTime:F2}   GoingDown {GoingDown}");

            //Debug.DrawRay(animal.Main_Pivot_Point, Height * activeJump.JumpInterruptRay * Gravity, Color.white);

            //if (Physics.Raycast(animal.Main_Pivot_Point, Gravity, out var Hit, Height * activeJump.JumpInterruptRay, GroundLayer, IgnoreTrigger))
            //{
            //    if (Hit.distance < Height)
            //    {
            //        if (GoingDown)
            //        {
            //            //SUPER IMPORTANT!!! this is when the Animal is falling from a great height
            //            animal.Teleport_Internal(Hit.point);
            //            //var GroundedPos = Vector3.Project(Hit.point - animal.transform.position, Gravity);
            //            //animal.Teleport_Internal(animal.transform.position + GroundedPos);
            //            animal.ResetUPVector();
            //            Debugging("[Allow Exit] - Interrupt Ray Touched Ground FORCE ALIGMENT");
            //        }
            //        else
            //        {
            //            Debugging("[Allow Exit] - Interrupt Ray Touched Smooth align");
            //        }

            //        animal.Grounded = true; //IMPORTANT!
            //        AllowExit();
            //    }
            //}


            if (StartedJumpLogicTime >= activeJump.JumpTime)
            {
                AllowExit(3, 0); //Force Fall State
                Debugging("[Allow Exit]");
            }
        }

        /// <summary>Is called when a new State enters</summary>
        public override void NewActiveState(StateID newState)
        {
            if (newState.ID == ID) return; //Do nothing if this we are entering the same state

            //Reset all the jumps (Idle and Locomotion) and all the extra States required
            if (newState <= 1 || ResetJump.Contains(newState))
            {
                JumpsPerformanced = 0;          //Reset the amount of jumps performanced
                CanMultipleJump = true; //Can jump again
            }
            //If we were not jumping then increase the Double Jump factor when falling from locomotion
            else if (newState == StateEnum.Fall && animal.LastState && animal.LastState.ID != ID)
            {
                JumpsPerformanced++; //If we are in fall animation then increase a Jump perfomanced
            }
        }



#if UNITY_EDITOR
        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Fall is an automatic State, the Fall Speed is created internally
        }

        internal override void Reset()
        {
            base.Reset();
            Input = "Jump";

            SleepFromState = new List<StateID>() { /*MTools.GetInstance<StateID>("Fall"),*/ MTools.GetInstance<StateID>("Fly") };
            SleepFromMode = new List<ModeID>() { MTools.GetInstance<ModeID>("Action"), MTools.GetInstance<ModeID>("Attack1") };

            EnterTag.Value = "JumpStart";
            ExitTag.Value = "JumpEnd";

            General = new AnimalModifier()
            {
                RootMotion = false,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                CustomRotation = false,
                IgnoreLowerStates = true, //IMPORTANT!
                Persistent = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };

            // ExitFrame = false;

            profiles = new List<JumpBasicProfile>(1) { new ()
            {
                Height = new(6),
                GravityPower = new(1),
                JumpTime = 0.3f,
                VerticalSpeed = 0,
                name = "Default",
                StartGravityTime = 15,
                LastState = null,
            }
            };
        }
#endif
    }



    /// <summary>Different Jump parameters on different speeds</summary>
    [System.Serializable]
    public struct JumpBasicProfile
    {
        [Tooltip("Name to identify the Jump Profile")]
        public string name;

        //  [Header("Conditions")]
        [Tooltip("Last State the animal was before making the Jump")]
        public StateID LastState;

        [Tooltip("Minimal Vertical Speed to Activate this Profile")]
        public float VerticalSpeed;

        [Tooltip("Extra Force to push the animal forward ??")]
        public float ForwardPush;

        //  [Header("Values")]
        [Tooltip("Duration of the Jump logic")]
        public float JumpTime;

        [Tooltip("How High the animal can Jump")]
        public FloatReference Height;

        [Tooltip("Multiplier for the Gravity")]
        public FloatReference GravityPower;

        [Tooltip("The Jump can be interrupted if a ground is found in the middle of the jump. This is the multiplier to cast the Ray using the Animal Height.")]
        public FloatReference JumpInterruptRay;

        [Tooltip("Higher value makes the Jump more Arcady")]
        public int StartGravityTime;

        [Tooltip("Can the Animal be controlled while is on the Air")]
        public BoolReference AirControl;

        [Tooltip("Wait for the Animation to Activate the Jump Logic\n Use [void ActivateJump()] on the Animator with a Messsage Behavior")]
        public bool WaitForAnimation;

        [Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        public BoolReference JumpPressed;


        [Tooltip("Clamp the Speed Forward when jumping (Jumping Often Causes the character to accelerate)")]
        public float ClampForward;
    }


    /// <summary>Different Jump parameters for Multiple Jumps</summary>
    [System.Serializable]
    public struct JumpDoubleProfile
    {
        [Tooltip("Name to identify the Jump Profile")]
        public string name;

        [Header("Double Jump")]
        [Tooltip("Multiple Jump Number (Is it a Double or a Triple Jump. Default is 2 for Double Jump. This is the Value for the [Enter State Status]")]
        public int JumpNumber;

        [Tooltip("Duration of the Jump logic")]
        public float JumpTime;

        [Tooltip("How High the animal can Jump")]
        public FloatReference Height;

        [Tooltip("Multiplier for the Gravity")]
        public FloatReference GravityPower;



        [Tooltip("Higher value makes the Jump more Arcady")]
        public int StartGravityTime;

        [Tooltip("Can the Animal be controlled while is on the Air")]
        public BoolReference AirControl;

        [Space, Tooltip("Wait for the Animation to Activate the Jump Logic\n Use [void ActivateJump()] on the Animator with a Messsage Behavior")]
        public bool WaitForAnimation;


    }
}

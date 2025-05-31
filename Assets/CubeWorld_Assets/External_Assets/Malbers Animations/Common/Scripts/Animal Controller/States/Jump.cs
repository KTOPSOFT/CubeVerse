using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddTypeMenu("Jump/Root Motion Jump")]
    public class Jump : State
    {
        //public override string StateName => "Jump/Root Motion Jump";
        public override string StateIDName => "Jump";

        /// <summary>If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing</summary>
        [Header("Jump Parameters")]
        [Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        public BoolReference JumpPressed;
        [Tooltip("Check if the Animal can control the Forward Movement of the Jump")]
        public BoolReference ForwardPressed;
        [Tooltip("Lerp Value for Pressing Jump. THis will smooth out exiting the height of the jump ")]
        public float JumpPressedLerp = 5;



        [Tooltip("Can the Animal be Rotated while Jumping?")]
        public BoolReference AirControl = new(true);
        [Tooltip("How much Rotation the Animal can do while Jumping")]
        public FloatReference AirRotation = new(10);


        [Tooltip("Amount of jumps the Animal can do (Double and Triple Jumps)")]
        public IntReference Jumps = new(1);

        [Space, Tooltip("How much Movement the Animal can do while Jumping")]
        public List<StateID> ResetJump = new();


        [Tooltip("It will use the Index of the jumps profiles instead of getting the Index from ")]
        public int lockJumpProfile = -1;



        private float JumpPressHeight_Value = 1;
        private float JumpPressForward = 1;
        private float JumpPressForwardAdditive = 0;

        [Tooltip("If the animal is going up a hill, the Jump will align to the Slope of the Terrain.")]
        public bool KeepHeightOnHighSlope = false;

#if UNITY_2020_3_OR_NEWER
        //  [NonReorderable]
#endif
        public List<JumpProfile> jumpProfiles = new();
        protected MSpeed JumpSpeed;

        protected bool OneCastingFall_Ray = false;

        /// <summary> Current Jump Profile</summary>
        protected JumpProfile activeJump;
        private RaycastHit JumpRay;

        //private bool CanJumpAgain;
        private Vector3 JumpStartDirection;
        public int JumpsPerformanced { get; set; }
        //{
        //    get => jumpsPerformanced;
        //    set
        //    {
        //        jumpsPerformanced = value;
        //        Debug.Log("jumpsPerformanced = " + jumpsPerformanced);
        //    }
        //}
        //private int jumpsPerformanced;

        // public override bool TryActivate() => false;



        /// <summary> Use this to lock jumps profiles and not use the Vertical to check which is the valid profile </summary>
        public void LockJumpProfile(int index)
        {
            index = Mathf.Clamp(index, 0, jumpProfiles.Count - 1);
            activeJump = jumpProfiles[index];
            animal.VerticalSmooth = activeJump.VerticalSpeed;
            animal.SetAnimParameter(animal.hash_Vertical, animal.VerticalSmooth);
        }

        public override bool TryActivate()
        {
            return InputValue && (JumpsPerformanced < Jumps);
        }

        public override void ResetStateValues()
        {
            // CanJumpAgain = true;
            JumpPressHeight_Value = 1;
            JumpPressForward = 1;
            JumpPressForwardAdditive = 0;
            OneCastingFall_Ray = false;

            // GoingDown = false;
        }

        public override void AwakeState()
        {
            if (string.IsNullOrEmpty(EnterTag)) EnterTag.Value = "JumpStart";
            if (string.IsNullOrEmpty(ExitTag)) ExitTag.Value = "JumpEnd";

            if (!CanTransitionToItself) CanTransitionToItself = true; //make sure CanTransition to itself is on

            base.AwakeState();
        }


        public override void Activate()
        {
            if (JumpsPerformanced < Jumps)
            {
                //meaning we are Transition from itself and the animation has not cover its 25% yet
                if (ActiveState == this && animal.StateTime < 0.25f) return;

                base.Activate();

                //Make sure while you are on Jump State above the list cannot check for Trying to activate State below him
                IgnoreLowerStates = true;

                animal.currentSpeedModifier.animator = 1;

                JumpsPerformanced++;

                SetEnterStatus(JumpsPerformanced); //Sent to the animator the Performanced Jump

                FindJumpProfile();
            }
        }

        public override void EnterTagAnimation()
        {
            if (CurrentAnimTag == EnterTagHash)
            {
                Debugging($"[EnterTag - {EnterTag.Value}]");

                animal.DeltaPos = Vector3.zero;

                if (!animal.RootMotion)
                {
                    var JumpStartSpeed = new MSpeed(animal.CurrentSpeedModifier)
                    {
                        name = "JumpStartSpeed",
                        position = animal.HorizontalSpeed,
                        Vertical = animal.CurrentSpeedModifier.Vertical.Value,
                        animator = 1,
                        rotation = AirRotation.Value,
                    };

                    animal.SetCustomSpeed(JumpStartSpeed, false);       //Set the Current Speed to the Jump Speed Modifier
                }

                JumpStartDirection = animal.Forward;

                if (animal.TerrainSlope > 0) //Means we are jumping uphill
                    animal.UseCustomRotation = true;
            }
            else if (CurrentAnimTag == ExitTagHash) //Do not do this on State ON Trigger 
            {
                CanExit = true;
                Debugging($"[Using ExitTag - {ExitTag.Value}] - Allow Exit");
                AllowExit();
            }
        }

        public override Vector3 Speed_Direction()
        {
            if (animal.HasExternalForce)
            {
                return Vector3.ProjectOnPlane(animal.ExternalForce, animal.UpVector);
            }
            else if (AirControl)
            {
                return base.Speed_Direction();
            }
            else
            {
                return JumpStartDirection;
            }
        }

        /// <summary> Make the Jump Start</summary>
        public override void EnterCoreAnimation()
        {
            Debugging($"[Enter Core Tag - [Jump]");

            animal.ResetSlopeValues();

            OneCastingFall_Ray = false;                                 //Reset Values IMPROTANT
            JumpPressHeight_Value = 1;
            JumpPressForward = 1;
            JumpPressForwardAdditive = 0;
            //IsPersistent = true;
            animal.UseGravity = false;
            animal.Gravity_ResetValues();

            JumpSpeed = new MSpeed(animal.CurrentSpeedModifier) //Inherit the Vertical and the Lerps
            {
                name = $"Jump [{activeJump.name}]",
                position = animal.RootMotion ? 0 : animal.HorizontalSpeed * activeJump.ForwardMultiplier, //Inherit the Horizontal Speed you have from the last state
                animator = 1,
                rotation = (AirRotation.Value),
                lerpPosAnim = JumpPressedLerp,
                lerpPosition = JumpPressedLerp,
            };


            animal.SetCustomSpeed(JumpSpeed);       //Set the Current Speed to the Jump Speed Modifier
            JumpStartDirection = animal.Forward;

            if (animal.TerrainSlope > 0 && animal.MovementDetected && KeepHeightOnHighSlope)    //Means we are jumping uphill HACK
                animal.UseCustomRotation = true;



            //Debug.Log($"isPersistent: {IsPersistent} ");
        }

        private void FindJumpProfile()
        {
            activeJump = jumpProfiles != null ? jumpProfiles[0] : new JumpProfile();

            var HasLastState = false;

            foreach (var jump in jumpProfiles)                          //Save/Search the Current Jump Profile by the Lowest Speed available
            {
                if (jump.LastState == null)
                {
                    if (jump.VerticalSpeed <= animal.VerticalSmooth && !HasLastState)
                    {
                        activeJump = jump;
                    }
                }
                else
                {
                    if (jump.VerticalSpeed <= animal.VerticalSmooth && jump.LastState == animal.LastState.ID)
                    {
                        activeJump = jump;
                        HasLastState = true;
                    }
                }
            }

            if (activeJump.CliffTime.minValue == 0 && activeJump.CliffTime.maxValue == 0)
            {
                activeJump.CliffTime = new RangedFloat(0.333f, 0.666f);
            }

            Debugging($"Jump Profile: <B><color=green>[{activeJump.name}]</color> - Speed [{activeJump.VerticalSpeed}] Current Speed: [{animal.VerticalSmooth:F2}]</B>");
        }

        public override void OnStateMove(float deltaTime)
        {
            if (InCoreAnimation)
            {
                if (animal.StateTime >= activeJump.fallingTime) IsPersistent = false;
                //Do this after the Fall time has passed! IMPORTANT!



                if (activeJump.JumpLandDistance == 0) return; //Meaning is a false Jump Like neigh on the Horse IMPORTANT!!!!

                if (JumpPressed)
                {
                    JumpPressHeight_Value = Mathf.MoveTowards(JumpPressHeight_Value, InputValue ? 1 : 0, deltaTime * JumpPressedLerp);
                }

                if (ForwardPressed)
                {
                    JumpPressForward = Mathf.MoveTowards(JumpPressForward, animal.MovementAxis.z, deltaTime * JumpPressedLerp);
                    JumpPressForwardAdditive = Mathf.MoveTowards(JumpPressForwardAdditive, animal.MovementAxis.z, deltaTime * JumpPressedLerp);
                }

                if (!General.RootMotion) //If the Jump is NOT Root Motion!!
                {
                    Vector3 ExtraJumpHeight = (animal.Up * activeJump.HeightMultiplier);
                    animal.AdditivePosition += deltaTime * JumpPressHeight_Value * ExtraJumpHeight;
                }
                else //If the Jump IS Root Motion!! ***********************************************************************
                {
                    Vector3 RootMotionUP = Vector3.Project(Anim.deltaPosition, animal.Up);         //Get the Up vector of the Root Motion Animation

                    bool isGoingUp = Vector3.Dot(RootMotionUP, animal.Up) > 0;  //Check if the Jump Root Animation is going  UP;

                    if (isGoingUp)
                    {
                        //Remove the default Root Motion Jump
                        animal.AdditivePosition -= RootMotionUP;

                        //Add the New Root Motion Jump scaled by the Height Multiplier 
                        animal.AdditivePosition +=
                            (activeJump.HeightMultiplier * JumpPressHeight_Value * RootMotionUP);
                    }

                    // Vector3 RootMotionForward = Vector3.ProjectOnPlane(Anim.deltaPosition, animal.Up);
                    Vector3 RootMotionForward = Anim.deltaPosition - RootMotionUP;

                    //Remove the default Root Motion Jump
                    animal.AdditivePosition -= RootMotionForward;

                    //If is cannot controlled in air?
                    if (!AirControl.Value)
                    {
                        //Add the New Root Motion Jump scaled by the Height Multiplier 
                        animal.AdditivePosition +=
                            activeJump.ForwardMultiplier * RootMotionForward.magnitude * JumpStartDirection;

                    }
                    else
                        //Add the New Root Motion Jump scaled by the Height Multiplier 
                        animal.AdditivePosition +=
                                 (activeJump.ForwardMultiplier * JumpPressForward * RootMotionForward) +
                                 (activeJump.ForwardPressed * deltaTime * JumpPressForwardAdditive * animal.Forward);
                }
            }
            else if (ExitTagHash == CurrentAnimTag) //meaning is on Exit Jump
            {
                if (!animal.CheckIfGrounded())
                {
                    animal.UseGravity = true;
                }
            }
        }


        public override void TryExitState(float DeltaTime)
        {
            if (animal.StateTime >= activeJump.fallingTime && !OneCastingFall_Ray)
            {
                Check_for_Falling();
            }

            if (activeJump.ExitTime >= activeJump.fallingTime && animal.StateTime >= activeJump.ExitTime)
            {
                Debugging($"[Allow Exit] - Exit Time [{activeJump.ExitTime}] ");
                animal.CheckIfGrounded();
                AllowExit(); //Using Exit Time Internally * New
            }

            CheckForGround(animal.StateTime);
        }


        private void CheckForGround(float normalizedTime)
        {
            if (activeJump.CliffLandDistance == 0) return; //Do nothing if RayLenght is zero

            if (activeJump.CliffTime.IsInRange(normalizedTime)) //Need to happen the First 1/3 of the Root Jump Animation
            {
                var RayLength = activeJump.CliffLandDistance * ScaleFactor;
                var MainPivot = animal.Main_Pivot_Point;

                if (m_debug)
                    Debug.DrawRay(MainPivot, -animal.Up * RayLength, Color.black, 0.1f);

                if (Physics.Raycast(MainPivot, -animal.Up, out JumpRay, RayLength, GroundLayer, IgnoreTrigger))
                {
                    if (m_debug) MDebug.DebugTriangle(JumpRay.point, 0.1f, Color.black);

                    var TerrainSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.SlopeLimit;

                    if (!DeepSlope)       //Jump to a jumpable cliff not an inclined one
                    {
                        Debugging($"[Allow Exit] Cliff Time - Near Ground. Normalized time: {normalizedTime:F2} ");
                        AllowExit();
                        animal.CheckIfGrounded();
                        //  Debug.Break();
                    }
                }
            }
        }

        /// <summary>Check if the animal can change to fall state if there's no future ground to land on</summary>
        private void Check_for_Falling()
        {
            AllowExit(); //Set the Allow exit just in case
            OneCastingFall_Ray = true;

            if (activeJump.JumpLandDistance == 0)
            {
                animal.CheckIfGrounded(); //We are still on the ground
                return;  //Meaning that is a False Jump (like Neigh on the Horse)
            }

            float RayLength = animal.ScaleFactor * activeJump.JumpLandDistance; //Ray Distance with the Scale Factor
            var MainPivot = animal.Main_Pivot_Point;
            var Direction = -animal.Up;


            if (activeJump.JumpLandDistance > 0) //greater than 0 it can complete the Jump on an even Ground 
            {
                if (m_debug)
                    Debug.DrawRay(MainPivot, Direction * RayLength, Color.red, 0.25f);

                if (Physics.Raycast(MainPivot, Direction, out JumpRay, RayLength, GroundLayer, IgnoreTrigger))
                {
                    Debugging($"Min Distance to complete <B>[{activeJump.name}]-> <Color=green>[[{JumpRay.distance:F4}]]</color></B>");
                    if (m_debug) MDebug.DebugTriangle(JumpRay.point, 0.1f, Color.yellow);

                    var GroundSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);

                    if (GroundSlope > animal.SlopeLimit)     //if we found something but there's a deep slope
                    {
                        Debugging($"[AllowExit] Try to Land but the Sloope was too Deep. Slope: {GroundSlope:F2}");
                        animal.UseGravity = General.Gravity;
                        return;
                    }


                    if (JumpRay.distance < animal.Height) //Meaning is near the ground
                    {
                        Debugging($"Allow Exit, Near the Ground  Jump Distance: [{JumpRay.distance}]. Height {animal.Height}");
                        AllowExit();
                        return;
                    }

                    IgnoreLowerStates = true;                           //Means that it can complete the Jump Ignore Fall Locomotion and Idle
                    Debugging($"Can finish the Jump. Going to Jump End");
                }
                else
                {
                    animal.UseGravity = General.Gravity;
                    Debugging($"[Allow Exit] - <B>Jump [{activeJump.name}] </B> Go to Fall..No Ground was found");
                }
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
                OnHoldByReset = false;
            }
            //If we were not jumping then increase the Double Jump factor when falling from locomotion
            else if (newState == StateEnum.Fall && animal.LastState && animal.LastState.ID <= 1)
            {
                JumpsPerformanced++; //If we are in fall animation then increase a Jump perfomanced
                OnHoldByReset = false;
            }
        }





#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Jump is an automatic State, the Fall Speed is created internally
        }

        internal override void Reset()
        {
            base.Reset();
            Input = "Jump";

            SleepFromState = new List<StateID>() { MTools.GetInstance<StateID>("Fall"), MTools.GetInstance<StateID>("Fly") };
            SleepFromMode = new List<ModeID>() { MTools.GetInstance<ModeID>("Action"), MTools.GetInstance<ModeID>("Attack1") };


            EnterTag.Value = "JumpStart";
            ExitTag.Value = "JumpEnd";

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true, //IMPORTANT!
                Persistent = false,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };

            //  ExitFrame = false;

            jumpProfiles = new List<JumpProfile>()
            { new JumpProfile()
            { name = "Jump",  fallingTime = 0.7f,    CliffTime = new RangedFloat( 0.333f,0.666f),  ForwardMultiplier = 1,  HeightMultiplier =  1, JumpLandDistance = 1.7f}
            };
        }
#endif
    }




    /// <summary>Different Jump parameters on different speeds</summary>
    [System.Serializable]
    public struct JumpProfile
    {
        /// <summary>Name to identify the Jump Profile</summary>
        public string name;

        /// <summary>Maximum Vertical Speed to Activate this Jump</summary>
        [Tooltip("Minimal Vertical Speed to Activate this Jump")]
        [Min(0)] public float VerticalSpeed;

        /// <summary>Min Distance to Complete the Land when the Jump is on the Highest Point, this needs to be calculate manually</summary>
        [Tooltip("Min Distance to Complete the Land when the Jump is on the Highest Point")]
        [Min(0)] public float JumpLandDistance;

        /// <summary>Animation normalized time to change to fall animation if the ray checks if the animal is falling </summary>
        [Tooltip("Animation normalized time to change to fall animation if the ray checks if the animal is falling ")]
        [Range(0, 1)]
        public float fallingTime;


        /// <summary>Animation normalized time to change to fall animation if the ray checks if the animal is falling </summary>
        [Tooltip("Set Allow Exit to the Jump Profile (*New)")]
        [Range(0, 1)]
        public float ExitTime;


        [Tooltip("Animation normalized time to check if we can end the jump sooner. if its set to zero I will use 0.333 normalize value as default")]
        [MinMaxRange(0, 1)]
        public RangedFloat CliffTime;

        /// <summary>Maximum distance to land on a Cliff </summary>
        [Tooltip("Maximum distance to land on a Cliff")]
        [Min(0)] public float CliffLandDistance;

        /// <summary>Height multiplier to increase/decrease the Height Jump</summary>
        public float HeightMultiplier;
        ///// <summary>Forward multiplier to increase/decrease the Forward Speed of the Jump</summary>
        [Tooltip("Forward multiplier to increase/decrease the Forward  Rootmotion speed of the Jump")]
        public float ForwardMultiplier;

        [Tooltip("Extra forward Movement to move the Animal Forward")]
        public float ForwardPressed;

        [Tooltip("Last State the animal was before making the Jump")]
        public StateID LastState;
    }
}

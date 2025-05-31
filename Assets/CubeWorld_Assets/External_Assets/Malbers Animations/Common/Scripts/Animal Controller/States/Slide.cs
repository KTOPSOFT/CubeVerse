using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddTypeMenu("Ground/Slide")]
    public class Slide : State
    {
        //public override string StateName => "Slide";
        public override string StateIDName => "Slide";

        [Header("Slide Movement & Rotation")]

        [Tooltip("Check if the Ground Changer include any of Malbers Tag")]
        public Tag[] tags;


        [Tooltip("Lerp value for the Alignment to the surface")]
        public FloatReference OrientLerp = new(10f);

        [Tooltip("If the current Slope of the character is greater than this value, the state can be activated\n" +
         "If the current Slope of the character is lower than this value. The state will exit.")]
        public FloatReference MinSlopeAngle = new(0);

        private bool IgnoreRotation;

        [Tooltip("When Sliding the Animal will be able to orient towards the direction of this given angle")]
        public FloatReference RotationAngle = new(30f);
        [Tooltip("When Sliding the Animal will be able to Move horizontally with this value")]
        public FloatReference SideMovement = new(5f);

        [Header("Exit Conditions")]
        [Tooltip("If the Speed is lower than this value the Slide state will end.")]
        public FloatReference ExitSpeed = new(0.5f);

        [Tooltip("If the Slope of the Slide ground is greater that this value, the Slide State will exit to Fall")]
        public FloatReference ExitSlopeAngleFall = new(60f);

        [Tooltip("When a Flat terrain is reached. it will wait this time to transition to Locomotion or Idle")]
        public FloatReference ExitWaitTime = new(0.5f);

        [Space]
        [Tooltip("Activate the Sliding state if the character is on a slope")]
        public bool AutoSlope = false;
        [Tooltip("If the Slope of the Slide ground is greater that this value, the Slide State be Activated. Zero value will ignore Enter by Slope")]
        [Hide(nameof(AutoSlope))] public FloatReference EnterAngleSlope = new(0);
        [Tooltip("If the Slope of the Slide ground is greater that this value, the Slide State be Activated. Zero value will ignore Enter by Slope")]
        [Hide(nameof(AutoSlope))] public FloatReference EnterMaxAngleSlope = new(70);

        [Tooltip("Enter the Slide state if the Character is facing the slope.. Default value 90")]
        [Hide(nameof(AutoSlope))]
        public FloatReference FacingAngleSlope = new(90);
        [Tooltip("The rotation of the character while sliding will be ignored")]
        [Hide(nameof(AutoSlope))]
        public BoolReference ignoreRotation = new();

        private float currentExitTime;

        //[Header("Exit Status Values")]
        //[Tooltip("The Exit Status will be set to 1 if the Exit Condition was the Exit Speed")]
        //public IntReference ExitSpeedStatus = new(1);
        //[Tooltip("The Exit Status will be set to 2 if the Exit Condition was that there's no longer a Ground Changer")]
        //public IntReference NoChangerStatus = new(2);

        public override bool TryActivate()
        {
            return TrySlideGround();
        }

        public override void OnPlataformChanged(Transform newPlatform)
        {
            //Debug.Log($"OnPlataformChanged {(newPlatform ? newPlatform.name : "null")}");

            if (!IsActiveState && TrySlideGround() && CanBeActivated)
            {
                Activate();
            }
            //else if (IsActiveState && !animal.InGroundChanger && CanExit)
            //{
            //    Debugging("[Allow Exit] No Ground Changer");
            //    //SetExitStatus(NoChangerStatus);
            //    SetExitStatus(NoChangerStatus);
            //    AllowExit();
            //}
        }

        public override void Activate()
        {
            base.Activate();
            KeepForwardMovement = true;
            IgnoreRotation = ignoreRotation.Value; //Set the default value
            additiveSide = 0;  //Set the default value
            //Add the additional Speed
            if (animal.InGroundChanger)
            {
                animal.CurrentSpeedModifier.position.Value += animal.GroundChanger.SlideData.AdditiveForwardSpeed;
                IgnoreRotation = animal.GroundChanger.SlideData.IgnoreRotation;
                additiveSide = animal.GroundChanger.SlideData.AdditiveHorizontalSpeed;
            }

            currentExitTime = 0;
        }
        private float additiveSide;

        private bool TrySlideGround()
        {
            if (tags.Length > 0 && !animal.platform.HasMalbersTag(tags)) return false; //Check if the Ground Changer include any of Malbers Tag


            if (m_debug && animal.debugGizmos && animal.Grounded)
            {
                MDebug.Draw_Arrow(Position, Vector3.ProjectOnPlane(animal.SlopeDirection, Up), Color.white);
            }


            if (animal.InGroundChanger
            && animal.GroundChanger.SlideData.Slide                                     //Meaning the terrain is set to slide
            && animal.SlopeDirectionAngle >= animal.GroundChanger.SlideData.MinAngle     //The character is looking at the Direction of the slope
            && animal.SlopeDirectionAngle > MinSlopeAngle     //The Slope greater that the min angle
            )
            {
                //CHECK THE DIRECTION OF THE SLIDE
                if (Vector3.Angle(animal.Forward, animal.SlopeDirection) < animal.GroundChanger.SlideData.ActivationAngle / 2)
                {
                    return true;
                }
            }
            //When is not using GroundChanger use the Enter AngleSlope
            else if (AutoSlope && EnterAngleSlope > 0 && animal.Grounded
                    && animal.SlopeDirectionAngle > EnterAngleSlope.Value
                    && animal.SlopeDirectionAngle < EnterMaxAngleSlope.Value
                    && (Vector3.Angle(animal.Forward, Vector3.ProjectOnPlane(animal.SlopeDirection, Up)) < (FacingAngleSlope.Value / 2))
                )
            {
                return true;
            }

            return false;
        }


        /// <summary> Override the Input Axis to match the State movement   </summary>
        public override void InputAxisUpdate()
        {
            var move = animal.RawInputAxis;


            if (AlwaysForward) animal.RawInputAxis.z = 1;

            DeltaAngle = move.x;
            var NewInputDirection = Vector3.ProjectOnPlane(animal.SlopeDirection, animal.UpVector);

            if (animal.MainCamera)
            {
                //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
                var Cam_Forward = Vector3.ProjectOnPlane(animal.MainCamera.forward, UpVector).normalized;
                var Cam_Right = Vector3.ProjectOnPlane(animal.MainCamera.right, UpVector).normalized;

                move = (animal.RawInputAxis.z * Cam_Forward) + (animal.RawInputAxis.x * Cam_Right);
                DeltaAngle = Vector3.Dot(animal.Right, move);
            }

            NewInputDirection = Quaternion.AngleAxis(RotationAngle * DeltaAngle, animal.Up) * NewInputDirection;

            if (currentExitTime > 0) NewInputDirection = Vector3.zero;


            //NewInputDirection *= animal.RawInputAxis.z;
            animal.MoveFromDirection(NewInputDirection);  //Move using the slope Direction instead

            // MDebug.Draw_Arrow(transform.position, NewInputDirection, Color.green);

            HorizontalLerp = Vector3.Lerp(HorizontalLerp, Vector3.Project(move, animal.Right), animal.DeltaTime * CurrentSpeed.lerpPosition);

            if (GizmoDebug)
                MDebug.Draw_Arrow(transform.position, HorizontalLerp, Color.white);

        }

        /// <summary>Smooth Horizontal Lerp value to move left and right </summary>
        private Vector3 HorizontalLerp { get; set; }

        float DeltaAngle;

        public override Vector3 Speed_Direction()
        {
            var speedDir = animal.SlopeDirection;

            if (!IgnoreRotation) //Use the Slope Direction to move the state
            {
                speedDir = Quaternion.AngleAxis(RotationAngle * DeltaAngle, animal.Up) * speedDir;
            }

            if (speedDir == Vector3.zero)
            {
                KeepForwardMovement = false;
                speedDir = animal.Forward * animal.RawInputAxis.z; //If there's no direction use the forward direction
            }
            else
            {
                KeepForwardMovement = true;
            }

            return speedDir;
        }


        public override void OnStateMove(float deltatime)
        {
            if (InCoreAnimation)
            {
                //Calculate the Horizontal Direction
                var Right = Vector3.Cross(animal.Up, animal.SlopeDirection);
                Right = Vector3.Project(animal.MovementAxisSmoothed, Right);

                if (GizmoDebug)
                    MDebug.Draw_Arrow(transform.position, Right, Color.red);

                animal.AdditivePosition += (deltatime * (SideMovement + additiveSide)) * HorizontalLerp; //Move Left or right while sliding
                //Orient to the Ground
                animal.AlignRotation(animal.SlopeNormal, deltatime, OrientLerp);

                if (IgnoreRotation)
                {
                    animal.AlignRotation(animal.Forward, animal.SlopeDirection, deltatime, OrientLerp); //Make your own Aligment
                    animal.UseAdditiveRot = false; //Remove Rotations
                }

                if (!animal.Grounded)
                {
                    animal.UseGravity = true;
                }
            }
        }


        public override void TryExitState(float DeltaTime)
        {
            if (!animal.InGroundChanger)
            {
                if (!AutoSlope) AllowExit();
            }
            else  //if we are on a ground changer
            {
                if (animal.SlopeDirectionAngle > ExitSlopeAngleFall || !animal.MainRay)
                {
                    animal.Grounded = false;
                    Debugging("[Allow Exit] Exit to Fall. Terrain Slope is too deep");
                    AllowExit(3, 0); //Exit to Fall!!!
                }
                else if (!animal.GroundChanger.SlideData.Slide)
                {
                    Debugging("[Allow Exit] No longer in a Slide Ground Changer");
                    AllowExit();
                }
            }

            //There's no an angle slope
            if (/*animal.HorizontalSpeed < ExitSpeed && */animal.SlopeDirectionAngle <= MinSlopeAngle)
            {
                currentExitTime += DeltaTime;

                if (currentExitTime > ExitWaitTime)
                {
                    Debugging("[Allow Exit] No longer in a slope angle");
                    // SetExitStatus(ExitSpeedStatus);
                    AllowExit();
                    currentExitTime = 0;
                }
            }
            else
            {
                currentExitTime = 0;
            }

            ////Exit when there no more speed in the horizontal???
            //if (animal.HorizontalSpeed <= ExitSpeed)
            //{
            //    Debugging("[Allow Exit] Speed is Slow");
            //    //SetExitStatus(ExitSpeedStatus);
            //    AllowExit();
            //    return;
            //}
        }


        internal override void Reset()
        {
            base.Reset();

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }
    }
}
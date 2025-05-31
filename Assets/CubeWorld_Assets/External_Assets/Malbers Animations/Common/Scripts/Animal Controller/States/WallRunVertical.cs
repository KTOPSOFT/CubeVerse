using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/wallrun")]
    [AddTypeMenu("WallRun/Wall Run Vertical")]
    public class WallRunVertical : State
    {
        //public override string StateName => "WallRun/Wall-Run Vertical";
        public override string StateIDName => "WallRunVertical";

        [Tooltip("If the Animal is going to find the wall automatically to activate the State")]
        public BoolReference Automatic = new(true);

        [Tooltip("Another Filter to activate  wallrun Vertical")]
        public StringReference WallTag = new("WallRun");

        [Header("Wall Parameters")]
        [Tooltip("Max Distance to find walls left and Right.")]
        public FloatReference WallCheck = new(1);

        [Tooltip("Distance to align the Character to the Wall")]
        public FloatReference WallDistance = new(0.5f);

        [Tooltip("Pivot to cast rays from the Animal, to find walls left and right")]
        public Vector3Reference Center = new(0, 1, 0);

        public LayerReference WallLayer = new(1);


        [Tooltip("Min Value Angle from the Forward Direction of the caracter  and the wall's normal and activate the state")]
        public float WallFaceAngle = 20f;

        [Tooltip("Angle Limit to Exit the WallRun")]
        public float WallLimitAngle = 45f;

        [Tooltip("Use the Rotator to Rotate 90 degree the animal. Use this if you do not have Wall Run Vertical animations")]
        public bool UseRotator = false;

        [Tooltip("Smoothness value to align the animal to the wall")]
        public float AlignSmoothness = 10f;

        [Header("Side Movement")]
        [Tooltip("Move the Character left and Right while going Up the wall")]
        public FloatReference SideMovement = new(1f);
        public float Bank = 0f;

        [Header("Exit Features")]
        [Tooltip("When there's no wall, wait This time to propelry exit")]
        public float ExitDelay = 0.5f;
        private float currentExitDelay;
        [Tooltip("Force to apply when exiting the WallRun")]
        [Min(0)] public float ExitForce = 15;
        [Hide(nameof(ExitForce), true)]
        [Min(0)] public float ExitForceAceleration = 2;


        /// <summary>Check if the Current Wall its a valid wall</summary>
        public Transform ValidWall { get; private set; }


        public float WallCurrentDistance { get; private set; }
        public Vector3 WallNormal { get; private set; }


        public override bool TryActivate()
        {
            //  var needsSprint = OnSprint && animal.Sprint || !OnSprint.Value;
            var CanActivate = /*InputValue ||*/ Automatic.Value;// && needsSprint;

            if (!CanActivate) return false;

            var HasFoundWall = FindWall(); //Do not find a wall if easy conditions are not met
            if (!HasFoundWall) return false;



            var angle = Vector3.Angle(-Forward, Vector3.ProjectOnPlane(WallNormal, Up));
            CanActivate = angle < WallFaceAngle && CanActivate;

            //Debug.Log(angle);

            if (CanActivate)
                Debugging("[Try Activate] Wall detected Front");

            return CanActivate;
        }

        public override void ResetStateValues()
        {
            animal.ResetCameraInput();
            currentExitDelay = 0;
            WallCurrentDistance = 0;
            WallNormal = Vector3.zero;

            //  if (UseRotator) animal.Rotator.localEulerAngles = new Vector3(0, 0, 0); //Angle for the Rotator
        }

        private bool FindWall()
        {
            ValidWall = null;

            var WoldPos = transform.TransformPoint(Center);

            if (m_debug)
            {
                MDebug.DrawRay(WoldPos, animal.Forward * WallCheck, Color.red);
                MDebug.DrawRay(WoldPos, animal.Forward * WallDistance, Color.green);
            }

            if (Physics.Raycast(WoldPos, animal.Forward, out RaycastHit WallHit, WallCheck, WallLayer.Value, QueryTriggerInteraction.Ignore))
            {
                var WallFound = WallHit.transform;

                if (ValidWall != WallFound)
                {
                    if (WallTag.Empty || WallFound.CompareTag(WallTag)) //Check Wall Filter
                    {
                        animal.SetPlatform(WallFound);

                        //  if (!IsActiveState) Debugging("[Try Activate] Wall detected Front");


                        MDebug.DrawWireSphere(WallHit.point, 0.05f, Color.green, 0.2f);

                        ValidWall = WallFound;
                        WallNormal = WallHit.normal;
                        WallCurrentDistance = WallHit.distance;

                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Current Direction Speed Applied to the Additional Speed, by default is the Animal Forward Direction</summary>
        public override Vector3 Speed_Direction()
        {
            return Up * (animal.VerticalSmooth) + (animal.HorizontalSmooth * SideMovement) * Right; //IMPORTANT OF ADDITIONAL SPEED
        }

        public override void Activate()
        {
            base.Activate();
            animal.UseCameraInput = false;       //Climb cannot use Camera Input

            currentExitDelay = 0;
            WallCurrentDistance = WallDistance * 1.5f;
            WallNormal = Vector3.zero;

            //if (UseRotator) General.FreeMovement = true; //Enable Free Movement for the Rotator (Double Check)
        }

        public override void OnStateMove(float deltatime)
        {
            //Keep the distance to the valid wall using the WallDistance

            if (InCoreAnimation)
            {
                FindWall();

                AlignToWall(WallCurrentDistance, deltatime);

                var angle = Vector3.Angle(WallNormal, animal.UpVector);

                if (angle > WallLimitAngle) //If the angle is bigger than the limit Orient the Animal to the Wall
                    OrientToWall(WallNormal, deltatime);


                if (UseRotator)
                {
                    animal.PitchDirection = animal.UpVector;
                    animal.FreeMovementRotator(90, Bank);
                }
            }
        }


        public override void TryExitState(float DeltaTime)
        {
            if (ValidWall == null)
            {
                currentExitDelay += DeltaTime;

                if (currentExitDelay > ExitDelay)
                {
                    Debugging("[Try Exit] Wall not detected");
                    AllowExit();

                    animal.Force_Add(animal.Up, ExitForce, ExitForceAceleration, false, true, 0);

                    animal.Delay_Action(0.5f, () => animal.Force_Remove(ExitForceAceleration));
                }
            }
        }

        //Align the Animal to the Wall
        private void AlignToWall(float distance, float deltatime)
        {
            float difference = distance - WallDistance * animal.ScaleFactor;

            if (!Mathf.Approximately(distance, WallDistance * animal.ScaleFactor))
            {
                Vector3 align = AlignSmoothness * deltatime * difference * ScaleFactor * animal.Forward;
                animal.AdditivePosition += align;
            }
        }

        private void OrientToWall(Vector3 normal, float deltatime)
        {
            Quaternion AlignRot = Quaternion.FromToRotation(Forward, -normal) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, deltatime * AlignSmoothness);      //Calculate the Delta Align Rotation
            animal.AdditiveRotation *= Delta;



            //Update the Rotation to always look Upwards
            var UP = Vector3.Cross(Forward, UpVector);
            UP = Vector3.Cross(UP, Forward);

            if (Bank != 0 && !UseRotator) UP = Quaternion.Euler(animal.HorizontalSmooth * Bank, 0, 0) * UP;

            AlignRot = Quaternion.FromToRotation(transform.up, UP) * transform.rotation;  //Calculate the orientation to Terrain 
            Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Target = Inverse_Rot * AlignRot;
            animal.AdditiveRotation *= Target;
        }

        public override void StatebyInput()
        {
            if (InputValue && FindWall())
            {
                Activate();
            }
        }


        public override void StateGizmos(MAnimal animal)
        {
            if (m_debug && !Application.isPlaying)
            {
                var t = animal.transform;
                var ScaleFactor = animal.ScaleFactor;

                var Forward = t.forward * ScaleFactor;

                var WoldPos = t.TransformPoint(Center);

                //draw a ray from the pivot to the front
                Gizmos.color = Color.red;
                Gizmos.DrawRay(WoldPos, Forward * WallCheck);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(WoldPos, Forward * WallDistance);
                // Gizmos.DrawWireSphere(WoldPos, 0.05f * ScaleFactor);
            }
        }

        internal override void Reset()
        {
            base.Reset();

            TryLoop = new(6); //Set the find wall to be every 6 frames

            AlwaysForward = new(true);

            this.Input = "Sprint";

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                RootMotion = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                Gravity = false,
                CustomRotation = true,
                FreeMovement = false,
                IgnoreLowerStates = true,
            };



            SpeedSets = new List<MSpeedSet>()
            {
                new MSpeedSet()
                {
                    name = "Wall Run Vertical",
                    StartVerticalIndex = new(1),
                    TopIndex = new(1),
                    states = new List<StateID>(1) { MTools.GetInstance<StateID>(StateIDName) },

                    Speeds = new List<MSpeed>() { new ("Wall Run Vertical")
                    { position = new (5) }
                    }
                }
            };
        }


        public override void SetSpeedSets(MAnimal animal)
        {
            //var setName = "Wall Run Vertical";

            //if (animal.SpeedSet_Get(setName) == null)
            //{
            //    animal.speedSets.Add(
            //        new MSpeedSet()
            //        {
            //            name = setName,
            //            StartVerticalIndex = new(1),
            //            TopIndex = new(1),
            //            states = new List<StateID>(1) { MTools.GetInstance<StateID>(StateIDName) },

            //            Speeds = new List<MSpeed>() { new MSpeed("Wall Run Vertical")
            //            { position = new (5) }
            //            }
            //        }
            //        );
            //}
        }

    }
}

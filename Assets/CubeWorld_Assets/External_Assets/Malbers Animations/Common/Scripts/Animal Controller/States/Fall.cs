using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/fall")]
    [AddTypeMenu("Air/Fall")]
    public class Fall : State
    {
        //TODO: DO Fall Rotator
        // public override string StateName => "Fall";
        public override string StateIDName => "Fall";
        public enum FallBlending { DistanceNormalized, Distance, VerticalVelocity }

        /// <summary>Air Resistance while falling</summary>
        [Tooltip("Can the Animal be controller while falling?")]
        public BoolReference AirControl = new(true);
        [Tooltip("Rotation while falling")]
        public FloatReference AirRotation = new(10);
        [Tooltip("Maximum Movement while falling")]
        public FloatReference AirMovement = new(0);
        [Tooltip("Lerp value for the Air Movement adjusment")]
        public FloatReference AirSmooth = new(2);

        [Space]

        [Tooltip("Forward Offset Position of the Fall Ray")]
        public FloatReference Offset = new();

        [Tooltip("Forward Offset Multiplier Position of the Fall Ray while moving")]
        public FloatReference MoveMultiplier = new(0.1f);

        [Hide("ShowFront")]
        [Tooltip("A ray will be cast in front of the animal to check if there's an obstacle in front of it")]
        public bool CheckFrontObstacle = true;



        [Tooltip("Multiplier for the Fall Ray Length. The Default Value is the Animal's Height")]
        public FloatReference lengthMultiplier = new(1f);

        [Tooltip("RayHits Allowed on the Raycast NonAloc (Try Fall Logic)")]
        public IntReference rayHits = new(6);

        [Space, Tooltip("State Float Value in the animator. This is used to blend between different Fall Animations")]
        public FallBlending BlendFall = FallBlending.DistanceNormalized;

        [Tooltip("Used to Set fallBlend to zero before reaching the ground")]
        public FloatReference LowerBlendDistance;

        /// <summary>Distance to Apply a Fall Hard Animation</summary>
        [Space, Header("Fall Damage")]
        public StatID AffectStat;

        [Tooltip("Minimum Distance to Apply Fall Damage, If the distance falled is lesser than this value, no damage will be applied")]
        public FloatReference FallMinDistance = new(5f);

        [Tooltip("Maximum Distance to Apply Fall Damage, If the distance falled is greater  than this value,the animal will die")]
        public FloatReference FallMaxDistance = new(15f);

        [Tooltip("The Fall State will set the Exit State Status Depending the Fall Distance (X: Distance Y:Exit Status Value)")]
#if UNITY_2020_3_OR_NEWER
        [NonReorderable]
#endif
        public Vector2[] landStatus;

        [Tooltip("Fix the animal when is stuck on weird places (Experimental)")]
        public bool StuckAnimal = true;

        [Tooltip("When Falling, the animal may get stuck falling. The animal will be force to move forward.")]
        public FloatReference PushForward = new(2);
        /// <summary>Stores the max heigth before going Down</summary>
        public float MaxHeight { get; set; }

        /// <summary>Acumulated Fall Distance</summary>
        public float FallCurrentDistance { get; set; }

        protected Vector3 fall_Point;
        private RaycastHit[] FallHits;
        private RaycastHit FallRayCast;

        private GameObject GameObjectHit;
        private bool IsDebree;

        /// <summary>While Falling this is the distance to the ground</summary>
        private float DistanceToGround;

        /// <summary> Normalized Value of the Height </summary>
        float Fall_Float;
        // public Vector3 UpImpulse { get; private set; }
        //public bool Has_UP_Impulse { get; private set; }

        private MSpeed FallSpeed = MSpeed.Default;

        ///// <summary>  UpIntertia at the moment of the fall Activation </summary>
        //public Vector3 UpIntertia { get; private set; }

        public Vector3 FallPoint { get; private set; }

        /// <summary> UP Impulse was going UP </summary>

        private bool GoingDown;
        private int Hits;

        public override void AwakeState()
        {
            base.AwakeState();
            animalStats = animal.FindComponent<Stats>(); //Find the Stats

            FallHits = new RaycastHit[rayHits];//set the hits
        }

        public override bool TryActivate()
        {
            float SprintMultiplier = (animal.VerticalSmooth);
            var fall_Pivot = animal.Main_Pivot_Point + (Offset * ScaleFactor * animal.Forward) +
                (MoveMultiplier * ScaleFactor * SprintMultiplier * animal.Forward); //Calculate ahead the falling ray

            //fall_Pivot += animal.DeltaPos; //Check for the Next Frame (Does not work now with


            //Check Front 
            if (CheckFrontObstacle && MoveMultiplier > 0)
            {
                if (GizmoDebug)
                {
                    MDebug.DrawLine(fall_Pivot, fall_Pivot, Color.magenta);
                }
                if (Physics.Linecast(fall_Pivot, fall_Pivot, GroundLayer, IgnoreTrigger)) return false;
            }


            float Multiplier = animal.Pivot_Multiplier * lengthMultiplier * 0.999f * ScaleFactor;
            return TryFallRayCasting(fall_Pivot, Multiplier);
        }

        private bool TryFallRayCasting(Vector3 fall_Pivot, float Multiplier)
        {
            // var Direction = animal.TerrainSlope > 0 ? Gravity : -transform.up;
            var Direction = Gravity;
            // var Direction =   -transform.up;

            var Radius = animal.RayCastRadius * ScaleFactor;
            Hits = Physics.SphereCastNonAlloc(fall_Pivot, Radius, Direction, FallHits, Multiplier, GroundLayer, IgnoreTrigger);

            if (GizmoDebug)
            {
                var Dir = Direction * Multiplier;
                MDebug.DrawRay(fall_Pivot, Dir, Color.black);
                MDebug.DrawRay(fall_Pivot, Dir, Color.magenta);
                MDebug.DrawRay(FallRayCast.point, 0.2f * ScaleFactor * FallRayCast.normal, Color.magenta);
                // MDebug.DrawWireSphere(fall_Pivot + Dir - (Dir.normalized * Radius), Color.magenta, Radius);
                MDebug.DrawCapsule(fall_Pivot, fall_Pivot + Dir - (Dir * Radius), Radius, Color.magenta);
            }

            // Debugging($"[Try] Hits {Hits} - Direction {Direction} - Multiplier {Multiplier} - Radius {Radius}");

            if (Hits > 0)
            {
                if (animal.Grounded) //Check when its grounded
                {
                    for (int i = 0; i < Hits; i++)
                    {
                        var hit = FallHits[i];
                        float TerrainSlope = Vector3.SignedAngle(hit.normal, animal.UpVector, animal.Right);
                        MDebug.DrawWireSphere(fall_Pivot + Direction * DistanceToGround, Color.magenta, Radius);
                        FallRayCast = hit;

                        if (TerrainSlope > -animal.SlopeLimit) //Check for the first Good Fall Ray that does not break the Fall.
                            break;
                    }

                    if (FallRayCast.transform.gameObject != GameObjectHit) //Check if what the Fall Ray Hit was a Debree
                    {
                        GameObjectHit = FallRayCast.transform.gameObject;
                        IsDebree = GameObjectHit.CompareTag(animal.DebrisTag);
                    }
                }
                else   //If the Animal is in the air  NOT GROUNDED
                {

                    FallRayCast = FallHits[0];

                    DistanceToGround = FallRayCast.distance;

                    float FallSlope = Vector3.Angle(FallRayCast.normal, animal.UpVector);

                    if (FallSlope > animal.SlopeLimit)
                    {
                        Debugging($"[Try] The Animal is on the Air and the angle SLOPE of the ground hitted is too Deep [{FallSlope}].  " +
                            $"- [{FallRayCast.transform.name}]");

                        return true;
                    }

                    // Debug.Log($"DistanceToGround {DistanceToGround} : Height {Height}");

                    if (Height >= DistanceToGround) //If the distance to ground is very small means that we are very close to the ground
                    {
                        if (animal.ExternalForce != Vector3.zero) return true; //Hack for external forces

                        var isgrounded = animal.CheckIfGrounded();//means whe are very close to the ground!! so check if we are grounded

                        Debugging($"[Try Failed] Distance to the ground is very small. Checking if we are grounded [{isgrounded}]");
                        if (animal.Grounded)
                        {
                            animal.Grounded = true; //Force Grounded
                            animal.UseGravity = false;

                            animal.AlignPosLerpDelta = animal.AlignPosLerp * 5;

                            var GroundedPos = Vector3.Project(FallRayCast.point - animal.transform.position, Gravity);
                            animal.Teleport_Internal(animal.transform.position + GroundedPos);

                            animal.ResetUPVector(); //IMPORTANT!
                            animal.hit_Hip.distance = Height;


                            //This is for Helping on Slopes
                            //  animal.InertiaPositionSpeed = Vector3.ProjectOnPlane(animal.RB.velocity * animal.DeltaTime, animal.UpVector); 

                        }
                        return false;
                    }
                }
            }
            else
            {
                // MDebug.DrawWireSphere(fall_Pivot, 0.2f, Color.magenta, 2f);
                Debugging($"[Try] There's no Ground beneath the Animal");
                // Debug.Break();
                return true;
            }

            //Debug.Log("fa");
            return false;
        }

        public override void Activate()
        {
            KeepForwardMovement = !AirControl.Value;

            //  if (!animal.ActiveState.KeepForwardMovement) AirControlFrom = false; //Inherit the Air Control from the last s

            base.Activate();

            //StartingSpeedDirection = Vector3.zero;

            StartingSpeedDirection = animal.DeltaPos;

            // Debug.Log($"StartingSpeedDirection: {StartingSpeedDirection}");

            if (animal.LastState.ID == StateEnum.Jump || animal.LastState.ID.ID <= 2)
            {
                StartingSpeedDirection = animal.HorizontalVelocity; //Clean from JUMP
                KeepForwardMovement = animal.LastState.KeepForwardMovement;
            }

            ResetStateValues();
            Fall_Float = animal.State_Float;
            // animal.UpInertia_Store();
        }

        // public override bool KeepForwardMovement => true;

        // private bool KeepForwardFall;

        public override void EnterCoreAnimation()
        {
            SetEnterStatus(0);
            IgnoreLowerStates = false;

            var Speed = animal.HorizontalSpeed / ScaleFactor; //Remove the scaleFactor since it will be added later 

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

            FallSpeed = new MSpeed(animal.CurrentSpeedModifier)
            {
                name = "FallSpeed",
                position = Speed,
                strafeSpeed = Speed,
                animator = 1,
                rotation = AirRotation.Value,
                lerpPosition = AirSmooth.Value,
                lerpStrafe = AirSmooth.Value,
                lerpAnimator = 8
            };

            //if (!animal.MovementDetected)
            //{
            //    animal.SetCustomSpeed(FallSpeed, true);                  
            //    animal.ResetDeltaRootMotion();
            //}
            //else
            {
                animal.SetCustomSpeed(FallSpeed, false);                     //Set the Current Speed to the Jump Speed Modifier
            }

            //Disable the Gravity if we are on an external Force (Wind, Spring) //IMPORTANT
            if (animal.HasExternalForce && animal.InZone) animal.UseGravity = false;

            CanExit = true; // FORCE CAN EXIT IF WE ARE ALREADY ON THE ANIMATION Is this working or not???

            //  Debug.Log("ENTERING CORE ANIMATION");

            //Set the Target Speed to the Horizontal Velocity (coming from RootMotion Like Jump RootMotion or FallFrom Ledge)
            if (animal.TargetSpeed == Vector3.zero)
            {
                animal.DeltaRootMotion = Vector3.zero; //Reset the Delta Root Motion
                animal.UpInertia_Store(); //Store the UpInertia at the moment of the Fall Activation
                animal.ResetInertiaSpeed(animal.HorizontalVelocity * animal.DeltaTime);
            }
        }

        public override Vector3 Speed_Direction()
        {
            if (GizmoDebug)
                MDebug.Draw_Arrow(transform.position, StartingSpeedDirection, Color.magenta);

            if (!KeepForwardMovement)
            {
                return (base.Speed_Direction());
            }
            else
            {
                return StartingSpeedDirection;
            }
        }


        Vector3 StartingSpeedDirection;
        private Stats animalStats;

        public override void OnStateMove(float deltaTime)
        {
            if (InCoreAnimation)
            {
                if (animal.InZone && animal.HasExternalForce) animal.GravityTime = 0; //Reset the gravity when the animal is on a Force Zone.

                if (!KeepForwardMovement && AirMovement > 0 && AirMovement > CurrentSpeedPos)
                {
                    if (!animal.ExternalForceAirControl) return;

                    CurrentSpeedPos = Mathf.Lerp(CurrentSpeedPos, AirMovement, (AirSmooth != 0 ? (deltaTime * AirSmooth) : 1));
                }
                ////Keep the Up Momentum
                animal.UpInertia_Apply();
            }
        }

        public override void TryExitState(float DeltaTime)
        {
            var Radius = animal.RayCastRadius * ScaleFactor;

            float SprintMultiplier = (animal.VerticalSmooth);
            var FallPoint = animal.Main_Pivot_Point + (Offset * ScaleFactor * animal.Forward) +
               (animal.Forward * (SprintMultiplier * MoveMultiplier * ScaleFactor)); //Calculate ahead the falling ray

            //  var Gravity = this.Gravity;
            // var Gravity = animal.DeepSlope ? this.Gravity :  -animal.Up;

            FallPoint += animal.AdditivePosition; //Check for the with the additive position.. IMPORTANT
            //FallPoint = animal.Main_Pivot_Point;


            float DeltaDistance = 0;

            GoingDown = Vector3.Dot(DeltaPos, Gravity) > 0; //Check if is falling down

            if (GoingDown)
            {
                DeltaDistance = Vector3.Project(DeltaPos, Gravity).magnitude / ScaleFactor;
                FallCurrentDistance += DeltaDistance;
            }

            if (GizmoDebug)
            {
                MDebug.DrawWireSphere(FallPoint, Color.magenta, Radius);
                MDebug.DrawWireSphere(FallPoint + Gravity * Height, Color.white, Radius);
                Debug.DrawRay(FallPoint, Gravity * 100f, Color.magenta);
            }

            var FoundGround = (Physics.Raycast(FallPoint, Gravity, out FallRayCast, 100f, GroundLayer, IgnoreTrigger));


            //var RBMovement = Vector3.ProjectOnPlane(animal.RB.velocity * DeltaTime, animal.UpVector); //This is for Helping on Slopes
            //animal.InertiaPositionSpeed = RBMovement;

            if (FoundGround)
            {
                DistanceToGround = FallRayCast.distance;

                if (GizmoDebug)
                {
                    // MDebug.DrawWireSphere(FallRayCast.point, (Color.blue + Color.red) / 2, Radius);
                    MDebug.DrawWireSphere(FallPoint, (Color.magenta), Radius);
                }

                switch (BlendFall)
                {
                    case FallBlending.DistanceNormalized:
                        {
                            var realDistance = DistanceToGround - Height;

                            if (MaxHeight < realDistance)
                            {
                                MaxHeight = realDistance; //get the Highest Distance the first time you touch the ground
                                Fall_Float = Mathf.Lerp(Fall_Float, 0, DeltaTime * 5); //Small blend in case there's a new ground found
                                animal.State_SetFloat(Fall_Float); //Blend between High and Low Fall
                            }
                            else
                            {
                                realDistance -= LowerBlendDistance;

                                //Small blend in case there's a new ground found
                                Fall_Float = Mathf.Lerp(Fall_Float, 1 - realDistance / MaxHeight, DeltaTime * 10);

                                animal.State_SetFloat(Fall_Float); //Blend between High and Low Fall
                            }
                        }
                        break;
                    case FallBlending.Distance:
                        animal.State_SetFloat(FallCurrentDistance);
                        break;
                    case FallBlending.VerticalVelocity:
                        var UpInertia = Vector3.Project(animal.DeltaPos, animal.UpVector).magnitude;   //Clean the Vector from Forward and Horizontal Influence    
                        animal.State_SetFloat(UpInertia / animal.DeltaTime * (GoingDown ? 1 : -1));
                        break;
                    default:
                        break;
                }

                //If we touch the Ground!
                if (Height >= DistanceToGround || ((DistanceToGround - DeltaDistance) < 0))
                {
                    var FallRayAngle = Vector3.SignedAngle(FallRayCast.normal, animal.UpVector, animal.Right);

                    if (FallRayCast.transform.gameObject != GameObjectHit) //Check if what the Fall Ray Hit was a Debree
                    {
                        GameObjectHit = FallRayCast.transform.gameObject;
                        IsDebree = GameObjectHit.CompareTag(animal.DebrisTag);
                    }

                    var DeepSlope = Mathf.Abs(FallRayAngle) >= animal.SlopeLimit;


                    if (!DeepSlope || IsDebree) //Check if we are not on a deep slope
                    {
                        AllowExit();
                        animal.CheckIfGrounded();

                        //Meaning we still are in the Fall state (Check if Grounded can change to a new state) IMPORTANT
                        if (IsActiveState)
                        {
                            animal.Grounded = true; //Force Grounded
                            animal.UseGravity = false;

                            animal.AlignPosLerpDelta = animal.AlignPosLerp * 5;

                            var GroundedPos = Vector3.Project(FallRayCast.point - animal.transform.position, Gravity);
                            animal.Teleport_Internal(animal.transform.position + GroundedPos);

                            animal.ResetUPVector(); //IMPORTANT!
                            animal.hit_Hip.distance = Height;
                            animal.InertiaPositionSpeed = Vector3.ProjectOnPlane(animal.InertiaPositionSpeed, animal.UpVector); //This is for Helping on Slopes
                            Debugging($"[Try Exit] (Grounded) + [Terrain Angle = {FallRayAngle:F2}]. [Align to Ground]");
                            return;
                        }
                    }
                    else
                    {
                        FallCurrentDistance = 0;
                        return; //Do not check if the rigidbody has Increase Velocity
                    }
                }
            }
            ResetRigidbody(DeltaTime, Gravity);
        }


        public override void ExitState()
        {
            var status = 0;
            if (landStatus != null && landStatus.Length >= 1)
            {

                foreach (var ls in landStatus)
                    if (ls.x < FallCurrentDistance) status = (int)ls.y;
            }
            SetExitStatus(status);  //Set the Landing Status!! IMPORTANT for Multiple Landing Animations

            if (AffectStat != null && animalStats != null
                && FallCurrentDistance > FallMinDistance.Value && animal.Grounded) //Meaning if we are on the safe minimun distance we do not get damage from falling
            {
                var StatFallValue = (FallCurrentDistance) * 100 / FallMaxDistance;
                animalStats.Stat_ModifyValue(AffectStat, StatFallValue, StatOption.ReduceByPercent);
            }
            base.ExitState();
        }


        private void ResetRigidbody(float DeltaTime, Vector3 Gravity)
        {
            if (StuckAnimal && GoingDown)
            {
                //Debug.Log("GoinDown");

                var RBOldDown = Vector3.Project(animal.RB.linearVelocity, Gravity);
                var RBNewDown = Vector3.Project(animal.DesiredRBVelocity, Gravity);
                var NewDMagn = RBNewDown.magnitude;
                var Old_DMagn = RBOldDown.magnitude;

                if (GizmoDebug)
                {
                    MDebug.Draw_Arrow(animal.Main_Pivot_Point + Forward * 0.02f, RBOldDown * 0.5f, Color.white);
                    MDebug.Draw_Arrow(animal.Main_Pivot_Point + Forward * 0.04f, RBNewDown * 0.5f, Color.green);
                }

                ResetCount++;

                if (NewDMagn == Old_DMagn) return;

                if (NewDMagn > (Old_DMagn * Old_DMagn) &&  //New Desired Velocity is greatere 
                    Old_DMagn < 0.1f && ResetCount > 5) //5 seems to be good
                {
                    if (animal.DesiredRBVelocity.magnitude > Height)
                    {
                        Debugging($"Reset Rigidbody Velocity. Animal may be stuck");

                        animal.ResetUPVector();
                        animal.GravityTime = animal.StartGravityTime;

                        if (PushForward > 0)
                            animal.InertiaPositionSpeed = animal.ScaleFactor * DeltaTime * PushForward * animal.Forward;  //Force going forward HACK

                        ResetCount = 0;
                    }
                }
            }
            //else
            //{
            //    //ResetCount = 0;
            //}
        }

        /// <summary> This is for cleaning the Ribidbody with unnecesary velocity </summary>
        private int ResetCount;

        public override void ResetStateValues()
        {
            DistanceToGround = float.PositiveInfinity;
            GoingDown = false;
            IsDebree = false;
            FallSpeed = new MSpeed();
            FallRayCast = new RaycastHit();
            GameObjectHit = null;
            FallHits = new RaycastHit[rayHits];
            MaxHeight = float.NegativeInfinity; //Resets MaxHeight
            FallCurrentDistance = 0;
            Fall_Float = 0; //IMPORTANT

            // UpIntertia = Vector3.zero;
        }


        public override void StateGizmos(MAnimal animal)
        {
            if (!Application.isPlaying)
            {
                var fall_Pivot = animal.transform.position +
                    ((animal.Forward * Offset + new Vector3(0, animal.height)) * animal.ScaleFactor); //Calculate ahead the falling ray

                float Multiplier = animal.Pivot_Multiplier * lengthMultiplier;

                Debug.DrawRay(fall_Pivot, animal.Gravity.normalized * Multiplier, Color.magenta);
            }
        }

#if UNITY_EDITOR


        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Fall is an automatic State, the Fall Speed is created internally
        }

#pragma warning disable 414
        [HideInInspector, SerializeField] private bool ShowFront;
#pragma warning restore 414


        private void OnValidate()
        {
            ShowFront = MoveMultiplier.Value > 0;
        }

        /// <summary>This is Executed when the Asset is created for the first time </summary>
        internal override void Reset()
        {
            base.Reset();
            General = new AnimalModifier()
            {
                RootMotion = false,
                AdditivePosition = true,
                AdditiveRotation = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,

                Gravity = true,
                CustomRotation = false,
                modify = (modifier)(-1),
            };

            LowerBlendDistance = 0.1f;
            MoveMultiplier = 0.1f;
            lengthMultiplier = 1f;

            FallSpeed.name = "FallSpeed";

            //SleepFromState = new System.Collections.Generic.List<StateID>() {   MTools.GetInstance<StateID>("Fly") };

            // ExitFrame = false; //IMPORTANT
        }
#endif
    }
}
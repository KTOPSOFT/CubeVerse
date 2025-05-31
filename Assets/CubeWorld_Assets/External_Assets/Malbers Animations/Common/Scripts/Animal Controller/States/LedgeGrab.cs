using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/ledge-grab")]
    [AddTypeMenu("Climb/Ledge Grab")]
    public class LedgeGrab : State
    {
        //public override string StateName => "Ledge Grab";
        public override string StateIDName => "LedgeGrab";

        /// <summary>Air Resistance while falling</summary>
        [Header("Ledge Parameters"), Space]
        [Tooltip("Layer to identify climbable surfaces")]
        public LayerReference LedgeLayer = new(1);

        [Tooltip("Climb the Ledge automatically when is near a climbable surface")]
        public BoolReference automatic = new();


        [Tooltip("LedgeGrab will be set automatic if any of these state are playing")]
        public List<StateID> automaticByState = new();
        public bool Automatic_By_State { get; private set; }

        [Tooltip("Set the Animal Rigidbody to Kinematic while is on this state. This avoid the colliders to interfiere with ledge.")]
        public BoolReference Kinematic = new(true);

        [Tooltip("Disable the Main Collider while the state is active (Main Collider can Interfiere with the animation)")]
        public BoolReference DisableMainCollider = new(true);

        [Tooltip("Correct Distance from the wall to the character")]
        [Min(0)] public float wallDistance = 0.5f;

        //[Tooltip("Correct Vertical Distance from the wall to the character")]
        // public float VerticalOffset = -1;

        [Tooltip("Min Angle needed to enable the Ledge Grab State")]
        public float MinTerrainAngle = 45f;

        [Tooltip("Distance required to check a wall in front of the character")]
        [Min(0)] public float ForwardLength = 1f;

        //[Tooltip("Length of the Ledge Ray when pointing Down")]
        //[Min(0)] public float DownLength = 1f;

        [Tooltip("Correct Distance from the wall to the character")]
        [Min(0)] public float WallChecker = 0.1f;


        [Tooltip("Transform Created to Store the Hit Position of the Ledge Rays. Use it to show UI When a wall  ")]
        public string HitTransform = "LedgeHit";

        public List<LedgeProfiles> profiles = new();

        /// <summary>Aligmnet offset found from the character to the ledge</summary>
        private Vector3 AlignmentOffset;
        private float AngleDifference;

        private Vector3 StartPosition;
        private Quaternion StartRotation;
        private Vector3 TargetPosition;
        private Vector3 WallNormal;

        /// <summary> Store the Current Ledge Profile </summary>
        private LedgeProfiles LedgeProfile;
        private RaycastHit FoundLedgeHit;
        private RaycastHit FoundWallHit;

        private bool OrientToWall = false;

        /// <summary> Use this with Messages to override the default value of the Climb at runtime   /// </summary>
        public void LedgeAutomatic() => automatic.Value = true;

        public override Vector3 Speed_Direction() => Vector3.zero; //This State does not require a speed


        private Transform m_HitTransform;
        public override void AwakeState()
        {
            base.AwakeState();

            //Find the Ledge Hit Transform
            m_HitTransform = animal.transform.FindGrandChild(HitTransform);

            if (m_HitTransform == null)
            {
                m_HitTransform = new GameObject(HitTransform).transform;
                m_HitTransform.parent = transform;
                m_HitTransform.ResetLocal();
            }
            EnableHitTransform(false);
        }


        private List<LedgeProfiles> UpdatedProfiles = new(); //Update the 

        public override bool TryActivate()
        {
            if (InClimb && MovementRaw.z < 0) return false; //Means is going down on a climb

            if (automatic || Automatic_By_State || InputValue)
            {
                return FindLedge();
            }
            else
            {
                //Find the ledge when is NOT set to automatic
                if (!string.IsNullOrEmpty(HitTransform))
                {
                    FindLedge();
                }
            }
            return false;
        }

        public bool FindLedge()
        {
            foreach (var p in UpdatedProfiles)
            {
                if (p.OnlyGrounded && !animal.Grounded) continue; //Check Ray only when the animal is grounded
                if (p.MaxVSpeed != 0 && p.MaxVSpeed > animal.CurrentSpeedModifier.Vertical.Value) continue; //Do not check when the speed does not match
                if (p.LastState != null && p.LastState != animal.ActiveStateID) continue; //Check if the Last State is the same as the current state

                //Check if we are in Vertical Speed Range
                // if (p.MaxVSpeed == 0 || p.MaxVSpeed <= animal.CurrentSpeedModifier.Vertical.Value)
                {
                    var LedgeForwardPoint1 = transform.TransformPoint(new Vector3(0, p.Height, 0)) + DeltaPos;
                    var WallPoint1 = animal.transform.TransformPoint(new Vector3(0, p.Height - p.LedgeExitDistance - WallChecker, 0)) + DeltaPos;

                    var ForwardDistance = ForwardLength * ScaleFactor * p.ForwardMultiplier;
                    var LedgeExitDistance = p.LedgeExitDistance * ScaleFactor;
                    var LedgeDownPoint1 = LedgeForwardPoint1 + (Forward * ForwardDistance);


                    if (animal.debugGizmos)
                    {
                        MDebug.DrawRay(LedgeForwardPoint1, (Forward * ForwardDistance), Color.green);
                        MDebug.DrawRay(WallPoint1, (Forward * ForwardDistance), Color.yellow);
                        MDebug.DrawRay(LedgeDownPoint1, -Up * LedgeExitDistance, Color.red);
                    }

                    if (p.CheckUpwards)
                    {
                        var ChestPoint = animal.transform.TransformPoint(new Vector3(0, animal.Height, 0)) + DeltaPos;

                        MDebug.DrawLine(ChestPoint, LedgeForwardPoint1, Color.gray);

                        if (Physics.Linecast(ChestPoint, LedgeForwardPoint1, out _, LedgeLayer.Value, IgnoreTrigger))
                        {
                            MDebug.DrawLine(ChestPoint, LedgeForwardPoint1, Color.red);

                            continue;
                        }
                    }

                    var seg = 3f;

                    //Cast the first Ray--- to see if there nothing in front of the character
                    //No walls poiting forward 
                    if (Physics.Raycast(LedgeForwardPoint1, Forward, out _, ForwardDistance, LedgeLayer.Value, IgnoreTrigger) == false)
                    {
                        //Check Ledge Pointing Down the Second First Ray
                        if (Physics.Raycast(LedgeDownPoint1, -Up, out FoundLedgeHit, LedgeExitDistance, LedgeLayer.Value, IgnoreTrigger))
                        {
                            MDebug.DrawRay(FoundLedgeHit.point, FoundLedgeHit.normal, Color.cyan, seg);

                            var LedgeAngle = Vector3.Angle(FoundLedgeHit.normal, Up);

                            //Do not Grab ledge on a Slope Angle
                            //We need to not find wall 
                            if (LedgeAngle < animal.SlopeLimit)
                            {
                                if (Physics.Raycast(WallPoint1, Forward, out FoundWallHit, ForwardDistance, LedgeLayer.Value, IgnoreTrigger))
                                {
                                    //Debug.DrawRay(FoundWallHit.point, FoundWallHit.normal * 2, Color.white, 2);

                                    var WallAngle = Vector3.Angle(FoundWallHit.normal, Up);

                                    if (Mathf.Abs(WallAngle - LedgeAngle) < MinTerrainAngle) continue; //Hack to avoid grabbing Weird Ledge Angles

                                    var CrossLedgeHit = Vector3.Cross(-FoundLedgeHit.normal, FoundWallHit.normal);
                                    WallNormal = Vector3.Cross(FoundLedgeHit.normal, CrossLedgeHit).normalized;

                                    //Find the Correct Orientation
                                    var Y_Point = MTools.ClosestPointOnPlane(transform.position, WallNormal, FoundLedgeHit.point);

                                    MDebug.DrawWireSphere(Y_Point, Color.green, 0.02f, seg);

                                    Y_Point = Y_Point.ClosestPointOnLine(transform.position + (UpVector * 5), transform.position - (UpVector * 5));

                                    MDebug.DrawWireSphere(Y_Point, Color.yellow, 0.02f, seg);

                                    //  var CloseEdgePoint = FoundWallHit.collider.ClosestPoint(Y_Point);

                                    var H_Point = MTools.ClosestPointOnPlane(FoundWallHit.point, WallNormal, transform.position);


                                    LedgeProfile = p; //Store the current Ledge Profile


                                    var YAxis = Vector3.Distance(Y_Point, transform.position) + (LedgeProfile.AlingOffset.y * ScaleFactor);

                                    var ZAxis = (wallDistance * ScaleFactor) - Vector3.Distance(H_Point, transform.position)
                                        + (LedgeProfile.AlingOffset.x * ScaleFactor);


                                    var UPDifference = YAxis * FoundLedgeHit.normal;
                                    var HorizontalDifference = ZAxis * WallNormal;

                                    AlignmentOffset = UPDifference + (HorizontalDifference);
                                    AngleDifference = Vector3.SignedAngle(Forward, -WallNormal, Up); //?????


                                    //animal.SetPlatform(FoundWallHit.transform); //We need the Platform for moving grabing ledges    
                                    //CheckKinematic();

                                    StartPosition = Position;
                                    StartRotation = Rotation;

                                    TargetPosition = StartPosition + (AlignmentOffset);

                                    // Debugging($"Distance Ledge and Start Position: {Vector3.Distance(Y_Point, StartPosition):F3}");

                                    #region Debug
                                    MDebug.DrawRay(FoundLedgeHit.point, CrossLedgeHit, Color.white, seg);
                                    MDebug.DrawRay(FoundLedgeHit.point, WallNormal * 5, Color.green, seg);

                                    // MDebug.DrawWireSphere(CloseEdgePoint, Color.blue, 0.1f, seg);

                                    MDebug.DrawWireSphere(Y_Point, Color.red, 0.1f, seg);
                                    MDebug.DrawWireSphere(FoundLedgeHit.point, Color.yellow, 0.1f, seg);


                                    MDebug.DrawWireSphere(H_Point, Color.red, 0.1f, seg);
                                    MDebug.DrawWireSphere(transform.position, Color.yellow, 0.1f, seg);

                                    MDebug.DrawLine(Y_Point, FoundLedgeHit.point, Color.yellow, seg);
                                    MDebug.DrawLine(H_Point, transform.position, Color.red, seg);
                                    MDebug.DrawWireSphere(StartPosition, Color.white, 0.02f, seg);

                                    MDebug.DrawWireSphere(TargetPosition, Color.green, 0.02f, seg);

                                    MDebug.DrawLine(StartPosition, TargetPosition, Color.white, seg);
                                    #endregion

                                    //  WallNormal = FoundWallHit.normal;
                                    OrientToWall = p.Orient;


                                    m_HitTransform.position = Y_Point;

                                    EnableHitTransform(true);

                                    Debugging($"Try [Ledge-Grab] Wall and Ledge found. <B>[{p.name}]</B>. Wall-Hit Difference: [{HorizontalDifference}]");
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            EnableHitTransform(false);
            return false;
        }


        public override void Activate()
        {
            base.Activate();

            LedgeProfile ??= profiles[0]; //If there's no profile set the first one

            SetEnterStatus(LedgeProfile.EnterStatus);
            animal.Reset_Movement(); //Remove all Input stuff
            animal.Force_Remove(); //Remove all forces when grabbing a ledge

            EnableHitTransform(false);
            animal.InertiaPositionSpeed = Vector3.zero; //Remove internia
            animal.AdditivePosition = Vector3.zero; //Remove additive
            CheckKinematic();
            animal.SetPlatform(FoundLedgeHit.transform);


            // StartPosition = transform.InverseTransformPoint(StartPosition);

            AlignDir = (TargetPosition - StartPosition) / 2;
            AlignDirDelta = Vector3.zero;

            UpdateProfileLastState();
        }

        private Vector3 AlignDir;
        private Vector3 AlignDirDelta;

        private void CheckKinematic()
        {
            animal.InertiaPositionSpeed = Vector3.zero;         //Remove internia
            animal.DeltaPos = Vector3.zero;                     //Remove Delta position
            animal.DeltaRootMotion = Vector3.zero;              //Remove Delta position

            if (Kinematic.Value)
            {
                animal.RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                animal.RB.isKinematic = true;
            }

            //Disable the Main Collider while is doing the state
            if (DisableMainCollider.Value && animal.MainCollider)
                animal.MainCollider.enabled = false;
        }

        private bool InTransition;
        private bool ExitTransition;
        private bool InClimb;

        public override void OnStateMove(float deltatime)
        {
            if (InCoreAnimation)
            {
                InTransition = false;

                if (Anim.IsInTransition(0))
                {
                    var TransTime = Anim.GetAnimatorTransitionInfo(0).normalizedTime;
                    animal.Reset_Movement();
                    animal.AdditivePosition = Vector3.zero; //Remove additive

                    Quaternion AlignRot = Quaternion.FromToRotation(Forward, -WallNormal) * Rotation;  //Calculate the orientation to Terrain 

                    var delta = Vector3.Lerp(Vector3.zero, AlignDir, TransTime);

                    AlignDirDelta = delta - AlignDirDelta;

                    Position += AlignDirDelta;

                    //Orient to wall 
                    if (OrientToWall)
                        Rotation = Quaternion.Lerp(StartRotation, AlignRot, TransTime);
                    InTransition = true;
                }


                if (!InTransition && !ExitTransition && IsActiveState)
                {
                    //TargetPosition += animal.DeltaPlatformPos;
                    //animal.transform.position = TargetPosition;
                    ExitTransition = true;
                    animal.Reset_Movement();
                    //Debug.Log("ExitTransition");
                }


                animal.InertiaPositionSpeed = Vector3.zero; //Remove internia
                //animal.PlatformMovement();

                if (LedgeProfile != null)
                {
                    if (LedgeProfile.Orient)
                    {
                        float DeltaAngle = Mathf.Lerp(0, AngleDifference, deltatime * LedgeProfile.OrientSmoothness * 2f);
                        AngleDifference -= DeltaAngle;
                        //animal.AdditiveRotation *= Quaternion.Euler(0, DeltaAngle, 0); //NOT WORKING DON't KNWO WHY
                        animal.transform.rotation *= Quaternion.Euler(0, DeltaAngle, 0);
                    }

                    if (LedgeProfile.AdditivePosition)
                    {
                        var time = animal.AnimState.normalizedTime;
                        //  Debug.Log($"animal { time:F3}");
                        animal.AdditivePosition += deltatime * LedgeProfile.HeightCurve.Evaluate(time) * LedgeProfile.HeightSpeed * Up;
                        animal.AdditivePosition += deltatime * LedgeProfile.ForwardCurve.Evaluate(time) * LedgeProfile.ForwardSpeed * Forward;
                    }
                }

                if (animal.AnimState.normalizedTime > LedgeProfile.ExitTime)
                {
                    IsPersistent = false;
                }
            }
        }

        public override void TryExitState(float DeltaTime)
        {
            if (animal.AnimState.normalizedTime > LedgeProfile.ExitTime) //Exit after the Current Ledge Profile time
            {
                AllowExit();
                animal.Grounded = true;
                //animal.CheckIfGrounded();
                Debugging($"Allow Exit - {LedgeProfile.name} After Exit Time {animal.AnimState.normalizedTime:F3} > {LedgeProfile.ExitTime}");
            }
        }

        public override void NewActiveState(StateID newState)
        {
            UpdateProfileLastState();

            Automatic_By_State = false;

            if (automaticByState.Count > 0)
            {
                Automatic_By_State = automaticByState.Contains(newState);
            }

            InClimb = newState == StateEnum.Climb;
        }

        private void UpdateProfileLastState()
        {
            UpdatedProfiles = profiles;
            var FilterLastState = profiles.FindAll(p => p.LastState != null && p.LastState == animal.ActiveStateID);
            if (FilterLastState != null && FilterLastState.Count != 0) UpdatedProfiles = FilterLastState; //Find all profiles using the last state
        }


        /// <summary> Using this to show if a surface can be climb  </summary>
        private void EnableHitTransform(bool v)
        {
            m_HitTransform.gameObject.SetActive(v);
        }

        public override void ResetStateValues()
        {
            UpdatedProfiles = profiles;
            LedgeProfile = null;
            InTransition = false;
            ExitTransition = false;
            OrientToWall = false;
            AngleDifference = 0;

            StartPosition =
            TargetPosition =
            WallNormal =
            AlignmentOffset = Vector3.zero;

            StartRotation = Quaternion.identity;

            FoundLedgeHit = new RaycastHit();
            FoundWallHit = new RaycastHit();

            if (Kinematic.Value && animal)
                animal.RB.isKinematic = false;



            //Hide the Hit Transform
            if (m_HitTransform)
            {
                EnableHitTransform(false);
                m_HitTransform.ResetLocal();
            }

            //Restore Main Collider 
            if (DisableMainCollider.Value && animal.MainCollider)
                animal.MainCollider.enabled = true;
        }


#if UNITY_EDITOR
        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Ledge Grab does not require a Speed Set
        }


        public override void StateGizmos(MAnimal animal)
        {
            if (Application.isPlaying) return;

            foreach (var p in profiles)
            {
                var point1 = animal.transform.TransformPoint(new Vector3(0, p.Height, 0));
                var pointWall1 = animal.transform.TransformPoint(new Vector3(0, p.Height - p.LedgeExitDistance - WallChecker, 0));

                var scale = animal.ScaleFactor;

                var dir = ForwardLength * p.ForwardMultiplier * scale * animal.Forward;
                var dirWall = scale * wallDistance * animal.Forward;
                var point2 = point1 + dir;
                var downExit = p.LedgeExitDistance * scale * -animal.Up;


                if (p.CheckUpwards)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(point1, animal.transform.TransformPoint(new Vector3(0, animal.Height, 0)));
                }


                Gizmos.color = Color.green;

                Gizmos.DrawRay(point1, dir);
                // Gizmos.DrawRay(point2, downDir);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(pointWall1, dir);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(pointWall1, dirWall);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(point2, downExit);
            }

        }
        internal override void Reset()
        {
            base.Reset();

            automatic.Value = true;

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                RootMotion = true,
                AdditivePosition = true,
                AdditiveRotation = false,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                Gravity = false,
                CustomRotation = false,
                FreeMovement = false,
                IgnoreLowerStates = true,
            };

            profiles = new List<LedgeProfiles>();

            var prof = new LedgeProfiles();

            profiles.Add(prof);

            Input = "Jump";

            Editor_Tabs1 = 3;
        }
#endif
    }
    [System.Serializable]
    public class LedgeProfiles
    {
        public string name = "Ledge Grab";

        [Tooltip("State Enter Status to Activate while")]
        public int EnterStatus = 0;

        [Tooltip("Max Vertical Speed Needed to Check this Profile")]
        public float MaxVSpeed = 0;

        [Tooltip("Check the Last State as a condition to activate the profile")]
        public StateID LastState;

        [Tooltip("Cast a Ray Upwards to check if there's a roof blocking the ledge")]
        public bool CheckUpwards = false;

        [Tooltip("The Ledge will be check only if the character is grounded")]
        public bool OnlyGrounded = false;

        [Tooltip("Forward Length Multiplier applied to the Global Length")]
        public float ForwardMultiplier = 1;

        [Tooltip("Height Offset to cast the Ray for checking a ledge")]
        [Min(0)] public float Height = 1.5f;

        [Tooltip("Ray to check if we have found a ledge")]
        [Min(0)] public float LedgeExitDistance = 0.25f;

        [Tooltip("If the Animation Normalized Time of this state (Ledge Grab) is greater Exit Animation time,\n" +
            " the State will Allow Exit()... so other states can try activate themselves.")]
        [Range(0, 1)] public float ExitTime = 0.9f;

        [Tooltip("Horizontal(X) and Vertical(Y) values needed to apply offset movement to have better alignment with the Ledge")]
        public Vector2 AlingOffset;


        [Tooltip("Align the character to the Wall's normal direction")]
        public bool Orient = true;

        [Tooltip("Smoothness value to align the character to the wall")]
        [Hide("Orient", false)]
        [Min(0)] public float OrientSmoothness = 10f;

        public bool AdditivePosition = false;

        [Hide("AdditivePosition", false)]
        [Min(0)] public float HeightSpeed = 0.5f;
        [Hide("AdditivePosition", false)]
        [Min(0)] public float ForwardSpeed = 0.5f;

        [Hide("AdditivePosition", false)]
        public AnimationCurve HeightCurve = new(
               new Keyframe(0, 1), new Keyframe(0.45f, 1), new Keyframe(0.55f, 0f), new Keyframe(1, 0f)
            );

        [Hide("AdditivePosition", false)]
        public AnimationCurve ForwardCurve = new(
              new Keyframe(0, 0), new Keyframe(0.45f, 0), new Keyframe(0.55f, 1f), new Keyframe(1, 1f)
           );
    }
}
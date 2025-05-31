using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Controller
{

    [AddTypeMenu("Climb/Free Climb")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/climb")]
    /// <summary>Climb Logic </summary>
    public class Climb : State
    {
        [System.Serializable]
        public struct ClimbRayOverride
        {
            public StateID state;
            public float length;
            public float sprintLength;
        }

        // public override string StateName => "Climb/Free Climb";
        public override string StateIDName => "Climb";

        /// <summary>Air Resistance while falling</summary>
        [Header("Climb Parameters"), Space]
        [Tooltip("Layer to identify climbable surfaces")]
        public LayerReference ClimbLayer = new(1);



        //[Tooltip("Tag used to identify climbable surfaces. Default: [Climb]")]
        //public StringReference SurfaceTag =  new StringReference("Climb");
        public PhysicsMaterial Surface;
        [Tooltip("Climb automatically when is near a climbable surface")]
        public BoolReference automatic = new();

        [Tooltip("Disable the Main Collider while the state is active (Main Collider can Interfiere with the animation)")]
        public BoolReference DisableMainCollider = new(true);

        [Tooltip("Disable Moving on Left and Right while climbing")]
        public BoolReference NoHorizontal = new();

        [Tooltip("The Wall to climb is always Vertical")]
        public BoolReference StraigthWall = new();

        /// <summary>Air Resistance while falling</summary>
        [Header("Climb Pivot"), Space]

        [Tooltip("Pivot to Cast a Ray from the Chest")]
        public Vector3 ClimbChest = Vector3.up;
        [Tooltip("Pivot to Cast a Ray from the Hip")]
        public Vector3 ClimbHip = Vector3.zero;
        [Tooltip("Distance of the Climb Point Ray")]
        public float ClimbRayLength = 1f;
        [Tooltip("Distance of the Climb Point Ray when sprinting")]
        public float SprintClimbRayLength = 2f;


        [Tooltip("LedgeGrab will be set automatic if any of these state are playing")]
        public List<StateID> automaticByState = new();
        public bool Automatic_By_State { get; private set; }
        [Tooltip("Changes the Ray Lenght depending the current active state")]
        public List<ClimbRayOverride> ClimbRayLengthOverride = new();

        protected float CurrentRayLength;
        protected float CurrentRayLengthSprint;

        [Tooltip("Disable the pivot chest to calculate Falling from Pivot Hip instead")]
        public bool DisablePivotChest = true;

        [Tooltip("Radius of the Ray to find a Climable Wall")]
        [Min(0.01f)] public float m_rayRadius = 0.1f;

        [Header("Wall Detection"), Space]
        [Tooltip("This is the distance to align the character to the wall while climbing")]
        public float WallDistance = 0.2f;

        [Tooltip("When using the Start Climb animations this is the distance to position the character in the correct position on the wall")]
        public float StartWallDistance = 0f;

        [Tooltip("Smoothness value to align the animal to the wall")]
        public float AlignSmoothness = 10f;
        [Tooltip("Distance from the Hip Pivot to the Ground to exit the state while climbing down.")]
        public float GroundDistance = 0.5f;
        [Tooltip("Length of the Horizontal Rays to detect Inner Corners")]
        [Min(0)] public float InnerCorner = 0.4f;
        [Min(0)] public float InnerCornerLerp = 5f;
        [Tooltip("Speed Multiplier to move faster around outter corners")]
        [Min(0)] public float OuterCornerSpeed = 0.4f;
        [Range(0, 90)] public float ExitSlope = 30f;

        [Tooltip("Transform Created to Store the Hit Position of the Climb Rays. Use it to show UI When a WAll ")]
        public string HitTransform = "ClimbHit";


        [Header("Ledge Detection (Exit Status: Climb Ledge)")]
        [Tooltip("Checks On top of the Climb to Exit\nDisable this if your Animal has the <[Grab Ledge]> State")]
        public bool UseLedgeDetection = true;

        [Hide("UseLedgeDetection", false)]
        public string LedgeTag = "ClimbEdge";
        private int LedgeHash;

        [Tooltip("Offset Position to Cast the First Ray on top on the animal to find the ledge")]
        [Hide("UseLedgeDetection", false)]
        public Vector3 RayLedgeOffset = Vector3.up;

        [Tooltip("Length of the Ledge Ray")]
        [Hide("UseLedgeDetection", false)]
        [Min(0)] public float RayLedgeLength = 0.4f;

        [Tooltip("MinDistance to exit Climb over a ledge")]
        [Hide("UseLedgeDetection", false)]
        [Min(0)] public float LedgeExitDistance = 0.175f;

        [Tooltip("Minimun Ledge Angle ")]
        [Hide("UseLedgeDetection", false)]
        [Min(0)] public float MinLedgeAngle = 65f;

        [Header("Exit State Status")]
        [Tooltip("When the Exit Condition Climb Up an edge is executed. The State Exit Status will change to this value")]
        public int ClimbLedge = 1;
        [Tooltip("When the Exit Condition Climb Off is executed. The State Exit Status will change to this value")]
        public int ClimbOff = 2;
        [Tooltip("If the character is Climbing Down and a Ground is found. The State Exit Status will change to this value to execute the ClimbDown Animation")]
        public int ClimbDown = 3;
        [Tooltip("When the Exit Condition Climb Down to the Ground executed. The State Exit Status will change to this value")]
        public int ClimbExitSlope = 4;

        public float RayRadius => m_rayRadius * animal.ScaleFactor;

        /// <summary> Reference for the current Climbable surface</summary>
        public Transform WallChest { get; private set; }

        /// <summary> The Animal is on an Inner Corner</summary>
        private bool InInnerCorner;

        private readonly RaycastHit[] EdgeHit = new RaycastHit[1];

        private RaycastHit HitChest;
        private RaycastHit HitHip;
        private RaycastHit HitSide;


        private Transform m_HitTransform;

        // private bool DefaultCameraInput;
        //private bool WallClimbTag;

        /// <summary> World Position of the Climb Pivot </summary>
        public Vector3 ClimbPivotChest(Transform transform) => transform.TransformPoint(ClimbChest);
        public Vector3 ClimbPivotHip(Transform transform) => transform.TransformPoint(ClimbHip);


        /// <summary> Average Normal of the Wall</summary>
        public Vector3 AverageNormal { get; private set; }

        /// <summary>Check if the Current Wall its a valid wall</summary>
        public Transform ValidWall// {get; private set; }
        {
            get => validwall;
            set
            {
                validwall = value;
                //   Debug.Log($"ValidWall [{value}]");
            }
        }
        Transform validwall;


        /// <summary>LastClimbableWall</summary>
        public Transform LastWall { get; private set; }

        /// <summary>Valid Exit on Ledge</summary>
        public bool ExitOnLedge { get; private set; }

        /// <summary> Angle From the Up Vector and the Wall Normal</summary>
        public float WallAngle { get; private set; }

        /// <summary> Check the states that will make the state automatic </summary>
        /// <param name="newState"></param>
        public override void NewActiveState(StateID newState)
        {
            if (newState == ID) return; //Do nothing if we are on the same state


            //Automatic By State!
            Automatic_By_State = false;
            if (automaticByState.Count > 0)
                Automatic_By_State = automaticByState.Contains(newState);

            if (ClimbRayLengthOverride.Count > 0)
            {
                CurrentRayLength = ClimbRayLength;
                CurrentRayLengthSprint = SprintClimbRayLength;

                var newOverride = ClimbRayLengthOverride.FirstOrDefault(x => x.state == newState);

                if (newOverride.state != null)
                {
                    CurrentRayLength = newOverride.length;
                    CurrentRayLengthSprint = newOverride.sprintLength;
                }
            }
        }

        public override void AwakeState()
        {
            //DefaultCameraInput = animal.UseCameraInput; //Store the Animal Current CameraInput
            base.AwakeState();

            CurrentRayLength = ClimbRayLength;
            CurrentRayLengthSprint = SprintClimbRayLength;


            m_HitTransform = animal.transform.FindGrandChild(HitTransform);

            if (m_HitTransform == null)
            {
                m_HitTransform = new GameObject(HitTransform).transform;
                m_HitTransform.parent = transform;
                m_HitTransform.ResetLocal();
            }

            LedgeHash = Animator.StringToHash(LedgeTag);

            EnableHitTransform(false);
        }

        public override void StatebyInput()
        {
            ValidWall = null; //Reset Valid Wall Always when the state uses Input

            // Debug.Log($"StatebyInput InputValue {InputValue} ExitInputValue{ExitInputValue} {CheckClimbRay()}");
            if (InputValue && CheckClimbRay())
            {
                Activate();
            }
        }

        public override void StateExitByInput()
        {
            SetExitStatus(ClimbOff);
            Debugging($"Exit with Climb Input [{ExitInput.Value}]");
        }

        private bool FoundTargetPos = false;

        public override void Activate()
        {
            FoundTargetPos = false;

            TargetPosition =
            StartPosition = animal.Position; //Reset start and target position
            AverageNormal = -transform.forward;

            ValidWall = CheckClimbRay();

            if (ValidWall) //it cannot be activated there's no Wall to Climb
            {
                base.Activate();
                animal.UseCameraInput = false;       //Climb cannot use Camera Input

                if (DisablePivotChest) animal.DisablePivotChest();
                InInnerCorner = /*InOuterCorner =*/ false;

                EnableHitTransform(false);

                animal.Reset_Movement(); //Remove all Input stuff
                animal.Force_Remove(); //Remove all forces when grabbing a ledge

                //Disable the Main Collider while is doing the state
                if (DisableMainCollider.Value && animal.MainCollider)
                    animal.MainCollider.enabled = false;
            }
        }

        public override bool TryActivate()
        {
            var newWall = CheckClimbRay();
            //Debug.Log($"newWall {newWall}");

            if (animal.MovementDetected && animal.VerticalSmooth > 0.9f && automatic.Value || Automatic_By_State)
            {
                ValidWall = newWall;

                return (ValidWall != null);
            }

            return false;
        }

        public override void ResetStateValues()
        {
            WallChest = null;
            WallAngle = 90;
            ExitOnLedge = false;
            animal.ResetCameraInput();
            InInnerCorner = /*InOuterCorner =*/false;
            ExitInputValue = false;


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

        public override void RestoreAnimalOnExit()
        {
            if (DisablePivotChest) animal.ResetPivotChest();

            animal.ResetCameraInput();
        }

        private Vector3 StartPosition { get; set; }
        private Vector3 TargetPosition { get; set; }

        /// <summary>Current Direction Speed Applied to the Additional Speed, by default is the Animal Forward Direction</summary>
        public override Vector3 Speed_Direction()
        {
            return Up * (animal.VerticalSmooth) + Right * animal.HorizontalSmooth; //IMPORTANT OF ADDITIONAL SPEED
        }

        private Transform CheckClimbRay()
        {
            //Do nothing when the Animal is changing on the inner corner. Keep the Last Valid wall
            if (InInnerCorner) return ValidWall;

            var Point_Chest = ClimbPivotChest(transform);
            var Point_Hip = ClimbPivotHip(transform);

            var ClimbRayLength = !animal.Sprint ? CurrentRayLength : CurrentRayLengthSprint; //Find the override

            var Length = animal.ScaleFactor * ClimbRayLength;

            HitChest = new RaycastHit();
            HitHip = new RaycastHit();

            var ForwardScale = Forward * ScaleFactor;


            if (GizmoDebug)
            {
                MDebug.DrawRay(Point_Chest, ForwardScale * ClimbRayLength, Color.green);
                MDebug.DrawRay(Point_Chest, ForwardScale * WallDistance, Color.red);

                MDebug.DrawRay(Point_Hip, ForwardScale * ClimbRayLength, Color.green);
                MDebug.DrawRay(Point_Hip, ForwardScale * WallDistance, Color.red);
            }


            //  var ValidWall = false;
            AverageNormal = -ForwardScale;

            if (Physics.SphereCast(Point_Chest, RayRadius, Forward, out HitChest, Length, ClimbLayer.Value, IgnoreTrigger))
            {
                var valid = HitChest.collider.sharedMaterial == Surface;

                if (valid)
                {
                    AverageNormal = HitChest.normal;
                    DebugRays(HitChest.point, HitChest.normal);
                    //ValidWall = HitChest.transform;

                    if (Physics.SphereCast(Point_Hip, RayRadius, Forward, out HitHip, Length, ClimbLayer.Value, IgnoreTrigger))
                    {
                        valid = HitHip.collider.sharedMaterial == Surface;
                        DebugRays(HitHip.point, HitHip.normal);

                        var Dir = HitChest.point - HitHip.point;

                        var avNorm = AverageNormal + HitHip.normal;
                        var avNorm2 = Vector3.Cross(Dir, -avNorm);
                        var AverageResult = Vector3.Cross(Dir, avNorm2);

                        #region Debug Normal
#if UNITY_EDITOR
                        if (m_debug && animal.debugGizmos)
                        {
                            var Pos = (HitChest.point + HitHip.point) / 2;
                            MDebug.DrawLine(HitChest.point, HitHip.point, Color.green);
                            MDebug.DrawRay(Pos, AverageResult.normalized * 0.5f, Color.white);
                        }
#endif
                        #endregion

                        AverageNormal = AverageResult;

                        if (!FoundTargetPos)
                        {
                            //TargetPosition = Position + (Forward * HitHip.distance) + (Forward * StartWallDistance);
                            // TargetPosition = Position + Vector3.ProjectOnPlane((Forward * HitHip.distance) + (Forward * StartWallDistance), HitHip.normal);

                            TargetPosition = Position.ProjectPointOnPlane(HitHip.normal, HitHip.point);
                            TargetPosition += -HitHip.normal.normalized * StartWallDistance;

                            //Debug.Log($"TargetPosition {TargetPosition}");

                            Quaternion AlignRot = Quaternion.FromToRotation(Forward, -HitHip.normal) * Rotation;  //Calculate the orientation to Terrain 
                            Quaternion Inverse_Rot = Quaternion.Inverse(Rotation);
                            Quaternion Target = Inverse_Rot * AlignRot;

                            //TargetPosition = transform.DeltaPositionFromRotate(Position + HitHip.point, Target);

                            var Dtime = 0;

                            MDebug.DrawWireSphere(TargetPosition, Color.yellow, 0.05f * ScaleFactor, Dtime);
                            MDebug.DrawWireSphere(TargetPosition, Color.yellow, 0.04f * ScaleFactor, Dtime);
                            MDebug.DrawWireSphere(TargetPosition, Color.yellow, 0.03f * ScaleFactor, Dtime);
                            MDebug.DrawWireSphere(HitHip.point, Color.cyan, 0.05f * ScaleFactor, Dtime);
                            // MDebug.DrawLine(HitHip.point, TargetPosition, Color.cyan, Dtime);

                            // ValidWall = HitHip.transform;
                            //AverageNormal += HitHip.normal;

                        }
                    }

                    //Set new Platform
                    if (IsActiveState && animal.platform != HitChest.transform && HitChest.transform != null)
                        animal.SetPlatform(HitChest.transform);

                    //Get the Wall Angle!!
                    WallAngle = Vector3.SignedAngle(AverageNormal, animal.UpVector, animal.Right);

                    if (animal.activeState != this)
                    {
                        m_HitTransform.position = HitChest.point;
                        EnableHitTransform(valid);
                    }

                    return HitChest.transform;
                }
            }
            else
            {
                if (GizmoDebug)
                {
                    MDebug.DrawWireSphere(Point_Hip + (ClimbRayLength * ScaleFactor * Forward), Color.gray, RayRadius);
                    MDebug.DrawWireSphere(Point_Chest + (ClimbRayLength * ScaleFactor * Forward), Color.gray, RayRadius);
                }

                EnableHitTransform(false);
            }

            return null;
        }

        public override void OnStateMove(float deltatime)
        {
            if (CurrentAnimTag == LedgeHash || CurrentAnimTag == ExitTagHash) //Do nothing
            {
            }
            else if (InCoreAnimation)
            {
                //Remove Horizontal Side movement (This is used for quick Ladder Setups)
                if (NoHorizontal)
                {
                    animal.MovementAxis.x = 0;
                    animal.MovementAxisRaw.x = 0;
                }

                if (LastWall != ValidWall) LastWall = ValidWall;

                var Right = animal.Right;

                ValidWall = CheckClimbRay();

                if (ValidWall)
                {
                    animal.SetPlatform(HitChest.transform);

                    if (MovementRaw.x > 0) //Means is going Right
                    {
                        CalculateSideClimbHit(Right);
                    }
                    else if (MovementRaw.x < 0)//Means is going Left
                    {
                        CalculateSideClimbHit(-Right);
                    }

                    OrientToWall(AverageNormal, deltatime);
                    AlignToWall(HitChest.distance, deltatime);
                }

                if (InInnerCorner)
                {
                    OrientToWall(AverageNormal, deltatime);
                    var Angle = Vector3.Angle(animal.Forward, -AverageNormal);
                    if (Angle < 5f) InInnerCorner = false;
                }
            }
            else if (InEnterAnimation)  //If we are on Climb Start do a quick alignment to the Wall.
            {
                //Do the correct positioning on Start Climb animation
                if (Anim.IsInTransition(0))
                {
                    var TransTime = Anim.GetAnimatorTransitionInfo(0).normalizedTime;
                    animal.AdditivePosition = Vector3.zero;
                    animal.AdditiveRotation = Quaternion.identity;
                    transform.position = Vector3.Lerp(StartPosition, TargetPosition, TransTime);

                    FoundTargetPos = true;
                }
                else
                {
                    //AlignToWall(HitChest.distance, deltatime);
                }

                ValidWall = CheckClimbRay();

                if (StraigthWall.Value)
                {
                    AverageNormal = transform.position - ((HitChest.point + HitHip.point) / 2);
                    AverageNormal = Vector3.ProjectOnPlane(AverageNormal, animal.UpVector).normalized;
                }

                OrientToWall(AverageNormal, deltatime);
                animal.SetPlatform(HitChest.transform);

                //if (CheckClimbRay())
                //{

                //    //   AlignToWall(WallDistance, deltatime);
                //    // CheckMovingWall(HitChest.transform, deltatime);
                //}
            }
        }




        //Align the Animal to the Wall
        private void AlignToWall(float distance, float deltatime)
        {
            float difference = distance - WallDistance * animal.ScaleFactor;

            if (!Mathf.Approximately(distance, WallDistance * animal.ScaleFactor))
            {
                Vector3 align = AlignSmoothness * deltatime * difference * ScaleFactor * animal.Forward;
                animal.Position += align;
            }
        }

        private void OrientToWall(Vector3 normal, float deltatime)
        {

            Quaternion AlignRot = Quaternion.FromToRotation(Forward, -normal) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;

            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, deltatime * AlignSmoothness);      //Calculate the Delta Align Rotation
            animal.AdditiveRotation *= Delta;

            //var AveragePoint = (HitChest.point + HitHip.point) / 2;
            //var pos = transform.DeltaPositionFromRotate(AveragePoint, Delta);

            //MDebug.DrawWireSphere(AveragePoint, Color.cyan, 0.02f * ScaleFactor, 1);
            //MDebug.DrawWireSphere(animal.Position, Color.cyan, 0.02f * ScaleFactor, 1);
            //MDebug.DrawLine(animal.Position, animal.Position + pos, Color.cyan, 1);
            // animal.AdditivePosition += (Delta * (AveragePoint - Position)) * deltatime;  //Rotaton Around the Average Point


            //Update the Rotation to always look Upwards
            var UP = Vector3.Cross(Forward, UpVector);
            UP = Vector3.Cross(UP, Forward);
            AlignRot = Quaternion.FromToRotation(transform.up, UP) * transform.rotation;  //Calculate the orientation to Terrain 
            Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Target = Inverse_Rot * AlignRot;
            animal.AdditiveRotation *= Target;
        }

        public override void TryExitState(float DeltaTime)
        {
            var MainPivot = ClimbPivotChest(transform) + animal.AdditivePosition;

            if (CurrentAnimTag != MainTagHash) return; //Do nothing if the Animal is not on the Main Tag


            //if (CurrentAnimTag == ExitTagHash)
            //{
            //    if (animal.CheckIfGrounded())
            //    {
            //        AllowExit();
            //        return;
            //    }
            //}

            //if (InInnerCorner) return; //Fo nothing when the animal is changing from inner corners

            //The Animal did not touch a Wall Tagged Climb
            if (!ValidWall)
            {
                Debugging("[Allow Exit] Exit when Wall is not Climbable");
                ValidWall = LastWall;
                AllowExit();
                return;
            }

            if (Mathf.Abs(WallAngle) < ExitSlope)//Exit when the angle is max from the slope
            {
                Debugging("[Allow Exit] Slope is walkable");
                AllowExit(StateEnum.Locomotion, ClimbExitSlope); //Force the Idle State to be the next State
                animal.CheckIfGrounded();
                return;
            }

            //Moving Down
            if (MovementRaw.z < 0) //Means the animal is going down
            {
                MDebug.DrawRay(MainPivot, GroundDistance * ScaleFactor * -Up, Color.white);

                //Means that the Animal is going down and touching the ground
                if (Physics.Raycast(MainPivot, -Up, out var hit, ScaleFactor * GroundDistance, animal.GroundLayer, IgnoreTrigger))
                {
                    var FallRayAngle = Vector3.SignedAngle(hit.normal, animal.UpVector, animal.Right);

                    var DeepSlope = Mathf.Abs(FallRayAngle) >= animal.SlopeLimit;

                    if (hit.transform.gameObject != GameObjectHit) //Check if what the Fall Ray Hit was a Debree
                    {
                        GameObjectHit = hit.transform.gameObject;
                        IsDebree = GameObjectHit.CompareTag(animal.DebrisTag);
                    }

                    if (!DeepSlope || IsDebree) //Check if we are not on a deep slope
                    {
                        if (animal.CheckIfGrounded())
                        {
                            Debugging("[Allow Exit] when Grounded and pressing Down and touched the ground");
                            AllowExit(StateEnum.Idle, ClimbDown); //Force the Idle State to be the next State
                        }
                        else
                        {
                            Debugging("[Set Exit Status] ClimbDown when near the Ground");
                            SetExitStatus(ClimbDown);
                            IsPersistent = true;
                        }
                    }
                }

                var Point_Hip = ClimbPivotHip(transform) + DeltaPos;
                var Length = animal.ScaleFactor * ClimbRayLength;

                MDebug.DrawRay(Point_Hip, Forward * Length, Color.white);

                if (!Physics.Raycast(Point_Hip, Forward, out _, Length, animal.GroundLayer, IgnoreTrigger))
                {
                    Debugging("[Allow Exit] No Front Wall ");
                    AllowExit();
                }
            }
            if (!ExitOnLedge) //Means the Animal going Up
            {
                CheckLedgeExit();
            }
        }

        private GameObject GameObjectHit;
        private bool IsDebree;

        /// <summary> Using this to show if a surface can be climb  </summary>
        private void EnableHitTransform(bool v)
        {
            m_HitTransform.gameObject.SetActive(v);
        }

        private void DebugRays(Vector3 p, Vector3 Normal)
        {
#if UNITY_EDITOR
            if (m_debug && animal.debugGizmos)
            {
                MDebug.DrawCircle(p, Normal, RayRadius, Color.green, true);

                // MDebug.DrawWireSphere(p + (Normal * RayRadius), Color.green, RayRadius);
                Debug.DrawRay(p, 2 * RayRadius * Normal, Color.green);
            }
#endif
        }

        /// <summary>Use Climb side</summary>
        /// <param name="Direction"></param>
        private void CalculateSideClimbHit(Vector3 Direction)
        {
            var Ray1 = Color.blue;
            var Ray2 = Color.blue;
            var Ray3 = Color.blue;

            var Forward = animal.Forward;
            // var ScaleFactor = animal.ScaleFactor;
            var CornerLength = InnerCorner * animal.ScaleFactor;
            var point = ClimbPivotChest(transform);

            MovementAxisMult.x = 1;

            if (Physics.Raycast(point, Direction, out HitSide, CornerLength, ClimbLayer, IgnoreTrigger))
            {

                Ray1 = Color.green;

                if (HitSide.collider.sharedMaterial == Surface) //Next Surface is Climbable
                {
                    AverageNormal = HitSide.normal;
                    ValidWall = HitSide.transform;
                    InInnerCorner = true;
                }
            }
            else
            {
                var SecondPoint = point + Direction * CornerLength;
                if (Physics.Raycast(SecondPoint, Forward, out HitSide, CornerLength, ClimbLayer, IgnoreTrigger))
                {
                    Ray2 = Color.green;
                    if (HitSide.transform != HitChest.transform) //Stop if the Surface is not climbable
                    {
                        if (HitSide.collider.sharedMaterial != Surface)
                        {
                            MovementAxisMult.x = 0;
                            animal.additivePosition.x = 0;
                        }
                    }
                }
                else
                {
                    var ThirdPoint = SecondPoint + Forward * CornerLength;

                    if (Physics.Raycast(ThirdPoint, -Direction, out HitSide, CornerLength, ClimbLayer, IgnoreTrigger))
                    {
                        Ray3 = Color.green;
                        if (HitSide.collider.sharedMaterial != Surface)
                        {
                            MovementAxisMult.x = 0;
                            Ray3 = Color.red;
                        }
                        else
                        {
                            animal.AdditivePosition += animal.DeltaTime * OuterCornerSpeed * Direction; //Make a Fast Movement to quickly move to the next corner
                        }
                    }

                    MDebug.DrawRay(ThirdPoint, -Direction * CornerLength, Ray3);
                }

                MDebug.DrawRay(SecondPoint, Forward * CornerLength, Ray2);
            }

            MDebug.DrawRay(point, Direction * CornerLength, Ray1);
        }

        //private Vector3 platform_Pos;
        //private Quaternion platform_Rot;




        private void CheckLedgeExit()
        {
            if (UseLedgeDetection)
            {
                var LedgePivotUP = transform.TransformPoint(ClimbChest + RayLedgeOffset + 2 * m_rayRadius * ScaleFactor * Up) + DeltaPos;

                //Check Upper Ground legde Detection
                // bool LedgeHit = Physics.RaycastNonAlloc(LedgePivotUP, Forward, EdgeHit, ScaleFactor * RayLedgeLength, ClimbLayer.Value, IgnoreTrigger) > 0;
                bool LedgeHit = Physics.Raycast(LedgePivotUP, Forward, out EdgeHit[0], ScaleFactor * RayLedgeLength, ClimbLayer.Value, IgnoreTrigger);

                MDebug.DrawWireSphere(LedgePivotUP, Color.green, 0.01f * ScaleFactor);
                MDebug.DrawWireSphere(EdgeHit[0].point, Color.green, 0.01f * ScaleFactor);
                Debug.DrawRay(LedgePivotUP, RayLedgeLength * ScaleFactor * Forward, Color.green);


                if (!LedgeHit)
                {
                    var SecondRayPivot = new Ray(LedgePivotUP, Forward * ScaleFactor).GetPoint(RayLedgeLength * ScaleFactor);

                    MDebug.DrawWireSphere(SecondRayPivot, Color.green, 0.01f * ScaleFactor);

                    // Debug.DrawRay(SecondRayPivot, 2 * RayLedgeLength * Gravity, Color.green);
                    Debug.DrawRay(SecondRayPivot, LedgeExitDistance * ScaleFactor * Gravity, Color.yellow);

                    //LedgeHit = Physics.RaycastNonAlloc(SecondRayPivot, Gravity, EdgeHit, ScaleFactor * RayLedgeLength * 2, ClimbLayer.Value, IgnoreTrigger) > 0;
                    //  if (LedgeHit)
                    if (Physics.Raycast(SecondRayPivot, Gravity, out var DownHit, ScaleFactor * RayLedgeLength * 2, ClimbLayer.Value, IgnoreTrigger))
                    {
                        // Debug.Break();

                        //var LedgeAngle = Vector3.Angle(DownHit.normal, Up);
                        //var WallAngle = Vector3.Angle(HitHip.normal, Up);

                        MDebug.DrawWireSphere(DownHit.point, Color.white, 0.01f * ScaleFactor);

                        //if ((WallAngle - LedgeAngle) > MinLedgeAngle) //Only check the angles if they match to be a Ledge
                        {
                            if (DownHit.distance > LedgeExitDistance * ScaleFactor)
                            {
                                // LedgeHitDifference = (DownHit.distance - LedgeExitDistance * ScaleFactor);
                                ExitOnLedge = true; //Activate Exit OnLedge
                                Debugging($"Allow Exit - Exit on a Ledge [{DownHit.collider.name}]");
                                SetExitStatus(ClimbLedge); //Keep this State as the Active State
                            }
                        }
                    }
                }
            }
        }



        public override void StateGizmos(MAnimal animal)
        {
            if (m_debug && !Application.isPlaying)
            {
                var Forward = animal.Forward;
                var Right = animal.Right;
                var t = animal.transform;
                var Gravity = animal.Gravity;
                var ScaleFactor = animal.ScaleFactor;


                var Chest_Point = ClimbPivotChest(t);
                var Hip_Point = ClimbPivotHip(t);


                if (UseLedgeDetection)
                {
                    var LedgePivotUP = t.TransformPoint(ClimbChest + RayLedgeOffset);
                    var SecondRayPivot = new Ray(LedgePivotUP, ScaleFactor * ScaleFactor * ScaleFactor * animal.Forward).GetPoint(RayLedgeLength * ScaleFactor);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(SecondRayPivot, 2 * RayLedgeLength * ScaleFactor * Gravity);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(SecondRayPivot, LedgeExitDistance * ScaleFactor * Gravity);

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(LedgePivotUP, RayLedgeLength * ScaleFactor * Forward);
                }

                Gizmos.DrawRay(Chest_Point, ClimbRayLength * ScaleFactor * Forward);
                Gizmos.DrawRay(Hip_Point, ClimbRayLength * ScaleFactor * Forward);
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(Chest_Point, ScaleFactor * WallDistance * Forward);
                Gizmos.DrawRay(Hip_Point, ScaleFactor * WallDistance * Forward);


                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(Chest_Point + Forward * ScaleFactor * (ClimbRayLength - (m_rayRadius * ScaleFactor)), m_rayRadius * ScaleFactor);
                Gizmos.DrawWireSphere(Hip_Point + (ClimbRayLength - (m_rayRadius * ScaleFactor)) * ScaleFactor * Forward, m_rayRadius * ScaleFactor);

                if (!NoHorizontal)
                {
                    Gizmos.DrawRay(Chest_Point, InnerCorner * ScaleFactor * Right);
                    Gizmos.DrawRay(Chest_Point, InnerCorner * ScaleFactor * -Right);
                }
                Gizmos.color = Color.white;
                var MainPivot = ClimbPivotChest(t);
                Gizmos.DrawRay(MainPivot, GroundDistance * ScaleFactor * -animal.Up);
            }
        }

        private void OnValidate()
        {
            LedgeExitDistance = Mathf.Clamp(LedgeExitDistance, 0, RayLedgeLength);

            if (SprintClimbRayLength < ClimbRayLength) SprintClimbRayLength = ClimbRayLength;
        }


#if UNITY_EDITOR
        internal override void Reset()
        {
            base.Reset();

            Surface = MTools.GetResource<PhysicsMaterial>("Climbable");

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
                CustomRotation = true,
                FreeMovement = false,
                IgnoreLowerStates = true,
            };

            //Debug.Log("GUAT = " );

            //m_HitTransform = new GameObject(HitTransform).transform;
            //m_HitTransform.parent = transform;
            //m_HitTransform.ResetLocal();
        }

        public override void SetSpeedSets(MAnimal animal)
        {
            var setName = "Climb";

            if (animal.SpeedSet_Get(setName) == null)
            {
                animal.speedSets.Add(
                    new MSpeedSet()
                    {
                        name = setName,
                        StartVerticalIndex = new IntReference(1),
                        TopIndex = new IntReference(1),
                        states = new List<StateID>(1) { ID },
                        Speeds = new List<MSpeed>() { new MSpeed(setName) }
                    }
                    );
            }
        }
#endif
    }
}
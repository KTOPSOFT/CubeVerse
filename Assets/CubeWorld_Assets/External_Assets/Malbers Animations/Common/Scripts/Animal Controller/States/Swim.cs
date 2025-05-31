using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddTypeMenu("Water/Swim")]
    /// <summary>Swim Logic</summary>
    public class Swim : State
    {
        //public override string StateName => "Swim";
        public override string StateIDName => "Swim";

        #region Public Variables    

        [Header("Swim Paramenters")]
        public LayerMask WaterLayer = 16;


        [Tooltip("Ray to Shoot Down To find the water leve;")]
        public float UpSearch = 3;

        [Tooltip("Lerp value for the animal to stay align to the water level ")]
        [Min(0)] public float AlignSmooth = 10;


        [Tooltip("When entering the water the Animal will sink for a while... Higher values, will return to the surface faster")]
        [Min(0)] public float bounce = 2f;
        //[Tooltip("If the character is inside a water Volume and we cannot find the Water level then the character will be pushed upwards")]
        //[Min(0)] public float upForce = 1f;
        [Tooltip("Lerp value to do the Bounce Feature")]
        [Min(0)] public float bounceLerp = 10;


        [Tooltip("Gives an extra impulse when entering the state using the accumulated  inertia")]
        public bool KeepInertia = true;
        [Tooltip("Spherecast radius to find water using the Water Pivot")]
        [Min(0.01f)] public float m_Radius = 0.1f;

        [Tooltip("Ray to the Front to check if the Animal has touched a Front Ground and it cannot push it")]
        [Min(0)] public float FrontRayLength = 1;

        [Tooltip("When checking he ground, this will be the multiplier for the height value")]
        [Range(0f, 0.9f)]
        public float HeightMult = 0.9f;


        [Header("Reactions")]
        [SerializeReference, SubclassSelector]
        public Reaction OnTouchedWaterEnter;

        [SerializeReference, SubclassSelector]
        public Reaction OnTouchedWaterExit;

        /// <summary>Has the animal found Water</summary>
        [Disable]
        public bool IsInWater;//{ get; protected set; }

        /// <summary>Is the animal pivot above water level</summary>
        [Disable]
        public bool PivotAboveWater;// { get; protected set; }


        /// <summary>The Character has touched Water</summary>
        public bool TouchedWater { get; protected set; }

        protected float EnterWaterTime;
        public MPivots WaterPivot { get; protected set; }

        protected Vector3 WaterNormal = Vector3.up;
        protected Vector3 HorizontalInertia;

        /// <summary>Difference to align the character to the water</summary>
        public Vector3 WaterLine_Difference { get; internal set; }

        /// <summary>WaterLine postion</summary>
        public Vector3 WaterLevel { get; internal set; }

        readonly Vector3 NoWaterLevel = new(0, float.MinValue, 0);

        /// <summary>Value to push forward the Animal while is not finding the water Level</summary>
        [Disable] public Vector3 BounceUp;
        /// <summary>Value to push Down the Animal when entering the water</summary>
        [Disable] public Vector3 BounceDown;

        protected Vector3 BounceUpTarget;
        protected int TryLoopOriginal;
        public Vector3 WaterPivotPoint => WaterPivot.World(animal.transform) + animal.DeltaVelocity;

        /// <summary>Water Collider used on the Sphere Cast</summary>
        protected Collider[] WaterCollider;

        #endregion


        public override void InitializeState()
        {
            WaterPivot = animal.pivots.Find(p => p.name.ToLower().Contains("water"));      //Find the Water Pivot
            if (WaterPivot == null) Debug.LogError("No Water Pivot Found.. please create a Water Pivot");

            WaterCollider = new Collider[1];
            IsInWater = false;
            TouchedWater = false;
            TryLoopOriginal = TryLoop;
        }

        //Checks if the Animal is inside a water volume
        public override bool TryActivate()
        {
            return FindWaterLevel2() && !PivotAboveWater;
        }

        public override void Activate()
        {
            base.Activate();

            HorizontalInertia = Vector3.ProjectOnPlane(animal.DeltaPos, animal.UpVector);

            //Clean the Vector from Forward and Horizontal Influence    
            if (bounce > 0)
                BounceDown = Vector3.Project(animal.DeltaPos, animal.Up);

            BounceUp = Vector3.one * 0.0001f;

            IgnoreLowerStates = true;                                               //Ignore Falling, Idle and Locomotion while swimming 
            animal.UseGravity = false; //IMPORTANT

            //animal.InertiaPositionSpeed = Vector3.zero;                             //THIS MOTHER F!#$ER was messing with the water entering
            animal.Force_Reset();
            WaterNormal = Vector3.up;

            BounceUpTarget = -Gravity * bounce;

            animal.SetPlatform(null);//IMPORTANT
        }

        /// <summary>  Check if the animal is inside a Water Trigger  </summary>
        public bool CheckWater()
        {
            int WaterFound = Physics.OverlapSphereNonAlloc(WaterPivotPoint, m_Radius * animal.ScaleFactor,
                WaterCollider, WaterLayer, QueryTriggerInteraction.Collide);

            if (GizmoDebug) MDebug.DrawWireSphere(WaterPivotPoint, transform.rotation, Color.cyan, m_Radius * animal.ScaleFactor);

            return WaterFound > 0;
        }

        /// <summary>Check if the Animal in a water surface </summary>
        public bool FindWaterLevel2()
        {
            if (WaterPivot == null) return false;

            var UpPoint = WaterPivotPoint + (Vector3.up * (UpSearch * ScaleFactor));
            var RayLength = (UpSearch + WaterPivot.position.y) * ScaleFactor;
            var rad = m_Radius * ScaleFactor;

            if (GizmoDebug)
            {
                MDebug.DrawWireSphere(UpPoint, Color.cyan, rad);
                MDebug.DrawWireSphere(WaterPivotPoint, Color.cyan, rad);
                MDebug.DrawWireSphere(UpPoint + (Gravity * RayLength), Color.cyan, rad);
            }

            if (Physics.Raycast(UpPoint, Gravity, out RaycastHit WaterHit, RayLength, WaterLayer, QueryTriggerInteraction.Collide))
            {
                WaterLevel = WaterHit.point;  //Find the water Level
                WaterNormal = WaterHit.normal;

                if (!TouchedWater)
                {
                    TouchedWater = true;
                    OnTouchedWaterEnter?.React(animal);
                    TryLoop = 1; //Force using TryLoop1
                }

                Vector3 PointBelow = WaterPivotPoint - WaterLevel;
                PivotAboveWater = Vector3.Dot(PointBelow, Gravity) < 0; //Check if the point is below the water
                IsInWater = !PivotAboveWater;

                if (!IsInWater && MTools.DoSpheresIntersect(WaterLevel, m_Radius, WaterPivotPoint, m_Radius))
                {
                    IsInWater = true;
                }

                if (GizmoDebug)
                {
                    Debug.DrawLine(UpPoint, WaterLevel, Color.blue + Color.cyan);
                    MDebug.DrawWireSphere(WaterLevel, Color.white, rad);
                    Debug.DrawRay(WaterLevel, WaterPivot.position.y * HeightMult * Gravity, Color.white);
                }
                return IsInWater;
            }
            else
            {
                if (TouchedWater)
                {
                    TouchedWater = false;
                    OnTouchedWaterExit?.React(animal);
                    TryLoop = TryLoopOriginal; //Reset TryLoop
                }
                if (GizmoDebug) Debug.DrawRay(UpPoint, ScaleFactor * UpSearch * Gravity, Color.cyan);

                IsInWater = false;
                return IsInWater;
            }
        }


        /// <summary> The animal cast a ray to the ground and if the ground is hitted then is not longer in the water</summary>
        public bool CheckNearGround()
        {
            var length = HeightMult * WaterPivot.position.y * ScaleFactor;

            if (GizmoDebug)
            {
                Debug.DrawRay(WaterPivotPoint, length * Gravity, Color.cyan);
                MDebug.DrawWireSphere(WaterPivotPoint + (length * Gravity), Color.cyan, 0.1f);
            }

            if (Physics.Raycast(WaterPivotPoint, Gravity, out RaycastHit NearGround, length, animal.GroundLayer, IgnoreTrigger))
            {
                float GroundSlope = Vector3.Angle(NearGround.normal, animal.UpVector);
                BounceDown = Vector3.zero; //If we touched Ground Remove all Down Bounce
                return (GroundSlope < animal.SlopeLimit);
            }

            return false;
        }

        public override void TryExitState(float DeltaTime)
        {
            //  Debug.Log($"IsInWater {IsInWater},  CheckNearGround() : {CheckNearGround()}  PivotBelowWater {!PivotAboveWater}");

            bool NearGround = CheckNearGround();
            if (BounceUp != Vector3.zero) return;

            if (!IsInWater || NearGround)
            {
                Debugging("[Allow Exit] No Longer in water");
                animal.CheckIfGrounded();
                AllowExit();
            }
        }



        public override void OnStateMove(float deltatime)
        {
            if (KeepInertia) AddInertia(ref HorizontalInertia, 3, deltatime);

            WaterNormal = animal.UpVector; //have a normal water vector first

            FindWaterLevel2();

            //Aling-rotate the Animal to the Water 
            animal.AlignRotation(WaterNormal, deltatime, AlignSmooth > 0 ? AlignSmooth : 5);

            WaterLine_Difference = Vector3.Project((WaterLevel - WaterPivotPoint), UpVector);

            BounceEnteringWater(deltatime);


            var rayColor = (Color.blue + Color.cyan) / 2;

            //HACK so it does not come out of the water on incline deep slopes
            if (FrontRayLength > 0 &&
                Physics.Raycast(WaterPivotPoint, Forward, out RaycastHit FrontRayWater, FrontRayLength, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                var FrontPivot = Vector3.Angle(FrontRayWater.normal, animal.UpVector);

                rayColor = Color.cyan;

                if (FrontPivot > animal.SlopeLimit) //BAD Angle Slope
                {
                    rayColor = Color.black;
                    {
                        Position += WaterLine_Difference;
                        animal.ResetUPVector();
                    }
                }
            }
            else
            {
                if (IsInWater)
                {
                    if (AlignSmooth > 0)
                    {
                        Position += Vector3.Lerp(Vector3.zero, WaterLine_Difference, (deltatime * AlignSmooth));
                    }
                    else
                    {
                        Position += WaterLine_Difference;
                        animal.ResetUPVector();
                    }
                }

            }
            if (GizmoDebug) Debug.DrawRay(WaterPivotPoint, animal.Forward * FrontRayLength, rayColor);
        }


        /// <summary>
        /// IMPROVEEEEEEEEEEEEEE THIS
        /// </summary>
        /// <param name="delta"></param>
        private void BounceEnteringWater(float delta)
        {
            if (BounceUp != Vector3.zero)
            {
                //Push the animal upwards if there we are inside the water volume
                BounceDown =
                  Vector3.Lerp(BounceDown, Vector3.zero, bounceLerp * delta);

                animal.AdditivePosition += (BounceDown); //Buoyance  Down!!

                //Push the animal upwards if there we are inside the water volume
                BounceUp =
                  Vector3.Lerp(BounceUp, BounceUpTarget, bounceLerp * delta);

                var NextPos = WaterPivotPoint + (BounceUp) * (delta * bounceLerp);

                if (GizmoDebug) MDebug.DrawWireSphere(NextPos, Color.green, m_Radius);

                Vector3 PointAvobe = NextPos - WaterLevel;
                PivotAboveWater = Vector3.Dot(PointAvobe, Gravity) < 0; //Check if the next position will be above water

                if (PivotAboveWater)
                {
                    BounceDown = Vector3.zero;
                    BounceUp = Vector3.zero;
                }
                else
                {
                    animal.AdditivePosition += (BounceUp) * (delta * bounceLerp);
                    WaterLine_Difference = Vector3.zero; //Clear the water line difference
                }
            }
        }

        void AddInertia(ref Vector3 value, float speed, float DeltaTime)
        {
            transform.position += value;
            value = Vector3.Lerp(value, Vector3.zero, DeltaTime * speed);
        }


        public override void ResetStateValues()
        {
            WaterCollider = new Collider[1];
            IsInWater = false;
            TouchedWater = false;
            PivotAboveWater = false;
            BounceDown = Vector3.zero;
            BounceUp = Vector3.zero;
            WaterLine_Difference = Vector3.zero;
            HorizontalInertia = Vector3.zero;
            WaterNormal = Vector3.up;
            WaterLevel = NoWaterLevel;
        }

#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            var setName = "Swim";

            if (animal.SpeedSet_Get(setName) == null)
            {
                animal.speedSets.Add(
                    new MSpeedSet()
                    {
                        name = setName,
                        StartVerticalIndex = new IntReference(1),
                        TopIndex = new IntReference(2),
                        states = new List<StateID>(1) { ID },
                        Speeds = new List<MSpeed>() { new MSpeed(setName), new MSpeed(setName + " Fast", 2, 4, 4) { animator = new FloatReference(1.33f) } }
                    }
                    );
            }
        }

        internal override void Reset()
        {
            base.Reset();

            WaterCollider = new Collider[1];            //Set the Array to 1

            ExitCooldown = 1f;
            EnterCooldown = 1f;

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true, //IMPORTANT
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),

            };
            // SpeedIndex = 0;
        }

        public override void StateGizmos(MAnimal animal)
        {
            if (!Application.isPlaying)
            {
                var scale = animal.ScaleFactor;
                var cyan = Color.cyan;
                cyan.a = 0.5f;

                var Pivot = Application.isPlaying ? WaterPivot : animal.pivots.Find(x => x.name == "Water");

                if (Pivot != null)
                {
                    var UPsearch = animal.transform.position + Vector3.up * (UpSearch);

                    Gizmos.color = cyan;
                    Gizmos.DrawSphere(Pivot.World(animal.transform), m_Radius * scale);
                    Gizmos.DrawSphere(UPsearch, m_Radius * scale);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(Pivot.World(animal.transform), m_Radius * scale);
                    Gizmos.DrawWireSphere(UPsearch, m_Radius * scale);
                    Gizmos.DrawRay(UPsearch, animal.Gravity * UpSearch);
                }
            } //Show only when is not playing
        }
#endif
    }
}

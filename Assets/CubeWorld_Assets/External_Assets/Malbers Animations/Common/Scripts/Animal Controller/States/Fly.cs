using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/fly")]

    [AddTypeMenu("Air/Fly")]
    public class Fly : State
    {
        //public override string StateName => "Fly";
        public override string StateIDName => "Fly";
        public enum FlyInput { Toggle, Press, None }

        [Header("Fly Parameters")]
        [Tooltip("Bank amount used when turning")]
        public float Bank = 30;
        [Tooltip("Pitch Limit to Rotate the Rotator Up and Down")]
        [FormerlySerializedAs("Ylimit")]
        public float PitchLimit = 80;

        [Tooltip("Bank amount used when turning while straffing")]
        public float BankStrafe = 0;
        [Tooltip("Limit to go Up and Down while straffing")]
        public float PitchStrafe = 0;

        [Space]
        [Tooltip("Max Y Height the Animal Can Fly. If this value is Zero this value will be ignored")]
        public float MaxFlyHeight = 0;

        [Tooltip("When Entering the Fly State... The animal will keep the Velocity from the last State if this value is greater than zero")]
        public FloatReference InertiaLerp = new(1);

        [Header("TakeOff")]
        [Tooltip("Impulse to push the animal Upwards for a time to help him take off.\nIf set to zero this logic will be ignored, the Animation needs to be tagged with the Enter animation tag")]
        public FloatReference Impulse = new();
        [Tooltip("Time the Impulse will be applied")]
        public FloatReference ImpulseTime = new(0.5f);
        [Tooltip("Curve to apply to the Impulse Logic")]
        public AnimationCurve ImpulseCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        private float elapsedImpulseTime;

        [Header("Landing")]
        [Tooltip("When the Animal is close to the Ground it will automatically Land")]
        public BoolReference canLand = new(true);
        [Tooltip("Layers to Land on")]
        public LayerMask LandOn = (1);
        [Tooltip("Ray Length multiplier to check for ground and automatically land (increases or decreases the MainPivot Lenght for the Fall Ray")]
        public FloatReference LandMultiplier = new(1f);



        [Space, Tooltip("Avoids a surface to land when Flying. E.g. if the animal does not have a swim state, set this to void landing/entering the water")]
        public bool AvoidSurface = false;
        [Tooltip("RayCast distance to find the Surface to avoid"),
            Hide(nameof(AvoidSurface), false)]
        public float SurfaceDistance = 0.5f;
        [Tooltip("Which layers to search to avoid that surface. Triggers are not inlcuded"),
            Hide(nameof(AvoidSurface), false)]
        public LayerMask SurfaceLayer = 16;
        [Tooltip("Check if it can collide with triggers"),
            Hide(nameof(AvoidSurface), false)]
        public QueryTriggerInteraction trigger = QueryTriggerInteraction.Collide;

        [Hide(nameof(AvoidSurface), false)]
        public float avoidLerp = 7f;

        //[Hide(nameof(AvoidSurface), false)]
        //[Range(0, 1)]
        //public float AvoidLimit = 0.5f;

        [Header("Gliding")]

        [Tooltip("Vertical Speed on the Animator set to Flap the wings")]
        public float FlapSpeed = 1;
        [Tooltip("Vertical Speed on the Animator set as Glide")]
        public float GlideSpeed = 2;

        [Space]
        [Tooltip("The character will activate only the glide animations and it cannot go upwards")]
        public BoolReference GlideOnly = new(false);

        [Tooltip("When the Forward Input is Released,this will be the movement speed the gliding will have")]
        public FloatReference GlideOnlyIdleS = new(0.5f);
        [Tooltip("When the Forward Input is Pressed ,this will be the movement speed the gliding will have")]
        public FloatReference GlideOnlyIdleV = new(1);

        [Header("Auto Glide")]
        [Tooltip("It will do Auto gliding while flying")]
        public BoolReference AutoGlide = new(true);
        [MinMaxRange(0, 10)]
        public RangedFloat GlideChance = new(0.8f, 4);
        [MinMaxRange(0, 10)]
        public RangedFloat FlapChange = new(0.5f, 4);


        [Tooltip("Variation to make Random Flap and Glide Animation")]
        public float Variation = 0.3f;
        protected bool isGliding = false;
        protected float FlyStyleTime = 1;
        protected float AutoGlide_CurrentTime = 1;

        [Header("Down Acceleration")]
        public FloatReference GravityDrag = new(0);
        public FloatReference DownAcceleration = new(0.5f);

        private float acceleration = 0;

        protected Vector3 verticalInertia;

        [Tooltip("Somethimes the Head blocks the Landing Ray.. this will solve the landing by raycasting a ray from the Bone that is blocking the Logic")]
        /// <summary>If the Animal is a larger one sometimes </summary>
        public bool BoneBlockingLanding = false;
        [Hide("BoneBlockingLanding", true), Tooltip("Name of the blocker bone")]
        public string BoneName = "Head";
        [Hide("BoneBlockingLanding", true), Tooltip("Local Offset from the Blocker Bone")]
        public Vector3 BoneOffsetPos = Vector3.zero;
        [Hide("BoneBlockingLanding", true), Tooltip("Distance of the Landing Ray from the blocking Bone")]
        public float BlockLandDist = 0.4f;
        private Transform BlockingBone;


        public override void InitializeState()
        {
            AutoGlide_CurrentTime = Time.time;
            FlyStyleTime = GlideChance.RandomValue;
            SearchForContactBone();
        }

        /// <summary>When using Contact bone Find it on the Animal that is using it</summary>
        void SearchForContactBone()
        {
            BlockingBone = null;

            if (BoneBlockingLanding)
                BlockingBone = animal.transform.FindGrandChild(BoneName);
        }

        public override void Activate()
        {
            base.Activate();
            InputValue = true; //Make sure the Input is set to True when the flying is not being activated by an input player

            animal.Force_Reset(); //Remove forces
        }

        public override bool KeepForwardMovement => AlwaysForward.Value;

        public override void EnterCoreAnimation()
        {
            verticalInertia = Vector3.Project(animal.DeltaPos, animal.UpVector); //Find the Up Inertia to keep it while entering the Core Anim
            animal.PitchDirection = animal.Forward;
            animal.DeltaPos = Vector3.zero;
            acceleration = 0;

            animal.InertiaPositionSpeed = animal.HorizontalVelocity * animal.DeltaTime;

            if (GlideOnly.Value)
            {
                animal.currentSpeedModifier.Vertical = GlideSpeed;
                animal.UseSprintState = false;
            }
            else
            {
                animal.currentSpeedModifier.Vertical = FlapSpeed;
                isGliding = true;
            }
        }

        public override Vector3 Speed_Direction()
        {
            if (GlideOnly)
            {
                MovementAxisMult.y = 0;
                var Mult = animal.MovementAxis.z < 0.1 ? GlideOnlyIdleS.Value : 1f;

                animal.currentSpeedModifier.Vertical = animal.MovementAxis.z < 0.1 ? GlideOnlyIdleV.Value : GlideSpeed;

                animal.MovementAxis.z = 1;
                return animal.Forward * Mult;
            }

            if (animal.FreeMovement)
                return animal.PitchDirection;

            return animal.Forward;
        }

        public override void OnStateMove(float deltatime)
        {
            if (InCoreAnimation) //While is flying
            {
                var limit = PitchLimit;
                var bank = Bank;

                if (animal.Strafe)
                {
                    limit = PitchStrafe;
                    bank = BankStrafe;
                }

                if (AutoGlide && !GlideOnly.Value)
                    AutoGliding();


                //Limit Fly Max Height
                if (MaxFlyHeight > 0 && animal.transform.position.y > MaxFlyHeight)
                {
                    limit = 0;
                    var Pos = animal.transform.position;
                    Pos.y = MaxFlyHeight;
                    animal.transform.position = Pos;
                }



                if (TryAvoidSurface(deltatime))
                {
                    animal.FreeMovementRotator(0, 0);
                    acceleration = 0; //Remove Down Acceleration
                    return;
                }
                else
                {
                    GravityPush(deltatime); //Add artificial gravity to the Fly
                    animal.FreeMovementRotator(limit, bank);
                }

                //Inertia acumulate from last state
                if (InertiaLerp.Value > 0)
                    animal.AddInertia(ref verticalInertia, InertiaLerp);
            }


            //Takeoff Impulse Logic
            if (InEnterAnimation)
            {
                if (Impulse > 0 && ImpulseTime > 0 &&
                   (animal.LastState.ID.ID <= 1) &&         //Do it only if the last states were Idle or Locomotion
                   (elapsedImpulseTime <= ImpulseTime)
                   )
                {
                    var takeOffImp = Impulse * ImpulseCurve.Evaluate(elapsedImpulseTime / ImpulseTime);
                    animal.AdditivePosition += deltatime * takeOffImp * animal.UpVector;
                    elapsedImpulseTime += deltatime;
                }
            }
        }

        public override void OnModeStart(Mode mode)
        {
            if (!mode.AllowMovement) verticalInertia = Vector3.zero; //Remove the vertical inertia
        }

        /// <summary> Use this with messages to change the Avoid Surface value </summary>
        public virtual void SetAvoidSurface(bool value) => AvoidSurface = value;

        private bool TryAvoidSurface(float deltatime)
        {
            if (AvoidSurface)
            {
                //Debug.Log("TryAvoidSurface");
                var surfacePos = transform.position + animal.AdditivePosition;

                var Dist = SurfaceDistance * ScaleFactor;



                if (Physics.Raycast(surfacePos, Gravity, out RaycastHit hit, Dist, SurfaceLayer, trigger))
                {
                    Color LandRay = Color.cyan;


                    if (animal.MovementAxis.y < 0) animal.MovementAxis.y = 0;

                    if (hit.distance < Dist * 0.75f)
                    {
                        animal.AdditivePosition += -(Dist * 0.75f - hit.distance) * deltatime * avoidLerp * Gravity;
                    }

                    if (m_debug) MDebug.DrawRay(surfacePos, Gravity * Dist, LandRay);
                    return true;
                }
            }
            return false;
        }

        public override void TryExitState(float DeltaTime)
        {
            if (!InputValue) AllowExit();

            if (canLand.Value)
            {
                var Point = BlockingBone ? BlockingBone.TransformPoint(BoneOffsetPos) : animal.Main_Pivot_Point;
                var Dist = (BlockingBone ? BlockLandDist : LandMultiplier.Value) * animal.ScaleFactor;

                if (Physics.Raycast(Point, Gravity, out RaycastHit landHit, Dist, LandOn, IgnoreTrigger))
                {
                    FlyAllowExit(landHit);
                    Debugging($"[AllowExit] Can Land on <{landHit.collider.name}> [Using Blocking Bone: {BlockingBone != null}]");
                    return;
                }

                if (Physics.Raycast(animal.Main_Pivot_Point, Gravity, out RaycastHit landHitMain, LandMultiplier.Value * animal.ScaleFactor, LandOn, IgnoreTrigger))
                {
                    FlyAllowExit(landHitMain);
                    Debugging($"[AllowExit] Can Land on <{landHitMain.collider.name}> ");
                    return;
                }
            }
        }

        private void FlyAllowExit(RaycastHit hit)
        {
            var DistanceToGround = hit.distance;

            if (Height >= DistanceToGround)
            {
                var FallRayAngle = Vector3.SignedAngle(hit.normal, animal.UpVector, animal.Right);
                var DeepSlope = Mathf.Abs(FallRayAngle) >= animal.SlopeLimit;
                if (!DeepSlope) //Check if we are not on a deep slope
                {
                    AllowExit();
                    animal.CheckIfGrounded();

                    //Meaning we still are in the Fall state (Check if Grounded can change to a new state)
                    if (IsActiveState)
                    {
                        animal.Grounded = true;   //Force Grounded to activate locomotion
                        animal.UseGravity = false;

                        animal.AlignPosLerpDelta = animal.AlignPosLerp * 5;

                        //SUPER IMPORTANT!!! this is when the Animal is falling from a great height
                        var GroundedPos = Vector3.Project(hit.point - animal.transform.position, Gravity);
                        animal.Teleport_Internal(animal.transform.position + GroundedPos);

                        animal.ResetUPVector(); //IMPORTANT!
                        animal.hit_Hip.distance = Height;

                        Debugging($"[Try Exit] (Grounded) + [Terrain Angle = {FallRayAngle:F2}]. [Align to Ground]");
                        return;
                    }
                }
            }

            animal.FreeMovement = false; //Disable the Free Movement
            animal.UseGravity = true;
            AllowExit();
        }

        void GravityPush(float deltaTime)
        {
            if (animal.Strafe) return; //Do not push Down when 


            if (animal.MovementAxisRaw.y < 0f)
            {
                acceleration += Mathf.Abs(animal.MovementAxis.y) * deltaTime * DownAcceleration;
            }
            else
            {
                //Deacelerate slowly all the acceleration you earned..
                acceleration = Mathf.MoveTowards(acceleration, 0, deltaTime * DownAcceleration);
            }

            //USE INERTIA SPEED INSTEAD OF TARGET POSITION
            if (acceleration != 0)
                animal.AdditivePosition += acceleration * deltaTime * animal.InertiaPositionSpeed.normalized;

            if (GravityDrag > 0)
            {
                animal.AdditivePosition += (GravityDrag * animal.ScaleFactor) * deltaTime * Gravity;
            }
        }

        void AutoGliding()
        {
            if (MTools.ElapsedTime(FlyStyleTime, AutoGlide_CurrentTime))
            {
                AutoGlide_CurrentTime = Time.time;
                isGliding ^= true;

                FlyStyleTime = isGliding ? GlideChance.RandomValue : FlapChange.RandomValue;

                var newGlideSpeed = Random.Range(GlideSpeed - Variation, GlideSpeed);
                var newFlapSpeed = Random.Range(FlapSpeed, FlapSpeed + Variation);

                animal.currentSpeedModifier.Vertical = (isGliding && !animal.Strafe) ? newGlideSpeed : newFlapSpeed;
            }
        }

        public override void ResetStateValues()
        {
            verticalInertia = Vector3.zero;
            acceleration = 0;
            isGliding = false;
            InputValue = false;
            elapsedImpulseTime = 0;
        }

        public override void RestoreAnimalOnExit()
        {
            animal.FreeMovement = false;
            //  animal.AlwaysForward = LastAlwaysForward;
            //  animal.Speed_Lock(false);
            animal.InputSource?.SetInput(Input, false); //Hack to reset the toggle when it exit on Grounded
            animal.LockUpDownMovement = false;
        }

        public override void AllowStateExit()
        {
            base.InputValue = false;        //release the base Input value
            base.ExitInputValue = false;    //release the base Input value
        }



        //public override bool InputValue //lets override to Allow exit when the Input Changes
        //{
        //    get => base.InputValue;
        //    set
        //    {
        //        base.InputValue = value;

        //        if (InCoreAnimation && IsActiveState && !value && CanExit) //When the Fly Input is false then allow exit
        //        {
        //            AllowExit();
        //        }
        //    }
        //}

#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            var setName = "Fly";

            if (animal.SpeedSet_Get(setName) == null)
            {
                animal.speedSets.Add(
                    new MSpeedSet()
                    {
                        name = setName,
                        StartVerticalIndex = new IntReference(1),
                        PitchLerpOn = new FloatReference(3),
                        PitchLerpOff = new FloatReference(3),
                        TopIndex = new IntReference(2),
                        states = new List<StateID>(1) { ID },
                        Speeds = new List<MSpeed>() { new(setName), new(setName + " Fast", 2, 4, 4) { animator = new FloatReference(1.33f) } }
                    }
                    );
            }
        }


        internal override void Reset()
        {
            base.Reset();
            Input = "Fly";

            EnterTag.Value = "TakeOff";

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = false,
                IgnoreLowerStates = true,
                Gravity = false,
                modify = (modifier)(-1),
                AdditivePosition = true,
                AdditiveRotation = true,
                FreeMovement = true,
            };
        }

        public override void StateGizmos(MAnimal animal)
        {
            if (canLand && animal.debugGizmos)
            {
                var Gravity = animal.Gravity;

                Gizmos.color = Color.yellow;

                var width = 2f;

                var PointDown = (LandMultiplier) * animal.transform.lossyScale.y * Gravity.normalized;

                MDebug.DrawLine(animal.Main_Pivot_Point, animal.Main_Pivot_Point + PointDown, width);

                if (BlockingBone)
                {
                    var HitPoint = BlockingBone.TransformPoint(BoneOffsetPos);
                    MDebug.DrawLine(HitPoint, HitPoint + animal.transform.lossyScale.y * BlockLandDist * Gravity, width);
                }

                if (AvoidSurface && !Application.isPlaying)
                {
                    Gizmos.color = Color.cyan;
                    var Dist = SurfaceDistance * animal.ScaleFactor;

                    Gizmos.DrawRay(animal.Center, Gravity.normalized * Dist);
                }
            }
        }
#endif
    }
}

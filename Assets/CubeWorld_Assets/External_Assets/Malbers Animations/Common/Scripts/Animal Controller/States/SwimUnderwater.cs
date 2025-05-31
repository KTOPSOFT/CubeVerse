using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>UnderWater Logic</summary>
    [AddTypeMenu("Water/Swim UnderWater")]
    public class SwimUnderwater : State
    {
        //public override string StateName => "UnderWater";
        public override string StateIDName => "UnderWater";

        [Header("UnderWater Parameters")]
        [Range(0, 90)]
        public float Bank = 30;
        [Range(0, 90), Tooltip("Limit to go Up and Down")]
        public float Ylimit = 80;
        [Tooltip("It will push the animal down into the water for a given time")]
        public float EnterWaterDrag = 10;

        //[Tooltip("If the Animal Enters it will wait this time to try exiting the water")]
        //public float TryExitTime = 0.5f;
        //protected float EnterWaterTime;


        [Tooltip("When the Underwater state exits, it will activate the Fall State")]
        public bool AllowFallOnExit = true;

        protected Vector3 Inertia;
        protected Swim SwimState;


        public override void InitializeState()
        {
            SwimState = null;
            SwimState = (Swim)animal.State_Get(StateEnum.Swim); //Cache the Swim State

            if (SwimState == null)
            {
                Debug.LogError($"UnderWater State needs Swim State in order to work, please add the Swim State to {animal.name}", animal);
            }
        }

        public override void Activate()
        {
            base.Activate();
            Inertia = animal.DeltaPos;
        }

        public override Vector3 Speed_Direction() => animal.FreeMovement ? animal.PitchDirection : animal.Forward;

        public override bool TryActivate()
        {
            if (SwimState == null) return false;

            if (SwimState.IsActiveState)
            {
                if (/*SwimState.CheckWater() && */animal.RawInputAxis.y < -0.25f) //Means that Key Down is Pressed;
                {
                    IgnoreLowerStates = true;
                    return true;
                }
            }
            else
            {
                if (SwimState.CheckWater() && animal.MovementAxisSmoothed.y < -0.25f) //Means that Key Down is Pressed;
                {
                    IgnoreLowerStates = false;
                    return true;
                }
            }
            return false;
        }

        public override void OnStateMove(float deltatime)
        {
            animal.FreeMovementRotator(Ylimit, Bank);
            animal.AddInertia(ref Inertia, EnterWaterDrag);
            animal.UseGravity = false; //Hack to remove gravity
        }


        public override void TryExitState(float DeltaTime)
        {
            var checkWater = SwimState.CheckWater();
            //  SwimState.FindWaterLevel2();

            // var radius = SwimState.m_Radius;

            if (!checkWater)
            {
                if (AllowFallOnExit && animal.Sprint && animal.UpDownSmooth > 0f)
                {
                    Debugging("[Exit to Fall]");
                    animal.State_Force(StateEnum.Fall);
                    AllowExit();
                }
                //If we  touched the waterLevel
                else
                {
                    Debugging("[Allow Exit to Swim]");
                    SwimState.Activate();
                    SwimState.BounceDown = Vector3.zero;
                }
            }
        }

        public override void ResetStateValues()
        {
            Inertia = Vector3.zero;
        }

        public override void RestoreAnimalOnExit()
        {
            animal.FreeMovement = false; //Important!!!!
        }


#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            var setName = "UnderWater";

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

            General = new AnimalModifier()
            {
                RootMotion = false,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = false,
                FreeMovement = true,
                IgnoreLowerStates = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
            IgnoreLowerStates = false;
        }

        public override void StateGizmos(MAnimal animal)
        {
            if (Application.isPlaying && SwimState != null && animal != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(SwimState.WaterPivotPoint, SwimState.m_Radius * animal.ScaleFactor);
            }
        }
#endif
    }
}

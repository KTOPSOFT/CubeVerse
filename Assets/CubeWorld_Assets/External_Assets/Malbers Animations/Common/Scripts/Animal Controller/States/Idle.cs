using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>Idle Should be the Last State on the Queue, when nothing is moving Happening </summary>
    [AddTypeMenu("Ground/Idle")]
    public class Idle : State
    {
        //  public override string StateName => "Idle";
        public override string StateIDName => "Idle";

        public bool HasLocomotion { get; private set; }

        //Check if the animal has Idle State if it does not have then Locomotion is IDLE TOO
        public override void InitializeState() => HasLocomotion = animal.HasState(StateEnum.Locomotion);

        public override void Activate()
        {
            base.Activate();
            CanExit = true; //This allow the Locomotion state to enable any time he want! without waiting the transition to be finished
        }

        public override bool TryActivate()
        {
            //Activate when the animal is not moving and is grounded
            if (HasLocomotion) //If the animal has Locomotion then check if the animal is not moving
            {
                return (
                    animal.MovementAxisSmoothed == Vector3.zero &&
                    //animal.MovementAxis == Vector3.zero && 
                    !animal.MovementDetected &&
                    General.Grounded == animal.Grounded
                    );
            }
            else  //Meaning the Idle works as locomotino too so only check if the animal is grounded
            {
                return (General.Grounded == animal.Grounded);
            }
        }


        /// <summary>
        /// Make sure RootMotion Root is enable on Idle and Locomotion last changes
        /// </summary>
        [SerializeField, HideInInspector] private bool noRMRot = false;
        private void OnValidate()
        {
            if (!noRMRot)   //If the RootMotion is not set then set it to true
            {
                noRMRot = true;
                General.RootMotionRotation = true;
                MTools.SetDirty(this);
            }
        }

#if UNITY_EDITOR
        internal override void Reset()
        {
            base.Reset();

            ResetLastState = true; //Important por Idle

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = false,
                OrientToGround = true,
                CustomRotation = false,
                FreeMovement = false,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }
        //Do nothing... the Animal Controller already does it on Start
        public override void SetSpeedSets(MAnimal animal) { }
#endif
    }
}
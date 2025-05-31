using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.kraxblx9518t")]
    [AddTypeMenu("Death/Death (Animation)")]
    public class Death : State
    {
        // public override string StateName => "Death/Death (Animation)";
        public override string StateIDName => "Death";

        [Tooltip("Disable all components when the animal dies. Use this when your animal will not respawn")]
        public bool DisableAllComponents = true;
        [Tooltip("Disable the main collider when the animal dies. Use this when your animal will not respawn")]
        public bool DisableMainCollider = true;
        [Tooltip("Disable the internal collider when the animal dies. Use this when your animal will not respawn")]
        public bool DisableInternalColliders = false;
        // public bool RemoveAllTriggers = true;
        public bool IsKinematic = true;
        //public bool DisableModes = true;
        public int DelayFrames = 2;
        public float RigidbodyDrag = 5;
        public float RigidbodyAngularDrag = 15;

        [Space]
        public bool disableAnimal = true;

        [Hide("disableAnimal")]
        public float disableAnimalTime = 5f;

        public override void EnterCoreAnimation()
        {
            animal.Mode_Interrupt();

            if (animal.RB && IsKinematic)
            {
                animal.RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; //For Kinematic!!
                animal.RB.isKinematic = true;
            }

            animal.StopMoving();
            animal.InputSource?.Enable(false); //Disable the Input Source
            animal.Mode_Stop();
            animal.SetModeStatus(0);
            animal.Delay_Action(DelayFrames, () => DisableAll()); //Wait 2 frames
        }

        public override void OnStateMove(float deltatime)
        {
            if (!animal.Grounded)
            {
                animal.CheckIfGrounded();
                animal.UseGravity = false;
            }
        }

        void DisableAll()
        {
            SetEnterStatus(0);

            if (DisableMainCollider && animal.MainCollider != null) { animal.MainCollider.enabled = false; }

            if (DisableAllComponents)
            {
                var AllComponents = animal.GetComponentsInChildren<MonoBehaviour>();

                foreach (var comp in AllComponents)
                {
                    if (comp == animal) continue;
                    if (comp != null) comp.enabled = false;
                }
            }

            if (DisableInternalColliders)
                foreach (var c in animal.colliders) c.SetEnable(false);


            animal.SetCustomSpeed(new MSpeed("Death")); //Clear the Current Speed

            if (animal.RB)
            {
                animal.RB.linearDamping = RigidbodyDrag;
                animal.RB.angularDamping = RigidbodyAngularDrag;
            }

            if (disableAnimal)
                animal.DisableSelf(disableAnimalTime); //Disable the Animal Component after x time
        }


#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... Death does not need a Speed Set
        }

        internal override void Reset()
        {
            base.Reset();

            ID = MTools.GetInstance<StateID>("Death");

            noModes.Value = true;

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                Persistent = true,
                LockInput = true,
                LockMovement = true,
                AdditiveRotation = true,
            };
        }
#endif
    }
}
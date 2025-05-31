using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Set Strafe")]
    public class SetStrafeTask : MTask
    {
        public override string DisplayName => "Animal/Set Strafe";
        void Reset() => Description = "Enable/Disable Strafing on the Animal Controller";


        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        public BoolReference strafe = new(true);

      //  public enum StrafeActions { }


        [Hide(nameof(showSelf))]
        [Tooltip("The Strafe Target of this AI Character, will be this Current AI Target")]
        public bool TargetIsStrafeTarget;
        [Hide(nameof(showTarget))]
        [Tooltip("The Strafe Target of the current AI Target, will be this AI Character")]
        public bool SelfIsStrafeTarget = true;

        [Tooltip("Add a completely new Strafe Target to the Animal")]
        [Hide(nameof(showTransform))]
        public TransformVar NewStrafeTarget;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            var StrafeTarget = this.NewStrafeTarget != null ? this.NewStrafeTarget.Value : null;

            if (affect == Affected.Self)
            {
                brain.Animal.Strafe = strafe.Value;

                if (StrafeTarget == null) StrafeTarget = brain.AIControl.Target; 
                if (TargetIsStrafeTarget) brain.Animal.Aimer.SetTarget(StrafeTarget);
            }
            else
            {
                if (brain.TargetAnimal)
                {
                    brain.TargetAnimal.Strafe = strafe.Value;
                    if (StrafeTarget == null) StrafeTarget = brain.Animal.transform;
                    if (SelfIsStrafeTarget) brain.TargetAnimal.Aimer.SetTarget(StrafeTarget);
                }
            }

            brain.TaskDone(index); //Set Done to this task
        }

        [HideInInspector, SerializeField] bool showTransform ,showSelf, showTarget;

        private void OnValidate()
        {
            if (NewStrafeTarget != null)
            {
                TargetIsStrafeTarget = false;
                SelfIsStrafeTarget = false;
            }

            showTransform = affect == Affected.Self && !TargetIsStrafeTarget || affect == Affected.Target && !SelfIsStrafeTarget;
            showSelf = affect == Affected.Self && NewStrafeTarget == null;
            showTarget = affect == Affected.Target && NewStrafeTarget == null;

        }
    }
}
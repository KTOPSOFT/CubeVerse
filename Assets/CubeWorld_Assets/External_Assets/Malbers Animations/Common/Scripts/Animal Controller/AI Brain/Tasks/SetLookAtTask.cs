using MalbersAnimations.Scriptables;
using UnityEngine;
namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Look At-Aim", fileName = "new Aim Task")]
    public class SetLookAtTask : MTask
    {
        public override string DisplayName => "General/Set Look At-Aim";
        public enum LookAtOption1 { CurrentTarget, TransformVar, ClearTarget }
        public enum LookAtOption2 { This, TransformVar, ClearTarget }

        [Tooltip("Check the Look At Component on the Target or on Self")]
        [UnityEngine.Serialization.FormerlySerializedAs("SetLookAtOn")]
        public Affected SetAimOn = Affected.Self;

        [Hide("SetAimOn", (int)Affected.Self)]
        public LookAtOption1 LookAtTargetS = LookAtOption1.CurrentTarget;
        [Hide("SetAimOn", (int)Affected.Target)]
        public LookAtOption2 LookAtTargetT = LookAtOption2.This;
        [Hide("showTransformVar")]
        public TransformVar TargetVar;

        [Tooltip("If true .. it will Look for a gameObject on the Target with the Tag[tag].... else it will look for the gameObject name")]
        public bool UseTag = false;

        [Hide("UseTag", true), Tooltip("Search for the Target Child gameObject name")]
        public string BoneName = "Head";
        [Hide("UseTag"), Tooltip("Look for a child gameObject on the Target with the Tag[tag]")]
        public Tag tag;
        [Tooltip("When the Task ends it will Remove the Target on the Aim Component")]
        public bool DisableOnExit = true;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            Transform FinalTarget;

            IAim Aimer = SetAimOn == Affected.Self ?
                brain.Animal.FindInterface<IAim>() :
                brain.Target != null ? brain.Target.FindInterface<IAim>() : null;

            if (Aimer == null) { brain.TaskDone(index); return; }

            //Try to clear target first
            if (SetAimOn == Affected.Self && LookAtTargetS == LookAtOption1.ClearTarget) Aimer.ClearTarget();
            else if (SetAimOn == Affected.Target && LookAtTargetT == LookAtOption2.ClearTarget) Aimer.ClearTarget();

            else if (SetAimOn == Affected.Self)
            {
                Transform AimTarget = LookAtTargetS == LookAtOption1.CurrentTarget ? brain.Target : TargetVar.Value;
                FinalTarget = UseTag ? AimTarget.FindWithMalbersTag(tag) : GetChildByName(AimTarget);
                Aimer.SetTarget(FinalTarget);
            }
            else
            {
                var AimTarget = LookAtTargetT == LookAtOption2.This ? brain.Animal.transform : TargetVar.Value;
                FinalTarget = UseTag ? AimTarget.FindWithMalbersTag(tag) : GetChildByName(AimTarget);
                Aimer.SetTarget(FinalTarget);
            }

            brain.TaskDone(index);
        }


        private Transform GetChildByName(Transform Target)
        {
            if (Target && !string.IsNullOrEmpty(BoneName))
            {
                var child = Target.FindGrandChild(BoneName);
                if (child != null) return child;
            }
            return Target;
        }



        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            if (DisableOnExit)
            {
                brain.Animal.FindInterface<IAim>()?.ClearTarget();
                if (brain.Target) brain.Target.FindInterface<IAim>()?.ClearTarget();
            }
        }

        [HideInInspector] public bool showTransformVar = false;
        private void OnValidate()
        {
            showTransformVar =
                (LookAtTargetS == LookAtOption1.TransformVar && SetAimOn == Affected.Self) ||
                (LookAtTargetT == LookAtOption2.TransformVar && SetAimOn == Affected.Target);
        }

        private void Reset() { Description = "Find a child gameObject with the name given on the Target and set it as the Target for Aim Component"; }
    }
}

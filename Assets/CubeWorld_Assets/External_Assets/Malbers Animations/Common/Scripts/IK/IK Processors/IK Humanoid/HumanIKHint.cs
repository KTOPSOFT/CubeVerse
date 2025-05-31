using UnityEngine;

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Hint")]
    public class HumanIKHint : IKProcessor
    {
        public override bool RequireTargets => false;
        public AvatarIKHint hint;

        public override void OnAnimatorIK(IKSet set, Animator animator, int index, float weight)
        {
            var Target = set.Targets[TargetIndex];
            if (Target == null || Target == null) return;

            animator.SetIKHintPositionWeight(hint, weight);
            animator.SetIKHintPosition(hint, Target.position);
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (set.Targets.Length < TargetIndex)
            {
                Debug.LogError($"The IK Set <B>[{set.Name}]</B> has no Transform set on the [Targets] array - Index {TargetIndex}." +
                    $" <B>[IK Processor: {name}]</B> Needs an a value in Index [{TargetIndex}]." +
                    $" Please add a reference for that index in the [Targets] array", animator);
            }
            else
            {
                Debug.Log($"<B>[IK Processor: {name}][IKHuman Hint]</B>  <color=yellow>[OK]</color>");
            }
        }

    }
}

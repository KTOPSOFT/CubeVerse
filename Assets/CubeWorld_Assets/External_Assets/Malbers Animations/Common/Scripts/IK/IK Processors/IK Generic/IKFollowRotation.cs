using System;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [Serializable]
    [AddTypeMenu("Generic/IK Follow Rotation")]
    public class IKFollowRotation : IKProcessor
    {
        [Tooltip("The Index of the Transform that will be used as the Master for the Rotation")]
        public int MasterIndex = 0;

        public override bool RequireTargets => true;

        public override void LateUpdate(IKSet IKSet, Animator anim, int index, float FinalWeight)
        {
            var Bone = IKSet.Targets[index].Value;
            var MasterTransform = IKSet.Targets[MasterIndex].Value;
            Bone.rotation = Quaternion.Lerp(Bone.rotation, MasterTransform.rotation, FinalWeight);
        }

        public override void Validate(IKSet set, Animator animator, int BoneIndex)
        {
            var isValid = true;

            if (set.Targets.Length < BoneIndex || set.Targets[BoneIndex] == null)
            {
                Debug.LogWarning($"The IK Set <B>[{set.Name}]</B> has no Transform set on the [Targets] array - Index [{BoneIndex}]." +
                    $" <B>[IK Processor: {name}]</B> Needs an a value in Index [{BoneIndex}]." +
                    $"Please add a reference for that index in the [Targets] array.", animator);
                // set.active = false;
                isValid = false;
            }

            if (set.Targets.Length < MasterIndex || set.Targets[MasterIndex] == null)
            {
                Debug.LogWarning($"The IK Set <B>[{set.Name}]</B> has no Transform set on the [Targets] array - Master Index [{MasterIndex}]." +
                    $" <B>[IK Processor: {name}]</B> Needs an a value in for the Master Index [{MasterIndex}]." +
                    $"Please add a reference for that index in the [Targets] array.", animator);
                // set.active = false;
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log($"<B>[IK Processor: {name}][External Rotation]</B>  <color=yellow>[OK]</color>");
            }
        }
    }
}

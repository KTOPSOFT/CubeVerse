using UnityEngine;

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Goal")]
    public class HumanIKGoal : IKProcessor
    {
        public override bool RequireTargets => true;
        [Tooltip("Target to to lock any of the limbs ")]
        public AvatarIKGoal goal;
        public bool position = true;
        [Hide("position")]
        public Vector3 OffsetP;
        public bool rotation = true;
        [Hide("rotation")]
        public Vector3 OffsetR;

        [Tooltip("Min and Max Distance to the Goal to modify the weight. Id the distance is lower than the Min the weight is 1. If is greater than the max then the weight is zero")]
        public RangedFloat Distance = new();
        public bool gizmos = true;

        private Transform bone;

        public override void Start(IKSet set, Animator animator, int index)
        {
            //Cache the Bone
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    bone = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    break;
                case AvatarIKGoal.RightFoot:
                    bone = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    break;
                case AvatarIKGoal.LeftHand:
                    bone = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    break;
                case AvatarIKGoal.RightHand:
                    bone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    break;
                default:
                    break;
            }
        }

        public override void OnAnimatorIK(IKSet set, Animator animator, int index, float weight)
        {
            var Target = set.Targets[TargetIndex]; //Always get the Target from the IK Set (it might vary)

            if (Target == null) return; //If there's no target skip

            //Check Max and Min Distance if is greater than Zero
            if (Distance.Min != 0 && Distance.Max != 0)
            {
                //var bone = set.Var[index].RootBone; //Get the Local RootBone

                var DistanceFromRoot = Vector3.Distance(bone.position, Target.position);
                weight *= DistanceFromRoot.CalculateRangeWeight(Distance.Min, Distance.Max);

                if (gizmos)
                {
                    var dir = (Target.position - bone.position).normalized;
                    MDebug.DrawRay(bone.position, dir * Distance.Max, Color.gray);
                    MDebug.DrawRay(bone.position, dir * Distance.Min, Color.green);
                }
            }

            if (position)
            {
                animator.SetIKPositionWeight(goal, weight);
                animator.SetIKPosition(goal, Target.Value.TransformPoint(OffsetP));
            }
            if (rotation)
            {
                animator.SetIKRotationWeight(goal, weight);
                animator.SetIKRotation(goal, Target.rotation * Quaternion.Euler(OffsetR));
            }
        }

        //public override void CheckForNullReferences(IKSet IKSet, Animator anim)
        //{
        //    if (IKSet.Targets.Length < TargetIndex)
        //    {
        //        Debug.LogError($"The IK Set <B>[{IKSet.name}]</B> has no Transform set on the [Targets] array - Index {TargetIndex}." +
        //            $" <B>[IK Processor: {name}]</B> Needs an a value in Index {TargetIndex}." +
        //            $" Please add a reference for that index in the [Targets] array", anim);
        //        // IKSet.active = false;
        //    }
        //}

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
                Debug.Log($"<B>[IK Processor: {name}][HumanIK Goal]</B>  <color=yellow>[OK]</color>");
            }
        }
    }
}

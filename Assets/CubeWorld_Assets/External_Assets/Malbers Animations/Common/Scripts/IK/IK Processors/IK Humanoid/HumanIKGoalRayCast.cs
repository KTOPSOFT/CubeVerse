using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Goal RayCast")]
    public class HumanIKGoalRayCast : IKProcessor
    {
        [Tooltip("Target to to lock any of the limbs ")]
        public AvatarIKGoal goal;

        public RangedFloat RayDistance = new(0.5f, 2);
        public LayerReference HitMask = new(1);
        public AxisDirection direction = AxisDirection.Forward;
        public override bool RequireTargets => false;


        public bool position = true;
        [Hide(nameof(position))]
        public float NormalOffset;
        public bool rotation = true;
        [Hide(nameof(rotation))]
        public Vector3 Offset;

        public bool gizmos = true;

        public Vector3 Direction(Animator anim)
        {
            return direction switch
            {
                AxisDirection.None => Vector3.zero,
                AxisDirection.Right => anim.transform.right,
                AxisDirection.Left => -anim.transform.right,
                AxisDirection.Up => anim.transform.up,
                AxisDirection.Down => -anim.transform.up,
                AxisDirection.Forward => anim.transform.forward,
                AxisDirection.Backward => -anim.transform.forward,
                _ => Vector3.zero,
            };
        }

        public Vector3 NormalFromDirection(Animator anim)
        {
            return direction switch
            {
                AxisDirection.None => Vector3.up,
                AxisDirection.Right => anim.transform.forward,
                AxisDirection.Left => -anim.transform.forward,
                AxisDirection.Up => anim.transform.right,
                AxisDirection.Down => -anim.transform.right,
                AxisDirection.Forward => anim.transform.up,
                AxisDirection.Backward => -anim.transform.up,
                _ => Vector3.up,
            };
        }
        public override void Start(IKSet set, Animator anim, int index)
        {
            //Cache the RootBone in the Local Vars 
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                    set.Var[index].RootBone = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    break;
                case AvatarIKGoal.RightFoot:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                    set.Var[index].RootBone = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    break;
                case AvatarIKGoal.LeftHand:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                    set.Var[index].RootBone = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    break;
                case AvatarIKGoal.RightHand:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.RightHand);
                    set.Var[index].RootBone = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    break;
                default: break;
            }

            // set.Var[index].Normal = NormalFromDirection(anim);
            //  set.Var[index].vectors.Add(NormalHash, NormalFromDirection(anim));
        }


        public override void OnAnimatorIK(IKSet set, Animator anim, int index, float weight)
        {

            var Dir = this.Direction(anim);
            var MinDistDir = Dir * RayDistance.Min;

            var Bone = set.Var[index].Bone; //Get the RootBone

            var NormalV = NormalFromDirection(anim); //Get the Normal Vector

            var StartPoint = MTools.ClosestPointOnPlane(set.Var[index].RootBone.position, Dir, Bone.position);


            MDebug.DrawWireSphere(StartPoint, Color.magenta, 0.025f);
            MDebug.DrawWireSphere(Bone.position, Color.white, 0.025f);
            MDebug.DrawRay(StartPoint, MinDistDir, Color.green);
            MDebug.DrawRay(StartPoint + MinDistDir, Dir * RayDistance.Difference, Color.red);

            Vector3 Hit;
            Quaternion Normal;

            if (Physics.Raycast(StartPoint, Dir, out var hit, RayDistance.maxValue, HitMask, QueryTriggerInteraction.Ignore))
            {
                Hit = hit.point;
                MDebug.DrawWireSphere(StartPoint, Color.green, 0.04f);

                weight *= hit.distance.CalculateRangeWeight(RayDistance.Min, RayDistance.Max); //Get the Average Hit


                MDebug.DrawRay(Hit, hit.normal * 0.2f, Color.yellow);

                Quaternion AlignRot = Quaternion.FromToRotation(-Dir, hit.normal);  //Calculate the orientation to Terrain 
                AlignRot = Quaternion.Inverse(Bone.rotation) * AlignRot; //Convert the rotation to Local
                                                                         // AlignRot = Quaternion.Inverse(AlignRot); //Convert the rotation to Local
                Quaternion Target = anim.rootRotation * Bone.rotation * AlignRot * Quaternion.Euler(Offset);


                Normal = Target;

                Hit += (hit.normal * NormalOffset);
            }
            else
            {
                return;
            }

            if (position)
            {
                anim.SetIKPositionWeight(goal, weight);
                anim.SetIKPosition(goal, Hit);
            }
            if (rotation)
            {
                anim.SetIKRotationWeight(goal, weight);
                anim.SetIKRotation(goal, Normal);
            }
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            Debug.Log($"<B>[IK Processor: {name}][HumanIK Goal RayCast]</B>  <color=yellow>[OK]</color>");
        }

        public override void OnDrawGizmos(IKSet IKSet, Animator anim, float weight)
        {
            if (gizmos)
            {
                var Dir = this.Direction(anim);

                if (!Application.isPlaying)
                {
                    Transform bn = null;

                    //Cache the RootBone
                    switch (goal)
                    {
                        case AvatarIKGoal.LeftFoot:
                            bn = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                            break;
                        case AvatarIKGoal.RightFoot:
                            bn = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                            break;
                        case AvatarIKGoal.LeftHand:
                            bn = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                            break;
                        case AvatarIKGoal.RightHand:
                            bn = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                            break;
                    }

                    var StartPositon = bn.position;

                    Gizmos.color = Color.green;
                    MDebug.GizmoRay(StartPositon, Dir * RayDistance.Min, 2);
                    Gizmos.DrawSphere(StartPositon, 0.02f);
                    Gizmos.color = Color.red;
                    MDebug.GizmoRay(StartPositon + Dir * RayDistance.Min, Dir * (RayDistance.Difference), 2);
                    Gizmos.DrawSphere(StartPositon + Dir * RayDistance.Max, 0.02f);
                }
            }
        }
    }
}

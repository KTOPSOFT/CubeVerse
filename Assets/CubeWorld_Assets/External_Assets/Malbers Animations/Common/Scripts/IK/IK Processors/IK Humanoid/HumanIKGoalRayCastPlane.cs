using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Goal RayCast Plane")]
    public class HumanIKGoalRayCastPlane : IKProcessor
    {
        [Tooltip("Target to to lock any of the limbs ")]
        public AvatarIKGoal goal;

        public LayerReference HitMask = new(1);
        public AxisDirection direction = AxisDirection.Forward;
        public override bool RequireTargets => false;

        public float AdditiveDistance = 0.2f;

        [Min(0.001f)] public float radius = 0.05f;
        //   public RangedFloat RayDistance = new(0.5f, 2);

        public bool position = true;
        [Hide(nameof(position))]
        public float NormalOffset;
        public bool rotation = true;
        [Hide(nameof(rotation))]
        public Vector3 Offset;

        public bool gizmos = true;


        private Transform Bone;
        private Transform RootBone;

        private Quaternion BeforeRotation;

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
                    Bone = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                    RootBone = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    break;
                case AvatarIKGoal.RightFoot:
                    Bone = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                    RootBone = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    break;
                case AvatarIKGoal.LeftHand:
                    Bone = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                    RootBone = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    break;
                case AvatarIKGoal.RightHand:
                    Bone = anim.GetBoneTransform(HumanBodyBones.RightHand);
                    RootBone = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    break;
                default: break;
            }
        }


        public override void OnAnimatorIK(IKSet set, Animator anim, int index, float weight)
        {
            //if (!JustCasted) { JustCasted = true; return; }

            var Dir = this.Direction(anim);
            var StartPoint = MTools.ClosestPointOnPlane(RootBone.position, Dir, Bone.position);
            var BonePos = Bone.position; //Bone Position before IK
            var BeforeDist = Vector3.Distance(StartPoint, BonePos); //Store the Dist Before
            var MinDistDir = Dir * (BeforeDist);


            MDebug.DrawWireSphere(StartPoint, Color.white, radius);
            MDebug.DrawWireSphere(BonePos, Color.white, radius);

            MDebug.DrawRay(StartPoint, MinDistDir * 2, Color.green);


            Vector3 Hit;
            Quaternion Normal;

            BeforeRotation = Bone.rotation;

            float RotWeight;
            float PosWeight;

            if (Physics.SphereCast(StartPoint, radius, Dir, out var hit, (BeforeDist * 2), HitMask, QueryTriggerInteraction.Ignore))
            {
                Hit = hit.point;
                MDebug.DrawWireSphere(StartPoint, Color.green, radius);
                MDebug.DrawWireSphere(Hit, Color.yellow, radius);
                MDebug.DrawRay(Hit, hit.normal * 0.2f, Color.yellow);

                var CheckDist = hit.distance - NormalOffset;


                if (CheckDist + radius < BeforeDist)
                {
                    PosWeight = 1;
                }
                else
                {
                    PosWeight = 0;
                }


                // weight *= hit.distance.CalculateRangeWeight(RayDistance.Min, RayDistance.Max); //Get the Average Hit

                RotWeight = (CheckDist).CalculateRangeWeight(BeforeDist, BeforeDist + AdditiveDistance);



                Quaternion AlignRot = Quaternion.FromToRotation(-Dir, hit.normal) * anim.rootRotation;  //Calculate the orientation to Terrain 

                AlignRot = Quaternion.Inverse(Bone.rotation) * AlignRot; //Convert the rotation to Local

                // AlignRot = Quaternion.Inverse(AlignRot); //Convert the rotation to Local

                Quaternion Target = /*anim.rootRotation * */Bone.rotation * AlignRot * Quaternion.Euler(Offset);


                Normal = Target;

                Hit += (hit.normal * NormalOffset);
            }
            else
            {
                return;
            }

            MDebug.DrawRay(Bone.position, Bone.rotation * Vector3.forward * 0.2F, Color.blue);
            MDebug.DrawRay(Bone.position, Bone.rotation * Vector3.right * 0.2F, Color.red);
            MDebug.DrawRay(Bone.position, Bone.rotation * Vector3.up * 0.2F, Color.green);


            //Debug.Log(Bone.rotation);

            if (position)
            {
                anim.SetIKPositionWeight(goal, Mathf.Min(PosWeight, weight));
                anim.SetIKPosition(goal, Hit);
            }

            if (rotation)
            {
                anim.SetIKRotationWeight(goal, Mathf.Min(RotWeight, weight));
                anim.SetIKRotation(goal, Normal);
            }
            else
            {
                anim.SetIKRotationWeight(goal, 1);
                anim.SetIKRotation(goal, anim.rootRotation * BeforeRotation * Quaternion.Inverse(Bone.rotation));
            }


            //JustCasted = false;
        }

        // private bool JustCasted = true;

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
                    Transform rootBn = null;

                    //Cache the RootBone
                    switch (goal)
                    {
                        case AvatarIKGoal.LeftFoot:
                            bn = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                            rootBn = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                            break;
                        case AvatarIKGoal.RightFoot:
                            bn = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                            rootBn = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                            break;
                        case AvatarIKGoal.LeftHand:
                            bn = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                            rootBn = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                            break;
                        case AvatarIKGoal.RightHand:
                            bn = anim.GetBoneTransform(HumanBodyBones.RightHand);
                            rootBn = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                            break;
                        default: break;
                    }

                    var StartPositon = rootBn.position;
                    var dist = Vector3.Distance(rootBn.position, bn.position);

                    var Dist = Dir * (dist + AdditiveDistance);

                    Gizmos.color = Color.green;
                    MDebug.GizmoRay(StartPositon, Dir * dist, 2);
                    Gizmos.DrawSphere(StartPositon, radius);

                    Gizmos.color = Color.red;
                    MDebug.GizmoRay(StartPositon + (Dir * dist), Dir * AdditiveDistance, 2);
                    Gizmos.DrawSphere(StartPositon + Dist, radius);
                }
            }
        }
    }
}

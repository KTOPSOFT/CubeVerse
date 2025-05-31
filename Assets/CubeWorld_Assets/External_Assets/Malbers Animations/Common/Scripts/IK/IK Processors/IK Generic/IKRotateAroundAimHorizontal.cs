using System;
using UnityEngine;


//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace MalbersAnimations.IK
{
    [Serializable]
    [AddTypeMenu("Generic/Rotate Around Aim Horizontal")]
    public class IKRotateAroundAimHorizontal : IKProcessor
    {
        public enum RotateAroundType
        {
            [InspectorName("Horizontal Aim (Green)")]
            Horizontal,
            [InspectorName("Vertical Aim (Red)")]
            Vertical
        }

        public RotateAroundType RotateAround = RotateAroundType.Horizontal;

        public float multiplier = 1;
        public Vector3 Offset;

        [Tooltip("Restore the Child bone's rotations after the IK is applied to the bone")]
        public bool KeepChildRot;

        [Hide(nameof(KeepChildRot))]
        public int[] childs;

        [Tooltip("Show Gizmos")]
        public bool Gizmos;

        public override bool RequireTargets => true;

        // public List<GenericIKOffset> Bones = new();

        public override void LateUpdate(IKSet IKSet, Animator anim, int index, float FinalWeight)
        {
            var Bone = IKSet.Targets[index].Value;

            if (Bone == null) return;   //Missing Bone

            var UpVector = anim.transform.up;
            var DirLook = IKSet.aimer.AimDirection;
            var HorizontalRotationAxis = Vector3.Cross(UpVector, DirLook).normalized;

            switch (RotateAround)
            {
                case RotateAroundType.Horizontal:
                    Bone.RotateAround(Bone.position, HorizontalRotationAxis, IKSet.aimer.VerticalAngle * -FinalWeight);
                    break;
                case RotateAroundType.Vertical:
                    Bone.RotateAround(Bone.position, UpVector, IKSet.aimer.HorizontalAngle * FinalWeight);
                    break;
                default:
                    break;
            }
            Bone.rotation *= Quaternion.Euler(Offset * FinalWeight);
            RestoreChildRotation(IKSet);

            if (Gizmos)
            {
                MDebug.Draw_Arrow(Bone.position, HorizontalRotationAxis * 2, Color.red);
                MDebug.Draw_Arrow(Bone.position, DirLook * 2, Color.blue);
                MDebug.Draw_Arrow(Bone.position, UpVector * 2, Color.green);
            }
        }

        private void RestoreChildRotation(IKSet iKSet)
        {
            //Store the bone's Child Rotation
            if (KeepChildRot)
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    var TargetIndex = childs[i];
                    iKSet.Targets[TargetIndex].Value.rotation = iKSet.CacheTargets[TargetIndex].rotation; //Restore the rotationof the child
                }
            }
        }

        public override void Validate(IKSet set, Animator animator, int BoneIndex)
        {
            var isValid = true;

            if (set.aimer == null)
            {
                Debug.LogWarning($"There's no Aimer on the IK Set. <B>[IK Processor: {name}]</B> needs an Aimer to get the Aim Direction", animator);
                isValid = false;
            }
            else
            {
                //Check for errors and Null references
                // foreach (var bn in Bones)
                {
                    if (set.Targets.Length < BoneIndex || set.Targets[BoneIndex] == null)
                    {
                        Debug.LogWarning($"The IK Set <B>[{set.Name}]</B> has no Transform set on the [Targets] array - Index [{BoneIndex}]." +
                            $" <B>[IK Processor: {name}]</B> Needs an a value in Index {BoneIndex}." +
                            $"Please add a reference for that index in the [Targets] array.", animator);
                        // set.active = false;

                        isValid = false;
                    }
                }
            }

            if (isValid)
            {
                Debug.Log($"<B>[IK Processor: {name}][IKGeneric]</B>  <color=yellow>[OK]</color>");
            }
        }

        public override void OnDrawGizmos(IKSet IKSet, Animator anim, float weight)
        {

        }
    }

    //[Serializable]
    //public struct GenericIKOffset
    //{
    //    [Range(0, 1)]
    //    public float Weight;
    //    [Tooltip("Bone Reference from the Targets Array the IK Offset")]
    //    public int BoneIndex;
    //    public IKGenerigType IK;
    //    public Vector3 Offset;

    //    //[Tooltip("Use the Aimer Direction to calculate the LookAt Direction")]
    //    //[Hide("IK", (int)IKGenerigType.LookAt)]
    //    //public bool UseAimDirection;


    //    //[Hide(nameof(IK), (int)IKGenerigType.LookAt)]
    //    //[Tooltip("Limits the Look At from the Min to Max Value")]
    //    //public RangedFloat LookAtLimit;

    //    //[Hide(nameof(IK), (int)IKGenerigType.LookAt)]
    //    public UpVectorType upVector;
    //    [Hide(nameof(upVector), (int)UpVectorType.Local)]
    //    public Vector3 LocalUp;
    //    [Hide(nameof(upVector), (int)UpVectorType.Global)]
    //    public Vector3Var WorldUp;


    //    [Tooltip("Restore the Child bone's rotations after the IK is applied to the bone")]
    //    public bool KeepChildRot;


    //    [Hide(nameof(KeepChildRot), (int)UpVectorType.Global)]
    //    public int[] childs;

    //    [Tooltip("Show Gizmos")]
    //    public bool Gizmos;


    //    //  public TransformValues[] CacheChilds { get; set; }

    //    public Vector3 UpVector => upVector switch
    //    {
    //        UpVectorType.Local => LocalUp,
    //        UpVectorType.Global => (Vector3)WorldUp,
    //        _ => Vector3.up,
    //    };
    //}

}

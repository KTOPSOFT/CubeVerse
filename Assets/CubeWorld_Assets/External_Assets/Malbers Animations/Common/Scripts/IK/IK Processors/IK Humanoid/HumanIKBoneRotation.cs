using UnityEngine;
using MalbersAnimations.Scriptables;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Human Bone Offset <Rotation>")]
    public class HumanIKBoneRotation : IKProcessor
    {
        public enum RotationOffsetType
        {
            [InspectorName("Local Rotation Additive")]
            LocalAdditive,
            [InspectorName("Local Rotation Override")]
            LocalOverride,
            [InspectorName("Root Relative Local Rotation Additive")]
            RootRelativeRotationAdditive,
            [InspectorName("Root Relative Local Rotation Override")]
            RootRelativeRotationOverride,
            [InspectorName("Rotation Relative to [Target]")]
            WorldRotation,
        }

        public RotationOffsetType rotationType;

        [SearcheableEnum] public HumanBodyBones humanBone;


        [Tooltip("Rotation Offset applied to the bone")]
        public Vector3Reference Offset;

        public bool gizmos = true;

        public override bool RequireTargets => false;

        public override void Start(IKSet IKSet, Animator anim, int index)
        {
            IKSet.Var[index].rotations.TryAdd((int)humanBone, Quaternion.identity);
        }

        public override void OnAnimatorIK(IKSet set, Animator anim, int index, float weight)
        {
            var root = anim.transform;
            // var FinalWeight = Weight * weight;

            var Bone = anim.GetBoneTransform(humanBone);

            set.Var[index].rotations[(int)humanBone] = Bone.rotation; //Store the Current World Rotation

            var OffsetRot = Quaternion.Euler(Offset.Value);
            var InverseRot = Quaternion.Inverse(Bone.parent.rotation); //This is the Bone Rotation in world coordinates

            Quaternion finalRotation = Quaternion.identity;

            switch (rotationType)
            {
                case RotationOffsetType.LocalAdditive:
                    finalRotation = Bone.localRotation * OffsetRot;
                    break;
                case RotationOffsetType.LocalOverride:
                    finalRotation = OffsetRot;
                    break;
                case RotationOffsetType.WorldRotation:
                    if (set.Targets == null || set.Targets.Length < TargetIndex || set.Targets[TargetIndex] == null)
                    {
                        Debug.LogWarning($"<B>[IK Processor: {name}].</B>  Target failed in {TargetIndex}");
                        Active = false;
                        return;
                    }
                    var TargetRelative = Quaternion.identity;
                    var Target = set.Targets[TargetIndex];

                    if (Target.Value != null)
                        TargetRelative = Target.rotation;

                    finalRotation = InverseRot * TargetRelative * OffsetRot;
                    break;
                case RotationOffsetType.RootRelativeRotationOverride:
                    finalRotation = InverseRot * root.rotation * OffsetRot;
                    break;

                case RotationOffsetType.RootRelativeRotationAdditive:
                    finalRotation = Bone.localRotation * root.rotation * OffsetRot;
                    break;
                default:
                    break;
            }

            if (!(System.Single.IsNaN(finalRotation.x) || System.Single.IsNaN(finalRotation.y) || System.Single.IsNaN(finalRotation.z)))
            {
                var result = Quaternion.Slerp(Bone.localRotation, finalRotation, weight);
                anim.SetBoneLocalRotation(humanBone, result);
            }
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (animator.GetBoneTransform(humanBone) == null)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}].</B> The Bone [{humanBone}] is not valid on the Avatar");
                return;
            }
            if (rotationType == RotationOffsetType.WorldRotation)
            {
                if (TargetIndex == -1)
                {
                    Debug.LogWarning($"<B>[IK Processor: {name}].</B>  The Target Index is -1 . Please a valid Index that can be used on the Target List");
                    return;
                }
                if (set.Targets.Length <= TargetIndex)
                {
                    Debug.LogWarning($"<B>[IK Processor: {name}].</B>  The Index [{TargetIndex}]  is gratere than the Target List. " +
                        $"Please Add a Target to the Target List on the Index [{TargetIndex}]");
                    return;
                }
                if (set.Targets[TargetIndex].Value == null)
                {
                    Debug.LogWarning($"<B>[IK Processor: {name}].</B>  The Target Index [{TargetIndex}] is null. Please Add a valid Target to the Target List on the Index [{TargetIndex}]");
                    return;
                }

            }


            Debug.Log($"<B>[IK Processor: {name}][HumanIK Bone Rotation]</B>  <color=yellow>[OK]</color>");
        }



#if UNITY_EDITOR
        internal override void OnSceneGUI(IKSet set, Animator animator, UnityEngine.Object Target, int index)
        {
            if (gizmos)
            {
                var RootBone = animator.GetBoneTransform(humanBone);

                Handles.color = Color.yellow;
                if (RootBone != null)
                    Handles.SphereHandleCap(0, RootBone.position, Quaternion.identity, 0.04f, EventType.Repaint);

                foreach (Transform child in RootBone)
                {
                    Handles.SphereHandleCap(0, child.position, Quaternion.identity, 0.02f, EventType.Repaint);
                    Handles.DrawLine(RootBone.position, child.position);
                    Handles.DrawLine(RootBone.position, child.position);
                }

                if (Application.isPlaying)
                {
                    Quaternion startRotation;

                    if (Tools.current == Tool.Rotate)
                    {
                        using (var cc = new EditorGUI.ChangeCheckScope())
                        {
                            Vector3 Pos = RootBone.position;
                            Quaternion NewRotation = Quaternion.identity;

                            switch (rotationType)
                            {
                                case RotationOffsetType.LocalAdditive:
                                    startRotation = set.Var[index].rotations[(int)humanBone]; //Get the Rotation before IK 
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset.Value), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;

                                case RotationOffsetType.LocalOverride:
                                    startRotation = RootBone.parent.rotation;
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset.Value), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;

                                case RotationOffsetType.RootRelativeRotationOverride:
                                    startRotation = animator.rootRotation; ;
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset.Value), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;

                                case RotationOffsetType.RootRelativeRotationAdditive:
                                    startRotation = animator.rootRotation * set.Var[index].rotations[(int)humanBone]; //Get the Rotation before IK 
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset.Value), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;

                                default:
                                    break;
                            }

                            if (cc.changed)
                            {
                                Undo.RecordObject(Target, "Change Rot");
                                Offset.Value = NewRotation.eulerAngles;
                                EditorUtility.SetDirty(Target);
                            }
                        }
                    }
                }
            }
        }

#endif
    }
}

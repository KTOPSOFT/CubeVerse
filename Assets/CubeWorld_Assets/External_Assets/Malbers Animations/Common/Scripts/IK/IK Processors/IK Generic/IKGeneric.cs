using System;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.IK
{
    [Serializable]
    [AddTypeMenu("Generic/IK Offset Rotation")]
    public class IKGeneric : IKProcessor
    {
        public enum IKRotationType
        {
            [InspectorName("Local Rotation Additive")]
            RotationAdditive,
            [InspectorName("Local Rotation Override")]
            RotationOverride
        }

        public IKRotationType IK;
        public Vector3 Offset;

        [Tooltip("Restore the Child bone's rotations after the IK is applied to the bone")]
        public bool KeepChildrenRotation;

        [Tooltip("Restore these extra bones rotations using target index")]
        public int[] KeepBonesInitialRotation;

        [Tooltip("Show Gizmos")]
        public bool Gizmos;

        public override bool RequireTargets => true;

        public override void LateUpdate(IKSet IKSet, Animator anim, int index, float FinalWeight)
        {
            var Bone = IKSet.Targets[index].Value;

            Quaternion TargetRotation = (IK) switch
            {
                IKRotationType.RotationAdditive => Bone.rotation * Quaternion.Euler(Offset),
                IKRotationType.RotationOverride => anim.transform.rotation * Quaternion.Euler(Offset),
                _ => Quaternion.identity,
            };

            Bone.rotation = Quaternion.Lerp(Bone.rotation, TargetRotation, FinalWeight);

            RestoreChildRotation(IKSet);
        }

        private void RestoreChildRotation(IKSet iKSet)
        {
            //Store the bone's Child Rotation
            for (int i = 0; i < KeepBonesInitialRotation.Length; i++)
            {
                var TargetIndex = KeepBonesInitialRotation[i];

                iKSet.Targets[TargetIndex].Value.rotation = iKSet.CacheTargets[TargetIndex].rotation;
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

#if UNITY_EDITOR
        internal override void OnSceneGUI(IKSet set, Animator animator, UnityEngine.Object Target, int index)
        {
            if (Application.isPlaying)
            {
                if (Gizmos && Active)
                {
                    var bone = set.Targets[index].Value;

                    Quaternion startRotation;

                    if (Tools.current == Tool.Rotate)
                    {
                        using (var cc = new EditorGUI.ChangeCheckScope())
                        {
                            Vector3 Pos = bone.position;
                            Quaternion NewRotation = Quaternion.identity;

                            switch (IK)
                            {
                                case IKRotationType.RotationAdditive:
                                    startRotation = bone.parent.rotation; //Get the Rotation before IK 
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;

                                case IKRotationType.RotationOverride:
                                    startRotation = animator.transform.rotation;
                                    NewRotation = Handles.RotationHandle(startRotation * Quaternion.Euler(Offset), Pos);
                                    NewRotation = Quaternion.Inverse(startRotation) * NewRotation;
                                    break;
                                default:
                                    break;
                            }

                            if (cc.changed)
                            {
                                Undo.RecordObject(Target, "Change Rot");
                                Offset = NewRotation.eulerAngles;
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

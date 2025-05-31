using MalbersAnimations.Scriptables;
using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace MalbersAnimations.IK
{

    [Serializable]
    [AddTypeMenu("Generic/LookAt")]
    public class IKGenericLookAt : IKProcessor
    {
        public override bool RequireTargets => true;
        public enum UpVectorType { VectorUp, Local, Global }
        public Vector3 Offset;
        public UpVectorType upVector;
        [Hide(nameof(upVector), (int)UpVectorType.Local)]
        public Vector3 LocalUp = Vector3.up;
        [Hide(nameof(upVector), (int)UpVectorType.Global)]
        public Vector3Var WorldUp;

        public Vector3 UpVector(Animator anim) => upVector switch
        {
            UpVectorType.Local => anim.transform.TransformDirection(LocalUp),
            UpVectorType.Global => (Vector3)WorldUp,
            _ => Vector3.up,
        };

        public override void Start(IKSet IKSet, Animator anim, int index)
        {
            if (index >= IKSet.Targets.Length)
            {
                Debug.LogWarning($"Target index  is out of range for this processor [{name}] -> [{IKSet.Owner.name}]. Disabling Processor!");
                Active = false; // Disable this processor
                return;
            }

            if (IKSet.aimer == null)
            {
                Debug.LogWarning($"There's no Aimer on the IK Set. Generic IK needs an Aimer");
                Active = false; // Disable this processor
                return;
            }
        }

        public override void LateUpdate(IKSet IKSet, Animator anim, int index, float weight)
        {
            if (weight == 0) return; //Do nothing if the weight is zero
            if (IKSet.aimer.AimDirection == Vector3.zero) return; //Do nothing if the Aim Direction is zero

            Transform Bone = IKSet.Targets[index];
            if (Bone == null) return;   //Missing Bone

            Quaternion TargetRotation;
            // var BoneStartRot = IKSet.CacheTargets[index];

            TargetRotation = Quaternion.LookRotation(IKSet.aimer.AimDirection, UpVector(anim)) * Quaternion.Euler(Offset);
            Bone.rotation = Quaternion.Lerp(Bone.rotation, TargetRotation, weight);
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (set.Targets.Length == 0)
            {
                Debug.LogWarning($"There's no Targets on the IK Set. Generic IK needs a Target on on Index [{TargetIndex}]");
            }
            if (set.Targets.Length <= TargetIndex)
            {
                Debug.LogWarning($"The Target Index [{TargetIndex}] is out of range on the IK Set. The IK Set has only {set.Targets.Length} targets");
            }
            if (set.Targets[TargetIndex].Value == null)
            {
                Debug.LogWarning($"The Target in Index [{TargetIndex}] is Empty. Make sure you set a proper value. in the Editor, or at Runtime");
            }
            else
            {
                Debug.Log($"<B>[IK Processor: {name}][IK Generic Look At]</B>  <color=yellow>[OK]</color>");
            }
        }


        //internal override void OnSceneGUI(IKSet set, Animator animator, UnityEngine.Object target, int index)
        //{
        //    if (Application.isPlaying)
        //    {
        //        if (Active)
        //        {
        //            var bone = set.c[index].Value;


        //            if (Tools.current == Tool.Rotate)
        //            {
        //                using (var cc = new EditorGUI.ChangeCheckScope())
        //                {
        //                    Vector3 Pos = bone.position;
        //                    // Quaternion NewRotation = Quaternion.identity;

        //                    var TargetRotation = Quaternion.LookRotation(set.aimer.RawAimDirection, UpVector);


        //                    var rootRotation = bone.parent.rotation; //Get the Rotation before IK 


        //                    var NewRotation = Handles.RotationHandle(TargetRotation * Quaternion.Euler(Offset), Pos);


        //                    NewRotation = Quaternion.Inverse(rootRotation) * NewRotation; //Get the Local Rotation


        //                    if (cc.changed)
        //                    {
        //                        Undo.RecordObject(target, "Change Rot");
        //                        Offset = NewRotation.eulerAngles;
        //                        EditorUtility.SetDirty(target);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}

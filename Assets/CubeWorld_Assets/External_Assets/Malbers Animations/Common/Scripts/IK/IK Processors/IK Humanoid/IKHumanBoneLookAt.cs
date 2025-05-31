using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.IK
{
    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Human Bone Rotation <LookAt>")]
    public class IKHumanBoneLookAt : IKProcessor
    {
        public enum IKRotationType
        {
            [InspectorName("Local Look At (Aim)")]
            LookAt,
            LookAtUpDown,
            [InspectorName("Local Rotation Additive")]
            AdditiveOffset,
            [InspectorName("Local Rotation Override")]
            RotationOverride
        }


        public override bool RequireTargets => false;

        public enum UpVectorType { VectorUp, Local, Global }

        [SearcheableEnum] public HumanBodyBones humanBone;

        [Tooltip("Rottation Offset applied to the bone")]
        public Vector3 offset;


        [Tooltip("Use the Aimer Direction to calculate the LookAt Direction")]
        [Hide("IK", (int)IKRotationType.LookAt)]
        public bool UseAimDirection = true;


        //[Hide("IK", (int)IKGenerigType.LookAt)]
        //[Tooltip("Limits the Look At from the Min to Max Value")]
        //public RangedFloat LookAtLimit = new(90, 120);

        [Hide("IK", (int)IKRotationType.LookAt)]
        public UpVectorType upVector = UpVectorType.VectorUp;
        [Hide("upVector", (int)UpVectorType.Local)]
        public Vector3 LocalUp = new(0, 1, 0);
        [Hide("upVector", (int)UpVectorType.Global)]
        public Vector3Var WorldUp;

        [Tooltip("Show Gizmos")]
        public bool Gizmos = false;

        public Vector3 UpVector
        {
            get
            {
                return upVector switch
                {
                    UpVectorType.Local => LocalUp,
                    UpVectorType.Global => (Vector3)WorldUp,
                    _ => Vector3.up,
                };
            }
        }

        /// <summary>  LookAt the Aimer Direction</summary>
        public override void OnAnimatorIK(IKSet set, Animator anim, int index, float weight)
        {
            var Direction = set.aimer.AimDirection; //Get the Direction from the Aimer
            var Bone = anim.GetBoneTransform(humanBone); //Get the Bone


            var angle = Vector3.Angle(anim.transform.forward, Direction);

            //if (LookAtLimit.maxValue != 0 && LookAtLimit.minValue != 0) //Check the Limit in case there is a limit
            //    weight *= angle.CalculateRangeWeight(LookAtLimit.minValue, LookAtLimit.maxValue);

            if (Gizmos) MDebug.DrawRay(Bone.transform.position, Direction.normalized, Color.Lerp(Color.black, Color.green, weight));

            if (weight == 0) return;


            var OffsetRot = Quaternion.Euler(offset);
            var InverseRot = Quaternion.Inverse(Bone.parent.rotation); //This is the Bone Rotation in world coordinates

            var BoneRotation = Bone.localRotation;



            Quaternion targetRot = InverseRot * Quaternion.LookRotation(Direction, UpVector) * OffsetRot;

            var localRotation = Quaternion.Slerp(BoneRotation, targetRot, weight); //Covert the Look Rotation to Local

            anim.SetBoneLocalRotation(humanBone, localRotation);
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (set.aimer == null)
            {
                Debug.LogError($"The IK Set <B>[{set.Name}]</B> has no Aimer set on the [Aimer] field." +
                    $" <B>[IK Processor: {name}]</B> Needs an Aimer to work." +
                    $" Please add a reference for that index in the [Aimer] field", animator);
            }
            else
            {
                Debug.Log($"<B>[IK Processor: {name}][IKHuman - BoneLookAt]</B>  <color=yellow>[OK]</color>");
            }
        }

#if UNITY_EDITOR && MALBERS_DEBUG
        //public override void OnDrawGizmos(IKSet IKSet, Animator anim, float weight)
        //{
        //    // bool AppIsPlaying = Application.isPlaying;
        //    if (anim == null || anim.avatar == null || !Gizmos) return;

        //    var Bone = anim.GetBoneTransform(humanBone);



        //    var FinalWeight = weight * Weight * GetProcessorAnimWeight(anim);

        //    Handles.color = new Color(0, 1, 0, 0.1f);
        //    Handles.DrawSolidArc(Bone.position, UpVector,
        //        Quaternion.Euler(0, -LookAtLimit.minValue, 0) * anim.transform.forward, LookAtLimit.minValue * 2, 1);



        //    Handles.color = Color.green;
        //    Handles.DrawWireArc(Bone.position,
        //        UpVector, Quaternion.Euler(0, -LookAtLimit.minValue, 0) * anim.transform.forward, LookAtLimit.minValue * 2, 1);


        //    Handles.color = new Color(0, 0.3f, 0, 0.2f);
        //    var Maxlimit = (LookAtLimit.minValue - LookAtLimit.maxValue);

        //    Handles.DrawSolidArc(Bone.position,
        //        UpVector, Quaternion.Euler(0, -(LookAtLimit.minValue), 0) * anim.transform.forward, (Maxlimit), 1);

        //    Handles.DrawSolidArc(Bone.position,
        //        UpVector, Quaternion.Euler(0, (LookAtLimit.minValue), 0) * anim.transform.forward, -(Maxlimit), 1);


        //    Handles.color = Color.black;

        //    Handles.DrawWireArc(Bone.position,
        //        UpVector, Quaternion.Euler(0, -(LookAtLimit.minValue), 0) * anim.transform.forward, (Maxlimit), 1);

        //    Handles.DrawWireArc(Bone.position,
        //        UpVector, Quaternion.Euler(0, (LookAtLimit.minValue), 0) * anim.transform.forward, -(Maxlimit), 1);
        //}

        internal override void OnSceneGUI(IKSet set, Animator animator, Object target, int index)
        {
            if (Gizmos && animator && animator.avatar)
            {
                var Bone = animator.GetBoneTransform(humanBone);

                if (Bone)
                {
                    Bone.GetPositionAndRotation(out Vector3 pos, out var rot);

                    Handles.color = Color.green;
                    Handles.SphereHandleCap(0, pos, rot, 0.02f, EventType.Repaint);


                    if (Application.isPlaying)
                    {

                        if (Tools.current == Tool.Rotate)
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                Quaternion oldQ = animator.transform.rotation * Quaternion.Euler(offset);

                                Quaternion NewRotation = Handles.RotationHandle(oldQ, pos);

                                NewRotation = Quaternion.Inverse(animator.transform.rotation) * NewRotation;

                                if (cc.changed)
                                {
                                    Undo.RecordObject(target, "Change Offset");
                                    offset = NewRotation.eulerAngles;
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}

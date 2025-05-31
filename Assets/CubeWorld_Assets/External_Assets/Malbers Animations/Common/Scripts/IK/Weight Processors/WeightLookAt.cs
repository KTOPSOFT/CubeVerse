using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking the Look At Angle of the Animator / </summary>
    [System.Serializable]
    [AddTypeMenu("Look At Range")]
    public class WeightLookAt : WeightProcessor
    {
        public enum UpVectorType { VectorUp, Local, Global }

        [Tooltip("Limits the Look At from the Min to Max Value")]
        public RangedFloat LookAtLimit = new(90, 120);
        [Tooltip("Offset the Aim Direction by this angle")]
        public float AngleOffset = 0;

        [Tooltip("Normalize the weight by this value")]
        public float normalizedBy = 1;

        public UpVectorType upVector = UpVectorType.VectorUp;



        [Hide("upVector", (int)UpVectorType.Local)]
        public Vector3 LocalUp = new(0, 1, 0);
        [Hide("upVector", (int)UpVectorType.Global)]
        public Vector3Var WorldUp;


        public bool showGizmos = true;
        [Hide(nameof(showGizmos))]
        public float GizmoRadius = 1f;
        [Hide(nameof(showGizmos))]
        public Color GizmoColor = Color.green;
        // private Vector3 DirVector;
        public Vector3 UpVector(Animator anim) => upVector switch
        {
            UpVectorType.Local => anim.transform.TransformDirection(LocalUp),
            UpVectorType.Global => (Vector3)WorldUp,
            _ => Vector3.up,
        };


        public override float Process(IKSet set, float weight)
        {
            if (set.aimer == null) return 0; //Do nothing if there is no AIM

            var anim = set.Animator;
            var direction = set.aimer.AimDirection;//Get the Aim Direction


            var forwardDir = anim.transform.forward;

            if (AngleOffset != 0)
            {
                forwardDir = Quaternion.Euler(UpVector(anim) * AngleOffset) * forwardDir;
            }


            var angle = Vector3.Angle(forwardDir, direction);

            if (LookAtLimit.maxValue != 0 && LookAtLimit.minValue != 0) //Check the Limit in case there is a limit
                //weight *= angle.CalculateRangeWeight(LookAtLimit.minValue, LookAtLimit.maxValue);
                weight = Mathf.Min(weight, angle.CalculateRangeWeight(LookAtLimit.minValue, LookAtLimit.maxValue));

            return weight;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos(IKSet IKSet, Animator anim)
        {
            if (!showGizmos) return;

            if (anim == null || IKSet.aimer == null || IKSet.aimer.AimOrigin == null) return;

            var Bone = IKSet.aimer.AimOrigin;
            var UpVector = this.UpVector(anim);

            var forward = anim.transform.forward;

            if (AngleOffset != 0)
            {
                forward = Quaternion.Euler(UpVector * AngleOffset) * forward;
            }

            var MinRot_Neg = Quaternion.Euler(UpVector * -LookAtLimit.minValue) * forward;
            var MinRot_Pos = Quaternion.Euler(UpVector * LookAtLimit.minValue) * forward;


            var LightColor = GizmoColor;
            LightColor.a = 0.1f;

            var darkcolor = GizmoColor * 0.3f;
            darkcolor.a = 0.2f;

            Handles.color = LightColor;

            Handles.DrawSolidArc(Bone.position, UpVector, MinRot_Neg, LookAtLimit.minValue * 2, GizmoRadius);


            Handles.color = GizmoColor; //Draw the lines
            Handles.DrawWireArc(Bone.position, UpVector, MinRot_Neg, LookAtLimit.minValue * 2, GizmoRadius);


            Handles.color = darkcolor;
            var Maxlimit = (LookAtLimit.minValue - LookAtLimit.maxValue);

            Handles.DrawSolidArc(Bone.position, UpVector, MinRot_Neg, (Maxlimit), GizmoRadius);

            Handles.DrawSolidArc(Bone.position, UpVector, MinRot_Pos, -(Maxlimit), GizmoRadius);

            darkcolor.a = 1f;

            Handles.color = darkcolor;

            Handles.DrawWireArc(Bone.position, UpVector, MinRot_Neg, (Maxlimit), GizmoRadius);

            Handles.DrawWireArc(Bone.position, UpVector, MinRot_Pos, -(Maxlimit), 1);
        }
#endif
    }
}


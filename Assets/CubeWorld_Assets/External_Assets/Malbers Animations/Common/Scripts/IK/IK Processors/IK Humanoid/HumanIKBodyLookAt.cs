using MalbersAnimations.Scriptables;
using System;
using UnityEngine;

namespace MalbersAnimations.IK
{

    [System.Serializable]
    [AddTypeMenu("Humanoid/IK Body Look At")]
    public class HumanIKBodyLookAt : IKProcessor
    {
        public override bool RequireTargets => false;

        //[Tooltip("Origin used from the IK Set. Targets Index Value")]
        //[Min(0)] public int OriginIndex = 1;

        [Header("Set LookAt Weights")]
        [Tooltip("(0-1) determines how much the body is involved in the LookAt.")]
        [Range(0, 1)] public float BodyWeight = 0.5f;
        [Tooltip("(0-1) determines how much the head is involved in the LookAt.")]
        [Range(0, 1)] public float HeadWeight = 1;
        [Tooltip("(0-1) determines how much the eyes is involved in the LookAt.")]
        [Range(0, 1)] public float EyesWeight = 1;

        [Tooltip(
            "(0-1) 0.0 means the character is completely unrestrained in motion, " +
            "1.0 means he's completely clamped (look at becomes impossible), " +
            "and 0.5 means he'll be able to move on half of the possible range (180 degrees).")]
        [Range(0, 1)] public float ClampWeight = 0.75f;

        [Header("Extras")]
        [Tooltip("(0-1) Distance to Determine the LookAtPosition")]
        public float Distance = 50f;

        [Tooltip("Offset of the BodyLookAt")]
        public Vector2Reference offset = new();

        public override void Start(IKSet set, Animator anim, int index)
        {
            if (TargetIndex >= set.Targets.Length)
            {
                Array.Resize(ref set.Targets, TargetIndex + 1);
            }

            if (set.Targets[TargetIndex] == null)
            {
                set.Targets[TargetIndex] = set.aimer.AimOrigin; //Use the Aimer origin by default if there is there is no Target
            }
        }


        public override void OnAnimatorIK(IKSet set, Animator animator, int index, float weight)
        {
            //var Target = set.Targets[TargetIndex];
            //var Origin = set.Targets[OriginIndex];
            if (TargetIndex >= set.Targets.Length) return;

            var Origin = set.Targets[TargetIndex];
            if (Origin == null) return; //Skip the code if we cannot apply IK (there's no Origin)


            // var Dir = MTools.DirectionTarget(Origin, Target);
            var Dir = set.aimer.AimDirection;

            Dir = Quaternion.AngleAxis(offset.x, Vector3.up) * Dir;
            var RightV = Vector3.Cross(Dir, Vector3.up);
            Dir = Quaternion.AngleAxis(offset.y, RightV) * Dir;

            var ray = new Ray(Origin.position, Dir);
            var Point = ray.GetPoint(Distance);
            Debug.DrawRay(Origin.position, Dir * Distance, Color.cyan);
            animator.SetLookAtWeight(weight, BodyWeight, HeadWeight, EyesWeight, ClampWeight);
            animator.SetLookAtPosition(Point);
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (set.Targets.Length == 0)
            {
                Debug.LogWarning($"There's no Targets on the IK Set. Human IK needs a Target on on Index [{TargetIndex}]");
            }
            else if (set.Targets.Length <= TargetIndex)
            {
                Debug.LogWarning($"The Target Index [{TargetIndex}] is out of range on the IK Set. The IK Set has only {set.Targets.Length} targets. Target in index [{index}] used for the Aim Origin");
            }
            else if (set.aimer == null)
            {
                Debug.LogWarning($"There's no Aimer on the IK Set. Human IK needs an Aimer to get the Aim Direction");
            }
            else if (set.Targets[TargetIndex] == null)
            {
                Debug.LogWarning($"Targets - Element[{TargetIndex}] on the IK Set is Null or Empty. " +
                    $"Please add a reference in the editor or at runtime.(IK Processor will be ignored)");
            }
            else
            {
                Debug.Log($"<B>[IK Processor: {name}][IKHuman - BodyLookAt]</B>  <color=yellow>[OK]</color>");
            }
        }
    }
}

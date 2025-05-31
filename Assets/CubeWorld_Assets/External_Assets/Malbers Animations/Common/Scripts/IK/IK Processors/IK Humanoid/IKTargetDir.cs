using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [Serializable, AddTypeMenu("Humanoid/IK Target Direction")]
    public class IKTargetDir : IKProcessor
    {
        [Tooltip("The specific body part (hand or foot) that will be controlled by the IK system.")]
        [SerializeField] AvatarIKGoal avatarIKGoal;

        [Tooltip("LayerMask used for detecting valid IK target layers.")]
        [SerializeField] LayerMask detectionLayer;

        [Tooltip("The tag used to identify valid IK targets.")]
        [SerializeField] Tag iKTargetTag;

        [Tooltip("Radius of the SphereCast to detect IK targets.")]
        [SerializeField] float detectionRadius = 0.2f;

        [Tooltip("If true, the direction of the IK target detection is flattened along the Y-axis (useful for horizontal detection).")]
        [SerializeField] bool flattenDirection;

        [Tooltip("Offset applied to the hit point where the raycast intersects the target. This offset is relative to the hit surface.")]
        [SerializeField] Vector3 positionOffset = Vector3.zero;

        [Tooltip("Offset applied to the start position of the raycast. This offset is relative to the character's transform.")]
        [SerializeField] Vector3 rayStartOffset = Vector3.zero;

        [Tooltip("Enable or disable the rotation adjustment for the IK target.")]
        [SerializeField] bool enableRotation;

        [Hide("enableRotation", false)]
        [Tooltip("Rotation offset applied to the hit normal at the raycast hit point.")]
        [SerializeField] Vector3 rotationOffset = Vector3.zero;

        private Transform iKTarget;

        private Transform bodyPart;

        public override bool RequireTargets => true;

        public float lerpSpeed = 5f; // Speed of the smooth transition
        private Vector3 currentIKPosition;

        public override void Start(IKSet IKSet, Animator anim, int index)
        {
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot:
                    bodyPart = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                    break;
                case AvatarIKGoal.RightFoot:
                    bodyPart = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                    break;
                case AvatarIKGoal.LeftHand:
                    bodyPart = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                    break;
                case AvatarIKGoal.RightHand:
                    bodyPart = anim.GetBoneTransform(HumanBodyBones.RightHand);
                    break;
            }
        }

        public override void OnAnimatorIK(IKSet IKSet, Animator anim, int index, float weight)
        {
            IKSet.Targets = FindIKTargetsUsingSphereCast(bodyPart, iKTargetTag, detectionRadius, detectionLayer);

            if (IKSet.Targets.Length != 0)
            {
                Transform targets;
                targets = anim.transform.NearestTransform(IKSet.Targets);

                if (targets != null)
                {
                    Vector3 targetPosition = PerformRaycast(anim, bodyPart, targets, positionOffset, rayStartOffset);
                    Vector3 finalPosition = targetPosition + anim.transform.TransformDirection(positionOffset);

                    // Smoothly interpolate position
                    currentIKPosition = Vector3.Lerp(currentIKPosition, finalPosition, Time.deltaTime * lerpSpeed);

                    anim.SetIKPositionWeight(avatarIKGoal, weight);
                    anim.SetIKPosition(avatarIKGoal, finalPosition);

                    if (enableRotation && targetPosition != bodyPart.position) // If the targetposition is the default position don't roatate
                    {
                        Quaternion finalRotation = anim.transform.rotation * Quaternion.Euler(rotationOffset);

                        anim.SetIKRotationWeight(avatarIKGoal, weight);
                        anim.SetIKRotation(avatarIKGoal, finalRotation);
                    }
                }
            }
        }

        /// <summary>
        /// SphereCast method to detect IK targets and search within child objects of the hit
        /// </summary>
        public TransformReference[] FindIKTargetsUsingSphereCast(TransformReference parent, Tag tag, float radius, LayerMask detectionLayer)
        {
            List<TransformReference> targets = new();

            // Perform the SphereCastAll
            RaycastHit[] hits = Physics.SphereCastAll(parent.position, radius, parent.Value.forward, 0.1f, detectionLayer);

            {
                // If there are hits, search for targets in the children of hit objects
                foreach (RaycastHit hit in hits)
                {
                    Transform[] children = hit.transform.GetComponentsInChildren<Transform>();

                    foreach (Transform child in children)
                    {
                        if (child.HasMalbersTag(tag))
                        {
                            targets.Add(child);
                        }
                    }
                }
            }

            return targets.ToArray();  // Return the array of found targets
        }

        private Vector3 PerformRaycast(Animator anim, Transform rayStart, Transform target, Vector3 hitOffset, Vector3 rayStartOffset)
        {
            if (rayStart == null || target == null)
            {
                return rayStart.position;
            }

            Vector3 rayStartPosition = rayStart.position + anim.transform.TransformDirection(rayStartOffset);

            Vector3 direction = (target.position - rayStartPosition).normalized;

            if (flattenDirection)
            {
                direction = direction.FlattenY();
            }

            if (Physics.Raycast(rayStartPosition, direction, out RaycastHit hit, detectionRadius, detectionLayer))
            {
                Vector3 rayHitPosition = hit.point + anim.transform.TransformDirection(hitOffset);

                return rayHitPosition;
            }

            return rayStart.position;
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            if (iKTargetTag == null)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}]</B>  <color=red>[No Tag defined]</color>");
                return;
            }


            Debug.Log($"<B>[IK Processor: {name}]</B>  <color=green>[OK]</color>");
        }

        public override void OnDrawGizmos(IKSet IKSet, Animator anim, float weight)
        {
            if (bodyPart != null)
            {
                // Calculate the starting position of the ray with the offset applied
                Vector3 rayStartPosition = bodyPart.position + anim.transform.TransformDirection(rayStartOffset);

                // Default color: white
                Gizmos.color = Color.white;

                // If there are targets, calculate direction and draw ray/sphere
                if (IKSet.Targets.Length > 0)
                {
                    Transform target = anim.transform.NearestTransform(IKSet.Targets);

                    if (target != null)
                    {
                        Vector3 direction = (target.position - rayStartPosition).normalized;
                        if (flattenDirection)
                        {
                            direction = direction.FlattenY();
                        }

                        // Check if the ray hits something
                        if (Physics.Raycast(rayStartPosition, direction, out RaycastHit hit, detectionRadius, detectionLayer))
                        {
                            // If hit, change the color to green
                            Gizmos.color = Color.green;

                            // Draw a sphere at the hit point
                            Gizmos.DrawSphere(hit.point, 0.1f);
                        }
                        else
                        {
                            // If no hit, keep the default white color
                            Gizmos.color = Color.white;
                        }

                        // Draw the ray
                        Gizmos.DrawLine(rayStartPosition, rayStartPosition + (direction * detectionRadius));
                    }
                }
                else
                {
                    // If no targets, draw the sphere at the ray start in white
                    Gizmos.color = Color.white;
                }

                // Always draw the sphere at the start point (white if no hit, green if hit)
                Gizmos.DrawWireSphere(rayStartPosition, detectionRadius);
            }
        }
    }
}

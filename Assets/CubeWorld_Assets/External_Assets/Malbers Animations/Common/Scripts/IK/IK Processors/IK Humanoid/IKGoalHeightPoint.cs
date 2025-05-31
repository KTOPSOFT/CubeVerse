using System;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [Serializable]
    [AddTypeMenu("Humanoid/IK Goal Height Point")]
    public class IKGoalHeightPoint : IKProcessor
    {
        [Tooltip("Target limb to lock using IK. Options: LeftFoot, RightFoot, LeftHand, RightHand.")]
        [SerializeField] AvatarIKGoal avatarIKGoal;

        [Tooltip("Layer mask to specify which objects are detected.")]
        [SerializeField] LayerMask detectionLayer;

        [Tooltip("Radius of the SphereCast used for initial detection.")]
        [Min(0), SerializeField] float sphereCastRadius = 0.15f;

        [Tooltip("Maximum distance for the SphereCast.")]
        [Min(0), SerializeField] float sphereCastDistance = 1.0f;

        [Tooltip("Adjustment for how deep into the surface the SphereCast should detect.")]
        [Min(0), SerializeField] float heightOriginPenetrationDepth = 0.05f;

        [Tooltip("Height offset applied to adjust the raycast origin relative to the hit surface. What should be the max height?")]
        [Min(0), SerializeField] float heightOriginUpOffset = 0.5f;

        [Tooltip("Forward offset applied to the initial ray origin, relative to the character's position.")]
        [SerializeField] Vector3 sphereCastOffset = new Vector3(0,0, -0.5f);

        [Tooltip("Additional offset applied to adjust the IK target's final position.")]
        [SerializeField] Vector3 targetPosOffsetDistance;

        [Tooltip("Enable or disable the rotation adjustment for the IK target.")]
        [SerializeField] bool enableRotation;

        [Hide("enableRotation", false)]
        [Tooltip("Offset to apply in Euler angles when adjusting rotation.")]
        [SerializeField] Vector3 rotationOffset;

        private Transform bodyPart;

        public override bool RequireTargets => false; // This processor does not require specific targets

        public override void Start(IKSet set, Animator anim, int index)
        {
            // Cache the RootBone in the local variables 
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                    break;
                case AvatarIKGoal.RightFoot:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                    break;
                case AvatarIKGoal.LeftHand:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                    break;
                case AvatarIKGoal.RightHand:
                    set.Var[index].Bone = anim.GetBoneTransform(HumanBodyBones.RightHand);
                    break;
                default: break;
            }
        }

        public override void OnAnimatorIK(IKSet set, Animator anim, int index, float weight)
        {
            bodyPart = set.Var[index].Bone; // Get the Bone for the IK goal

            CheckForHeighestPoint(set, anim, index, weight);

        }

        public void CheckForHeighestPoint(IKSet set, Animator anim, int index, float weight)
        {            
            Vector3 origin = bodyPart.position + anim.transform.TransformDirection(sphereCastOffset);

            // Perform the initial check to detect a valid hit point
            bool forwardHitFound = (Physics.SphereCast(origin, sphereCastRadius, anim.transform.forward, out RaycastHit forwardHit, sphereCastDistance, detectionLayer));

            if (forwardHitFound)
            {
                Vector3 upwardDirection = Vector3.up;// Vector3.Cross(Vector3.Cross(forwardHit.normal, Vector3.up), forwardHit.normal).normalized;

                Vector3 heightOrigin = forwardHit.point
                                     + (anim.transform.forward * heightOriginPenetrationDepth)  // Go a bit deeper than the hit point
                                     + (upwardDirection * heightOriginUpOffset);                // Move upwards by the offset to be set higher than the hit point

                // Shoot a ray downward from the adjusted heightOrigin to detect the highest surface.
                bool heightHitFound = Physics.Raycast(heightOrigin, -upwardDirection, out RaycastHit heightHit, heightOriginUpOffset*1.1f, detectionLayer); 
                
                if (heightHitFound)
                {
                    ApplyIKPositionRotation(anim, heightHit.point, weight);
                }
            }
        }

        private void ApplyIKPositionRotation(Animator anim, Vector3 position, float weight)
        {
            Vector3 finalPosition = position + anim.transform.TransformDirection(targetPosOffsetDistance);
            anim.SetIKPositionWeight(avatarIKGoal, weight);
            anim.SetIKPosition(avatarIKGoal, finalPosition);

            if (enableRotation)
            {
                Quaternion finalRotation = anim.transform.rotation * Quaternion.Euler(rotationOffset);

                anim.SetIKRotationWeight(avatarIKGoal, weight);
                anim.SetIKRotation(avatarIKGoal, finalRotation);
            }
        }

        public override void Validate(IKSet set, Animator animator, int index)
        {
            // Define thresholds for "too small" values
            const float minPenetrationDepth = 0.01f;
            const float minUpOffset = 0.1f;
            const float minSphereCastRadius = 0.05f;
            const float minSphereCastDistance = 0.1f;

            bool isValid = true;

            // Validate heightOriginPenetrationDepth
            if (heightOriginPenetrationDepth < minPenetrationDepth)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}]</B>  <color=red>[Warning]</color> Penetration Depth is too small, it should be greater than {minPenetrationDepth}.");
                isValid = false;
            }

            // Validate heightOriginUpOffset
            if (heightOriginUpOffset < minUpOffset)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}]</B>  <color=red>[Warning]</color> Height Up Offset is too small, it should be greater than {minUpOffset}.");
                isValid = false;
            }

            // Validate sphereCastRadius
            if (sphereCastRadius < minSphereCastRadius)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}]</B>  <color=red>[Warning]</color> SphereCast Radius is too small, it should be greater than {minSphereCastRadius}.");
                isValid = false;
            }

            // Validate sphereCastDistance
            if (sphereCastDistance < minSphereCastDistance)
            {
                Debug.LogWarning($"<B>[IK Processor: {name}]</B>  <color=red>[Warning]</color> SphereCast Distance is too small, it should be greater than {minSphereCastDistance}.");
                isValid = false;
            }

            // If everything is valid, print an OK message
            if (isValid)
            {
                Debug.Log($"<B>[IK Processor: {name}][IKGoalHeightPoint]</B>  <color=green>[OK]</color> All parameters are valid.");
            }
        }

        public override void OnDrawGizmos(IKSet IKSet, Animator anim, float weight)
        {
            if (bodyPart != null)
            {
                // Calculate the starting position for the sphere cast with the offset applied
                Vector3 sphereCastOrigin = bodyPart.position + anim.transform.TransformDirection(sphereCastOffset);

                // Default color: white
                Gizmos.color = Color.white;

                // Perform the sphere cast
                bool forwardHitFound = Physics.SphereCast(sphereCastOrigin, sphereCastRadius, anim.transform.forward, out RaycastHit forwardHit, sphereCastDistance, detectionLayer);

                // If a forward hit is detected, change the color to green
                if (forwardHitFound)
                {
                    Gizmos.color = Color.yellow;

                    // Draw the hit point (small green sphere)
                    Gizmos.DrawSphere(forwardHit.point, 0.05f);

                    // Draw the height ray (additional detection from the hit point)
                    Vector3 upwardDirection = Vector3.up;
                    Vector3 heightOrigin = forwardHit.point + (anim.transform.forward * heightOriginPenetrationDepth) + (upwardDirection * heightOriginUpOffset);

                    bool heightHitFound = Physics.Raycast(heightOrigin, -upwardDirection, out RaycastHit heightHit, heightOriginUpOffset*1.1f, detectionLayer);

                    // If a height hit is found, draw a green line and sphere at the hit point
                    if (heightHitFound)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(heightOrigin, heightHit.point);
                        Gizmos.DrawSphere(heightHit.point, 0.05f);
                    }
                    else
                    {
                        // Draw the height ray as white if no hit is detected
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(heightOrigin, heightOrigin - upwardDirection * heightOriginUpOffset);
                    }
                }

                // Draw the sphere cast and ray
                Gizmos.color = forwardHitFound ? Color.green : Color.white;
                Gizmos.DrawWireSphere(sphereCastOrigin, sphereCastRadius);
                Gizmos.DrawLine(sphereCastOrigin, sphereCastOrigin + anim.transform.forward * sphereCastDistance);
            }
        }

    }
}
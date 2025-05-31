using MalbersAnimations.Scriptables;
using System;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Rigidbody/AddForce")]
    public class RigidBodyForceReaction : Reaction
    {
        public enum RB_ReactionForce { AddForce, AddForceAtPosition, AddExplosion, AddTorque, AddRelativeForce, AddRelativeTorque }

        public override Type ReactionType => typeof(Rigidbody);

        public RB_ReactionForce action = RB_ReactionForce.AddForce;
        public ForceMode mode = ForceMode.Force;

        public bool useGravity = true;

        [Tooltip("Direction and Position to apply to the force")]
        public TransformReference direction = new();
        [Tooltip("Intensity of the force to apply to the Reaction")]
        public float force = 100f;

        [Hide("action", 2)]
        public float radius = 10f;
        [Hide("action", 2)]
        public float upModifier = 5f;

        protected override bool _TryReact(Component component)
        {
            var rb = component as Rigidbody;

            rb.isKinematic = false; //Setting kinematic to false because forces cannot be applied to kinematic objects
            rb.useGravity = useGravity;
            rb.constraints = RigidbodyConstraints.None; //make sure there's no constraints

            switch (action)
            {
                case RB_ReactionForce.AddForce:
                    rb.AddForce(direction.Value.forward * force, mode);
                    break;
                case RB_ReactionForce.AddForceAtPosition:
                    rb.AddForceAtPosition(direction.Value.forward * force, direction.Value.position, mode);
                    break;
                case RB_ReactionForce.AddExplosion:
                    rb.AddExplosionForce(force, direction.Value.position, radius, upModifier, mode);
                    break;
                case RB_ReactionForce.AddTorque:
                    rb.AddTorque(direction.Value.forward * force, mode);
                    break;
                case RB_ReactionForce.AddRelativeForce:
                    rb.AddRelativeForce(direction.Value.forward * force, mode);
                    break;
                case RB_ReactionForce.AddRelativeTorque:
                    rb.AddRelativeTorque(direction.Value.forward * force, mode);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}

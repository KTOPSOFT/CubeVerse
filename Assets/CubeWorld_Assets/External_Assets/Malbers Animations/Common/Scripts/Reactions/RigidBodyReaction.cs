 
using System;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Rigidbody/Properties")]
    public class RigidBodyReaction : Reaction
    {
        public enum RB_Reaction { IsKinematic, UseGravity, Drag,AngularDrag, Constraints, Collisions}

        public override Type ReactionType => typeof(Rigidbody);

        public RB_Reaction action = RB_Reaction.IsKinematic;
        [Hide("action", (int)RB_Reaction.IsKinematic, (int)RB_Reaction.UseGravity)]
        public bool m_value = true;

        [Hide("action", (int)RB_Reaction.Drag, (int)RB_Reaction.AngularDrag)]
        public float value = 0f;

        [Hide("action", (int)RB_Reaction.Constraints)]
        public RigidbodyConstraints _value = RigidbodyConstraints.None;
        [Hide("action", (int)RB_Reaction.Collisions, (int)RB_Reaction.IsKinematic)]
        public CollisionDetectionMode CollisionDetection = CollisionDetectionMode.Discrete;


        protected override bool _TryReact(Component component)
        {
            var rb = component as Rigidbody;

            switch (action)
            {
                case RB_Reaction.IsKinematic:
                    rb.isKinematic = m_value;
                    rb.collisionDetectionMode = CollisionDetection;
                    break;
                case RB_Reaction.UseGravity:
                    rb.useGravity = m_value;
                    break;
                case RB_Reaction.Drag:
                    rb.linearDamping = value;
                    break;
                case RB_Reaction.AngularDrag:
                    rb.angularDamping = value;
                    break;
                case RB_Reaction.Constraints:
                    rb.constraints = _value;
                    break;
                case RB_Reaction.Collisions:
                    rb.collisionDetectionMode = CollisionDetection;
                    break;
            }
            return true;
        }
    }
}

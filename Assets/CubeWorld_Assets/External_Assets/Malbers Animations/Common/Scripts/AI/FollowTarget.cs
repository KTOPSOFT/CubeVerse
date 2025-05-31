using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/AI/Follow Target")]
    public class FollowTarget : MonoBehaviour
    {
        public TransformReference target;
        [Min(0)] public float stopDistance = 3;
        [Min(0)] public float SlowDistance = 6;
        [Tooltip("Limit for the Slowing Multiplier to be applied to the Speed Modifier")]
        [Range(0, 1)][SerializeField] private float slowingLimit = 0.3f;

        public bool LookAt = true; // Boolean to enable or disable LookAt in the Inspector

        private ICharacterMove animal;
        private float RemainingDistance;

        public float SlowMultiplier
        {
            get
            {
                var result = 1f;
                if (SlowDistance > stopDistance && RemainingDistance < SlowDistance)
                    result = Mathf.Max(RemainingDistance / SlowDistance, slowingLimit);

                return result;
            }
        }

        void Awake()
        {
            animal = GetComponentInParent<ICharacterMove>();
        }

        void FixedUpdate()
        {
            if (target == null || target.Value == null) return; // Ensure target.Value is not null

            Vector3 direction = (target.Value.position - transform.position).normalized;
            RemainingDistance = Vector3.Distance(transform.position, target.Value.position);

            if (RemainingDistance > stopDistance)
            {
                animal.Move(direction * SlowMultiplier);
            }
            else
            {
                if (LookAt)
                {
                    animal.RotateAtDirection(direction);
                }
                else
                {
                    animal.StopMoving();
                }
            }
        }




        private void OnDisable()
        {
            animal.Move(Vector3.zero);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var center = transform.position;

            if (Application.isPlaying && target.Value)
            {
                center = target.position;
            }

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.up, stopDistance);

            if (SlowDistance > stopDistance)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(center, Vector3.up, SlowDistance);
            }
        }
#endif
    }
}

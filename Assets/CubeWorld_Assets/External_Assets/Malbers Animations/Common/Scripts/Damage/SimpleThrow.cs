using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Damage/Simple Throw")]
    public class SimpleThrow : MonoBehaviour, IAnimatorListener, IThrower
    {
        [Tooltip("Gameobject with a rigidbody to throw")]
        public GameObject projectile;
        [RequiredField, Tooltip("Origin of the trhower")]
        public Transform throwOrigin;

        [Delayed]
        public float MinForce = 20;
        [Delayed]
        public float MaxForce = 50;

        [Range(0f, 1f), Tooltip("0 = Min Force, 1 = Max Force")]
        public float ForceRange = 1f;

        [SerializeField] private LayerReference hitLayer = new(-1);
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [SerializeField, Tooltip("Gravity to apply to the Projectile. By default is set to Physics.gravity")]
        private Vector3Reference gravity = new(Physics.gravity);


        [SerializeField, Tooltip("Apply Gravity after certain distance is reached")]
        private FloatReference m_AfterDistance = new(0f);
        public float AfterDistance { get => m_AfterDistance.Value; set => m_AfterDistance.Value = value; }

        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }

        /// <summary> Is Used to calculate the Trajectory and Display it as a LineRenderer </summary>
        public System.Action<bool> Predict { get; set; } = delegate { };

        public Vector3 AimOriginPos => throwOrigin.position;

        public Transform AimOrigin => throwOrigin;

        public Vector3 Velocity => throwOrigin.forward * Mathf.Lerp(MinForce, MaxForce, ForceRange);

        public LayerMask Layer { get => hitLayer.Value; set => hitLayer.Value = value; }
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }

        public GameObject Owner => transform.gameObject;

        public void Throw() => Throw(projectile);

        public void Fire() => Throw(projectile);

        public void Throw(GameObject b)
        {
            if (b == null) return;
            projectile = b;

            var p = projectile;

            if (projectile.IsPrefab())  p = Instantiate(projectile);

            if (p)
            {
                p.transform.position = throwOrigin.position;
                p.transform.parent = null;

                var rb = p.GetComponent<Rigidbody>();
                var col = p.GetComponent<Collider>();


                if (col)
                {
                    col.enabled = true;
                    col.isTrigger = false;
                }

                if (rb)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Velocity, ForceMode.VelocityChange);
                }

                if (!projectile.IsPrefab()) projectile = null;
            }

            Predict.Invoke(false);
        }

        public void SetForceRange(float value)
        {
            ForceRange = Mathf.Clamp01(value);
            if (projectile) Predict.Invoke(true);
        }

        public void SetProjectile(GameObject b) { projectile = b; }


        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        private void OnValidate()
        {
            MinForce = Mathf.Min(MinForce, MaxForce);
        }


#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.ArrowHandleCap(0, transform.position, transform.rotation, MaxForce * 0.005f, EventType.Repaint);
        }
#endif

    }
}
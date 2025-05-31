using MalbersAnimations.Conditions;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary> This is used when the collider is in a different gameObject and you need to check the Trigger Events
    /// Create this component at runtime and subscribe to the UnityEvents </summary>
    [AddComponentMenu("Malbers/Utilities/Colliders/Trigger Enter")]
    [SelectionBase]
    public class TriggerEnter : MonoBehaviour
    {
        public LayerReference Layer = new(-1);
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Tooltip("On Trigger Enter only works with the first colliders that enters")]
        public bool UseOnce;

        [Tooltip("On Trigger Enter only works. Disables the entire gameObject after its use")]
        public bool DisableAfterUse = true;
        [Tooltip("Destroy the GameObject after the Trigger Enter")]
        public bool m_Destroy = false;
        [Tooltip("Destroy after x Seconds"), Hide(nameof(m_Destroy))]
        [Min(0)] public float DestroyAfter = 0;


        [Tooltip("Extra conditions to check to filter the colliders entering OnTrigger Enter")]
        public Conditions2 Conditions = new();

        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        public ColliderEvent onTriggerEnter = new();
        public GameObjectEvent onCoreObject = new();
        public RigidbodyEvent OnRigidBodyEnter = new();

        /// <summary> Collider Component used for the Trigger Proxy </summary>
        public Collider OwnCollider { get; private set; }
        public bool Active { get => enabled; set => enabled = value; }
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }

        private void OnEnable()
        {
            OwnCollider = GetComponent<Collider>();

            Active = true;

            if (OwnCollider)
            {
                OwnCollider.isTrigger = true;
            }
            else
            {
                Active = false;
                Debug.LogError("This Script requires a Collider, please add any type of collider", this);
            }
        }
        public bool TrueConditions(Collider other)
        {
            if (!Active) return false;
            if (Tags != null && Tags.Length > 0)
            {
                if (!other.gameObject.HasMalbersTagInParent(Tags)) return false;
            }

            if (OwnCollider == null) return false; // you are 
            if (other == null) return false; // you are CALLING A ELIMINATED ONE
            if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger) return false; // Check Trigger Interactions 
            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false;                 // Do not Interact with yourself
            if (other.transform.SameHierarchy(transform)) return false;    // Do not Interact with yourself

            if (!Conditions.Evaluate(other)) return false;

            return true;
        }
        void OnTriggerEnter(Collider other)
        {
            if (TrueConditions(other))
            {
                var core = other.GetComponentInParent<IObjectCore>();

                onCoreObject.Invoke(core != null ? core.transform.gameObject : other.transform.root.gameObject);
                onTriggerEnter.Invoke(other);

                if (other.attachedRigidbody)
                {
                    OnRigidBodyEnter.Invoke(other.attachedRigidbody);
                }

                if (UseOnce)
                {
                    Active = false;
                    OwnCollider.enabled = false;
                    if (DisableAfterUse) gameObject.SetActive(false);

                    if (m_Destroy)
                    {
                        Destroy(gameObject, DestroyAfter);
                    }

                }
            }
        }
    }
}
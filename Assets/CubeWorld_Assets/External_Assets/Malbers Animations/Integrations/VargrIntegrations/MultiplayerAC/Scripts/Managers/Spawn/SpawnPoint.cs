using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
	public class SpawnPoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the spawn point be registered immediately on awake? It can be hard to enforce an order of registration for multiple spawn points with this.")]
        private bool m_RegisterOnAwake = true;

        [SerializeField, Tooltip("How long before the spawn point can be used again.")]
        protected float m_ReuseDelay = 0f;

        [SerializeField, Tooltip("The collider volume type for checking if the spawn point is clear or overlapped by another object.")]
		private OverlapTest m_OverlapTest = OverlapTest.Box;

		[SerializeField, Tooltip("The vertical height of the bounding volume for overlap checks.")]
		private float m_BoundsHeight = 2.5f;

		[SerializeField, Tooltip("The horizontal dimension of the bounding volume for overlap checks.")]
		private float m_BoundsHorizontal = 1.2f;

        [SerializeField, Tooltip("Should the character's gravity be reoriented to match the spawn point. If the spawn is tilted on one side, this will make the character's down direction equal to the spawn point's")]
        protected bool m_ReorientGravity = false;

        [SerializeField, Tooltip("A UnityEvent fired when a character is spawned at this point. Allows for simple triggering of spawn audio and visual effects.")]
		protected UnityEvent m_OnSpawn = new UnityEvent();
        [SerializeField]
        private bool m_backStage = false;

#if UNITY_EDITOR
        public SpawnPointGroup group = null;
#endif

        private const float k_Tolerance = 0.005f;

        protected Coroutine m_CooldownCoroutine = null;
		private WaitForSeconds m_ReuseYield = null;

		public enum OverlapTest
		{
			Box,
			Capsule,
			None
		}

		private Transform m_SpawnTransform = null;
        public bool backStage 
        {
            get { return m_backStage; }
        }
		public Transform spawnTransform
		{
			get
			{ 
				if (m_SpawnTransform == null)
					m_SpawnTransform = transform;
				return m_SpawnTransform;
			}
		}
        
		public event UnityAction onSpawn
		{
			add { m_OnSpawn.AddListener (value); }
			remove { m_OnSpawn.RemoveListener (value); }
		}

		public bool cooldownActive
		{
			get { return m_CooldownCoroutine != null; }
		}

		private bool m_Registered = false;
		public bool registered
		{
			get { return m_Registered; }
        }

        public Vector3 up
        {
            get
            {
                if (m_ReorientGravity)
                    return spawnTransform.up;
                else
                    return Vector3.up;
            }
        }

        public Quaternion rotation
        {
            get
            {
                if (m_ReorientGravity)
                    return spawnTransform.rotation;
                else
                {
                    var spawnForward = Vector3.ProjectOnPlane(spawnTransform.forward, Vector3.up);
                    if (spawnForward.sqrMagnitude < 0.0001f)
                        spawnForward = Vector3.forward;
                    else
                        spawnForward.Normalize();

                    return Quaternion.LookRotation(spawnForward);
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            m_BoundsHeight = Mathf.Clamp(m_BoundsHeight, 1f, 5f);
            m_BoundsHorizontal = Mathf.Clamp(m_BoundsHorizontal, 0.5f, 5f);
            if (m_ReuseDelay < 0f)
                m_ReuseDelay = 0f;

            // Check group settings
            if (group != null)
            {

                bool found = false;
                for (int i = 0; i < group.spawnPoints.Length; ++i)
                {
                    if (group.spawnPoints[i] == this)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    m_RegisterOnAwake = false;
                else
                    group = null;
            }
        }
#endif

        protected virtual void Awake ()
		{
            if (m_RegisterOnAwake)
                Register();
			m_ReuseYield = new WaitForSeconds (m_ReuseDelay);
		}

        protected virtual void OnEnable ()
		{
            if (m_RegisterOnAwake)
                Register();
		}

        protected virtual void OnDisable ()
		{
            Unregister();
			m_CooldownCoroutine = null;
		}

        public virtual void Register()
        {
            if (!m_Registered)
            {
                SpawnManager.AddSpawnPoint(this);
                m_Registered = true;
            }
        }

        public virtual void Unregister()
        {
            if (m_Registered)
            {
                SpawnManager.RemoveSpawnPoint(this);
                m_Registered = false;
            }
        }

		protected IEnumerator Cooldown ()
		{
			yield return m_ReuseYield;
			m_CooldownCoroutine = null;
        }

        public virtual bool CanSpawnCharacter()
        {
            // Check timeout
            if (m_CooldownCoroutine != null)
                return false;

            // Check overlap
            switch (m_OverlapTest)
            {
                case OverlapTest.Box:
                    {
                        float halfX = m_BoundsHorizontal * 0.5f;
                        float halfY = m_BoundsHeight * 0.5f;
                        if (Physics.CheckBox(
                            spawnTransform.position + (up * halfY),
                            new Vector3(halfX - k_Tolerance, halfY - k_Tolerance, halfX - k_Tolerance),
                            rotation,
                            SpawnManager.spawnBlockers,
                            QueryTriggerInteraction.Ignore
                        ))
                            return false;
                    }
                    break;
                case OverlapTest.Capsule:
                    {
                        float radius = m_BoundsHorizontal * 0.5f;
                        float top = m_BoundsHeight - radius;
                        Vector3 position = spawnTransform.position;
                        if (Physics.CheckCapsule(
                            position + (up * radius),
                            position + (up * top),
                            radius - k_Tolerance,
                            SpawnManager.spawnBlockers,
                            QueryTriggerInteraction.Ignore
                        ))
                            return false;
                    }
                    break;
            }
            return true;
        }

        public virtual void useSpawn ()
        {
            // Fire event
			m_OnSpawn.Invoke ();

			// Start cooldown
			if (m_ReuseDelay > 0f)
				m_CooldownCoroutine = StartCoroutine (Cooldown ());
        }

        public virtual void OrientCharactor (Transform ct)
        {
            ct.position = spawnTransform.position;
            if (m_ReorientGravity)
            {
                ct.rotation = spawnTransform.rotation;
            }
            else
            {
                var spawnForward = Vector3.ProjectOnPlane(spawnTransform.forward, Vector3.up);
                if (spawnForward.sqrMagnitude < 0.0001f)
                    spawnForward = Vector3.forward;
                else
                    spawnForward.Normalize();

                ct.rotation = Quaternion.LookRotation(spawnForward);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            Color c = CanSpawnCharacter() ? Color.cyan : Color.red;

            // Draw spawn forwards direction
            ExtendedGizmos.DrawArrowMarkerFlat(spawnTransform.position, rotation, 0f, 1f, c);

            switch (m_OverlapTest)
            {
                case OverlapTest.Box:
                    ExtendedGizmos.DrawCuboidMarker(spawnTransform.position, m_BoundsHorizontal, m_BoundsHeight, rotation, c);
                    break;
                case OverlapTest.Capsule:
                    float radius = m_BoundsHorizontal * 0.5f;
                    float top = m_BoundsHeight - radius;
                    Vector3 position = spawnTransform.position;
                    ExtendedGizmos.DrawCapsuleMarker(position + (up * radius), position + (up * top), m_BoundsHorizontal * 0.5f, c);
                    break;
            }
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var group = serializedObject.FindProperty("group");
            EditorGUILayout.PropertyField(group);
            if (group.objectReferenceValue == null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RegisterOnAwake"));
            else
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RegisterOnAwake"));
                GUI.enabled = true;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReuseDelay"));

            var overlapTest = serializedObject.FindProperty("m_OverlapTest");
            EditorGUILayout.PropertyField(overlapTest);
            if (overlapTest.enumValueIndex != 2)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BoundsHeight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BoundsHorizontal"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_backStage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ReorientGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSpawn"));

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
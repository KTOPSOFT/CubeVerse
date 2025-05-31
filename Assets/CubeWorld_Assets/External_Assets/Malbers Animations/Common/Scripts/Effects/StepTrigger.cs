using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Works with the Step manager ... get the terrain below the animal </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Step Trigger")]
    public class StepTrigger : MonoBehaviour
    {
        [RequiredField]
        public StepsManager m_StepsManager;

        [Tooltip("Re Parent this GameObject to a new Bone on Awake")]
        public Transform parent;

        public AudioSource StepAudio;

        public SphereCollider m_Trigger;

        public Color DebugColor = Color.cyan;
        private LayerMask GroundLayer => m_StepsManager.GroundLayer.Value;

        private float waitTime;
        private float stepTime;

        void Awake()
        {
            if (m_StepsManager == null) m_StepsManager = transform.FindObjectCore().FindComponent<StepsManager>();

            if (m_Trigger == null) m_Trigger = GetComponent<SphereCollider>();

            if (m_StepsManager == null) //If there's no  StepManager Remove the Stepss
            {
                Destroy(gameObject);
                return;
            }

            //Reparent
            if (parent != null)
                transform.SetParent(parent, true);

            m_Trigger.isTrigger = true;

            if (m_StepsManager.Active == false) //If there's no  StepManager Remove the Stepss
            {
                gameObject.SetActive(false);
                return;
            }

            m_StepsManager.Feet ??= new();

            m_StepsManager.Feet.Add(this); //Add the reference to the step manager

            SetAudio();

            //wait = new WaitForSeconds(m_StepsManager.WaitNextStep);
            waitTime = m_StepsManager.WaitNextStep;

            stepTime = -waitTime * 2; //Set a negative value to make the first step to be played
        }

        private void SetAudio()
        {
            if (StepAudio == null && !TryGetComponent(out StepAudio))
                StepAudio = gameObject.AddComponent<AudioSource>();

            StepAudio.spatialBlend = 1;  //Make the Sound 3D
            if (m_StepsManager) StepAudio.volume = m_StepsManager.StepsVolume;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            if (!m_StepsManager.InTrackState) return; //Do not create tracks when is not on the correct state
            if (!MTools.CollidersLayer(other, GroundLayer)) return; //Ignore layers that are not Ground
            if (!MTools.ElapsedTime(stepTime, waitTime)) return;

            stepTime = Time.time;
            m_StepsManager.EnterStep(this, other);
        }


        [ContextMenu("Find Sphere Trigger")]
        void GetTrigger()
        {
            m_Trigger = GetComponent<SphereCollider>();
            MTools.SetDirty(this);
        }


        [ContextMenu("Set Current Parent")]
        void SetCurrentParent()
        {
            parent = transform.parent;
            MTools.SetDirty(this);
        }
        private void OnValidate()
        {
            if (m_Trigger == null) m_Trigger = GetComponent<SphereCollider>();
        }

        [ContextMenu("Find Audio Source")]
        private void FindAudioSource()
        {
            StepAudio = GetComponent<AudioSource>();
            if (StepAudio)
            {
                StepAudio.spatialBlend = 1;  //Make the Sound 3D
                if (m_StepsManager) StepAudio.volume = m_StepsManager.StepsVolume;
                StepAudio.maxDistance = 5;
                StepAudio.minDistance = 1;
                StepAudio.playOnAwake = false;
            }
            MTools.SetDirty(StepAudio);
        }

#if UNITY_EDITOR && MALBERS_DEBUG

        private void OnDrawGizmos() => GizmoSelected(false);

        void OnDrawGizmosSelected() => GizmoSelected(true);

        void GizmoSelected(bool sel)
        {
            if (m_Trigger && m_Trigger.enabled
#if UNITY_EDITOR
                &&
             UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this) //Show Gizmos only when the Inspector is Open
             )
#endif
            {
                var DebugColorWire = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.color = DebugColor;
                Gizmos.DrawSphere(Vector3.zero + m_Trigger.center, m_Trigger.radius);
                Gizmos.color = sel ? Color.yellow : DebugColorWire;
                Gizmos.DrawWireSphere(Vector3.zero + m_Trigger.center, m_Trigger.radius);
            }
        }
#endif
    }
}
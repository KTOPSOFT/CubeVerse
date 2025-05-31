using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/AI/Point Click")]
    public class PointClick : MonoBehaviour
    {
        public PointClickData pointClickData;
        [Tooltip("UI to intantiate on the Hit Point ")]
        public GameObject PointUI;

        [Tooltip("What mouse button to use for the joystick ")]
        public PointerEventData.InputButton Button = PointerEventData.InputButton.Left;

        [Tooltip("Radius to find <AI Targets> on the Hit Point")]
        public float radius = 0.2f;
        private const float navMeshSampleDistance = 4f;

        [Tooltip("If its hit a point on an empty space, it will clear the Current Target")]
        public bool ClearTarget = true;

        [Tooltip("How many AI Targets can be found on the SphereCast ")]
        [Min(2)] public int AITargetsSize = 10;
        public LayerReference FindTargets = new(-1);

        [Header("Events")]
        public Vector3Event OnPointClick = new();
        [FormerlySerializedAs("OnInteractableClick")]
        public TransformEvent OnAITargetClick = new();

        protected Collider[] AITargets;



        public IAIControl AIControl;

        void OnEnable()
        {
            if (pointClickData) pointClickData.baseDataPointerClick += OnGroundClick;

            var ObjectCore = this.FindInterface<IObjectCore>();

            if (ObjectCore != null)
                AIControl = ObjectCore.transform.FindInterface<IAIControl>();

            AITargets = new Collider[AITargetsSize];
        }


        void OnDisable()
        {
            if (pointClickData) pointClickData.baseDataPointerClick -= OnGroundClick;
        }

        Vector3 destinationPosition;

        public virtual void OnGroundClick(BaseEventData data)
        {
            PointerEventData pData = (PointerEventData)data;

            if (ClearTarget) AIControl?.SetTarget(null, true);

            if (pData == null) return;

            if (pData.button == Button)
            {
                if (NavMesh.SamplePosition(pData.pointerCurrentRaycast.worldPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    destinationPosition = hit.position;
                }
                else
                    destinationPosition = pData.pointerCurrentRaycast.worldPosition;

                MDebug.DrawWireSphere(destinationPosition, Color.red, radius, 1);

                // AITargets = new Collider[AITargetsSize]; //Clear the colliders!!!

                var found = Physics.OverlapSphereNonAlloc(destinationPosition, radius, AITargets, FindTargets.Value); //Find all the AI TARGETS on a Radius

                if (found > 0)
                {
                    for (int i = 0; i < found; i++)
                    {
                        var col = AITargets[i];

                        if (col == null) break; //If there's no more colliders break the loop

                        if (col.transform.SameHierarchy(transform)) continue; //Don't click on yourself

                        if (col.transform.FindInterface<IAITarget>() != null)
                        {
                            OnAITargetClick.Invoke(col.transform); //Invoke only the first interactable found

                            if (PointUI)
                                Instantiate(PointUI, col.transform.position, Quaternion.FromToRotation(PointUI.transform.up, pData.pointerCurrentRaycast.worldNormal));

                            return;
                        }
                    }
                }

                if (PointUI)
                    Instantiate(PointUI, destinationPosition, Quaternion.FromToRotation(PointUI.transform.up, pData.pointerCurrentRaycast.worldNormal));



                OnPointClick.Invoke(destinationPosition);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(destinationPosition, 0.1f);
                Gizmos.DrawSphere(destinationPosition, 0.1f);
            }
        }

        private void Reset()
        {
            pointClickData = MTools.GetInstance<PointClickData>("PointClickData");
            PointUI = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Malbers Animations/Common/Prefabs/Interactables/ClickPoint.prefab");
            MTools.SetDirty(this);

            var SetDestination = this.GetUnityAction<Vector3>("MAnimalAIControl", "SetDestination");
            if (SetDestination != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnPointClick, SetDestination);

            var SetTarget = this.GetUnityAction<Transform>("MAnimalAIControl", "SetTarget");
            if (SetTarget != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnAITargetClick, SetTarget);
        }

#endif
    }
}
using UnityEngine;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// This is used when the collider is in a different gameObject and you need to check the Collider Events
    /// Create this component at runtime and subscribe to the UnityEvents
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Colliders/Closest Trigger Proxy")]
    public class ClosestTriggerProxy : TriggerProxy
    {

        public ColliderEvent OnTrigger_Closest = new();
        public GameObjectEvent OnGameObjectClosest = new();

        protected Collider closestCollider;
        protected GameObject closestGameObject;

        public override void RemoveTrigger(Collider other, bool remove)
        {
            GameObject realRoot = MTools.FindRealRoot(other);
            if(other == closestCollider) closestCollider = null;
            if(realRoot == closestGameObject) closestGameObject = null;
            base.RemoveTrigger(other, remove);
        }

        public override void ResetTrigger()
        {
            closestCollider = null;
            closestGameObject = null;
            base.ResetTrigger();
        }

        protected override void Update()
        {
            base.Update();
            CheckClosest();
        }

        protected virtual void CheckClosest()
        {
            float dist = 100f;
            float newDist;
            bool change = false;

            if(EnteringGameObjects.Count > 0){
                if(closestGameObject != null) dist = Vector3.Distance(closestGameObject.transform.position, transform.position);

                foreach (var gos in EnteringGameObjects)
                {
                    if(gos == closestGameObject) continue;
                    newDist = Vector3.Distance(gos.transform.position, transform.position);
                    if(dist < newDist) continue;
                    dist = newDist;
                    closestGameObject = gos;
                    change = true;
                }

                if(change) OnGameObjectClosest.Invoke(closestGameObject);

                change = false;
                dist = 100f;
            }

            if(m_colliders.Count > 0){
                if(closestCollider != null) dist = Vector3.Distance(closestCollider.transform.position, transform.position);

                foreach (var col in m_colliders)
                {
                    if(col == closestCollider) continue;
                    newDist = Vector3.Distance(col.transform.position, transform.position);
                    if(dist < newDist) continue;
                    dist = newDist;
                    closestCollider = col;
                    change = true;
                }

                if(change) OnTrigger_Closest.Invoke(closestCollider);
            }
        }
    }


    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(ClosestTriggerProxy))]
    public class ClosestTriggerProxyEditor : TriggerProxyEditor
    {
        
        protected SerializedProperty OnTrigger_Closest, OnTrigger_LastOut, OnGameObjectClosest, OnGameObjectLastOut, Editor_Tabs1;

        protected void FindProperties()
        {
            OnTrigger_Closest = serializedObject.FindProperty("OnTrigger_Closest");
            OnGameObjectClosest = serializedObject.FindProperty("OnGameObjectClosest");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            FindProperties();

            int Selection = Editor_Tabs1.intValue;

            if (Selection != 0) DrawEvents();
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnTrigger_Closest, new GUIContent("On Trigger Closest"));
            EditorGUILayout.PropertyField(OnGameObjectClosest, new GUIContent("On GameObject Closest"));
        }
        
    }
#endif
    #endregion
}
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VargrIntegrations
{
    [CreateAssetMenu(fileName = "GO_Link", menuName = "NetSyncedObjects/GameObject Link", order = 1)]
    public class RSOGameObject : RSORoot
    {
        [SerializeField]
        private GameObject m_value;
        public GameObject Value { get{ return m_value; } }
        public RSOGameObject(int key, GameObject value)
        {
            m_key = key;
            m_value = value;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(RSOGameObject)), CanEditMultipleObjects]
    public class RSOGameObjectEditor : RSORootEditor
    {
        protected override void DBTool(int previousKey)
        {
            if((target as RSOGameObject).Key != 0){
                var keyValue = RSOManager.Get((target as RSOGameObject).Key);
                if(keyValue != null && keyValue == (target as RSOGameObject).Value)
                    return;
            }
            if(GUILayout.Button("Add to Libary", GUILayout.Height(40)))
            {
                RSOManager.Database.Add(target as RSOGameObject);
            }

            serializedObject.Update();

            if (previousKey != value.intValue && previousKey != 0 && 
            RSOManager.Get(previousKey) == (target as RSOGameObject).Value) {
                RSOManager.Database.Remove(previousKey);
                serializedObject.ApplyModifiedProperties();
                RSOManager.Database.Add(target as RSOGameObject, false);
            }else{
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
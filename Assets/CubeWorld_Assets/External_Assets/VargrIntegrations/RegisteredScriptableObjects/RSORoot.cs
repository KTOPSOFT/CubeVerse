using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VargrIntegrations
{
    public abstract class RSORoot : ScriptableObject
    {
        [SerializeField]
        protected int m_key;
        public int Key { get{ return m_key; } }
        public int GetKey() => m_key;    
        public void SetKey(int key) { m_key = key; }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(RSORoot)), CanEditMultipleObjects]
    public class RSORootEditor : Editor
    {
        protected SerializedProperty value;

        void OnEnable() {
            value = serializedObject.FindProperty("m_key");
        }
        
        public override void OnInspectorGUI()
        {
            int previousKey = value.intValue;
            base.OnInspectorGUI();
            DBTool(previousKey);
        }

        protected virtual void DBTool(int previousKey)
        {
            if((target as RSORoot).Key != 0){
                var keyValue = RSOManager.Get((target as RSORoot).Key);
                if(keyValue != null && keyValue == (ScriptableObject)target)
                    return;
            }
            if(GUILayout.Button("Add to Libary", GUILayout.Height(40)))
            {
                RSOManager.Database.Add(target as RSORoot);
            }

            serializedObject.Update();

            if (previousKey != value.intValue && previousKey != 0 && 
            RSOManager.Get(previousKey) == (ScriptableObject)target) {
                RSOManager.Database.Remove(previousKey);
                serializedObject.ApplyModifiedProperties();
                RSOManager.Database.Add(target as RSORoot, false);
            }else{
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
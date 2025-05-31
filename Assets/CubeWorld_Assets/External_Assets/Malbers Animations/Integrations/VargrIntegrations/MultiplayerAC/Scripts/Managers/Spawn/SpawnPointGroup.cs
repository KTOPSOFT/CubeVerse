using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class SpawnPointGroup : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the spawn points be registered immediately on awake?")]
        private bool m_RegisterOnAwake = true;

        [SerializeField, Tooltip("The individual spawn points (the first spawn is at the top of the list)")]
        private SpawnPoint[] m_SpawnPoints = { };

#if UNITY_EDITOR
        public SpawnPoint[] spawnPoints
        {
            get { return m_SpawnPoints; }
        }

        protected void OnValidate()
        {
            int nullSpawns = 0;
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] == null)
                    ++nullSpawns;
            }

            if (nullSpawns > 0)
            {
                var temp = new List<SpawnPoint>(m_SpawnPoints.Length - nullSpawns);
                for (int i = 0; i < m_SpawnPoints.Length; ++i)
                {
                    if (m_SpawnPoints[i] != null)
                        temp.Add(m_SpawnPoints[i]);
                }
                m_SpawnPoints = temp.ToArray();
            }
        }
#endif

        protected void Awake()
        {
            if (m_RegisterOnAwake)
                Register();
        }

        protected void OnEnable()
        {
            if (m_RegisterOnAwake)
                Register();
        }

        protected void OnDisable()
        {
            Unregister();
        }

        public void Register()
        {
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] != null)
                    m_SpawnPoints[i].Register();
            }
        }

        public void Unregister()
        {
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] != null)
                    m_SpawnPoints[i].Unregister();
            }
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(SpawnPointGroup), true)]
    public class SpawnPointGroupEditor : Editor
    {
        private ReorderableList m_SpawnList = null;

        void CheckSpawnList()
        {
            if (m_SpawnList == null)
            {
                m_SpawnList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_SpawnPoints"), true, false, false, true);
                m_SpawnList.onRemoveCallback += OnSpawnPointRemoved;
                m_SpawnList.drawElementCallback += DrawSpawnPointElement;
            }
        }

        private void DrawSpawnPointElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4;
            rect.y += 1;
            var prop = m_SpawnList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.ObjectField(rect, prop.objectReferenceValue, typeof(SpawnPoint), true);
        }

        private void OnSpawnPointRemoved(ReorderableList list)
        {
            // Reset group property on spawn point
            var prop = list.serializedProperty.GetArrayElementAtIndex(list.index);
            if (prop.objectReferenceValue != null)
            {
                var spawnSO = new SerializedObject(prop.objectReferenceValue);
                var group = spawnSO.FindProperty("group");
                if (group.objectReferenceValue != null && group.objectReferenceValue == target)
                {
                    group.objectReferenceValue = null;
                    spawnSO.ApplyModifiedProperties();
                }
            }
        }
    }
    #endif
}
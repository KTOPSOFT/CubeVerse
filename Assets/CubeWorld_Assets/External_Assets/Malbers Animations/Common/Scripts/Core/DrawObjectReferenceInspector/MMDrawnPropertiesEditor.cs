#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//Dev Samuel Sátiro - BaltaRed

namespace MalbersAnimations
{
    public abstract class MMDrawnPropertiesEditor : Editor
    {
        private static readonly Dictionary<Object, List<SerializedProperty>> _serializedProperties = new Dictionary<Object, List<SerializedProperty>>();

        public static void GetProperties(Object target)
        {
            if (target != null && _serializedProperties.ContainsKey(target) == false)
            {
                _serializedProperties.Add(target, GetSerializedProperties(target));
            }
        }

        public static void MMDrawnProperties(Object target, params string[] ignoreProperty)
        {
            GetProperties(target);

            SerializedObject serializedObject = new(target);
            serializedObject.Update();

            foreach (var property in _serializedProperties[target])
            {
                if (property.name.Equals("m_Script", System.StringComparison.Ordinal))
                {
                    //GUI.enabled = false;
                    //GUILayout.BeginHorizontal();
                    //{
                    //    EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
                    //}
                    //GUILayout.EndHorizontal();
                    //GUI.enabled = true;
                }
                else
                {
                    if (IgnoreProperty(property.name, ignoreProperty) == false)
                    {
                        property.serializedObject.Update();

                        if (property.isArray && property.propertyType != SerializedPropertyType.String)
                        {
                            MMReorderableListPropertyDrawer.Draw(property);
                        }
                        else
                        {
                            if (property.propertyType == SerializedPropertyType.Generic)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(-1);
                                    GUILayout.BeginVertical();
                                    {
                                        EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
                                    }
                                    GUILayout.EndVertical();
                                }

                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
                                }
                                GUILayout.EndHorizontal();
                            }
                        }

                        property.serializedObject.ApplyModifiedProperties();
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            Undo.RecordObject(target, target.name);
        }

        protected static List<SerializedProperty> GetSerializedProperties(Object target)
        {
            var serializedProperties = new List<SerializedProperty>();

            SerializedObject serializedObject = new SerializedObject(target);
            using (var iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        serializedProperties.Add(serializedObject.FindProperty(iterator.name));
                    } while (iterator.NextVisible(false));
                }
            }

            return serializedProperties;
        }

        protected static List<SerializedProperty> GetSerializedProperties(SerializedObject serializedObject)
        {
            var serializedProperties = new List<SerializedProperty>();

            using (var iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        serializedProperties.Add(serializedObject.FindProperty(iterator.name));
                    } while (iterator.NextVisible(false));
                }
            }

            return serializedProperties;
        }

        protected static bool IgnoreProperty(string fieldName, string[] ignoreFields)
        {
            bool ignore = false;

            foreach (string field in ignoreFields)
            {
                if (fieldName == field)
                {
                    ignore = true;
                    break;
                }
            }

            return ignore;
        }
    }
}
#endif
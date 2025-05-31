#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MalbersAnimations
{
#if UNITY_EDITOR
    public static class Vector3ContextMenu
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.contextualPropertyMenu += ContextualPropertyMenu;
        }

        private static void ContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Vector3)
            {
                menu.AddItem(new GUIContent("Zero"), false, () =>
                {
                    property.vector3Value = Vector3.zero;
                    property.serializedObject.ApplyModifiedProperties();
                }
                );

                menu.AddItem(new GUIContent("One"), false, () =>
                {
                    property.vector3Value = Vector3.one;
                    property.serializedObject.ApplyModifiedProperties();
                }
               );

            }
        }
    }
#endif
}
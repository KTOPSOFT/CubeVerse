using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
#if UNITY_EDITOR

    [InitializeOnLoad]
    public class CopyPasteSerializedReference
    {
        private static (string json, Type type) lastObject;

        private static SerializedProperty copyArray;

        static CopyPasteSerializedReference()
        {
            EditorApplication.contextualPropertyMenu += ShowSerializeReferenceCopyPasteContextMenu;
        }

        public static Action UpdateDropdownCallback;

        private static void ShowSerializeReferenceCopyPasteContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var copyProperty = property.Copy();
                menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                    (_) => { CopyReferenceValue(copyProperty); }, null);
                var pasteContent = new GUIContent("Paste Serialize Reference");
                menu.AddItem(pasteContent, false, (_) => PasteReferenceValue(copyProperty),
                    null);

                if (property.IsArray())
                {
                    var duplicateContent = new GUIContent("Duplicate Serialize Reference Array Element");
                    menu.AddItem(duplicateContent, false, (_) => DuplicateSerializeReferenceArrayElement(copyProperty),
                        null);
                }
            }
            //else if (property.isArray)
            //{
            //    menu.AddItem(new GUIContent("Copy Array"), false,
            //        (_) =>
            //        {
            //            copyArray = property.Copy();

            //            //COPY ARRAYs; 
            //        },
            //        null);

            //    menu.AddItem(new GUIContent("Paste Array"), false,
            //       (_) =>
            //       {
            //           if (copyArray == null) return;
            //           property = copyArray;
            //           property.serializedObject.Update();
            //           property.serializedObject.ApplyModifiedProperties();

            //       },
            //       null);

            //}
        }

        private static void CopyReferenceValue(SerializedProperty property)
        {
            var refValue = property.managedReferenceValue;
            lastObject.json = JsonUtility.ToJson(refValue);
            lastObject.type = refValue?.GetType();
        }

        private static void PasteReferenceValue(SerializedProperty property)
        {
            try
            {
                if (lastObject.type != null)
                {
                    var pasteObj = JsonUtility.FromJson(lastObject.json, lastObject.type);
                    property.managedReferenceValue = pasteObj;
                }
                else
                {
                    property.managedReferenceValue = null;
                }

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                // PropertyDrawer.UpdateDropdownCallback?.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void DuplicateSerializeReferenceArrayElement(SerializedProperty property)
        {
            var sourceElement = property.managedReferenceValue;
            var arrayProperty = MSerializedTools.GetArrayPropertyFromArrayElement(property);
            var newElementIndex = arrayProperty.arraySize;
            arrayProperty.arraySize = newElementIndex + 1;

            if (sourceElement != null)
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

                var json = JsonUtility.ToJson(sourceElement);
                var newObj = JsonUtility.FromJson(json, sourceElement.GetType());
                var newElementProperty = arrayProperty.GetArrayElementAtIndex(newElementIndex);
                newElementProperty.managedReferenceValue = newObj;
            }

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
    }
#endif
}
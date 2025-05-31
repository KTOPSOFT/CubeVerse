using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif


namespace MalbersAnimations.IK
{
    [CreateAssetMenu(menuName = "Malbers Animations/IK/IK Proccessor Profile")]


    public class IKProccesorProfile : ScriptableObject
    {
        [SerializeReference, NonReorderable]
        //[SubclassSelector]
        public List<IKProcessor> IKProcesors;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(IKProccesorProfile))]
    public class IKProccesorProfileEditor : Editor
    {
        ReorderableList Reo_Links;
        SerializedProperty IKProcesors;
        private readonly List<Type> derivedTypes;
        int SelectedAbility;


        private void OnEnable()
        {
            IKProcesors = serializedObject.FindProperty("IKProcesors");

            Reo_Links = new ReorderableList(serializedObject, IKProcesors, true, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;

                    var element = IKProcesors.GetArrayElementAtIndex(index);
                    if (element.managedReferenceValue == null) return;

                    var active = element.FindPropertyRelative("Active");
                    //var weight = element.FindPropertyRelative("weight");

                    //var IndexValue = element.FindPropertyRelative("Index");
                    var name = element.FindPropertyRelative("name");
                    var Weight = element.FindPropertyRelative("Weight");
                    var TargetIndex = element.FindPropertyRelative("TargetIndex");

                    var IDRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };



                    var height = EditorGUIUtility.singleLineHeight;

                    var activeRect = new Rect(rect.x, rect.y, 20, height);
                    var IndexRect = new Rect(rect.x + 20, rect.y, 35, height);
                    var NameRect = new Rect(rect.x + 60, rect.y, rect.width * 0.7f - 60, height);
                    var weightRect = new Rect(rect.width - rect.width * 0.3f + 25, rect.y, rect.width * 0.3f + 12f, height);


                    var dC = GUI.contentColor;

                    if (isFocused) GUI.contentColor = Color.green;

                    EditorGUIUtility.labelWidth = 30;
                    active.boolValue = EditorGUI.Toggle(activeRect, GUIContent.none, active.boolValue);
                    EditorGUI.PropertyField(NameRect, name, GUIContent.none);
                    EditorGUI.PropertyField(IndexRect, TargetIndex, GUIContent.none);
                    EditorGUI.PropertyField(weightRect, Weight, new GUIContent(" "));

                    GUI.contentColor = dC;

                    EditorGUIUtility.labelWidth = 0;
                },

                drawHeaderCallback = rect =>
                {
                    var IDRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight, width = 60 };

                    EditorGUI.LabelField(IDRect, new GUIContent("  Target [I]", "Target Index from the <Targets> array"));

                    var height = EditorGUIUtility.singleLineHeight;


                    var nameRect = new Rect(IDRect.x + 75, rect.y, 80, height);
                    var WeightRect = new Rect(rect) { x = rect.width - 30, width = 65 };



                    EditorGUI.LabelField(nameRect, "IK Processor");
                    EditorGUI.LabelField(WeightRect, "Weight");
                },

                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    var menu = new GenericMenu();

                    foreach (var type in derivedTypes)
                    {
                        var att = type.GetCustomAttribute<AddTypeMenuAttribute>(false); //Find the correct name
                        string LabelName = att != null ? att.MenuName : type.Name;

                        menu.AddItem(new GUIContent(LabelName), false, (x) => AddNewItem(x, list, IKProcesors), Activator.CreateInstance(type));
                    }
                    menu.ShowAsContext();
                },

                onSelectCallback = (list) => { SelectedAbility = list.index; }
            };
        }

        void AddNewItem(object target, ReorderableList list, SerializedProperty property)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            IKProcessor link = (IKProcessor)target;
            link.name = target.GetType().Name;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = target;

            property.serializedObject.ApplyModifiedProperties();
        }



        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            EditorGUI.indentLevel++;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                EditorGUILayout.PropertyField(IKProcesors);
            EditorGUI.indentLevel--;

            Reo_Links.DoLayoutList();


            Reo_Links.index = SelectedAbility;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                if (SelectedAbility != -1 && SelectedAbility < IKProcesors.arraySize)
                {
                    // Debug.Log("SelectedAbility = " + SelectedAbility);
                    SerializedProperty ikprocessor = IKProcesors.GetArrayElementAtIndex(SelectedAbility);

                    if (ikprocessor != null)
                    {
                        // EditorGUILayout.Space(-16);
                        EditorGUILayout.PropertyField(ikprocessor, true);
                        EditorGUILayout.PropertyField(ikprocessor.FindPropertyRelative("AnimParameter"), true);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(ActiveMeshes))]
    public class ActiveMeshesEditor : Editor
    {
        private ReorderableList list;
        SerializedProperty activeMeshesList, showMeshesList, random, RootBone, Owner, debug, OnMeshChanged,
            //  MeshItemUpdated,

            selectedMeshIndex
            ;

        private readonly Dictionary<string, ReorderableList> Reo_Abilities = new();

        private ActiveMeshes m;
        private readonly string[] tabs = new string[] { "General", "Reactions" };

        private void OnEnable()
        {
            m = (ActiveMeshes)target;

            activeMeshesList = serializedObject.FindProperty("Meshes");
            OnMeshChanged = serializedObject.FindProperty("OnMeshChanged");
            showMeshesList = serializedObject.FindProperty("showMeshesList");
            random = serializedObject.FindProperty("random");
            RootBone = serializedObject.FindProperty("RootBone");
            Owner = serializedObject.FindProperty("Owner");
            debug = serializedObject.FindProperty("debug");
            selectedMeshIndex = serializedObject.FindProperty("selectedMeshIndex");
            //MeshItemUpdated = serializedObject.FindProperty("MeshItemUpdated");

            list = new ReorderableList(serializedObject, activeMeshesList, true, true, true, true);
            {
                list.drawElementCallback = DrawElementCallback;
                list.drawHeaderCallback = HeaderCallbackDelegate;
                list.onAddCallback = OnAddCallBack;
                list.onSelectCallback = OnSelectCallBack;
            }

            //Populate the Owner with the current GameObject if is null
            if (Owner.objectReferenceValue == null)
            {
                Owner.objectReferenceValue = m.transform;
                serializedObject.ApplyModifiedProperties();
            }

            list.index = selectedMeshIndex.intValue;

            m.SyncMeshItem();

            if (m.ReviewNames())
                serializedObject.ApplyModifiedProperties();

            if (m.RootBone == null)
            {
                var animator = m.GetComponentInParent<Animator>();

                if (animator)
                {
                    m.RootBone = animator.avatarRoot;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }



        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Enable-Disable Meshes on the Character");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(Owner);
                    MalbersEditor.DrawDebugIcon(debug);

                }
                EditorGUILayout.PropertyField(RootBone);


                //using (new EditorGUI.DisabledGroupScope(true))
                //    EditorGUILayout.PropertyField(MeshItemUpdated);
            }


            EditorGUI.BeginChangeCheck();
            {
                //EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    list.DoLayoutList();


                    if (showMeshesList.boolValue)
                    {
                        if (list.index != -1 && activeMeshesList.arraySize > 0)
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                SerializedProperty SelectedMeshSet = activeMeshesList.GetArrayElementAtIndex(list.index);

                                if (SelectedMeshSet != null)
                                {
                                    SerializedProperty MeshItemList = SelectedMeshSet.FindPropertyRelative("MeshItems");
                                    SerializedProperty OnSetMeshChange = SelectedMeshSet.FindPropertyRelative("OnSetMeshChange");

                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(SelectedMeshSet, false);
                                    EditorGUI.indentLevel--;



                                    if (SelectedMeshSet.isExpanded)
                                    {
                                        DrawAbilities(SelectedMeshSet, MeshItemList);

                                        EditorGUILayout.PropertyField(OnSetMeshChange);
                                    }
                                }
                            }
                        }
                    }
                }
                //EditorGUILayout.EndVertical();

            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Active Meshes Inspector");
            }


            EditorGUILayout.PropertyField(OnMeshChanged);
            serializedObject.ApplyModifiedProperties();
        }



        private void DrawAbilities(SerializedProperty SelectedSet, SerializedProperty Abilities)
        {
            ReorderableList Reo_AbilityList;
            string listKey = SelectedSet.propertyPath;

            var SelectedAbility = SelectedSet.FindPropertyRelative("CurrentMeshItemIndex");

            if (Reo_Abilities.ContainsKey(listKey))
            {
                // fetch the reorderable list in dict
                Reo_AbilityList = Reo_Abilities[listKey];
            }
            else
            {
                Reo_AbilityList = new ReorderableList(SelectedSet.serializedObject, Abilities, true, true, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;

                        var element = Abilities.GetArrayElementAtIndex(index);

                        var Name = element.FindPropertyRelative("ItemName");
                        var Mesh = element.FindPropertyRelative("Mesh");
                        var Current = SelectedSet.FindPropertyRelative("Current");

                        var meshRectWidth = rect.width / 2;



                        if (Current.intValue == index)
                        {
                            var labelwidth = 50;


                            meshRectWidth -= labelwidth;
                            Rect CurrentRect = new(rect.width - labelwidth + 50, rect.y, labelwidth, EditorGUIUtility.singleLineHeight);

                            var guiColor = GUI.color;
                            GUI.color = Color.white;

                            EditorGUI.LabelField(CurrentRect, "ACTIVE", EditorStyles.miniBoldLabel);
                            GUI.color = guiColor;
                        }

                        Rect NameRect = new(rect.x, rect.y, rect.width / 2 - 15, EditorGUIUtility.singleLineHeight);
                        Rect MeshRect = new(rect.x + rect.width / 2, rect.y, meshRectWidth, EditorGUIUtility.singleLineHeight);


                        EditorGUI.PropertyField(NameRect, Name, GUIContent.none);
                        EditorGUI.PropertyField(MeshRect, Mesh, GUIContent.none);
                    },

                    drawHeaderCallback = rect =>
                    {
                        var IDRect = new Rect(rect)
                        {
                            height = EditorGUIUtility.singleLineHeight
                        };

                        var NameRect = new Rect(rect);

                        IDRect.x = rect.width / 4 * 3 + 40;
                        IDRect.width = 80;

                        NameRect.x += 25;
                        NameRect.width = rect.width / 4 * 3 - 50;

                        var eleName = SelectedSet.FindPropertyRelative("Name");

                        EditorGUI.LabelField(NameRect, $"[{eleName.stringValue}]  Mesh Items");
                        EditorGUI.LabelField(IDRect, "Transform");
                    },

                    //onAddCallback = (list) =>
                    //{
                    //    var index = list.count == 0 ? 0 : list.count - 1;
                    //    Abilities.InsertArrayElementAtIndex(list.count == 0 ? 0 : list.count - 1);
                    //    list.index = -1;
                    //},
                    onSelectCallback = (list) =>
                    {
                        SelectedAbility.intValue = list.index;
                    }
                };

                Reo_Abilities.Add(listKey, Reo_AbilityList);  //Store it on the Editor
            }

            var index = SelectedAbility.intValue;

            Reo_AbilityList.DoLayoutList();

            Reo_AbilityList.index = index;


            if (index != -1 && index < Abilities.arraySize)
            {
                var element = Abilities.GetArrayElementAtIndex(index);

                var EditorTab = element.FindPropertyRelative("EditorTab");

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(element, false);
                EditorGUI.indentLevel--;

                if (element.isExpanded)
                {

                    EditorTab.intValue = GUILayout.Toolbar(EditorTab.intValue, tabs);

                    if (EditorTab.intValue == 0)
                    {
                        var Parent = element.FindPropertyRelative("Parent");
                        var HideSet = element.FindPropertyRelative("HideSet");

                        var materials = element.FindPropertyRelative("materials");
                        var Renderers = element.FindPropertyRelative("Renderers");

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.PropertyField(Parent);
                            EditorGUILayout.PropertyField(HideSet);
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(materials, true);
                            EditorGUILayout.PropertyField(Renderers, true);
                            EditorGUI.indentLevel--;
                        }

                    }
                    else
                    {
                        var MeshOn = element.FindPropertyRelative("MeshOn");
                        var MeshOff = element.FindPropertyRelative("MeshOff");

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.PropertyField(MeshOn);
                            EditorGUILayout.PropertyField(MeshOff);
                        }
                    }
                }
            }
        }


        /// <summary>Reordable List Header </summary>
        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_0 = new(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_01 = new(rect.x + 14, rect.y, 35, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new(rect.x + 15 + 55, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new(rect.x + 35 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 25, EditorGUIUtility.singleLineHeight);

            showMeshesList.boolValue = EditorGUI.ToggleLeft(R_0, "", showMeshesList.boolValue);
            EditorGUI.LabelField(R_01, new GUIContent(" Index", "Index"), EditorStyles.miniLabel);
            EditorGUI.LabelField(R_1, "Active Mesh Set", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "CURRENT", EditorStyles.centeredGreyMiniLabel);

            Rect R_3 = new(rect.width + 5, rect.y - 1, 20, EditorGUIUtility.singleLineHeight - 2);
            random.boolValue = GUI.Toggle(R_3, random.boolValue, new GUIContent("R", "On Start Assigns a Random Mesh"), EditorStyles.miniButton);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = activeMeshesList.GetArrayElementAtIndex(index);
            rect.y += 2;

            Rect R_Active = new(rect.x, rect.y, 10, EditorGUIUtility.singleLineHeight);
            Rect R_Index = new(rect.x + 18, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_Name = new(rect.x + 25 + 15, rect.y, (rect.width / 2) - 45, EditorGUIUtility.singleLineHeight);
            Rect R_Button = new(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 8, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_Index, "[" + index.ToString() + "]", EditorStyles.boldLabel);

            var eleName = element.FindPropertyRelative("Name");
            var active = element.FindPropertyRelative("Active");
            //   var meshes = element.FindPropertyRelative("meshes");

            var OldColor = GUI.backgroundColor;
            var mult = 2f;

            var selectedColor = new Color(0.2f * mult, 0.5f * mult, 1f * mult, 1f);

            GUI.backgroundColor = index == selectedMeshIndex.intValue ? selectedColor : OldColor;

            var MeshItems = element.FindPropertyRelative("MeshItems");

            var Current = element.FindPropertyRelative("Current").intValue;

            eleName.stringValue = EditorGUI.TextField(R_Name, eleName.stringValue, EditorStyles.textArea);

            EditorGUI.PropertyField(R_Active, active, GUIContent.none);

            string ButtonName = "Empty";

            //var e = m.Meshes[index];
            //if (e.meshes != null && e.meshes.Length >Current)
            //{
            //    ButtonName = (e.meshes[Current] == null ? "Empty" : e.meshes[Current].name) + " (" + Current + ")";
            //}




            if (MeshItems.arraySize > Current)
            {
                var CurrentMesh = MeshItems.GetArrayElementAtIndex(Current);
                var meshes = CurrentMesh.FindPropertyRelative("Mesh");
                var name = CurrentMesh.FindPropertyRelative("ItemName");

                if (!string.IsNullOrEmpty(name.stringValue))
                {
                    ButtonName = name.stringValue + $"   [{Current}]";
                }
                else
                {
                    ButtonName = meshes.objectReferenceValue == null ? "Empty" : meshes.objectReferenceValue.name + $"   [{Current}]";
                }
            }


            if (GUI.Button(R_Button, ButtonName, EditorStyles.miniButton))
            {
                Undo.RecordObject(target, "Changed Mesh ");
                m.ChangeMesh(index);
                EditorUtility.SetDirty(m);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.backgroundColor = OldColor;
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (m.Meshes == null) m.Meshes = new();
            m.Meshes.Add(new ActiveSMesh());
            EditorUtility.SetDirty(m);
        }

        private void OnSelectCallBack(ReorderableList list)
        {
            selectedMeshIndex.intValue = list.index;
        }
    }
}
#endif

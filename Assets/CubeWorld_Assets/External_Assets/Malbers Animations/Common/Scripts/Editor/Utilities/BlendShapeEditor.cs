
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(BlendShape))/*,CanEditMultipleObjects*/]
    public class BlendShapeEditor : Editor
    {
        BlendShape M;
        // private MonoScript script;
        protected int index = 0;
        SerializedProperty blendShapes, preset, LODs, mesh, random, PinnedShape, Min, Max;

        private void OnEnable()
        {
            M = (BlendShape)target;
            // script = MonoScript.FromMonoBehaviour(M);
            blendShapes = serializedObject.FindProperty("blendShapes");
            preset = serializedObject.FindProperty("preset");
            LODs = serializedObject.FindProperty("LODs");
            mesh = serializedObject.FindProperty("mesh");
            random = serializedObject.FindProperty("random");

            Min = serializedObject.FindProperty("Min");
            Max = serializedObject.FindProperty("Max");
            PinnedShape = serializedObject.FindProperty("PinnedShape");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Adjust the Blend Shapes on the Mesh");

            EditorGUI.BeginChangeCheck();
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {

                    int Length = 0;
                    if (mesh.objectReferenceValue != null)
                    {
                        Length = blendShapes.arraySize;
                    }
                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(mesh);

                            if (mesh.objectReferenceValue != null)
                            {
                                if (GUILayout.Button(new GUIContent("⁇", "Randomize Blend shapes"), GUILayout.Width(30)))
                                {
                                    M.Randomize();
                                    EditorUtility.SetDirty(target);
                                    if (M.mesh) EditorUtility.SetDirty(M.mesh);

                                }

                                if (GUILayout.Button(new GUIContent("↺", "Reset Blend Shapes to Zero"), GUILayout.Width(30)))
                                {
                                    M.ResetToZero();
                                    EditorUtility.SetDirty(target);

                                    if (M.mesh) EditorUtility.SetDirty(M.mesh);

                                    foreach (var item in M.LODs)
                                    {
                                        if (item) EditorUtility.SetDirty(item);
                                    };
                                }

                                using (new EditorGUI.DisabledGroupScope(preset.objectReferenceValue != null))
                                {
                                    random.boolValue = GUILayout.Toggle(random.boolValue,
                                        new GUIContent("R", "Make Randoms Blend Shapes at Start"), EditorStyles.miniButton, GUILayout.Width(30));
                                }
                            }

                            if (cc.changed)
                            {
                                serializedObject.ApplyModifiedProperties();
                                M.SetShapesCount();
                                EditorUtility.SetDirty(target);
                            }
                        }
                    }

                    if (mesh.objectReferenceValue != null)
                    {

                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(LODs, new GUIContent("LODs", "Other meshes with Blend Shapes to change"));
                        EditorGUI.indentLevel--;

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            PinnedShape.isExpanded = MalbersEditor.Foldout(PinnedShape.isExpanded, "Blend Shapes");

                            if (PinnedShape.isExpanded)
                            {

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUIUtility.labelWidth = 40;
                                    EditorGUILayout.PropertyField(Min);
                                    EditorGUILayout.PropertyField(Max);
                                    EditorGUIUtility.labelWidth = 00;
                                }

                                using (new GUILayout.HorizontalScope())
                                {

                                    if (Length > 0)
                                    {
                                        int pin = PinnedShape.intValue;
                                        EditorGUILayout.LabelField(new GUIContent("Pin Shape:              (" + pin + ") |" + M.mesh.sharedMesh.GetBlendShapeName(pin) + "|", "Current Shape Store to modigy When accesing public methods from other scripts"));
                                    }


                                }

                                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    if (Length > 0)
                                    {
                                        if (M.blendShapes == null)
                                        {
                                            M.blendShapes = M.GetBlendShapeValues();
                                            serializedObject.ApplyModifiedProperties();
                                        }

                                        for (int i = 0; i < Length; i++)
                                        {
                                            if (i >= M.mesh.sharedMesh.blendShapeCount) continue;

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                var bs = blendShapes.GetArrayElementAtIndex(i);
                                                if (bs != null && M.mesh.sharedMesh != null)
                                                {

                                                    bs.floatValue =
                                                        EditorGUILayout.Slider("(" + i.ToString("D2") + ") " + M.mesh.sharedMesh.GetBlendShapeName(i),
                                                        bs.floatValue, Min.floatValue, Max.floatValue);
                                                }

                                                if (GUILayout.Button(new GUIContent("↺"), EditorStyles.miniButton, GUILayout.Width(25)))
                                                {
                                                    bs.floatValue = 0;
                                                    serializedObject.ApplyModifiedProperties();
                                                    EditorUtility.SetDirty(target);
                                                    EditorUtility.SetDirty(mesh.objectReferenceValue);
                                                }

                                            }
                                            EditorGUILayout.Space(2);
                                        }
                                    }
                                }
                            }
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            preset.isExpanded = MalbersEditor.Foldout(preset.isExpanded, "Presets");

                            if (preset.isExpanded)
                            {
                                EditorGUILayout.PropertyField(preset, new GUIContent("Preset", "Saves the Blend Shapes values to a scriptable Asset"));

                                if (preset.objectReferenceValue != null)
                                {
                                    EditorGUILayout.HelpBox("The Preset will be loaded on Start", MessageType.Info);
                                }
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button("Save"))
                                    {
                                        if (preset.objectReferenceValue == null)
                                        {
                                            string newBonePath =
                                                EditorUtility.SaveFilePanelInProject("Create New Blend Preset", "BlendShape preset", "asset", "Message");

                                            BlendShapePreset bsPreset = CreateInstance<BlendShapePreset>();

                                            AssetDatabase.CreateAsset(bsPreset, newBonePath);

                                            preset.objectReferenceValue = bsPreset;
                                            serializedObject.ApplyModifiedProperties();

                                            Debug.Log("New Blend Shape Preset Created");
                                            M.SavePreset();

                                        }
                                        else
                                        {
                                            if (EditorUtility.DisplayDialog("Overwrite Blend Shape Preset",
                                                "Are you sure to overwrite the preset?", "Yes", "No"))
                                            {
                                                M.SavePreset();
                                                GUIUtility.ExitGUI();
                                            }
                                        }
                                    }

                                    using (new EditorGUI.DisabledGroupScope(preset.objectReferenceValue == null))
                                    {
                                        if (GUILayout.Button("Load") && preset.objectReferenceValue != null)
                                        {
                                            if (M.preset.blendShapes == null || M.preset.blendShapes.Length == 0)
                                                Debug.LogWarning("The preset " + M.preset.name + " is empty, Please use a Valid Preset");
                                            else
                                            {
                                                M.LoadPreset();
                                                EditorUtility.SetDirty(target);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Blend Shapes Changed");
                if (M.mesh) Undo.RecordObject(M.mesh, "Blend Shapes Changed");

                M.UpdateBlendShapes();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

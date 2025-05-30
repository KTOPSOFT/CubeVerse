using UnityEngine;
using UnityEditor;
using System;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(GradientBackground)), CanEditMultipleObjects]
    public class GradientBackgroundInspector : GraphicalColorComponentInspector
    {
        private static Texture2D background = null;

        private void OnEnable()
        {
            if (background == null)
            {
                background = new Texture2D(1, 1);
                background.hideFlags = HideFlags.HideAndDontSave;
                background.SetPixels32(new Color32[1] { new Color32(255, 255, 255, 100) });
                background.Apply();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            SerializedProperty checkboard = serializedObject.FindProperty("displayCheckboard");
            SerializedProperty checkBoardSize = serializedObject.FindProperty("checkBoardSize");
            SerializedProperty gradient = serializedObject.FindProperty("gradient");
            SerializedProperty direction = serializedObject.FindProperty("direction");
            SerializedProperty centerType = serializedObject.FindProperty("centerType");
            SerializedProperty centerPos = serializedObject.FindProperty("centerPos");

            SerializedProperty borderSize = serializedObject.FindProperty("borderSize");
            SerializedProperty borderColor = serializedObject.FindProperty("m_Color");

            SerializedProperty colors = serializedObject.FindProperty("colors");

            // simple fields
            EditorGUILayout.PropertyField(checkboard);
            if (checkboard.boolValue)
                EditorGUILayout.PropertyField(checkBoardSize);
            EditorGUILayout.PropertyField(gradient);
            EditorGUILayout.PropertyField(direction);

            // center position
            EditorGUILayout.PropertyField(centerType);
            if ((GradientBackground.GradientCenterType)centerType.intValue == GradientBackground.GradientCenterType.Custom)
            {
                EditorGUILayout.PropertyField(centerPos, new GUIContent("center position", gradient.boolValue ? "The position (normalized) at which the gradient will be half way" : "The (normalized) position to switch colors"));
            }

            // BorderSize
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(borderSize);
            EditorGUILayout.PropertyField(borderColor);

            // Colors
            EditorGUILayout.Space();
            int size = (target as GradientBackground).Count;
            for (int i = 0; i < targets.Length; i++)
            {
                if (size != (targets[i] as GradientBackground).Count)
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            int arraySize = Math.Max(2, EditorGUILayout.IntField("Colors amount", colors.arraySize));
            if (EditorGUI.EndChangeCheck())
            {
                colors.arraySize = arraySize;
            }

            EditorGUI.showMixedValue = false;

            EditorGUI.indentLevel++;
            for (int i = 0; i < colors.arraySize; i++)
            {
                SerializedProperty color = colors.GetArrayElementAtIndex(i);

                SerializedProperty type = color.FindPropertyRelative("type");
                SerializedProperty c = color.FindPropertyRelative("color");

                SerializedProperty isV2Fixed = color.FindPropertyRelative("v2Fixed");
                SerializedProperty isV3Fixed = color.FindPropertyRelative("v3Fixed");
                SerializedProperty isAlphaFixed = color.FindPropertyRelative("alphaFixed");

                SerializedProperty fixedV2 = color.FindPropertyRelative("fixedV2");
                SerializedProperty fixedV3 = color.FindPropertyRelative("fixedV3");
                SerializedProperty fixedAlpha = color.FindPropertyRelative("fixedAlpha");

                // For some (to me unknown) reason, despite the docs stating otherwise, arraysize is not (always) that of the smallest number of elements. Thus thowing nullreference exceptions here...
                if (type == null)
                    break;

                float initHeight = 5f;
                if (type.hasMultipleDifferentValues)
                {
                    float height = EditorGUI.GetPropertyHeight(type) + 3;
                    Rect rect = EditorGUILayout.GetControlRect(true, initHeight);
                    rect.y += initHeight * 0.5f;
                    rect.height = height + initHeight;
                    GUI.DrawTexture(rect, background);

                    EditorGUILayout.PropertyField(type);
                }
                else
                {

                    GradientBackground.GradientPartType t = (GradientBackground.GradientPartType)type.intValue;

                    float height = EditorGUI.GetPropertyHeight(type) + 3;
                    switch (t)
                    {
                        case GradientBackground.GradientPartType.Custom:
                            height += EditorGUI.GetPropertyHeight(c) + 3;
                            break;
                        case GradientBackground.GradientPartType.Color:
                            height += EditorGUI.GetPropertyHeight(fixedAlpha) + 3;
                            break;
                        case GradientBackground.GradientPartType.RGB_R:
                        case GradientBackground.GradientPartType.RGB_G:
                        case GradientBackground.GradientPartType.RGB_B:
                        case GradientBackground.GradientPartType.HSV_H:
                        case GradientBackground.GradientPartType.HSV_S:
                        case GradientBackground.GradientPartType.HSV_V:
                        case GradientBackground.GradientPartType.HSL_H:
                        case GradientBackground.GradientPartType.HSL_S:
                        case GradientBackground.GradientPartType.HSL_L:
                            height += EditorGUI.GetPropertyHeight(fixedV2) + 3;
                            height += EditorGUI.GetPropertyHeight(fixedV3) + 3;
                            height += EditorGUI.GetPropertyHeight(fixedAlpha) + 3;
                            break;
                        default:
                            break;
                    }
                    Rect rect = EditorGUILayout.GetControlRect(true, initHeight);
                    rect.y += initHeight * 0.5f;
                    rect.height = height + initHeight;
                    GUI.DrawTexture(rect, background);

                    EditorGUILayout.PropertyField(type);

                    switch (t)
                    {
                        case GradientBackground.GradientPartType.Custom:
                            EditorGUILayout.PropertyField(c);
                            break;
                        case GradientBackground.GradientPartType.Color:
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.RGB_R:
                            DrawFixedFloat(isV2Fixed, fixedV2, "green");
                            DrawFixedFloat(isV3Fixed, fixedV3, "blue");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.RGB_G:
                            DrawFixedFloat(isV2Fixed, fixedV2, "red");
                            DrawFixedFloat(isV3Fixed, fixedV3, "blue");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.RGB_B:
                            DrawFixedFloat(isV2Fixed, fixedV2, "red");
                            DrawFixedFloat(isV3Fixed, fixedV3, "green");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSV_H:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Saturation");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Value");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSV_S:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Hue");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Value");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSV_V:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Hue");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Saturation");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSL_H:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Saturation");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Light");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSL_S:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Hue");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Light");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        case GradientBackground.GradientPartType.HSL_L:
                            DrawFixedFloat(isV2Fixed, fixedV2, "Hue");
                            DrawFixedFloat(isV3Fixed, fixedV3, "Saturation");
                            DrawFixedAlpha(isAlphaFixed, fixedAlpha);
                            break;
                        default:
                            break;
                    }
                }
                EditorGUILayout.GetControlRect(true, 5f);
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawFixedAlpha(SerializedProperty fixedBool, SerializedProperty fixedValue)
        {
            Rect position = EditorGUILayout.GetControlRect();
            float deduction = EditorGUI.IndentedRect(position).x - position.x;
            Rect left = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - deduction, position.height);
            Rect right = new Rect(position.x + EditorGUIUtility.labelWidth - deduction, position.y, position.width - EditorGUIUtility.labelWidth + deduction, position.height);

            EditorGUI.BeginChangeCheck();
            bool boolResult = EditorGUI.ToggleLeft(left, "Fixed Alpha", fixedBool.boolValue);
            if (EditorGUI.EndChangeCheck())
                fixedBool.boolValue = boolResult;

            GUI.enabled = fixedBool.boolValue && !fixedBool.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int intResult = EditorGUI.IntSlider(right, fixedValue.intValue, 0, 255);
            if (EditorGUI.EndChangeCheck())
                fixedValue.intValue = intResult;
            GUI.enabled = true;
        }

        private static void DrawFixedFloat(SerializedProperty fixedBool, SerializedProperty fixedValue, string name)
        {
            Rect position = EditorGUILayout.GetControlRect();
            float deduction = EditorGUI.IndentedRect(position).x - position.x;
            Rect left = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - deduction, position.height);
            Rect right = new Rect(position.x + EditorGUIUtility.labelWidth - deduction, position.y, position.width - EditorGUIUtility.labelWidth + deduction, position.height);

            EditorGUI.BeginChangeCheck();
            bool boolResult = EditorGUI.ToggleLeft(left, "Fixed " + name, fixedBool.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                fixedBool.boolValue = boolResult;
            }

            GUI.enabled = fixedBool.boolValue && !fixedBool.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            float floatResult = EditorGUI.Slider(right, fixedValue.floatValue, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                fixedValue.floatValue = floatResult;
            }
            GUI.enabled = true;
        }
    }
}
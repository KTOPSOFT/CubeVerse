
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace MalbersAnimations
{
    [CustomEditor(typeof(MobileJoystick))]
    public class MobileJoystickEditor : Editor
    {
        SerializedProperty Jbutton, bg, invertX, invertY, sensitivityX, sensitivityY, axisValue, OnJoystickDown, OnJoystickUp, Drag, Dynamic,
            OnAxisChange, OnXAxisChange, Button,
            pressed, OnYAxisChange, OnJoystickPressed, AxisEditor, EventsEditor, ReferencesEditor, StopJoyStick, deathpoint;

        private void OnEnable()
        {
            // CameraInput = serializedObject.FindProperty("CameraInput");
            bg = serializedObject.FindProperty("bg");
            Button = serializedObject.FindProperty("Button");
            Jbutton = serializedObject.FindProperty("Jbutton");
            Dynamic = serializedObject.FindProperty("Dynamic");
            invertX = serializedObject.FindProperty("invertX");
            invertY = serializedObject.FindProperty("invertY");
            sensitivityX = serializedObject.FindProperty("sensitivityX");
            sensitivityY = serializedObject.FindProperty("sensitivityY");
            axisValue = serializedObject.FindProperty("axisValue");
            pressed = serializedObject.FindProperty("pressed");
            Drag = serializedObject.FindProperty("m_Drag");
            StopJoyStick = serializedObject.FindProperty("StopJoyStick");


            OnJoystickDown = serializedObject.FindProperty("OnJoystickDown");
            OnJoystickUp = serializedObject.FindProperty("OnJoystickUp");
            OnAxisChange = serializedObject.FindProperty("OnAxisChange");
            OnXAxisChange = serializedObject.FindProperty("OnXAxisChange");
            OnYAxisChange = serializedObject.FindProperty("OnYAxisChange");
            OnJoystickPressed = serializedObject.FindProperty("OnJoystickPressed");

            AxisEditor = serializedObject.FindProperty("AxisEditor");
            EventsEditor = serializedObject.FindProperty("EventsEditor");
            ReferencesEditor = serializedObject.FindProperty("ReferencesEditor");
            deathpoint = serializedObject.FindProperty("deathpoint");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Mobile Joystick Logic");


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(Button);
                EditorGUILayout.PropertyField(Jbutton, new GUIContent("Button Image"));
                EditorGUILayout.PropertyField(bg, new GUIContent("Button Background"));
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (MalbersEditor.Foldout(AxisEditor, "Axis Properties"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        MalbersEditor.BoolButton(invertX, new GUIContent("Invert X"));
                        MalbersEditor.BoolButton(invertY, new GUIContent("Invert Y"));
                        MalbersEditor.BoolButton(Drag, new GUIContent("Drag"));
                    }


                    EditorGUILayout.PropertyField(deathpoint);
                    EditorGUILayout.PropertyField(sensitivityX);
                    EditorGUILayout.PropertyField(sensitivityY);
                    EditorGUILayout.PropertyField(StopJoyStick);

                    if (!Drag.boolValue) EditorGUILayout.PropertyField(Dynamic);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (MalbersEditor.Foldout(ReferencesEditor, "Exposed Values"))
                {
                    EditorGUILayout.PropertyField(axisValue);
                    EditorGUILayout.PropertyField(pressed);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EventsEditor.boolValue = MalbersEditor.Foldout(EventsEditor, "Events");

                if (EventsEditor.boolValue)
                {
                    EditorGUILayout.PropertyField(OnJoystickDown);
                    EditorGUILayout.PropertyField(OnJoystickUp);
                    EditorGUILayout.PropertyField(OnJoystickPressed);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(OnAxisChange);
                    EditorGUILayout.PropertyField(OnXAxisChange);
                    EditorGUILayout.PropertyField(OnYAxisChange);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
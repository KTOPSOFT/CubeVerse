//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(DateAndClock))]
    public class DateAndClockEditor : Editor
    {
        SerializedProperty dateFormat;

        void OnEnable()
        {
            dateFormat = serializedObject.FindProperty("dateFormat");
        }

        override public void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var DateAndClock = target as DateAndClock;

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(DateAndClock.objectType == DateAndClock.ObjectType.AnalogClock)))
            {
                if (group.visible == true)
                {
                    DateAndClock.clockHourHand = EditorGUILayout.ObjectField(DateAndClock.clockHourHand, typeof(Transform), true) as Transform;
                    DateAndClock.clockMinuteHand = EditorGUILayout.ObjectField(DateAndClock.clockMinuteHand, typeof(Transform), true) as Transform;
                    DateAndClock.clockSecondHand = EditorGUILayout.ObjectField(DateAndClock.clockSecondHand, typeof(Transform), true) as Transform;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(DateAndClock.objectType == DateAndClock.ObjectType.DigitalClock)))
            {
                if (group.visible == true)
                {
                    DateAndClock.digitalClockText = EditorGUILayout.ObjectField(DateAndClock.digitalClockText, typeof(TMPro.TextMeshProUGUI), true) as TMPro.TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(DateAndClock.objectType == DateAndClock.ObjectType.DigitalDate)))
            {
                if (group.visible == true)
                {
                    EditorGUILayout.PropertyField(dateFormat, new GUIContent("Date Format"), true);
                    serializedObject.ApplyModifiedProperties();
                    DateAndClock.digitalDateText = EditorGUILayout.ObjectField(DateAndClock.digitalDateText, typeof(TMPro.TextMeshProUGUI), true) as TMPro.TextMeshProUGUI;
                }
            }
        }
    }
}
#endif
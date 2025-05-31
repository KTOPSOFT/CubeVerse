#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations
{
    [CustomEditor(typeof(Stats))]
    public class StatsEd : Editor
    {
        private ReorderableList list;
        private Stats M;
        private SerializedProperty statList, Selected_StatIndex
            ;

        protected string[] Tabs1 = new string[] { "General", "Events" };

        private void OnEnable()
        {
            M = (Stats)target;

            statList = serializedObject.FindProperty("stats");
            Selected_StatIndex = serializedObject.FindProperty("Selected_StatIndex");
            //Set = serializedObject.FindProperty("Set");




            list = new(serializedObject, statList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack,
                onSelectCallback = (list) => { Selected_StatIndex.intValue = list.index; }
            };


            list.index = Selected_StatIndex.intValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Stats Manager");

            //  EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {

                if (Application.isPlaying)
                {

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        if (M.PinnedStat != null)
                            EditorGUILayout.ObjectField("Pin Stat: ", (StatID)M.PinnedStat.ID, typeof(StatID), false);
                        else
                            EditorGUILayout.LabelField("Pin Stat: NULL ");
                    }

                }

                list.DoLayoutList();

                if (list.index != -1)
                {
                    var element = statList.GetArrayElementAtIndex(list.index);

                    var ID = element.FindPropertyRelative("ID").objectReferenceValue;
                    var statName = ID != null ? ID.name : "";

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            var dC = GUI.color;
                            GUI.color = selected;

                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(element, new GUIContent($"[{statName} Properties]"), false);
                            EditorGUI.indentLevel--;
                            GUI.color = dC;
                            var debug = element.FindPropertyRelative("debug");
                            MalbersEditor.DrawDebugIcon(debug);

                        }

                        if (element.isExpanded)
                        {
                            var EditorTabs = element.FindPropertyRelative("EditorTabs");

                            EditorTabs.intValue = GUILayout.Toolbar(EditorTabs.intValue, Tabs1);

                            if (EditorTabs.intValue == 0) DrawGeneral(element);
                            else DrawEvents(element);
                        }
                    }
                }
            }
            //EditorGUILayout.PropertyField(Set,new GUIContent("Runtime Set")); 
            //    EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral(SerializedProperty element)
        {
            var DisableOnEmpty = element.FindPropertyRelative("DisableOnEmpty");
            var Value = element.FindPropertyRelative("value");
            var MaxValue = element.FindPropertyRelative("maxValue");
            var MinValue = element.FindPropertyRelative("minValue");

            var resetTo = element.FindPropertyRelative("resetTo");
            var ImmuneTime = element.FindPropertyRelative("ImmuneTime");
            var immune = element.FindPropertyRelative("immune");

            var Regenerate = element.FindPropertyRelative("regenerate");
            var RegenRate = element.FindPropertyRelative("RegenRate");
            var RegenWaitTime = element.FindPropertyRelative("RegenWaitTime");

            var Degenerate = element.FindPropertyRelative("degenerate");
            var DegenRate = element.FindPropertyRelative("DegenRate");
            var DegenWaitTime = element.FindPropertyRelative("DegenWaitTime");
            var multiplier = element.FindPropertyRelative("multiplier");
            var Round = element.FindPropertyRelative("Round");
            var ResetOnEnable = element.FindPropertyRelative("ResetOnEnable");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(Value);
                    EditorGUILayout.PropertyField(multiplier, new GUIContent("Mult", "Stat Multiplier to be used when the value is modified"));
                    EditorGUIUtility.labelWidth = 0;
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(MinValue, new GUIContent("Min"));
                    EditorGUILayout.PropertyField(MaxValue, new GUIContent("Max"));
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(DisableOnEmpty);
                EditorGUILayout.PropertyField(Round);
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(Regenerate, new GUIContent("Regenerate", "Can the Stat Regenerate over time?"));
                EditorGUILayout.PropertyField(RegenRate, new GUIContent("Rate", "Regeneration Rate, how fast/Slow the Stat will regenerate"));
                EditorGUILayout.PropertyField(RegenWaitTime, new GUIContent("Wait Time", "After the Stat is modified, the time to wait to start regenerating"));
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(Degenerate, new GUIContent("Degenerate", "Can the Stat Degenerate over time?"));
                EditorGUILayout.PropertyField(DegenRate, new GUIContent("Rate", "Degeneration Rate, how fast/Slow the Stat will Degenerate"));
                EditorGUILayout.PropertyField(DegenWaitTime, new GUIContent("Wait Time", "After the Stat is modified, the time to wait to start degenerating"));
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(resetTo, new GUIContent("Reset To", "When called the Funtion RESET()  it will reset to the Min Value or the Max Value"));
                EditorGUILayout.PropertyField(ResetOnEnable);
                EditorGUILayout.PropertyField(ImmuneTime, new GUIContent("Immune Time", "If greater than zero, the Stat cannot be modify until the inmune time have passed"));
                EditorGUILayout.PropertyField(immune);
            }

        }

        private void DrawEvents(SerializedProperty element)
        {
            var id = element.FindPropertyRelative("ID");
            var BelowValue = element.FindPropertyRelative("Below");
            var AboveValue = element.FindPropertyRelative("Above");

            var OnValueChange = element.FindPropertyRelative("OnValueChange");
            var OnValueChangeNormalized = element.FindPropertyRelative("OnValueChangeNormalized");
            var OnStatFull = element.FindPropertyRelative("OnStatFull");
            var OnStatEmpty = element.FindPropertyRelative("OnStatEmpty");
            var OnRegenerate = element.FindPropertyRelative("OnRegenerate");
            var OnDegenerate = element.FindPropertyRelative("OnDegenerate");
            var OnStatBelow = element.FindPropertyRelative("OnStatBelow");
            var OnStatAbove = element.FindPropertyRelative("OnStatAbove");
            var OnMaxValueChange = element.FindPropertyRelative("OnMaxValueChange");
            var OnActive = element.FindPropertyRelative("OnActive");
            var isPercent = element.FindPropertyRelative("isPercent");


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string name = "Stat";

                if (id.objectReferenceValue != null)
                {
                    name = id.objectReferenceValue.name;
                }

                EditorGUILayout.PropertyField(OnValueChange, new GUIContent($"On [{name}] change"));
                EditorGUILayout.PropertyField(OnValueChangeNormalized, new GUIContent($"On [{name}] change normalized"));
                EditorGUILayout.PropertyField(OnMaxValueChange, new GUIContent($"On [{name}] Max Value change"));
                MalbersEditor.DrawSplitter();
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(OnActive, new GUIContent($"On [{name}] Active "));
                EditorGUILayout.PropertyField(OnStatFull, new GUIContent($"On [{name}] full "));
                EditorGUILayout.PropertyField(OnStatEmpty, new GUIContent($"On [{name}] empty "));
                MalbersEditor.DrawSplitter();
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(OnRegenerate, new GUIContent($"On [{name}] Regenerate "));
                EditorGUILayout.PropertyField(OnDegenerate, new GUIContent($"On [{name}] Degenerate "));

                MalbersEditor.DrawSplitter();
                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())

                {
                    EditorGUIUtility.labelWidth = 55;
                    EditorGUILayout.PropertyField(BelowValue, new GUIContent("Below", "Used to Check when the Stat is below this value"));
                    EditorGUILayout.PropertyField(AboveValue, new GUIContent("Above", "Used to Check when the Stat is Above this value"));
                    isPercent.boolValue = GUILayout.Toggle(isPercent.boolValue, new GUIContent("%", "Check below/above using percent instead of static values"), EditorStyles.miniButton, GUILayout.Width(25));
                    EditorGUIUtility.labelWidth = 0;
                }
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(OnStatBelow, new GUIContent($"On [{name}] Below {BelowValue.floatValue}"));
                EditorGUILayout.PropertyField(OnStatAbove, new GUIContent($"On [{name}] Above {AboveValue.floatValue}"));
            }

        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 45, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.width / 2 + 25, rect.y, rect.x + (rect.width / 4) - 5 - 25, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new Rect(rect.width + 10, rect.y, rect.width + 25, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "     ID/Name", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "Value", EditorStyles.centeredGreyMiniLabel);
            EditorGUI.LabelField(R_3, "ID", EditorStyles.miniLabel);
        }

        private static readonly Color selected = new(1.5f, 1f, 0);

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.x += 5;
            rect.width -= 15;

            var element = statList.GetArrayElementAtIndex(index);
            var ID = element.FindPropertyRelative("ID");
            var active = element.FindPropertyRelative("active");
            var Value = element.FindPropertyRelative("value");


            rect.y += 2;

            Rect R_0 = new(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new(rect.x + 40, rect.y, (rect.width) / 2 - 22, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new(rect.x + 40 + ((rect.width) / 2), rect.y, rect.width - ((rect.width) / 2) - 40, EditorGUIUtility.singleLineHeight);
            Rect R_3 = new(rect.width + 45, rect.y, rect.width + 25, EditorGUIUtility.singleLineHeight);

            var dC = GUI.color;
            if (index == Selected_StatIndex.intValue) GUI.color = selected;

            EditorGUI.PropertyField(R_0, active, new GUIContent("", "Is the Stat Enabled? when Disable no modification can be done"));

            EditorGUI.PropertyField(R_1, ID, GUIContent.none);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(R_2, Value, GUIContent.none);

            if (ID.objectReferenceValue != null)
            {
                var od = ID.objectReferenceValue as StatID;
                EditorGUI.LabelField(R_3, od.ID.ToString(), EditorStyles.boldLabel);
            }

            if (EditorGUI.EndChangeCheck())
            {
                var ConstantMAX = element.FindPropertyRelative("maxValue").FindPropertyRelative("ConstantValue");
                var ConstantValue = element.FindPropertyRelative("value").FindPropertyRelative("ConstantValue");

                if (ConstantMAX.floatValue < ConstantValue.floatValue)
                {
                    ConstantMAX.floatValue = ConstantValue.floatValue;
                }
            }

            GUI.color = dC;
        }


        void OnAddCallBack(ReorderableList list)
        {
            M.stats ??= new List<Stat>();
            M.stats.Add(new Stat());
        }
    }
}

#endif
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Stats/Modify Stats")]

    public class ModifyStat : MonoBehaviour
    {
        public static readonly string[] Tooltips = {
          "[None] Skips the stat modification",
          "Adds to the stat Value",
          "Sets the stat value",
          "Substracts from the stat value",
          "Modifies the Stat maximum Value (Adds or Remove)",
          "Set the Stat maximum Value",
          "Enables the Degeneration and sets the Degen Rate Value. If the value is 0, the rate Value wont be changed",
          "Stops the Degeneration",
          "Enables the Regeneration and sets the Regen Rate Value.  If the value is 0, the rate Value wont be changed",
          "Stops the Regeneration",
          "Reset the Stat to the Default Min or Max Value",
          "Reduce the Value of the Stat by a percent",
          "Increase the Value of the Stat by a percent",
          "Sets the multiplier value of the stat",
          "Reset the Stat to the maximun Value",
          "Reset the Stat to the minimun Value",
          "Enable/Disable the Stat",
          "Set Imnune",
          "Starts the Regeneration",
          "Restore the Regeneration to its default",
          "Restore the Degeneration to its default",
          "Restore the Value to its default",
          "Restore the Max Value to its default",
          "Restore the Min to its default",
          "Restore the value to its default",
          "Restore the Mutliplier to its default",
          "Adds or Remove a value to the Multiplier",
    };


        public Stats stats;

        public List<StatModifier> modifiers = new();

        public virtual void SetStats(GameObject go) => stats = go.FindComponent<Stats>();
        public virtual void SetStats(Component go) => SetStats(go.gameObject);


        /// <summary> Apply All Modifiers to the Stats </summary>
        public virtual void Modify()
        {
            foreach (var statmod in modifiers)
                statmod.ModifyStat(stats);
        }

        public virtual void Modify(GameObject target)
        {
            SetStats(target);
            Modify();
        }
        public virtual void Modify(Component target)
        {
            Modify(target.gameObject);
        }

        /// <summary> Apply a Modifiers to the Stats using its Index</summary>
        public virtual void Modify(int index)
        {
            if (modifiers != null && index < modifiers.Count)
                modifiers[index]?.ModifyStat(stats);
        }
    }



    /// <summary> Modify a Stat usings its properties </summary>
    [System.Serializable]
    public class StatModifier
    {
        //public bool active = true;
        public StatID ID;
        public StatOption modify = StatOption.None;
        public FloatReference MinValue = new(10f);
        public FloatReference MaxValue = new(10f);
        public BoolReference enable = new(true);

        public float Value
        {
            get
            {
                return UnityEngine.Random.Range(MinValue, MaxValue); //Get the value from a random range from min to max
            }
            set
            {
                MinValue = new(value);
                MaxValue = new(value);
            }
        }

        public StatModifier()
        {
            ID = null;
            modify = StatOption.None;
            MinValue = new(10);
            MaxValue = new(10);
            enable = new(true);
        }

        public StatModifier(StatModifier mod)
        {
            ID = mod.ID;
            modify = mod.modify;
            MinValue = new(mod.MinValue.Value);
            MaxValue = new(mod.MaxValue.Value);
            enable = new(true);
        }


        //  public static implicit operator StatModifier(StatModifierPlus r) => new StatModifier() { ID = r.ID, modify = r.modify, Value = r.Value };

        /// <summary>There's No ID stat</summary>
        public bool IsNull => ID == null;


        /// <summary>Modify the Stats on an animal </summary>
        public bool ModifyStat(Stats stats)
        {
            if (stats && stats.enabled && !IsNull)
            {
                return ModifyStat(stats.Stat_Get(ID));
            }
            return false;
        }

        /// <summary>Modify the Stats on an animal applying a random value from Min to Max </summary>
        public bool ModifyStat(Stat s)
        {
            if (s != null)
            {
                if (modify == StatOption.Inmune || modify == StatOption.Enable)
                {
                    s.Modify(enable.Value ? 1 : 0, modify);
                }
                else
                {
                    s.Modify(Random.Range(MinValue, MaxValue), modify);
                }
                return true;

            }
            return false;
        }

        /// <summary>Modify the Stats on an animal applying a value from Min to Max get by the Normalized paramater</summary>
        public bool ModifyStat(Stat s, float Normalized)
        {
            if (s != null)
            {
                if (modify == StatOption.Inmune || modify == StatOption.Enable)
                {
                    s.Modify(enable.Value ? 1 : 0, modify);
                    return true;
                }
                else
                {
                    s.Modify(Mathf.Lerp(MinValue, MaxValue, Normalized), modify);
                    return true;
                }
            }
            return false;
        }
        /// <summary>Gets a value from the Modifier (Normalized value from Min to Max)</summary>
        public float GetValue(float Normalized) => Mathf.Lerp(MinValue, MaxValue, Normalized);

        /// <summary>Gets a value from the Modifier (Random from Min to Max)</summary>
        public float GetValue() => UnityEngine.Random.Range(MinValue, MaxValue);
    }



    //--------------------EDITOR----------------
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(StatModifier))]
    public class StatModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            var height = EditorGUIUtility.singleLineHeight;

            //  EditorGUI.indentLevel = 0;

            var ID = property.FindPropertyRelative("ID");
            var MaxValue = property.FindPropertyRelative("MaxValue");
            var MinValue = property.FindPropertyRelative("MinValue");
            var modify = property.FindPropertyRelative("modify");
            var enable = property.FindPropertyRelative("enable");

            var line = new Rect(position)
            {
                width = position.width * 0.6f,
                height = height,
            };

            var LabelWith = 50;

            EditorGUIUtility.labelWidth = LabelWith;
            EditorGUI.PropertyField(line, ID, new GUIContent("Stat", "Stat ID to modify"));
            EditorGUIUtility.labelWidth = 0;

            line.x += position.width * 0.6f + 5;
            line.width = position.width * 0.4f - 5;
            EditorGUI.PropertyField(line, modify, new GUIContent(string.Empty, ModifyStat.Tooltips[modify.intValue]));
            EditorGUI.LabelField(line, new GUIContent("             ", ModifyStat.Tooltips[modify.intValue]));

            var line2 = new Rect(position);
            line2.y += height + 2;


            EditorGUIUtility.labelWidth = LabelWith;
            if (CheckEnum(modify.intValue))
            {
                //Don't Draw anything
            }
            else if (modify.intValue == (int)StatOption.Enable || modify.intValue == (int)StatOption.Inmune)
            {
                EditorGUI.PropertyField(line2, enable, new GUIContent("Value"));
            }
            else
            {
                line2.width = position.width / 2;


                EditorGUI.PropertyField(line2, MinValue, new GUIContent("Min", "Minimun Value"));

                line2.x += position.width / 2 + 5;
                line2.width -= 5;
                EditorGUI.PropertyField(line2, MaxValue, new GUIContent("Max", "Maximum Value"));

            }
            EditorGUIUtility.labelWidth = 0;
            property.serializedObject.ApplyModifiedProperties();
            // EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var modify = property.FindPropertyRelative("modify");

            if (CheckEnum(modify.intValue))
            {
                return 18;
            }
            else
            {
                return (18 * 2) + 2;
            }
        }

        private bool CheckEnum(int modify)
        {
            return
                modify == (int)StatOption.None ||
                modify == (int)StatOption.Reset ||
                modify == (int)StatOption.DegenerateOff ||
                modify == (int)StatOption.RegenerateOff ||
                modify == (int)StatOption.ResetToMax ||
                modify == (int)StatOption.ResetToMin ||
                modify == (int)StatOption.RegenerateOn ||
                modify == (int)StatOption.DegenerateOn ||
                modify == (int)StatOption.RestoreValue ||
                modify == (int)StatOption.RestoreMax ||
                modify == (int)StatOption.RestoreMin ||
                modify == (int)StatOption.RestoreDegeneration ||
                modify == (int)StatOption.RestoreMultiplier ||
                modify == (int)StatOption.RestoreRegeneration;
        }
    }
#endif

}


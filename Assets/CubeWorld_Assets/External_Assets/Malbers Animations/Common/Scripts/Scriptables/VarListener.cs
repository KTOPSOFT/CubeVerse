using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;




#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations
{
    public abstract class VarListener : MonoBehaviour
    {
        [HideInInspector] public bool ShowEvents = false;

        [Tooltip("ID value is used on the AI Brain to know which Var Listener is picked, in case there more than one on one Game Object")]
        public IntReference ID;

        public bool Enable => gameObject.activeInHierarchy && enabled;

        [Tooltip("The Events will be invoked when the Listener Value changes.\nIf is set to false, call Invoke() to invoke the events manually")]
        public bool Auto = true;
        [Tooltip("Invokes the current value on Enable")]
        public bool InvokeOnEnable = true;

        public string Description = "";
        [HideInInspector] public bool ShowDescription = false;
        [ContextMenu("Show Description")]
        internal void EditDescription() => ShowDescription ^= true;

        public bool debug = false;
    }

    [System.Serializable]
    public class AdvancedIntegerEvent
    {
        public bool active = true;
        public string name;
        public string description;
        public ComparerInt comparer = ComparerInt.Equal;
        public IntReference Value = new();
        public IntEvent Response = new();

        [Tooltip("Update the value of the comparer with the incoming Master Value after the comparison")]
        public bool UpdateAfterCompare = false;

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="IntValue">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceIntegerEvent(int IntValue)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerInt.Equal:
                        if (IntValue == Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Greater:
                        if (IntValue > Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.Less:
                        if (IntValue < Value) Response.Invoke(IntValue);
                        break;
                    case ComparerInt.NotEqual:
                        if (IntValue != Value) Response.Invoke(IntValue);
                        break;
                    default:
                        break;
                }

                if (UpdateAfterCompare) Value.Value = IntValue;
            }
        }

        public void SetValue(int value) => Value.Value = value;

        public AdvancedIntegerEvent()
        {
            active = true;
            name = "NameHere";
            description = "";
            comparer = ComparerInt.Equal;
            Value = new IntReference();
            Response = new IntEvent();
        }
    }

    [System.Serializable]
    public class AdvancedFloatEvent
    {
        public bool active = true;
        public string name;
        public string description;
        public ComparerInt comparer = ComparerInt.Equal;
        public FloatReference Value = new();
        public FloatEvent Response = new();

        [Tooltip("Update the value of the comparer with the incoming Master Value after the comparison")]
        public bool UpdateAfterCompare = false;

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="v">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceFloatEvent(float v)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerInt.Equal:
                        if (v == Value) Response.Invoke(v);
                        break;
                    case ComparerInt.Greater:
                        if (v > Value) Response.Invoke(v);
                        break;
                    case ComparerInt.Less:
                        if (v < Value) Response.Invoke(v);
                        break;
                    case ComparerInt.NotEqual:
                        if (v != Value) Response.Invoke(v);
                        break;
                    default:
                        break;
                }

                if (UpdateAfterCompare) Value.Value = v;
            }
        }

        public void SetValue(float value) => Value.Value = value;
    }

    [System.Serializable]
    public class AdvancedBoolEvent
    {
        public bool active = true;
        public string name;
        public ComparerBool comparer = ComparerBool.Equal;
        public BoolReference Value = new();
        public UnityEvent Response = new();

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="boolValue">Value that comes from the IntEvent</param>
        public void ExecuteAdvanceBoolEvent(bool boolValue)
        {
            if (active)
            {
                switch (comparer)
                {
                    case ComparerBool.Equal:
                        if (boolValue == Value) Response.Invoke();
                        break;
                    case ComparerBool.NotEqual:
                        if (boolValue != Value) Response.Invoke();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetValue(bool value) => Value.Value = value;

    }

    [System.Serializable]
    public class AdvancedStringEvent
    {
        public bool active = true;
        public string name;
        public string description;
        public ComparerString comparer = ComparerString.Equal;
        public StringReference Value = new();
        public StringEvent Response = new();
        public UnityEvent OnTrue = new();
        public UnityEvent OnFalse = new();


        [Tooltip("Update the value of the comparer with the incoming Master Value after the comparison")]
        public bool UpdateAfterCompare = false;

        /// <summary>Use the comparer to execute a response using the Int Event and the Value</summary>
        /// <param name="val">Value that comes from the string event</param>
        public bool ExecuteAdvanceStringEvent(string val)
        {
            return comparer switch
            {
                ComparerString.Equal => StringComparisonResult(val, val == Value.Value),
                ComparerString.NotEqual => StringComparisonResult(val, val != Value.Value),
                ComparerString.Empty => StringComparisonResult(val, string.IsNullOrEmpty(val)),
                ComparerString.Contains => StringComparisonResult(val, val.Contains(Value.Value)),
                ComparerString.DoesNotContains => StringComparisonResult(val, !val.Contains(Value.Value)),
                _ => false,
            };
        }


        private bool StringComparisonResult(string value, bool result)
        {
            Response.Invoke(value);
            if (result) OnTrue.Invoke(); else OnFalse.Invoke();

            if (UpdateAfterCompare) Value.Value = value;
            return result;
        }

        public void SetValue(string value) => Value.Value = value;

    }


    //INSPECTOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AdvancedIntegerEvent))]
    [CustomPropertyDrawer(typeof(AdvancedFloatEvent))]
    public class AdvNumberComparerDrawer : PropertyDrawer
    {
        //const float labelwith = 27f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;
            var name = property.FindPropertyRelative("name");
            var UpdateAfterCompare = property.FindPropertyRelative("UpdateAfterCompare");
            var comparer = property.FindPropertyRelative("comparer");
            var Value = property.FindPropertyRelative("Value");
            var Response = property.FindPropertyRelative("Response");

            //bool isExpanded = property.isExpanded;


            if (name.stringValue == string.Empty) name.stringValue = "NameHere";

            var line = position;
            line.height = height;

            line.x += 4;
            line.width -= 8;

            var foldout = new Rect(line);
            foldout.width = 10;
            //foldout.x += 10;

            EditorGUIUtility.labelWidth = 16;
            property.isExpanded = EditorGUI.Foldout(foldout, property.isExpanded, GUIContent.none);
            EditorGUIUtility.labelWidth = 0;

            var rectName = new Rect(line);

            var part = line.width / 3;

            rectName.x += 12;
            rectName.width = part - 15;

            name.stringValue = GUI.TextField(rectName, name.stringValue);


            var ComparerRect = new Rect(line.x + part, line.y, part - 10, height);
            var ValueRect = new Rect(line.x + part * 2 + 10, line.y, part - 5 - 25, height);
            var UpdateRect = new Rect(line.width - 12, line.y, 25, height);

            //    line.y += height + 2;

            EditorGUI.PropertyField(ComparerRect, comparer, GUIContent.none);
            EditorGUI.PropertyField(ValueRect, Value, GUIContent.none);
            EditorGUI.PropertyField(UpdateRect, UpdateAfterCompare, GUIContent.none);

            if (property.isExpanded)
            {
                line.y += height + 2;
                EditorGUI.PropertyField(line, Response);
                position.height = line.height;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return base.GetPropertyHeight(property, label) + 2;

            var Response = property.FindPropertyRelative("Response");
            float ResponseHeight = EditorGUI.GetPropertyHeight(Response);

            return 16 + ResponseHeight + 10;
        }
    }


    [CustomEditor(typeof(VarListener))]
    public class VarListenerEditor : UnityEditor.Editor
    {
        protected UnityEditor.SerializedProperty value, Description, Index, ShowEvents, ShowDescription, Debug, InvokeOnEnable, Auto;
        protected GUIStyle style, styleDesc;

        private GUIContent scrollUP;
        public GUIContent ScrollUP
        {
            get
            {
                if (scrollUP == null)
                {
                    scrollUP = EditorGUIUtility.IconContent("d_scrollup");
                    scrollUP.tooltip = "Collapse";
                }
                return scrollUP;
            }
        }

        private GUIContent scrollDown;
        public GUIContent ScrollDown
        {
            get
            {
                if (scrollDown == null)
                {
                    scrollDown = EditorGUIUtility.IconContent("d_scrolldown");
                    scrollDown.tooltip = "Expand";
                }
                return scrollDown;
            }
        }


        void OnEnable() { SetEnable(); }

        protected void SetEnable()
        {
            value = serializedObject.FindProperty("value");
            Description = serializedObject.FindProperty("Description");
            ShowDescription = serializedObject.FindProperty("ShowDescription");
            Index = serializedObject.FindProperty("ID");
            ShowEvents = serializedObject.FindProperty("ShowEvents");
            Debug = serializedObject.FindProperty("debug");
            Auto = serializedObject.FindProperty("Auto");
            InvokeOnEnable = serializedObject.FindProperty("InvokeOnEnable");
        }

        public static GUIStyle StyleBlue => MTools.Style(MTools.MBlue);
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ShowDescription.boolValue)
            {
                if (ShowDescription.boolValue)
                {
                    if (style == null)
                    {
                        style = new GUIStyle(StyleBlue)
                        {
                            fontSize = 12,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleLeft,
                            stretchWidth = true
                        };

                        style.normal.textColor = EditorStyles.boldLabel.normal.textColor;

                    }

                    Description.stringValue = EditorGUILayout.TextArea(Description.stringValue, style);
                }
            }

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))

            {
                EditorGUIUtility.labelWidth = 55;
                EditorGUILayout.PropertyField(value, GUILayout.MinWidth(25));
                EditorGUIUtility.labelWidth = 40;
                EditorGUILayout.PropertyField(Index, new GUIContent("    ID"), GUILayout.MinWidth(15));
                EditorGUIUtility.labelWidth = 0;
                ShowEvents.boolValue =
                    GUILayout.Toggle(ShowEvents.boolValue,
                    //new GUIContent((ShowEvents.boolValue ? "▲" : "▼"), "Show Events"),
                    (ShowEvents.boolValue ? ScrollUP : ScrollDown)
                  , EditorStyles.miniButton, GUILayout.Width(26)
                    );
            }

            if (ShowEvents.boolValue)
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUIUtility.labelWidth = 55;
                    EditorGUILayout.PropertyField(Auto);
                    EditorGUIUtility.labelWidth = 65;
                    EditorGUILayout.PropertyField(InvokeOnEnable, new GUIContent("On Enable"));
                    EditorGUIUtility.labelWidth = 0;
                    MalbersEditor.DrawDebugIcon(Debug);
                    //Debug.boolValue = GUILayout.Toggle(Debug.boolValue, new GUIContent("D"), UnityEditor.EditorStyles.miniButton, GUILayout.Width(22));
                }


                DrawElemets();
            }
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawElemets() { }
    }
#endif
}
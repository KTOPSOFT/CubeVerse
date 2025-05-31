using MalbersAnimations;
using MalbersAnimations.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomPropertyDrawer(typeof(ConditionCore))]
public class ConditionCoreDrawer : PropertyDrawer
{
    const int k_MaxTypePopupLineCount = 8;
    static readonly Type k_UnityObjectType = typeof(UnityEngine.Object);
    static readonly GUIContent k_NullDisplayName = new(TypeMenuUtility.k_NullDisplayName);

    readonly Dictionary<string, TypePopupCache> m_TypePopups = new();
    readonly Dictionary<string, GUIContent> m_TypeNameCaches = new();

    SerializedProperty m_TargetProperty;

    private static GUIContent Icon_Delete;
    private static GUIContent Icon_Edit;
    private static GUIContent Icon_Global;
    private static GUIContent Icon_Local;
    private static GUIContent Icon_InvertOff;
    private static GUIContent Icon_InvertOn;
    //private bool editName;

    private static GUIContent debugCont;

    public static GUIContent DebugCont
    {
        get
        {
            debugCont ??= new GUIContent(EditorGUIUtility.IconContent("d_debug"));
            return debugCont;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var indent = EditorGUI.indentLevel;
        // EditorGUI.indentLevel = -1;
        FindIcons();

        var isNull = property.managedReferenceValue == null;
        var invert = property.FindPropertyRelative("invert");
        var desc = property.FindPropertyRelative("desc");
        var Condition = property.FindPropertyRelative("Condition");
        var Target = property.FindPropertyRelative("Target");
        var LocalTarget = property.FindPropertyRelative("LocalTarget");
        var debug = property.FindPropertyRelative("debug");

        if (!isNull)
        {
            Type type = MSerializedTools.GetPropertyType2(property);
            var att = type.GetCustomAttribute<ConditionDescriptionAttribute>(false); //Find the correct name

            if (desc.stringValue != string.Empty)
            {
                label.text = $" {desc.stringValue}";
            }
            else if (att != null)
            {
                desc.stringValue = att.Description;
            }
            else if (Target != null)
            {
                if (Condition != null)
                    label.text = Condition.enumDisplayNames[Condition.enumValueIndex];
                var targetName = string.Empty;

                var targetType = MSerializedTools.GetPropertyType2(property);

                if (targetType.IsSubclassOf(typeof(UnityEngine.Object)) && Target.objectReferenceValue)
                {
                    targetName = Target.objectReferenceValue.name;
                }
                else
                {
                    targetName = Target.type;
                }

                targetName = targetName.RemoveSpecialCharacters();
                targetName = targetName.Replace("PPtr", "");

                label.text += $" on [{targetName}]";
            }

            if (desc.isExpanded) label.text = string.Empty;
            if (invert.boolValue) label.text = "[NOT] " + label.text;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        {
            var width = 32;
            var popupPosition = new Rect(position);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;

            if (!isNull)
            {
                var OrAnd = property.FindPropertyRelative("OrAnd");

                var dC = GUI.color;

                var first = position.x + popupPosition.width + EditorGUIUtility.labelWidth - width;

                #region Rects

                var Height = EditorGUIUtility.singleLineHeight - 2;




                var RemoveRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width,
                    x = first
                };

                var DebugRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width,
                    x = RemoveRect.x - width - 2
                };


                var EditRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width,
                    x = DebugRect.x - width - 2
                };

                //----------
                var LocalTargetRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width + 2,
                    x = EditRect.x - width - 15
                };

                var invRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width - 4,
                    x = LocalTargetRect.x - width + 3
                };

                var andorRect = new Rect(position)
                {
                    height = Height,
                    y = position.y,
                    width = width + 4,
                    x = invRect.x - width - 5
                };

                var TextRect = new Rect(position)
                {
                    height = Height + 3,
                    y = position.y,
                    width = position.width - 240,
                    x = 1 + (invert.boolValue ? position.x + 50 : position.x + 15)
                };

                var style = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fontStyle = FontStyle.Bold
                };
                #endregion

                if (desc.isExpanded)
                    desc.stringValue = EditorGUI.TextField(TextRect, desc.stringValue);

                if (Target != null) //Draw the Target Global/Local
                    LocalTarget.boolValue = GUI.Toggle(LocalTargetRect, LocalTarget.boolValue, LocalTarget.boolValue ? Icon_Local : Icon_Global, style);


                var orandContent = new GUIContent(OrAnd.boolValue ? "OR" : "AND",
                    OrAnd.boolValue ? "OR. First Condition will be ignored" : "AND. First Condition will be ignored");


                if (!property.propertyPath.EndsWith("data[0]")) //Do not  AND- OR if if this is the first condition on the list
                {
                    GUI.color = OrAnd.boolValue ? MTools.MBlue * 2 : MTools.MGreen * 2;
                    OrAnd.boolValue = GUI.Toggle(andorRect, OrAnd.boolValue, orandContent, style);
                    GUI.color = dC;
                }

                GUI.color = invert.boolValue ? Color.red : dC;
                invert.boolValue = GUI.Toggle(invRect, invert.boolValue, invert.boolValue ? Icon_InvertOn : Icon_InvertOff, style);
                GUI.color = dC;

                GUI.color = debug.boolValue ? Color.red + Color.white : dC;
                debug.boolValue = GUI.Toggle(DebugRect, debug.boolValue, DebugCont, style);
                GUI.color = dC;


                desc.isExpanded = GUI.Toggle(EditRect, desc.isExpanded, Icon_Edit, EditorStyles.miniButton);

                if (GUI.Button(RemoveRect, Icon_Delete, EditorStyles.miniButton))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            var propertyName = TypePopupCache.GetTypeName(property, m_TypeNameCaches);

            if (isNull)
            {
                var guicolor = GUI.color;
                GUI.color = MTools.MRed;

                if (EditorGUI.DropdownButton(popupPosition, propertyName, FocusType.Keyboard))
                {
                    TypePopupCache popup = GetTypePopup(property);
                    m_TargetProperty = property;
                    popup.TypePopup.Show(popupPosition);
                }

                GUI.color = guicolor;
            }

            // Draw the managed reference property.
            EditorGUI.PropertyField(position, property, label, true);
        }
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = indent;
    }

    private static void FindIcons()
    {
        if (Icon_Delete == null)
        {
            Icon_Delete = EditorGUIUtility.IconContent("winbtn_win_close@2x");
            Icon_Delete.tooltip = "Clear the Condition";
        }
        if (Icon_InvertOff == null)
        {
            Icon_InvertOff = EditorGUIUtility.IconContent("console.erroricon.inactive.sml@2x");
            Icon_InvertOff.tooltip = "Invert Condition. The result will be the opposite";
        }
        if (Icon_InvertOn == null)
        {
            Icon_InvertOn = EditorGUIUtility.IconContent("console.erroricon.sml@2x");
            Icon_InvertOn.tooltip = "Invert Condition. The result will be the opposite";
        }
        if (Icon_Global == null)
        {
            Icon_Global = EditorGUIUtility.IconContent("d_ToolHandleGlobal@2x");
            Icon_Global.tooltip = "Dynamic Target. The target for the condition will be found dynamically";
        }
        if (Icon_Local == null)
        {
            Icon_Local = EditorGUIUtility.IconContent("d_ToolHandleLocal@2x");
            Icon_Local.tooltip = "Local Target. The target for the condition will be set in the Editor and it will not be changed";
        }
        if (Icon_Edit == null)
        {
            Icon_Edit = MalbersEditor.Icon_Edit;
            Icon_Edit.tooltip = "Edit the Condition Description";
        }
    }

    TypePopupCache GetTypePopup(SerializedProperty property)
    {
        // Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
        string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

        if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename, out TypePopupCache result))
        {
            var state = new AdvancedDropdownState();

            Type baseType = MSerializedTools.GetType(managedReferenceFieldTypename);
            var popup = new AdvancedTypePopup(
                TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
                    (p.IsPublic || p.IsNestedPublic) &&
                    !p.IsAbstract &&
                    !p.IsGenericType &&
                    !k_UnityObjectType.IsAssignableFrom(p) &&
                    Attribute.IsDefined(p, typeof(SerializableAttribute))
                ),
                k_MaxTypePopupLineCount, state);

            popup.OnItemSelected += item =>
            {
                Type type = item.Type;
                object obj = m_TargetProperty.SetManagedReference(type);
                m_TargetProperty.isExpanded = (obj != null);
                m_TargetProperty.serializedObject.ApplyModifiedProperties();
                m_TargetProperty.serializedObject.Update();
            };

            result = new TypePopupCache(popup, state);
            m_TypePopups.Add(managedReferenceFieldTypename, result);
        }
        return result;
    }

    GUIContent GetTypeName(SerializedProperty property)
    {
        // Cache this string.
        string managedReferenceFullTypename = property.managedReferenceFullTypename;

        if (string.IsNullOrEmpty(managedReferenceFullTypename))
        {
            return k_NullDisplayName;
        }
        if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out GUIContent cachedTypeName))
        {
            return cachedTypeName;
        }

        Type type = MSerializedTools.GetType(managedReferenceFullTypename);
        string typeName = null;

        AddTypeMenuAttribute typeMenu = TypeMenuUtility.GetAttribute(type);
        if (typeMenu != null)
        {
            typeName = typeMenu.GetTypeNameWithoutPath();
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                typeName = ObjectNames.NicifyVariableName(typeName);
            }
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            typeName = ObjectNames.NicifyVariableName(type.Name);
        }

        GUIContent result = new(typeName);
        m_TypeNameCaches.Add(managedReferenceFullTypename, result);
        return result;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}
#endif

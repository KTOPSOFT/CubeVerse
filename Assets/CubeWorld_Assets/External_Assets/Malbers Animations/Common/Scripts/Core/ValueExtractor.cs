using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public enum ValueExtractorType { Property, Variable, Method }

    [System.Serializable]
    public struct ValueExtractor<T1, T2> where T1 : UnityEngine.Object
    {
        public T1 Target;
        public ValueExtractorType valueType;
        public string Property;

        /// <summary> Drawer value for detecting the correct Object</summary>
        [SerializeField] private int index;

        private MethodInfo method;
        private PropertyInfo property;
        private FieldInfo variable;

        public T2 GetValue()
        {
            if (Target == null) return default;

            switch (valueType)
            {
                case ValueExtractorType.Property:
                    if (property == null) property = Target.GetProperty<T2>(Property); //Cache the property
                    return property != null ? (T2)property.GetValue(Target, null) : default;
                case ValueExtractorType.Variable:
                    if (variable == null) variable = Target.GetField<T2>(Property); //Cache the variable
                    return variable != null ? (T2)variable.GetValue(Target) : default;
                case ValueExtractorType.Method:
                    if (method == null) method = Target.GetMethod<T2>(Property); //Cache the method
                    return method != null ? (T2)method.Invoke(Target, null) : default;
                default:
                    return default;
            }
        }

        public Type TargetType()
        {
            if (Target == null) return null;
            return Target.GetType();
        }

        public T2 Value => GetValue();
        public void SetTarget(T1 target) => Target = target;

    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ValueExtractor<,>))]
    public class ValueExtractorDrawer : PropertyDrawer
    {
        private List<string> all = new();
        private List<string> allContent = new();
        private List<ValueExtractorType> allType = new();

        // private int index;
        //private bool FoundList = false;

        private System.Type currentTargetType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var Target = property.FindPropertyRelative("Target");
            var valueType = property.FindPropertyRelative("valueType");
            var Property = property.FindPropertyRelative("Property");
            var index = property.FindPropertyRelative("index");

            var div = position.width / 2;
            var height = EditorGUIUtility.singleLineHeight;

            var TargetRect = new Rect(position.x, position.y, div * 1.2f - 3, height);
            var PropertyRect = new Rect(position.x + (div * 1.2f), position.y, (div * 0.8f), height);

            var typ = MSerializedTools.GetPropertyType2(property);
            var GenericTyp = typ.GenericTypeArguments[1]; //Find the generic type of the ValueExtractor

            var ObjectType = typ.GenericTypeArguments[0]; //Find the generic type of the ValueExtractor

            FindMembers(Target, GenericTyp, ObjectType);

            string wantedTypeName = GenericTyp.Name switch
            {
                "Int32" => "int",
                "Single" => "float",
                "Boolean" => "bool",
                "String" => "string",
                _ => GenericTyp.Name,
            };

            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 120;
            EditorGUI.PropertyField(TargetRect, Target, new GUIContent($"Target ({wantedTypeName})")); //draw the target   
            EditorGUIUtility.labelWidth = 0;

            if (EditorGUI.EndChangeCheck())
            {
                Property.stringValue = string.Empty;
                valueType.intValue = 0;
                index.intValue = 0;
            }

            if (all.Count == 0)
            {
                index.intValue = EditorGUI.Popup(PropertyRect, index.intValue, new string[1] { "<None>" });
            }
            else
            {
                index.intValue = Mathf.Clamp(index.intValue, 0, all.Count);
                index.intValue = EditorGUI.Popup(PropertyRect, index.intValue, allContent.ToArray());

                Property.stringValue = all[index.intValue];
                valueType.intValue = (int)allType[index.intValue];
            }

            //  EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();

            void FindMembers(SerializedProperty Target, System.Type GenericTyp, System.Type ObjectType)
            {
                if (Target.objectReferenceValue != null && currentTargetType != Target.objectReferenceValue.GetType())
                {
                    currentTargetType = Target.objectReferenceValue.GetType();
                    GetMembers(currentTargetType, GenericTyp);
                }
                else if (Target.objectReferenceValue == null && currentTargetType != ObjectType)
                {
                    currentTargetType = ObjectType;
                    GetMembers(currentTargetType, GenericTyp);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
            //  return EditorGUI.GetPropertyHeight(property, true);
        }

        public void GetMembers(System.Type targetType, System.Type wantedType)
        {
            all = new();
            allContent = new();
            allType = new();

            // Get all public properties
            PropertyInfo[] propertiesInfo = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo property in propertiesInfo)
            {
                if (property.GetMethod != null && property.PropertyType == wantedType)
                {
                    all.Add(property.Name);
                    allContent.Add($"{property.Name}");
                    allType.Add(ValueExtractorType.Property);
                }
            }

            // Get all public fields (int variables)
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == wantedType)
                {
                    all.Add(field.Name);
                    allContent.Add($"{field.Name}");
                    allType.Add(ValueExtractorType.Variable);
                }
            }

            // Get all public methods
            MethodInfo[] methodsInfo = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methodsInfo)
            {
                if (method.ReturnType == wantedType && !method.Name.StartsWith("get_") && method.GetParameters().Length == 0)
                {
                    all.Add(method.Name);
                    allContent.Add($"{method.Name}()");
                    allType.Add(ValueExtractorType.Method);
                }
            }
        }
    }
#endif
}
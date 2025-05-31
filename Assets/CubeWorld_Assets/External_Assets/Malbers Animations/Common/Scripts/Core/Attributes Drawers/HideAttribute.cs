using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]

    //[AttributeUsage(AttributeTargets.All  ,  AllowMultiple = true, Inherited = true)]
    public sealed class HideAttribute : PropertyAttribute
    {
        public string Variable = "";
        public bool inverse = false;
        public bool hide = true;
        public int[] EnumValue;
        public bool flag = false;


        public HideAttribute(string conditionalSourceField)
        {
            this.Variable = conditionalSourceField;
            this.inverse = false;
            this.hide = true;
            flag = false;
        }

        public HideAttribute(string conditionalSourceField, bool inverse)
        {
            this.Variable = conditionalSourceField;
            this.inverse = inverse;
            this.hide = true;
            flag = false;
        }

        public HideAttribute(string conditionalSourceField, bool inverse, bool hide)
        {
            this.Variable = conditionalSourceField;
            this.inverse = inverse;
            this.hide = hide;
            flag = false;
        }

        public HideAttribute(string conditionalSourceField, bool inverse, params int[] EnumValue)
        {
            this.Variable = conditionalSourceField;
            this.inverse = inverse;
            this.EnumValue = EnumValue;
            this.hide = true;
            flag = false;
        }

        public HideAttribute(string conditionalSourceField, bool inverse, bool hide, params int[] EnumValue)
        {
            this.Variable = conditionalSourceField;
            this.inverse = inverse;
            this.EnumValue = EnumValue;
            this.hide = hide;
        }

        public HideAttribute(string conditionalSourceField, params int[] EnumValue)
        {
            this.Variable = conditionalSourceField;
            this.inverse = false;
            this.EnumValue = EnumValue;
            this.hide = true;
            flag = false;
        }

        public HideAttribute(string conditionalSourceField, bool inverse, bool hide, bool flag, params int[] EnumValue)
        {
            this.Variable = conditionalSourceField;
            this.inverse = inverse;
            this.EnumValue = EnumValue;
            this.hide = hide;
            this.flag = flag;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HideAttribute))]
    public class HidePropertyDrawer : PropertyDrawer
    {
        private bool enabled;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HideAttribute condHAtt = (HideAttribute)attribute;

            enabled = GetConditionalHideAttributeResult(condHAtt, property);
            //  CachePropertyDrawer(property);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;

            if (!condHAtt.hide || enabled)
            {

                // if (!CustomDrawerUsed())
                EditorGUI.PropertyField(position, property, label, true);

                // EditorGUI.PropertyField(position, property, label, true);

                //bool CustomDrawerUsed()
                //{
                //    if (_customPropertyDrawer == null) return false;

                //    try
                //    {
                //        _customPropertyDrawer.OnGUI(position, property, label);
                //        return true;
                //    }
                //    catch (Exception e)
                //    {
                //        //WarningsPool.LogWarning(property,
                //        //    "Unable to use CustomDrawer of type " + _customPropertyDrawer.GetType() + ": " + e,
                //        //    property.serializedObject.targetObject);

                //        return false;
                //    }
                //}
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HideAttribute condHAtt = (HideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            if (enabled || !condHAtt.hide)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(HideAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;

            //Handle primary property
            SerializedProperty sourcePropertyValue;

            //Get the full relative property path of the sourcefield so we can have nested hiding.Use old method when dealing with arrays
            //  if (!property.isArray)
            {
                //returns the property path of the property we want to apply the attribute to
                string propertyPath = property.propertyPath;

                //changes the path to the conditionalsource property path
                string conditionPath = propertyPath.Replace(property.name, condHAtt.Variable);

                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

                //if the find failed->fall back to the old system
                if (sourcePropertyValue == null)
                {
                    //original implementation (doens't work with nested serializedObjects)
                    sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.Variable);
                }
            }
            //else
            //{
            //    //original implementation (doens't work with nested serializedObjects)
            //    sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.Variable);
            //}


            if (sourcePropertyValue != null)
            {
                enabled = CheckPropertyType(sourcePropertyValue, condHAtt);
            }

            //wrap it all up
            if (condHAtt.inverse) enabled = !enabled;
            return enabled;
        }

        private bool CheckPropertyType(SerializedProperty sourcePropertyValue, HideAttribute condHAtt)
        {
            //Note: add others for custom handling if desired
            switch (sourcePropertyValue.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return sourcePropertyValue.boolValue;
                case SerializedPropertyType.Float:
                    return sourcePropertyValue.floatValue == 0;
                case SerializedPropertyType.Integer:
                    return sourcePropertyValue.intValue == 0;
                case SerializedPropertyType.ObjectReference:
                    return sourcePropertyValue.objectReferenceValue != null;
                case SerializedPropertyType.ManagedReference:
                    return sourcePropertyValue.objectReferenceValue != null;
                case SerializedPropertyType.ArraySize:
                    return sourcePropertyValue.arraySize == 0;
                case SerializedPropertyType.Enum:
                    if (!condHAtt.flag)
                    {
                        for (int i = 0; i < condHAtt.EnumValue.Length; i++)
                        {
                            if (sourcePropertyValue.enumValueIndex == condHAtt.EnumValue[i])
                                return true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < condHAtt.EnumValue.Length; i++)
                        {
                            if ((sourcePropertyValue.intValue & condHAtt.EnumValue[i]) == condHAtt.EnumValue[i])
                                return true;
                        }
                    }
                    return false;
                default:
                    Debug.LogError("Data type of the property used for conditional hiding [" + sourcePropertyValue.propertyType + "] is currently not supported");
                    return true;
            }
        }

        //      private bool _initialized;
        //      private PropertyDrawer _customPropertyDrawer;

        //      /// <summary>
        ///// Try to find and cache any PropertyDrawer or PropertyAttribute on the field
        ///// </summary>
        //private void CachePropertyDrawer(SerializedProperty property)
        //      {
        //          if (_initialized) return;
        //          _initialized = true;
        //          if (fieldInfo == null) return;

        //          var customDrawer = CustomDrawerUtility.GetPropertyDrawerForProperty(property, fieldInfo, attribute);
        //          if (customDrawer == null) customDrawer = TryCreateAttributeDrawer();

        //          _customPropertyDrawer = customDrawer;


        //          // Try to get drawer for any other Attribute on the field
        //          PropertyDrawer TryCreateAttributeDrawer()
        //          {
        //              var secondAttribute = TryGetSecondAttribute();
        //              if (secondAttribute == null) return null;

        //              var attributeType = secondAttribute.GetType();
        //              var customDrawerType = CustomDrawerUtility.GetPropertyDrawerTypeForFieldType(attributeType);
        //              if (customDrawerType == null) return null;

        //              return CustomDrawerUtility.InstantiatePropertyDrawer(customDrawerType, fieldInfo, secondAttribute);


        //              //Get second attribute if any
        //              Attribute TryGetSecondAttribute()
        //              {
        //                  return (PropertyAttribute)fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), false)
        //                      .FirstOrDefault(a => !(a is HideAttribute));
        //              }
        //          }
        //      }
    }


    //public static class CustomDrawerUtility
    //{
    //    /// <summary>
    //    /// Key is Associated with drawer type (the T in [CustomPropertyDrawer(typeof(T))])
    //    /// Value is PropertyDrawer Type
    //    /// </summary>
    //    private static readonly Dictionary<Type, Type> PropertyDrawersInAssembly = new Dictionary<Type, Type>();
    //    private static readonly Dictionary<int, PropertyDrawer> PropertyDrawersCache = new Dictionary<int, PropertyDrawer>();
    //    private static readonly string IgnoreScope = typeof(int).Module.ScopeName;

    //    /// <summary>
    //    /// Create PropertyDrawer for specified property if any PropertyDrawerType for such property is found.
    //    /// FieldInfo and Attribute will be inserted in created drawer.
    //    /// </summary>
    //    public static PropertyDrawer GetPropertyDrawerForProperty(SerializedProperty property, FieldInfo fieldInfo, Attribute attribute)
    //    {
    //        var propertyId = (int) property.GetHashCode();
    //        if (PropertyDrawersCache.TryGetValue(propertyId, out var drawer))
    //        {
    //            //Debug.Log("Tryget");
    //            return drawer; 
    //        }

    //        var targetType = fieldInfo.FieldType;
    //        var drawerType = GetPropertyDrawerTypeForFieldType(targetType);


    //        if (drawerType != null)
    //        {
    //           // Debug.Log("sasdasd");
    //            drawer = InstantiatePropertyDrawer(drawerType, fieldInfo, attribute);

    //            //if (drawer == null)
    //            //    WarningsPool.LogWarning(property,
    //            //        $"Unable to instantiate CustomDrawer of type {drawerType} for {fieldInfo.FieldType}",
    //            //        property.serializedObject.targetObject);
    //        }

    //        PropertyDrawersCache[propertyId] = drawer;
    //        return drawer;
    //    }

    //    public static PropertyDrawer InstantiatePropertyDrawer(Type drawerType, FieldInfo fieldInfo, Attribute insertAttribute)
    //    {
    //        try
    //        {
    //            var drawerInstance = (PropertyDrawer)Activator.CreateInstance(drawerType);

    //            // Reassign the attribute and fieldInfo fields in the drawer so it can access the argument values
    //            var fieldInfoField = drawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
    //            if (fieldInfoField != null) fieldInfoField.SetValue(drawerInstance, fieldInfo);
    //            var attributeField = drawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
    //            if (attributeField != null) attributeField.SetValue(drawerInstance, insertAttribute);
    //            Debug.Log("drawerInstance " + drawerInstance.GetType().Name);
    //            return drawerInstance;
    //        }
    //        catch (Exception)
    //        {
    //            return null;
    //        }
    //    }

    //    /// <summary>
    //    /// Try to get PropertyDrawer for a target Type, or any Base Type for a target Type
    //    /// </summary>
    //    public static Type GetPropertyDrawerTypeForFieldType(Type drawerTarget)
    //    {
    //        // Ignore .net types from mscorlib.dll
    //        if (drawerTarget.Module.ScopeName.Equals(IgnoreScope))
    //        {
    //           // Debug.Log("DRAWERTARGETNULL");
    //            return null; 
    //        }
    //        CacheDrawersInAssembly();

    //        // Of all property drawers in the assembly we need to find one that affects target type
    //        // or one of the base types of target type
    //        var checkType = drawerTarget;
    //        while (checkType != null)
    //        {
    //            if (PropertyDrawersInAssembly.TryGetValue(drawerTarget, out var drawer))
    //            {
    //                Debug.Log($"Drawer {drawer.FullName}");
    //                return drawer; 
    //            }
    //            checkType = checkType.BaseType;
    //        }

    //        return null;
    //    }

    //    private static Type[] GetTypesSafe(Assembly assembly)
    //    {
    //        try
    //        {
    //            return assembly.GetTypes();
    //        }
    //        catch (ReflectionTypeLoadException e)
    //        {
    //            return e.Types;
    //        }
    //    }

    //    private static void CacheDrawersInAssembly()
    //    {
    //        if (PropertyDrawersInAssembly == null || PropertyDrawersInAssembly.Count == 0) return;

    //        var propertyDrawerType = typeof(PropertyDrawer);
    //        var allDrawerTypesInDomain = AppDomain.CurrentDomain.GetAssemblies()
    //            .SelectMany(GetTypesSafe)
    //            .Where(t => t != null && propertyDrawerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

    //        foreach (var drawerType in allDrawerTypesInDomain)
    //        {
    //            var propertyDrawerAttribute = CustomAttributeData.GetCustomAttributes(drawerType).FirstOrDefault();
    //            if (propertyDrawerAttribute == null) continue;
    //            var drawerTargetType = propertyDrawerAttribute.ConstructorArguments.FirstOrDefault().Value as Type;
    //            if (drawerTargetType == null) continue;

    //            if (PropertyDrawersInAssembly.ContainsKey(drawerTargetType)) continue;
    //            PropertyDrawersInAssembly.Add(drawerTargetType, drawerType);
    //        }
    //    }
    //}
#endif
}
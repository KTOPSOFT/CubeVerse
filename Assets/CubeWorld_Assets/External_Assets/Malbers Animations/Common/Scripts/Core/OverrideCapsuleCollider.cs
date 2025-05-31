using UnityEngine;
using MalbersAnimations.Scriptables;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations
{
    public enum CapsuleModifier
    {
        enabled = 1 << 0,
        isTrigger = 1 << 1,
        center = 1 << 2,
        height = 1 << 3,
        radius = 1 << 4,
        direction = 1 << 5,
        material = 1 << 6,
    }

    [System.Serializable]
    public class OverrideCapsuleColliderReference : ReferenceVar
    {
        public OverrideCapsuleCollider ConstantValue;
        [RequiredField] public CapsuleColliderPreset Variable;

        public OverrideCapsuleColliderReference() => Value = new OverrideCapsuleCollider();

        public OverrideCapsuleColliderReference(CapsuleColliderPreset value) => Value = value.modifier;

        public OverrideCapsuleCollider Value
        {
            get => UseConstant || Variable == null ? ConstantValue : Variable.modifier;
            set
            {
                if (UseConstant || Variable == null)
                    ConstantValue = value;
                else
                    Variable.modifier = value;
            }
        }

        public void Modify(CapsuleCollider collider) => Value.Modify(collider);
    }



    [System.Serializable]
    public struct OverrideCapsuleCollider
    {
        public string name;
        public bool enabled;
        public bool isTrigger;
        public Vector3 center;
        [Min(0)] public float height;
        [Tooltip("[0: XAxis] [1:Y Axis] [2:Z Axis]")]
        [Min(0)] public int direction;
        [Min(0)] public float radius;
        public PhysicsMaterial material;

        [Utilities.Flag]
        public CapsuleModifier modify;

        public readonly bool IsNull => modify == 0;

        public OverrideCapsuleCollider(CapsuleCollider collider)
        {
            name = "Preset";
            enabled = collider.enabled;
            isTrigger = collider.isTrigger;
            center = collider.center;
            height = collider.height;
            radius = collider.radius;
            direction = collider.direction;
            material = collider.sharedMaterial;
            modify = (CapsuleModifier)(-1);
        }

        public void Modify(CapsuleCollider collider)
        {
            if ((int)modify == 0 || collider == null) return; //Means that the animal have no modification

            if (Modify(CapsuleModifier.enabled)) collider.enabled = enabled;
            if (Modify(CapsuleModifier.isTrigger)) collider.isTrigger = isTrigger;
            if (Modify(CapsuleModifier.center)) collider.center = center;
            if (Modify(CapsuleModifier.height)) collider.height = height;
            if (Modify(CapsuleModifier.radius)) collider.radius = radius;
            if (Modify(CapsuleModifier.direction)) collider.direction = direction;
            if (Modify(CapsuleModifier.material)) collider.material = material;
        }
        public bool Modify(CapsuleModifier modifier) => (modify & modifier) == modifier;

        public static bool Modify(int modify, CapsuleModifier modifier) => (modify & (int)modifier) == (int)modifier;
    }



#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(OverrideCapsuleColliderReference), true)]
    public class OverrideCapsuleColliderReferenceDrawer : PropertyDrawer
    {
        /// <summary>  Options to display in the popup to select constant or variable. </summary>
        private readonly string[] popupOptions = { "Use Local", "Use Global" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;
        private GUIContent plus;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            popupStyle ??= new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };
            plus ??= EditorGUIUtility.IconContent("d_Toolbar Plus");

            label = EditorGUI.BeginProperty(position, label, property);
            {
                Rect variableRect = new Rect(position);
                position = EditorGUI.PrefixLabel(position, label);


                float height = EditorGUIUtility.singleLineHeight;

                // Get properties
                SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");
                SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
                SerializedProperty variable = property.FindPropertyRelative("Variable");

                Rect propRect = new Rect(position) { height = height };

                // Calculate rect for configuration button
                Rect buttonRect = new Rect(position);
                buttonRect.yMin += popupStyle.margin.top;
                buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
                buttonRect.x -= 20;
                buttonRect.height = height;

                position.xMin = buttonRect.xMax;


                var AddButtonRect = new Rect(propRect) { x = propRect.width + propRect.x - 18, width = 20 };
                var ValueRect = new Rect(AddButtonRect);

                // Store old indent level and set it to 0, the PrefixLabel takes care of it
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);
                useConstant.boolValue = (result == 0);

                bool varIsEmpty = variable.objectReferenceValue == null;

                if (!useConstant.boolValue && varIsEmpty)
                {
                    propRect.width -= 20;
                }

                EditorGUI.PropertyField(propRect, useConstant.boolValue ? constantValue : variable, GUIContent.none, true);

                if (!useConstant.boolValue)
                {
                    if (varIsEmpty)
                    {
                        if (GUI.Button(AddButtonRect, plus, UnityEditor.EditorStyles.helpBox))
                        {
                            MTools.CreateScriptableAsset(variable, MalbersEditor.GetSelectedPathOrFallback());
#if UNITY_2020_1_OR_NEWER
                            GUIUtility.ExitGUI(); //Unity Bug!
#endif
                        }
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }


        public bool Modify(int modify, CapsuleModifier modifier) => ((modify & (int)modifier) == (int)modifier);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");

            if (!useConstant.boolValue) return base.GetPropertyHeight(property, label);

            int activeProperties = 0;

            var constant = property.FindPropertyRelative("ConstantValue");
            var modify = constant.FindPropertyRelative("modify");
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, CapsuleModifier.enabled)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.center)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.height)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.radius)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.direction)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.isTrigger)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.material)) activeProperties++;

            if (activeProperties == 0) return base.GetPropertyHeight(property, label);

            float lines = (activeProperties + 1);
            return 20 * lines;
        }
    }

    [CustomPropertyDrawer(typeof(OverrideCapsuleCollider))]
    public class OverrideCapsuleColliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            position.x += 2;
            position.width -= 2;

            position.y += 2;
            position.height -= 2;


            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;

            #region Serialized Properties
            var modify = property.FindPropertyRelative("modify");
            var enabled = property.FindPropertyRelative("enabled");
            var isTrigger = property.FindPropertyRelative("isTrigger");
            var radius = property.FindPropertyRelative("radius");
            var center = property.FindPropertyRelative("center");
            var height1 = property.FindPropertyRelative("height");
            var direction = property.FindPropertyRelative("direction");
            var material = property.FindPropertyRelative("material");
            #endregion

            var line = position;
            var lineLabel = line;
            line.height = height;

            var foldout = lineLabel;
            foldout.width = 10;
            foldout.x += 10;


            modify.intValue = (int)(CapsuleModifier)EditorGUI.EnumFlagsField(line, label, (CapsuleModifier)(modify.intValue));

            line.y += height + 2;

            int ModifyValue = modify.intValue;

            EditorGUIUtility.labelWidth = 80;
            if (Modify(ModifyValue, CapsuleModifier.enabled))
                DrawProperty(ref line, enabled);

            if (Modify(ModifyValue, CapsuleModifier.isTrigger))
                DrawProperty(ref line, isTrigger);

            if (Modify(ModifyValue, CapsuleModifier.material))
                DrawProperty(ref line, material);

            if (Modify(ModifyValue, CapsuleModifier.center))
                DrawProperty(ref line, center);

            if (Modify(ModifyValue, CapsuleModifier.radius))
                DrawProperty(ref line, radius);

            if (Modify(ModifyValue, CapsuleModifier.height))
                DrawProperty(ref line, height1);

            if (Modify(ModifyValue, CapsuleModifier.direction))
                DrawProperty(ref line, direction);
            EditorGUIUtility.labelWidth = 0;


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private void DrawProperty(ref Rect rect, SerializedProperty property)
        {
            EditorGUI.PropertyField(rect, property);
            rect.y += EditorGUIUtility.singleLineHeight + 2;
        }


        private bool Modify(int modify, CapsuleModifier modifier)
        {
            return ((modify & (int)modifier) == (int)modifier);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int activeProperties = 0;
            float additiveHeight = 0;
            var modify = property.FindPropertyRelative("modify");
            int ModifyValue = modify.intValue;



            if (Modify(ModifyValue, CapsuleModifier.enabled)) activeProperties++;

            if (Modify(ModifyValue, CapsuleModifier.center))
            {
                var center = property.FindPropertyRelative("center");
                additiveHeight += EditorGUI.GetPropertyHeight(center);
            }
            if (Modify(ModifyValue, CapsuleModifier.height)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.radius)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.direction)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.isTrigger)) activeProperties++;
            if (Modify(ModifyValue, CapsuleModifier.material)) activeProperties++;
            float lines = (int)(activeProperties + 2);
            return additiveHeight + base.GetPropertyHeight(property, label) * lines;// + (1 * lines);
        }
    }
#endif
}
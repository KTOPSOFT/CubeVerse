using UnityEngine;

namespace MalbersAnimations
{
    public class RequiredFieldAttribute : PropertyAttribute
    {
        public Color color;

        public RequiredFieldAttribute(FieldColor Fieldcolor = FieldColor.Red)
        {
            color = Fieldcolor switch
            {
                FieldColor.Red => Color.red + Color.yellow,
                FieldColor.Green => Color.green,
                FieldColor.Blue => Color.blue,
                FieldColor.Magenta => Color.magenta,
                FieldColor.Cyan => Color.cyan,
                FieldColor.Yellow => Color.yellow,
                FieldColor.Orange => new Color(1, 0.5f, 0),
                FieldColor.Gray => Color.gray,
                _ => Color.red,
            };
        }

        public RequiredFieldAttribute()
        {
            color = new Color(1, 0.4f, 0.4f, 1);
        }
    }

#if UNITY_EDITOR
    /// <summary>  Required Field Property Drawer from https://twitter.com/Rodrigo_Devora/status/1204031607583264769 Thanks for sharing! </summary>
    [UnityEditor.CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            RequiredFieldAttribute rf = attribute as RequiredFieldAttribute;
            if (property == null) return;

            if (property.objectReferenceValue == null)
            {
                var oldColor = GUI.color;

                GUI.color = new Color(1, 0.3f, 0);
                UnityEditor.EditorGUI.PropertyField(position, property, label);
                GUI.color = oldColor;
            }
            else
            {
                UnityEditor.EditorGUI.PropertyField(position, property, label);
            }
        }

        // Here!! Add me :))
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }
    }
#endif
}
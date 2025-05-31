using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    [System.Serializable]
    public class IDEnable<T> where T : IDs
    {
        public T ID;
        public bool enable = true;
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IDEnable<>), true)]
    public class IDEnableDrawer : IDDrawer
    {
        protected override void DrawProperty(Rect newPos, SerializedProperty property)
        {
            var IDRect = new Rect(newPos);
            IDRect.width -= 25;

            var toogleRect = new Rect(newPos);
            toogleRect.x = IDRect.x + IDRect.width + 5;
            toogleRect.width = 20;

            var ID = property.FindPropertyRelative("ID");
            var enable = property.FindPropertyRelative("enable");

            EditorGUI.PropertyField(IDRect, ID, GUIContent.none, false);
            EditorGUI.PropertyField(toogleRect, enable, GUIContent.none, false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            popupStyle ??= new(GUI.skin.GetStyle("PaneOptions"))
            {
                imagePosition = ImagePosition.ImageOnly
            };

            label = EditorGUI.BeginProperty(position, label, property);

            if (label.text.Contains("Element"))
            {
                position.x += 12;
                position.width -= 12;
            }
            else
                position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            float height = EditorGUIUtility.singleLineHeight;


            // Calculate rect for configuration button
            Rect buttonRect = new(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (EditorGUI.DropdownButton(buttonRect, GUIContent.none, FocusType.Passive, popupStyle))
            {
                var ID = property.FindPropertyRelative("ID");
                FindAllInstances(ID);  //Find the instances only when the dropdown is pressed
                menu.DropDown(buttonRect);
            }

            position.height = EditorGUIUtility.singleLineHeight;

            DrawProperty(position, property);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}

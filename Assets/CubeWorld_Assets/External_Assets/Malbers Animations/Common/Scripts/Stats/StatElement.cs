using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    /// <summary> This is created to add Elements attacks to the weapons  </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Element", fileName = "New Element ID", order = -1000)]
    public class StatElement : IDs
    {
        /// <summary>
        /// Base color for the Stat Element. This is used on the UI Floating Numbers to show the color of the element 
        /// </summary>
        public Color color = Color.white;
        //[Tooltip("Interaction list with other Elements")]
        //public List<ElementMultiplier> Interactions = new List<ElementMultiplier>();

        //protected override void OnValidate()
        //{
        //    base.OnValidate();

        //    foreach (var item in Interactions)
        //    {
        //        if (item.element != null)
        //        {
        //            item.displayName = item.element.name;

        //            if (item.multiplier.Value != 1)
        //                item.displayName = $"{(item.multiplier.Value < 1 ? "Weak" : "Strong")} versus [{item.element.name}] - [{item.multiplier.Value:F2}]";
        //        }
        //    }
        //}


        #region CalculateID
#if UNITY_EDITOR
        private void Reset() => GetID();

        [UnityEngine.ContextMenu("Get ID")]
        private void GetID() => FindID<StatElement>();
#endif
        #endregion 
    }

    [System.Serializable]
    public struct ElementMultiplier
    {
        [Tooltip("ID of the Element")]
        public StatElement element;

        [Tooltip("Multiplier applied when interacting with other elements." +
            "\nGreater than 1 means is weak agains this element." +
            "\nLess than one means is resistant to this element")]
        public FloatReference multiplier;


        //[Tooltip("Critical chance the Element gets to activate itself, or you can also use it to increase the damage it can do ")]
        //public FloatReference criticalChance = new FloatReference(1);

        public ElementMultiplier(StatElement element)
        {
            this.element = element;
            multiplier = new FloatReference(2);


        }
        public ElementMultiplier(StatElement element, float value)
        {
            this.element = element;
            multiplier = new FloatReference(value);
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ElementMultiplier))]
    public class MPivotDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var element = property.FindPropertyRelative("element");
            var multiplier = property.FindPropertyRelative("multiplier");

            var indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;
            var height = EditorGUIUtility.singleLineHeight;

            var line = new Rect(position);
            line.height = height;

            var RectElement = new Rect(line.x, line.y, line.width / 2, height);
            var RectMultiplier = new Rect(line.x + line.width / 2 + 5, line.y, line.width / 2 - 5, height);

            EditorGUI.PropertyField(RectElement, element);
            EditorGUIUtility.labelWidth = 30;
            EditorGUI.PropertyField(RectMultiplier, multiplier, new GUIContent("M", "Multiplier to affect the Element"));
            EditorGUIUtility.labelWidth = 0;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}
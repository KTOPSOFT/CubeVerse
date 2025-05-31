using UnityEngine;
using System.Collections.Generic;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations
{
    public abstract class IDs : ScriptableObject
    {
        [Tooltip("Display name on the ID Selection Context Button")]
        public string DisplayName;

        [Tooltip("Integer value to Identify IDs")]
        public int ID;

        public static implicit operator int(IDs reference) => reference != null ? reference.ID : 0; //  =>  reference.ID;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = name;
        }

        protected void FindID<T>() where T : IDs
        {
            int newID = 0;
            var allAdd = MTools.GetAllInstances<T>();

            bool Found = true;

            while (Found)
            {
                newID++;
                Found = allAdd.Exists(x => (x.ID == newID && x != this));
            }
            ID = newID;
            DisplayName = name;
            MTools.SetDirty(this);
        }

        /// <summary> Returns if an ID is inside a list (Include or Exclude) </summary>
        /// <param name="list">the list to verify </param>
        /// <param name="include">true: check if is included. false check if is excluded</param>
        /// <returns></returns>
        public bool Included<T>(List<T> list, bool include) where T : IDs
        {
            bool isIncluded = list.Contains(this);
            return include ? isIncluded : !isIncluded;
        }

        public bool Included(List<IDs> list) => Included(list, true);
        public bool Excluded(List<IDs> list) => Included(list, false);


#if UNITY_EDITOR 
        [ContextMenu("Get ID <Hash>")]
        private void GetIDHash()
        {
            ID = Animator.StringToHash(name);
            MTools.SetDirty(this);
        }
#endif
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IDs), true)]
    public class IDDrawer : PropertyDrawer
    {
        /// <summary> Cached style to use to draw the popup button. </summary>
        protected GUIStyle popupStyle;

        protected List<IDs> Instances;
        protected GenericMenu menu;

        //  readonly Dictionary<string, GUIContent> m_TypeNameCaches = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            popupStyle ??= new(GUI.skin.GetStyle("PaneOptions"))
            {
                imagePosition = ImagePosition.ImageOnly
            };

            label = EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue)
            {
                label.tooltip += $"\n ID Value: [{(property.objectReferenceValue as IDs).ID}]";
            }

            if (label.text.Contains("Element"))
            {
                position.x += 12;
                position.width -= 12;
            }
            else
                position = EditorGUI.PrefixLabel(position, label);

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rect for configuration button
            Rect buttonRect = new(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = EditorGUIUtility.singleLineHeight;

            //position.xMin = buttonRect.xMax;

            if (EditorGUI.DropdownButton(buttonRect, GUIContent.none, FocusType.Passive, popupStyle))
            {
                FindAllInstances(property);  //Find the instances only when the dropdown is pressed
                menu.DropDown(buttonRect);
            }

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, property, GUIContent.none, false);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }


        protected virtual void DrawProperty(Rect newPos, SerializedProperty property)
        {
            EditorGUI.PropertyField(newPos, property, GUIContent.none, false);
        }
        protected void SetPropertyValue(SerializedProperty property, IDs value)
        {
            property.objectReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        protected void FindAllInstances(SerializedProperty property)
        {
            if (Instances == null || Instances.Count == 0)
            {
                var NameOfType = GetPropertyType(property);
                string[] guids = AssetDatabase.FindAssets("t:" + NameOfType);  //FindAssets uses tags check documentation for more info

                Instances = new();


                for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var inst = AssetDatabase.LoadAssetAtPath<IDs>(path);
                    Instances.Add(inst);
                }

                Instances = Instances.OrderBy(x => x.ID).ToList(); //Order by ID


            }
            //Weird bug taht was not updating the IDS correctly on an array
            menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), false, () => SetPropertyValue(property, null));

            for (int i = 0; i < Instances.Count; i++)         //probably could get optimized 
            {
                var inst = Instances[i];
                var displayname = inst.name;
                var idString = "[" + Instances[i].ID.ToString() + "] ";

                if (Instances[i] is Tag) idString = ""; //Do not show On tag 

                if (!string.IsNullOrEmpty(inst.DisplayName))
                {
                    displayname = inst.DisplayName;
                    int pos = displayname.LastIndexOf("/") + 1;
                    displayname = displayname.Insert(pos, idString);
                }
                else
                {
                    displayname = idString + displayname;
                }

                menu.AddItem(new GUIContent(displayname), false, () => SetPropertyValue(property, inst));
            }

        }
        protected static string GetPropertyType(SerializedProperty property)
        {
            var type = property.type;
            var match = System.Text.RegularExpressions.Regex.Match(type, @"PPtr<\$(.*?)>");
            if (match.Success)
                type = match.Groups[1].Value;
            return type;
        }
    }
#endif
}
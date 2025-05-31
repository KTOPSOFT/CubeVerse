using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public abstract class ListBase
    {
        private static GUIStyle defaultHeaderStyle;
        protected static GUIStyle DefaultHeaderStyle
        {
            get
            {
                if (defaultHeaderStyle == null)
                {
                    defaultHeaderStyle = new GUIStyle(EditorStyles.helpBox) {fontSize = 14, alignment = TextAnchor.MiddleCenter};
                }

                return defaultHeaderStyle;
            }
        }
        
        private readonly string listId = Guid.NewGuid().ToString();

        protected abstract ReorderableList ReorderableList { get; }

        protected void SetNextListItemControlName(int index)
        {
            GUI.SetNextControlName($"{listId}_{index}");
        }

        protected void UpdateSelectedIndex()
        {
            var focusedControlName = GUI.GetNameOfFocusedControl();
            if (string.IsNullOrEmpty(focusedControlName)) return;   // The selected control, doesn't have a name.

            if (!focusedControlName.StartsWith(listId)) return;     // The control's name doesn't belong to this list.

            // Get the index part from the control's name.
            var selectedIndex = int.Parse(focusedControlName.Split('_')[1]);

            if (ReorderableList.index == selectedIndex) return;

            ReorderableList.index = selectedIndex;
            ReorderableList.onSelectCallback?.Invoke(ReorderableList);
        }
    }
}
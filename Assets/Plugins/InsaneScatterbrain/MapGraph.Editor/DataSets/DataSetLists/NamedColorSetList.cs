using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing named color sets.
    /// </summary>
    public class NamedColorSetList : DataSetList<NamedColor>
    {
        private readonly NamedColorSet namedColorSet;

        /// <inheritdoc cref="DataSetList{T}.DefaultName"/>
        protected override string DefaultName => "New Color";
        
        /// <inheritdoc cref="DataSetList{T}.LabelText"/>
        protected override string LabelText => "Named Colors";
        
        /// <inheritdoc cref="DataSetList{T}.AdditionalFieldsWidth"/>
        protected override int AdditionalFieldsWidth => 100;
        
        public event Action<string> OnNew;

        protected override NamedColor New(string name)
        {
            Color32 newColor;
            do
            {
                newColor = new Color32(
                    (byte) Random.Range(0, 255), 
                    (byte) Random.Range(0, 255), 
                    (byte) Random.Range(0, 255), 
                    255);
            } 
            while (namedColorSet.ContainsColor(newColor));
            
            var namedColor = new NamedColor(name, newColor);
            OnNew?.Invoke(namedColor.Id);

            return namedColor;
        }

        protected override void DrawAdditionalFields(Rect rect, string id)
        {
            EditorGUI.BeginChangeCheck();
            
            var color = namedColorSet.GetColorById(id);
            var changedToColor = EditorGUI.ColorField(
                new Rect(rect.x + rect.width - AdditionalFieldsWidth, rect.y, AdditionalFieldsWidth, EditorGUIUtility.singleLineHeight),
                GUIContent.none, color, true, false, false);

            if (!EditorGUI.EndChangeCheck()) return;
            
            changedToColor.a = 1f;
            try
            {
                namedColorSet.SetColor(id, changedToColor);
                EditorUtility.SetDirty(namedColorSet);
            }
            catch (ArgumentException)
            { 
                // Just don't apply the color for now.
            }
        }

        private readonly List<string> linkedSetNames = new List<string>();
        
        protected override string NameField(Rect rect, string itemName)
        {
            var colorId = namedColorSet.GetId(itemName);

            var tooltip = string.Empty;
            
            if (namedColorSet.LinkedSets.Count > 0 && namedColorSet.IsManuallyCreated(colorId))
            {
                tooltip += "Manually created.";
            }
            
            if (!namedColorSet.IsColorLinked(colorId))
            {
                if (!string.IsNullOrEmpty(tooltip))
                {
                    GUI.Label(rect, new GUIContent(GUIContent.none) { tooltip = tooltip});
                }
                return base.NameField(rect, itemName);
            }
            
            var enabled = GUI.enabled;
                
            // If the color is linked, it's not allowed to change its name as it needs to be synced to the
            // linked set's item.
            GUI.enabled = false;
            base.NameField(rect, itemName);

            // Show a tooltip on any disabled text fields, telling the user that it's linked an to which
            // sets it's linked to.
            var linkedSets = namedColorSet.GetColorLinkedSets(colorId);
            linkedSetNames.Clear();
            foreach (Object linkedSet in linkedSets)
            {
                linkedSetNames.Add(linkedSet.name);
            }

            var joinedLinkedSetNames = string.Join(", ", linkedSetNames);

            if (!string.IsNullOrEmpty(tooltip))
            {
                tooltip += "\n";
            }
            
            tooltip += $"{itemName} is linked with: {joinedLinkedSetNames}";
            
            GUI.Label(rect, new GUIContent(GUIContent.none) { tooltip = tooltip});
                
            GUI.enabled = enabled;
            return itemName;

        }

        public NamedColorSetList(NamedColorSet namedColorSet) : base(namedColorSet)
        {
            this.namedColorSet = namedColorSet;

            // If a linked color is selected in the list, disabled the remove button, as that's not allowed for linked colors.
            OnSelect += colorId => ReorderableList.displayRemove = !this.namedColorSet.IsColorLinked(colorId);
        }
    }
}
using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Set of named colors.
    /// </summary>
    public partial class NamedColorSet : DataSetScriptableObject<NamedColor>, ISerializationCallbackReceiver
    {
        private Dictionary<Color32, NamedColor> namedColorsByColor = new Dictionary<Color32, NamedColor>(EqualityComparer.Color32);

        /// <summary>
        /// Gets whether the color is defined inside the set.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>True if the color exists inside the set, false otherwise.</returns>
        public bool ContainsColor(Color32 color)
        {
            return namedColorsByColor.ContainsKey(color);
        }
        
        /// <summary>
        /// Gets the color by ID.
        /// </summary>
        /// <param name="id">The named color's ID.</param>
        /// <returns>The color.</returns>
        public Color32 GetColorById(string id)
        {
            return Get(id).Color;
        }
        
        /// <summary>
        /// Gets the color by name.
        /// </summary>
        /// <param name="colorName">The named color's name.</param>
        /// <returns>The color.</returns>
        public Color32 GetColorByName(string colorName)
        {
            return GetByName(colorName).Color;
        }

        /// <summary>
        /// Gets the color name by color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The color name.</returns>
        public string GetName(Color32 color)
        {
            return !namedColorsByColor.ContainsKey(color) ? null : namedColorsByColor[color].Name;
        }

        public string GetId(string colorName)
        {
            return GetByName(colorName).Id;
        }

        public override void Add(NamedColor element)
        {
            namedColorsByColor.Add(element.Color, element);
            
            base.Add(element);
        }

        public void SetColor(string id, Color newColor)
        {
            if (ContainsColor(newColor))
            {
                throw new ArgumentException($"Cannot set named color {id}: color already in use.");
            }

            var namedColor = Get(id);
            var oldColor = namedColor.Color;
            
            namedColor.Color = newColor;
            
            namedColorsByColor.Remove(oldColor);
            namedColorsByColor.Add(newColor, namedColor);
        }

        /// <summary>
        /// Removes the color with the given name from the set.
        /// </summary>
        /// <param name="id">The ID of the named color to remove.</param>
        public override void Remove(string id)
        {
            var namedColor = Get(id);
            namedColorsByColor.Remove(namedColor.Color);

            base.Remove(id);
        }

        #region DataSet
        
        [Serializable] private class NamedColorDataSet : OpenDataSet<NamedColor> { }
        
        [SerializeField] private NamedColorDataSet namedColorDataSet = new NamedColorDataSet();
        protected override IOpenDataSet<NamedColor> OpenSet => namedColorDataSet;
        
        #endregion

        #region Serialization
        
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            OnBeforeSerializeEditor();
#endif
        }

        public void OnAfterDeserialize()
        {
            namedColorsByColor = new Dictionary<Color32, NamedColor>(EqualityComparer.Color32);

            foreach (var id in OrderedIds)
            {
                var namedColor = Get(id);
                namedColorsByColor.Add(namedColor.Color, namedColor);
            }

#if UNITY_EDITOR
            OnAfterDeserializeEditor();
#endif
        }

        [Obsolete("Named color data is now stored in the named color object. Will be removed in version 2.0.")]
        [SerializeField, HideInInspector] private string[] serializedNames;
        
        [Obsolete("Named color data is now stored in the named color object. Will be removed in version 2.0.")]
        [SerializeField, HideInInspector] private Color32[] serializedColors;

        #endregion
    }
}
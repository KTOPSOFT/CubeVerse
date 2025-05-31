using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains a color paired with a name to refer it by.
    /// </summary>
    [Serializable]
    public class NamedColor : DataSetItem
    {
        [SerializeField] private Color32 color = default;

        public NamedColor(string name, Color32 color) : base(name)
        {
            this.color = color;
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        public Color32 Color
        {
            get => color;
            set => color = value;
        }
    }
}
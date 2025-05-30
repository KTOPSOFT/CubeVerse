using UnityEngine;
using System;

namespace AdvancedColorPicker
{
    [ExecuteInEditMode]
    public abstract class ColorTypeComponent : ColorComponent
    {
        [SerializeField]
        private ColorValueType type = ColorValueType.RGB_R;

        /// <summary>
        /// Which value this component can edit.
        /// </summary>
        public ColorValueType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type == value)
                    return;
                type = value;
                DisplayNewColor();
            }
        }
    }
}
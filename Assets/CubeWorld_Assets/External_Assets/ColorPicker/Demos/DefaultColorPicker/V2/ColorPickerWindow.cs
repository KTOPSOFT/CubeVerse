using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedColorPicker
{
    /// <summary>
    /// The intention of this class is that it can be used instead of PopupWindow, so that after calling <see cref="PopupWindow.Open(RectTransform, RectTransform, PopupWindow)"/>,
    /// you'll be able to directly access the ColorPicker to change the color through code while using this class to close the window
    /// </summary>
    public class ColorPickerWindow : PopupWindow
    {
        [SerializeField] private ColorPicker _picker = null;

        public ColorPicker Picker => _picker;
    }
}
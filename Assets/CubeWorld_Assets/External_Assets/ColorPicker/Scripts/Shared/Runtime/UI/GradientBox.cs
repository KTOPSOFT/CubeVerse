using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(Slider2D)), ExecuteInEditMode]
    public class GradientBox : GradientBackground2D
    {
        private Slider2D slider;

        private bool dontListenToSlider;

        protected override bool InverseX
        {
            get
            {
                return slider.InverseX;
            }
        }

        protected override bool InverseY
        {
            get
            {
                return slider.InverseY;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            slider = GetComponent<Slider2D>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            slider.onValueChanged.AddListener(UpdatePickerValues);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            slider.onValueChanged.RemoveListener(UpdatePickerValues);
        }

        private void UpdatePickerValues(float xValue, float yValue)
        {
            if (dontListenToSlider)
                return;

            if (Picker != null)
            {
                Picker.SetValueNormalized(ValueType1, xValue);
                Picker.SetValueNormalized(ValueType2, yValue);
            }

            DisplayNewColor();
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(slider, "ColorChanged");
#endif

            base.DisplayNewColor();

            dontListenToSlider = true;
            slider.valueX = Picker != null ? Picker.GetValueNormalized(ValueType1) : 0;
            slider.valueY = Picker != null ? Picker.GetValueNormalized(ValueType2) : 0;
            dontListenToSlider = false;
        }
    }
}
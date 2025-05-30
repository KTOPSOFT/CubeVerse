using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(InputField)), ExecuteInEditMode]
    public class ColorInput : ColorTypeComponent
    {
        [SerializeField]
        private float minValue = 0;
        [SerializeField]
        private float maxValue = 255;
        [SerializeField]
        private string formatter = "000";

        private InputField input;
        private bool dontListenToInput;
        private bool dontListenToColor;

        /// <summary>
        /// The minimum value a user can enter.
        /// </summary>
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                if (minValue == value)
                    return;

                minValue = value;
                maxValue = Mathf.Max(minValue, maxValue);
                DisplayNewColor();
            }
        }

        /// <summary>
        /// The maximum value a user can enter.
        /// </summary>
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                if (maxValue == value)
                    return;

                maxValue = value;
                minValue = Mathf.Min(minValue, maxValue);
                DisplayNewColor();
            }
        }


        /// <summary>
        /// Gets or sets the formatter
        /// </summary>
        public string Formatter
        {
            get
            {
                return formatter;
            }
            set
            {
                if (!0f.IsFormatValid(value))
                    return;

                if (formatter == value)
                    return;

                formatter = value;
                DisplayNewColor();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            input = GetComponent<InputField>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_5_2
            input.onValueChange.AddListener(InputChanged);
#else
            input.onValueChanged.AddListener(InputChanged);
#endif
            input.onEndEdit.AddListener(InputFinished);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_5_2
            input.onValueChange.RemoveListener(InputChanged);
#else
            input.onValueChanged.RemoveListener(InputChanged);
#endif
            input.onEndEdit.RemoveListener(InputFinished);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            minValue = Mathf.Min(minValue, maxValue);
            maxValue = Mathf.Max(minValue, maxValue);

            base.OnValidate();
        }
#endif

        private void InputChanged(string value)
        {
            if (Picker != null && !dontListenToInput)
            {
                float number;

                if (float.TryParse(value, out number))
                {
                    number = (Mathf.Clamp(number, minValue, maxValue) - minValue) / (maxValue - minValue);
                    dontListenToColor = true;
                    Picker.SetValueNormalized(Type, number);
                    dontListenToColor = false;
                }
            }
        }

        private void InputFinished(string value)
        {
            DisplayNewColor();
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled || dontListenToColor)
                return;

            dontListenToInput = true;
            if (Picker != null)
            {
                float number = minValue + (Picker.GetValueNormalized(Type) * (maxValue - minValue));
                input.text = ConvertToDisplayString(number);
            }
            else
            {
                input.text = ConvertToDisplayString(0);
            }
            dontListenToInput = false;
        }

        private string ConvertToDisplayString(float value)
        {
            return value.ToString(formatter);
        }

        public void SetDefaultMinMax()
        {
            switch (Type)
            {
                case ColorValueType.Alpha:
                case ColorValueType.RGB_R:
                case ColorValueType.RGB_G:
                case ColorValueType.RGB_B:
                    minValue = 0;
                    maxValue = 255;
                    break;
                case ColorValueType.HSV_H:
                case ColorValueType.HSL_H:
                    minValue = 0;
                    maxValue = 360;
                    break;
                case ColorValueType.HSV_S:
                case ColorValueType.HSV_V:
                case ColorValueType.HSL_S:
                case ColorValueType.HSL_L:
                    minValue = 0;
                    maxValue = 100;
                    break;
                default:
                    throw new System.NotImplementedException(Type.ToString());
            }
            DisplayNewColor();
        }
    }
}
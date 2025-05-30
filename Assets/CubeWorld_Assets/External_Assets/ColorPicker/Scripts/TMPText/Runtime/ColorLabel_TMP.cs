using UnityEngine;
using TMPro;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(TMP_Text))]
    public class ColorLabel_TMP : ColorTypeComponent
    {
        [SerializeField]
        private float minValue = 0;
        [SerializeField]
        private float maxValue = 255;
        [SerializeField]
        private string formatter = "000";

        private TMP_Text label;

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
            label = GetComponent<TMP_Text>();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            minValue = Mathf.Min(minValue, maxValue);
            maxValue = Mathf.Max(minValue, maxValue);

            base.OnValidate();
        }
#endif

        private string ConvertToDisplayString(float value)
        {
            return value.ToString(formatter);
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;

            if (Picker == null)
            {
                label.text = ConvertToDisplayString(0f);
            }
            else
            {
                float value = minValue + (Picker.GetValueNormalized(Type) * (maxValue - minValue));

                label.text = ConvertToDisplayString(value);
            }
        }

        public void SetDefaultValuesForType()
        {
            switch (Type)
            {
                case ColorValueType.Alpha:
                    minValue = 0;
                    maxValue = 255;
                    formatter = "A: 000;A: -000";
                    break;
                case ColorValueType.RGB_R:
                    minValue = 0;
                    maxValue = 255;
                    formatter = "R: 000;R: -000";
                    break;
                case ColorValueType.RGB_G:
                    minValue = 0;
                    maxValue = 255;
                    formatter = "G: 000;G: -000";
                    break;
                case ColorValueType.RGB_B:
                    minValue = 0;
                    maxValue = 255;
                    formatter = "B: 000;B: -000";
                    break;
                case ColorValueType.HSV_H:
                case ColorValueType.HSL_H:
                    minValue = 0;
                    maxValue = 360;
                    formatter = "H: 000°;H: -000°";
                    break;
                case ColorValueType.HSV_S:
                case ColorValueType.HSL_S:
                    minValue = 0;
                    maxValue = 1;
                    formatter = "S: 000%;S: -000%";
                    break;
                case ColorValueType.HSV_V:
                    minValue = 0;
                    maxValue = 1;
                    formatter = "V: 000%;V: -000%";
                    break;
                case ColorValueType.HSL_L:
                    minValue = 0;
                    maxValue = 1;
                    formatter = "L: 000%;L: -000%";
                    break;
                default:
                    throw new System.NotImplementedException(Type.ToString());
            }

            DisplayNewColor();
        }
    }
}
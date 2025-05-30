using UnityEngine;
using System.Collections;
using System;

namespace AdvancedColorPicker
{
    [ExecuteInEditMode]
    public abstract class GraphicalColorTypeComponent : GraphicalColorComponent
    {
        [SerializeField]
        private ColorType valueType1 = ColorType.RGB_R;

        [SerializeField]
        private ColorType valueType2 = ColorType.RGB_G;

        [SerializeField]
        private ColorType valueType3 = ColorType.RGB_B;

        /// <summary>
        /// If this value is greater or equal to zero, this value will be used as second value instead of the ColorPicker's value
        /// </summary>
        [SerializeField]
        private float fixedValue2 = -1;

        /// <summary>
        /// If this value is greater or equal to zero, this value will be used as third value instead of the ColorPicker's value
        /// </summary>
        [SerializeField]
        private float fixedValue3 = -1;

        /// <summary>
        /// To which range our types belong (RGB, HSV, etc.)
        /// </summary>
        public ColorType RangeType
        {
            get
            {
                return GetRange(valueType1);
            }
            set
            {
                if (RangeType == value)
                    return;

                switch (value)
                {
                    case RGBColor.Flag:
                        valueType1 = ColorType.RGB_R;
                        valueType2 = ColorType.RGB_G;
                        valueType3 = ColorType.RGB_B;
                        break;
                    case HSVColor.Flag:
                        valueType1 = ColorType.HSV_H;
                        valueType2 = ColorType.HSV_S;
                        valueType3 = ColorType.HSV_V;
                        break;
                    case HSLColor.Flag:
                        valueType1 = ColorType.HSL_H;
                        valueType2 = ColorType.HSL_S;
                        valueType3 = ColorType.HSL_L;
                        break;
                    default:
                        throw new System.ArgumentException(value + " is not valid as range");
                }
            }
        }

        /// <summary>
        /// The first value type of this Component
        /// </summary>
        public ColorType ValueType1
        {
            get
            {
                return valueType1;
            }
            set
            {
                if (CalculateNewValue(value, ref valueType1, ref valueType2, ref valueType3))
                    DisplayNewColor();
            }
        }

        /// <summary>
        /// The second value type of this component
        /// </summary>
        public ColorType ValueType2
        {
            get
            {
                return valueType2;
            }
            set
            {
                if (CalculateNewValue(value, ref valueType2, ref valueType1, ref valueType3))
                    DisplayNewColor();
            }
        }

        /// <summary>
        /// The third value type of this component.
        /// </summary>
        public ColorType ValueType3
        {
            get
            {
                return valueType3;
            }
            set
            {
                if (CalculateNewValue(value, ref valueType3, ref valueType1, ref valueType2))
                    DisplayNewColor();
            }
        }

        public float FixedValue2
        {
            get
            {
                return fixedValue2;
            }
            set
            {
                if (fixedValue2 == value)
                    return;

                fixedValue2 = value;
                DisplayNewColor();
            }
        }

        public float FixedValue3
        {
            get
            {
                return fixedValue3;
            }
            set
            {
                if (fixedValue3 == value)
                    return;

                fixedValue3 = value;
                DisplayNewColor();
            }
        }

        public virtual float GetValue1()
        {
            return Picker != null ? Picker.GetValueNormalized(valueType1) : 0;
        }

        public virtual float GetValue2()
        {
            return fixedValue2 >= 0 ? fixedValue2 : Picker != null ? Picker.GetValueNormalized(valueType2) : 0;
        }

        public virtual float GetValue3()
        {
            return fixedValue3 >= 0 ? fixedValue3 : Picker != null ? Picker.GetValueNormalized(valueType3) : 0;
        }

        /// <summary>
        /// Sets value1 to the newValue, and updates value2 and value3 to fit inside the range aswell
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <returns></returns>
        public static bool CalculateNewValue(ColorType newValue, ref ColorType value1, ref ColorType value2, ref ColorType value3)
        {
            if (newValue == value1)
                return false;

            // Check if value belongs to a range
            ColorType newRange = GetRange(newValue);

            // Check if range stays equal
            if ((GetRange(value1) & newValue) == newValue)
            {
                ColorType old = value1;
                value1 = newValue;

                if (value2 == newValue)
                    value2 = old;
                else if (value3 == newValue)
                    value3 = old;
            }
            else // Range changed
            {
                value1 = newValue;

                value2 = (ColorType)((int)value1 * 2);
                if (GetRange(value2) != newRange)
                    value2 = (ColorType)((int)value1 / 2);

                value3 = ((newRange & ~value1) & ~value2);
            }
            return true;
        }

        private static ColorType GetRange(ColorType type)
        {
            if ((RGBColor.Flag & type) == type)
                return RGBColor.Flag;

            if ((HSVColor.Flag & type) == type)
                return HSVColor.Flag;

            if ((HSLColor.Flag & type) == type)
                return HSLColor.Flag;

            return (ColorType)0;
        }
    }
}
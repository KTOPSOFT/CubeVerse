using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdvancedColorPicker
{
    public class GradientBoxSwitcher : MonoBehaviour
    {
        public Text verticalText;
        public Text horizontalText;
        public Text otherText;

        public GradientBox box;
        public ColorSlider slider;
        public GradientBackground sliderBackground;

        public bool alwaysShowHue;

        public void RGBToggled(bool on)
		{
            if (on)
                ToRGB();
		}

        public void HSVToggled(bool on)
		{
            if (on)
                ToHSV();
		}

        public void HSLToggled(bool on)
		{
            if (on)
                ToHSL();
		}

        public void ToRGB()
        {
            if (alwaysShowHue)
			{
                box.SetToDefaultType(ColorType.RGB_R, ColorType.RGB_G);
			}
            else
			{
                box.ValueType1 = ColorType.RGB_R;
                box.ValueType2 = ColorType.RGB_G;
			}
            slider.Type = ColorValueType.RGB_B;
            sliderBackground.SetToDefaultType(ColorType.RGB_B);
            horizontalText.text = "Red";
            verticalText.text = "Green";
            otherText.text = "Blue";
        }

        public void ToHSV()
        {
            if (alwaysShowHue)
            {
                box.SetToDefaultType(ColorType.HSV_S, ColorType.HSV_V);
            }
            else
            {
                box.ValueType1 = ColorType.HSV_S;
                box.ValueType2 = ColorType.HSV_V;
            }
            slider.Type = ColorValueType.HSV_H;
            sliderBackground.SetToDefaultType(ColorType.HSV_H);
            horizontalText.text = "Saturation";
            verticalText.text = "Brightness";
            otherText.text = "Hue";
        }

        public void ToHSL()
        {
            if (alwaysShowHue)
            {
                box.SetToDefaultType(ColorType.HSL_H, ColorType.HSL_L);
            }
            else
            {
                box.ValueType1 = ColorType.HSL_H;
                box.ValueType2 = ColorType.HSL_L;
            }
            slider.Type = ColorValueType.HSL_S;
            sliderBackground.SetToDefaultType(ColorType.HSL_S);
            horizontalText.text = "Hue";
            verticalText.text = "Light";
            otherText.text = "Saturation";
        }

        public void Rotate()
        {
            string otherT = otherText.text;
            otherText.text = verticalText.text;
            verticalText.text = horizontalText.text;
            horizontalText.text = otherT;

            ColorType horizT = box.ValueType1;
            ColorType vertiT = box.ValueType2;

            if (alwaysShowHue)
            {
                box.SetToDefaultType((ColorType)slider.Type, horizT);
            }
            else
            {
                box.ValueType1 = (ColorType)slider.Type;
                box.ValueType2 = horizT;
            }

            slider.Type = (ColorValueType)vertiT;
            sliderBackground.SetToDefaultType(vertiT);
        }

        public void Flip()
        {
            string horiT = horizontalText.text;
            horizontalText.text = verticalText.text;
            verticalText.text = horiT;

            box.ValueType1 = box.ValueType2;
        }

    }
}
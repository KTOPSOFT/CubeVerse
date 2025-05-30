using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdvancedColorPicker
{
    [AddComponentMenu("ColorPicker/PreviewGraphic")]
    [RequireComponent(typeof(Graphic)), ExecuteInEditMode]
    public class PreviewGraphic : ColorComponent
    {
        private Graphic graphic;

        [SerializeField]
        private bool alphaIsFixed = false;

        [SerializeField]
        private byte fixedAlpha = 255;

        public bool AlphaIsFixed
        {
            get
            {
                return alphaIsFixed;
            }
            set
            {
                if (alphaIsFixed == value)
                    return;

                alphaIsFixed = value;
                DisplayNewColor();
            }
        }

        public byte FixedAlpha
        {
            get
            {
                return fixedAlpha;
            }
            set
            {
                if (fixedAlpha == value)
                    return;

                fixedAlpha = value;
                DisplayNewColor();
            }
        }

        protected override void Awake()
        {
            graphic = GetComponent<Graphic>();
            base.Awake();
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;

            graphic.color = GetColor();
        }

        private Color32 GetColor()
        {
            Color32 color;

            if (Picker == null)
                color = Color.black;
            else
                color = Picker.CurrentColor;

            if (alphaIsFixed)
                color.a = fixedAlpha;

            return color;
        }
    }
}
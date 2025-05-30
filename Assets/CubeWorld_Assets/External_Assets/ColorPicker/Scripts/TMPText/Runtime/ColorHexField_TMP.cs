using UnityEngine;
using System.Text;
using System.Globalization;
using TMPro;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(TMP_InputField)), ExecuteInEditMode]
    public class ColorHexField_TMP : ColorComponent
    {
        [SerializeField] private bool displayAlpha = true;

        [SerializeField] private bool displayHashtag = true;

        [SerializeField]
        private HexfieldType acceptedInput = HexfieldType.RGB | HexfieldType.RGBA | HexfieldType.RRGGBB | HexfieldType.RRGGBBAA;

        [Tooltip("Contains which channels can be edited by this hexfield")]
        [SerializeField] private RGBAType editableChannels = RGBAType.R | RGBAType.G |RGBAType.B | RGBAType.A;

        private TMP_InputField hexInputField;

        public bool DisplayAlpha
        {
            get
            {
                return displayAlpha;
            }
            set
            {
                if (displayAlpha == value)
                    return;

                displayAlpha = value;
                DisplayNewColor();
            }
        }

        public bool DisplayHashtag
        {
            get
            {
                return displayHashtag;
            }
            set
            {
                if (displayHashtag == value)
                    return;

                displayHashtag = value;
                DisplayNewColor();
            }
        }

        public HexfieldType AcceptedInput
        {
            get
            {
                return acceptedInput;
            }
            set
            {
                if (acceptedInput == value)
                    return;

                acceptedInput = value;
                DisplayNewColor();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            hexInputField = GetComponent<TMP_InputField>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DisplayNewColor();
            hexInputField.onEndEdit.AddListener(HexChanged);
            hexInputField.onValidateInput = ValidateHexField;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            hexInputField.onEndEdit.RemoveListener(HexChanged);
        }

        private char ValidateHexField(string text, int charIndex, char addedChar)
        {
            switch (addedChar)
            {
                case '#': // Checking for first doesn't work sadly as when typing over selected text, and thus replacing it. The text and charIndex are wrong, as they are still that of the selected text in which the # would be invalid at given index
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'E':
                case 'e':
                case 'F':
                case 'f':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return addedChar;
                default:
                    return '\0';
            }
        }

        private void HexChanged(string newHex)
        {
            if (Picker != null)
            {
                if (ColorPickerUtils.HexToColor(newHex, acceptedInput, Picker.Alpha, out Color32 color))
                {
                    Color32 newColor = Picker.CurrentColor;

                    if ((editableChannels & RGBAType.R) != 0)
                        newColor.r = color.r;
                    if ((editableChannels & RGBAType.G) != 0)
                        newColor.g = color.g;
                    if ((editableChannels & RGBAType.B) != 0)
                        newColor.b = color.b;
                    if ((editableChannels & RGBAType.A) != 0)
                        newColor.a = color.a;

                    Picker.CurrentColor = newColor;
                }
            }

            hexInputField.text = ColorPickerUtils.ColorToHex(Picker?.CurrentColor ?? new Color(0, 0, 0, 0), displayHashtag, displayAlpha);
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;

            hexInputField.text = ColorPickerUtils.ColorToHex(Picker?.CurrentColor ?? Color.clear, displayHashtag, displayAlpha);
        }
    }
}
using AdvancedColorPicker;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerEditor
{
    public static class Color_BuiltinText_MenuItems
    {
        // ***********************************************
        // ******************* Labels ********************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Label/RGB/R", false, 11)]
        public static void CreateLabel_RGB_R(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.RGB_R);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/RGB/G", false, 11)]
        public static void CreateLabel_RGB_G(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/RGB/B", false, 11)]
        public static void CreateLabel_RGB_B(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSV/H", false, 11)]
        public static void CreateLabel_HSV_H(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSV_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSV/S", false, 11)]
        public static void CreateLabel_HSV_S(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSV/V", false, 11)]
        public static void CreateLabel_HSV_V(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSL/H", false, 11)]
        public static void CreateLabel_HSL_H(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSL_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSL/S", false, 11)]
        public static void CreateLabel_HSL_S(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/HSL/L", false, 11)]
        public static void CreateLabel_HSL_L(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.HSL_L);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label/Alpha", false, 11)]
        public static void CreateLabel_Alpha(MenuCommand menuCommand)
        {
            CreateLabel(menuCommand, ColorValueType.Alpha);
        }

        private static void CreateLabel(MenuCommand cmd, ColorValueType type)
        {
            ColorPicker picker = GetColorPicker(cmd.context as GameObject);

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 80, 20);
                result.name = "Label_" + type;

                ColorLabel label = result.AddComponent<ColorLabel>();
                label.Type = type;
                label.SetDefaultValuesForType();
                label.Picker = picker;
            }
        }



        // ***********************************************
        // **************** INPUT FIELDS *****************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/InputField/RGB/R", false, 11)]
        public static void CreateInput_RGB_R(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.RGB_R);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/RGB/G", false, 11)]
        public static void CreateInput_RGB_G(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/RGB/B", false, 11)]
        public static void CreateInput_RGB_B(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSV/H", false, 11)]
        public static void CreateInput_HSV_H(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSV_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSV/S", false, 11)]
        public static void CreateInput_HSV_S(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSV/V", false, 11)]
        public static void CreateInput_HSV_V(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSL/H", false, 11)]
        public static void CreateInput_HSL_H(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSL_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSL/S", false, 11)]
        public static void CreateInput_HSL_S(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/HSL/L", false, 11)]
        public static void CreateInput_HSL_L(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.HSL_L);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField/Alpha", false, 11)]
        public static void CreateInput_Alpha(MenuCommand menuCommand)
        {
            CreateInput(menuCommand, ColorValueType.Alpha);
        }

        private static void CreateInput(MenuCommand cmd, ColorValueType type)
        {
            ColorPicker picker = GetColorPicker(cmd.context as GameObject);

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 50, 20);
                result.name = $"Input_{type}";

                InputField input = AddInput(result, 0, 0);
                input.textComponent.alignment = TextAnchor.MiddleCenter;
                input.characterValidation = InputField.CharacterValidation.Decimal;
                input.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;

                ColorInput colorInput = result.AddComponent<ColorInput>();
                colorInput.Type = type;
                colorInput.SetDefaultMinMax();
                colorInput.Picker = picker;
            }
        }

        // ***********************************************
        // ******************* Hexfield ******************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Hexfield", false, 11)]
        public static void CreateColorHexfield(MenuCommand menuCommand)
        {
            ColorPicker picker = GetColorPicker(menuCommand.context as GameObject);

            if (CreateGameObject(out var result))
            {
                SetRectTransformSize(result.transform as RectTransform, 110, 30);
                result.name = "Hexfield";

                var input = AddInput(result, 5, 0);
                input.textComponent.alignment = TextAnchor.MiddleRight;

                var hex = result.AddComponent<ColorHexField>();

                hex.Picker = picker;
            }
        }

        // ***********************************************
        // ***********************************************

        private static InputField AddInput(GameObject to, float xOffset, float yOffset)
        {
            // Add image
            Image image = to.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            image.type = Image.Type.Sliced;

            InputField input = to.AddComponent<InputField>();

            // Add Text
            RectTransform textRect = new GameObject("Text").AddComponent<RectTransform>();
            textRect.SetParent(to.transform, false);
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(xOffset, yOffset);
            textRect.offsetMax = new Vector2(-xOffset, -yOffset);
            var text = textRect.gameObject.AddComponent<Text>();
            text.supportRichText = false;
            text.color = new Color32(50, 50, 50, 255);

            // Set image and text of inputfield
            input.targetGraphic = image;
            input.textComponent = text;

            return input;
        }

        private static ColorPicker GetColorPicker(GameObject go)
        {
            return go?.GetComponentInParent<ColorPicker>();
        }

        private static bool CreateGameObject(out GameObject created)
        {
            created = Selection.activeGameObject;

            if (created != null && created.GetComponentInParent<Canvas>() != null && EditorApplication.ExecuteMenuItem("GameObject/Create Empty Child"))
            {
                created = Selection.activeGameObject;
                return true;
            }
            else if (EditorApplication.ExecuteMenuItem("GameObject/UI/Image"))
            {
                created = Selection.activeGameObject;
                GameObject.DestroyImmediate(created.GetComponent<Image>());
                GameObject.DestroyImmediate(created.GetComponent<CanvasRenderer>());
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void SetRectTransformSize(RectTransform rect, float width, float height)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
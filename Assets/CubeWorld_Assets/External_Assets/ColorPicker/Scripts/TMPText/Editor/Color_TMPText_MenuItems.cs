using AdvancedColorPicker;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ColorPickerEditor
{
    public static class Color_TMPText_MenuItems
    {
        // ***********************************************
        // ************* INPUT FIELDS - TMP **************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/RGB/R", false, 11)]
        public static void CreateInput_TMP_RGB_R(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.RGB_R);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/RGB/G", false, 11)]
        public static void CreateInput_TMP_RGB_G(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/RGB/B", false, 11)]
        public static void CreateInput_TMP_RGB_B(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSV/H", false, 11)]
        public static void CreateInput_TMP_HSV_H(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSV_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSV/S", false, 11)]
        public static void CreateInput_TMP_HSV_S(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSV/V", false, 11)]
        public static void CreateInput_TMP_HSV_V(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSL/H", false, 11)]
        public static void CreateInput_TMP_HSL_H(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSL_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSL/S", false, 11)]
        public static void CreateInput_TMP_HSL_S(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/HSL/L", false, 11)]
        public static void CreateInput_TMP_HSL_L(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.HSL_L);
        }

        [MenuItem("GameObject/UI/ColorPicker/InputField - TMP/Alpha", false, 11)]
        public static void CreateInput_TMP_Alpha(MenuCommand menuCommand)
        {
            CreateInput_TMP(menuCommand, ColorValueType.Alpha);
        }

        private static void CreateInput_TMP(MenuCommand cmd, ColorValueType type)
        {
            ColorPicker picker = GetColorPicker(cmd.context as GameObject);

            var m = GetTMPMenuItemMethod("AddTextMeshProInputField");

            if (m != null)
            {
                m.Invoke(null, new object[] { cmd });
                var result = Selection.activeGameObject;
                result.name = $"Input_{type} (TMP)";
                var input = result.GetComponent<TMP_InputField>();
                input.textComponent.alignment = TextAlignmentOptions.Center;
                input.characterValidation = TMP_InputField.CharacterValidation.Decimal;
                input.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;

                var colorInput = result.gameObject.AddComponent<ColorInput_TMP>();
                colorInput.Type = type;
                colorInput.SetDefaultMinMax();
                colorInput.Picker = picker;
            }
        }

        // ***********************************************
        // **************** Labels - TMP *****************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/RGB/R", false, 11)]
        public static void CreateLabel_TMP_RGB_R(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.RGB_R);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/RGB/G", false, 11)]
        public static void CreateLabel_TMP_RGB_G(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/RGB/B", false, 11)]
        public static void CreateLabel_TMP_RGB_B(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSV/H", false, 11)]
        public static void CreateLabel_TMP_HSV_H(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSV_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSV/S", false, 11)]
        public static void CreateLabel_TMP_HSV_S(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSV/V", false, 11)]
        public static void CreateLabel_TMP_HSV_V(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSL/H", false, 11)]
        public static void CreateLabel_TMP_HSL_H(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSL_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSL/S", false, 11)]
        public static void CreateLabel_TMP_HSL_S(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/HSL/L", false, 11)]
        public static void CreateLabel_TMP_HSL_L(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.HSL_L);
        }

        [MenuItem("GameObject/UI/ColorPicker/Label - TMP/Alpha", false, 11)]
        public static void CreateLabel_TMP_Alpha(MenuCommand menuCommand)
        {
            CreateLabel_TMP(menuCommand, ColorValueType.Alpha);
        }

        private static void CreateLabel_TMP(MenuCommand cmd, ColorValueType type)
        {
            ColorPicker picker = GetColorPicker(cmd.context as GameObject);

            var m = GetTMPMenuItemMethod("CreateTextMeshProGuiObjectPerform");

            if (m != null)
            {
                m.Invoke(null, new object[] { cmd });
                var result = Selection.activeGameObject;
                result.name = $"Label_{type} (TMP)";

                var label = result.gameObject.AddComponent<ColorLabel_TMP>();
                label.Type = type;
                label.SetDefaultValuesForType();
                label.Picker = picker;
            }
        }

        // ***********************************************
        // *************** Hexfield - TMP ****************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Hexfield - TMP", false, 11)]
        public static void CreateColorHexfield_TMP(MenuCommand menuCommand)
        {
            ColorPicker picker = GetColorPicker(menuCommand.context as GameObject);

            var m = GetTMPMenuItemMethod("AddTextMeshProInputField");

            if (m != null)
            {
                m.Invoke(null, new object[] { menuCommand });
                var result = Selection.activeGameObject;
                result.name = "Hexfield (TMP)";
                var hex = result.gameObject.AddComponent<ColorHexField_TMP>();
                hex.Picker = picker;
            }
        }

        // ***********************************************
        // ***********************************************

        private static ColorPicker GetColorPicker(GameObject go)
        {
            return go?.GetComponentInParent<ColorPicker>();
        }

        private static MethodInfo GetTMPMenuItemMethod(string name)
        {
            var m = typeof(TMPro.EditorUtilities.TMPro_CreateObjectMenu).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (m == null)
            {
                // If you get here, the most likely cause is that the given method either changed name or position in a new version of TextMeshPro.
                // If this happens I (the creator of 'Advanced ColorPicker for Unity UI') need to update this so that it calls the correct method.
                // If you get this error, manually adding the components individually through the inspector will probably still work.
                Debug.LogError($"Couldn't create TMP component via this MenuItem, most likely due to a new version of TMP. Please contact the creator of the 'Advanced ColorPicker for Unity UI' so that I can update this package. Couldn't find method with name: {name}");
            }

            return m;
        }
    }
}
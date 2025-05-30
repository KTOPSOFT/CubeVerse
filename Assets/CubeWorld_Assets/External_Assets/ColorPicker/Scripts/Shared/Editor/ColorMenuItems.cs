using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;
using System;
using System.Reflection;
using UnityEditor.Events;

namespace ColorPickerEditor
{
    /// <summary>
    /// Creates MenuItems and handle the defaault creation of ColorPicker Components
    /// </summary>
    public static class ColorMenuItems
    {
        [MenuItem("GameObject/UI/ColorPicker/Empty", false, 0)]
        public static void CreateEmptyColorPicker(MenuCommand menuCommand)
        {
            GameObject root = GetNewObjectRoot();
            if (root != null)
            {
                root.AddComponent<ColorPicker>();
                root.name = "ColorPicker";
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        // ***********************************************
        // ******************* SLIDERS *******************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Slider/RGB/R", false, 11)]
        public static void CreateSlider_RGB_R(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.RGB_R);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/RGB/G", false, 11)]
        public static void CreateSlider_RGB_G(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/RGB/B", false, 11)]
        public static void CreateSlider_RGB_B(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSV/H", false, 11)]
        public static void CreateSlider_HSV_H(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSV_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSV/S", false, 11)]
        public static void CreateSlider_HSV_S(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSV/V", false, 11)]
        public static void CreateSlider_HSV_V(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSL/H", false, 11)]
        public static void CreateSlider_HSL_H(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSL_H);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSL/S", false, 11)]
        public static void CreateSlider_HSL_S(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/HSL/L", false, 11)]
        public static void CreateSlider_HSL_L(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.HSL_L);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/Alpha/ClearToColor", false, 11)]
        public static void CreateSlider_Alpha_CTC(MenuCommand menuCommand)
        {
            CreateSlider(ColorValueType.Alpha);
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/Alpha/BlackToWhite", false, 11)]
        public static void CreateSlider_Alpha_BTW(MenuCommand menuCommand)
        {
            GradientBackground slider = CreateSlider(ColorValueType.Alpha).GetComponentInChildren<GradientBackground>();
            slider.DisplayCheckboard = false;
            slider[0].Type = GradientBackground.GradientPartType.Custom;
            slider[0].Color = Color.black;
            slider[1].Type = GradientBackground.GradientPartType.Custom;
            slider[1].Color = Color.white;
        }

        [MenuItem("GameObject/UI/ColorPicker/Slider/Alpha/ClearToBlack", false, 11)]
        public static void CreateSlider_Alpha_CTB(MenuCommand menuCommand)
        {
            GradientBackground slider = CreateSlider(ColorValueType.Alpha).GetComponentInChildren<GradientBackground>();
            slider[0].Type = GradientBackground.GradientPartType.Custom;
            slider[0].Color = new Color32(0, 0, 0, 0);
            slider[1].Type = GradientBackground.GradientPartType.Custom;
            slider[1].Color = Color.black;
        }

        private static ColorSlider CreateSlider(ColorValueType type)
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 160, 20);
                result.name = "Slider_" + type.ToString();

                // Background
                CreateGradientBackground(type);

                // Handle slider area
                RectTransform handleArea = new GameObject("HandleArea").AddComponent<RectTransform>();
                handleArea.SetParent(result.transform, false);
                SetRectTransformSize(handleArea, Vector2.zero, Vector2.one, new Vector2(2.5f, 2.5f));

                // Handle
                RectTransform handleRect = new GameObject("Handle").AddComponent<RectTransform>();
                handleRect.SetParent(handleArea, false);
                handleRect.offsetMin = Vector2.zero;
                handleRect.offsetMax = Vector2.zero;
                handleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 6f);
                Image handle = handleRect.gameObject.AddComponent<Image>();
                handle.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

                // Slider
                Slider slider = result.AddComponent<Slider>();
                slider.handleRect = handleRect;
                slider.targetGraphic = handle;

                // ColorSlider
                ColorSlider colorSlider = result.AddComponent<ColorSlider>();
                colorSlider.Type = type;
                colorSlider.Picker = picker;

                Selection.activeGameObject = result.gameObject;
                return colorSlider;
            }
            return null;
        }


        // ***********************************************
        // ******************** Boxes ********************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Box/RGB/RG", false, 11)]
        public static void CreateBox_RGB_RG(MenuCommand menuCommand)
        {
            CreateBox(ColorType.RGB_R, ColorType.RGB_G);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/RGB/RB", false, 11)]
        public static void CreateBox_RGB_RB(MenuCommand menuCommand)
        {
            CreateBox(ColorType.RGB_R, ColorType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/RGB/GB", false, 11)]
        public static void CreateBox_RGB_GB(MenuCommand menuCommand)
        {
            CreateBox(ColorType.RGB_G, ColorType.RGB_B);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/HSV/HS", false, 11)]
        public static void CreateBox_HSV_HS(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSV_H, ColorType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/HSV/HV", false, 11)]
        public static void CreateBox_HSV_HV(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSV_H, ColorType.HSV_V);
        }
        [MenuItem("GameObject/UI/ColorPicker/Box/HSV/SV", false, 11)]
        public static void CreateBox_HSV_SV(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSV_S, ColorType.HSV_V);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/HSL/HS", false, 11)]
        public static void CreateBox_HSL_HS(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSL_H, ColorType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Box/HSL/HL", false, 11)]
        public static void CreateBox_HSL_HL(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSL_H, ColorType.HSL_L);
        }
        [MenuItem("GameObject/UI/ColorPicker/Box/HSL/SL", false, 11)]
        public static void CreateBox_HSL_SL(MenuCommand menuCommand)
        {
            CreateBox(ColorType.HSL_S, ColorType.HSL_L);
        }

        private static void CreateBox(ColorType x, ColorType y)
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 150, 150);
                result.name = "GradientBox_" + x + y.ToString().Substring(y.ToString().Length - 1);

                // Handle slider area
                RectTransform handleArea = new GameObject("HandleArea").AddComponent<RectTransform>();
                handleArea.SetParent(result.transform, false);
                SetRectTransformSize(handleArea, Vector2.zero, Vector2.one, new Vector2(6, 6));

                // Handle
                RectTransform handleRect = new GameObject("Handle").AddComponent<RectTransform>();
                handleRect.SetParent(handleArea, false);
                SetRectTransformSize(handleRect, 12, 12);
                Image handle = handleRect.gameObject.AddComponent<Image>();
                handle.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

                // Slider
                Slider2D slider = result.AddComponent<Slider2D>();
                slider.handleRect = handleRect;
                slider.targetGraphic = handle;

                // Box
                GradientBox box = result.AddComponent<GradientBox>();
                box.ValueType1 = x;
                box.ValueType2 = y;
                box.Picker = picker;
            }
        }


        // ***********************************************
        // ******************* Circle ********************
        // ***********************************************


        //[MenuItem("GameObject/UI/ColorPicker/Circle/RGB/RG", false, 11)]
        //public static void CreateCircle_RGB_RG(MenuCommand menuCommand)
        //{
        //    CreateCircle(ColorType.RGB_R, ColorType.RGB_G);
        //}

        //[MenuItem("GameObject/UI/ColorPicker/Circle/RGB/RB", false, 11)]
        //public static void CreateCircle_RGB_RB(MenuCommand menuCommand)
        //{
        //    CreateCircle(ColorType.RGB_R, ColorType.RGB_B);
        //}

        //[MenuItem("GameObject/UI/ColorPicker/Circle/RGB/GB", false, 11)]
        //public static void CreateCircle_RGB_GB(MenuCommand menuCommand)
        //{
        //    CreateCircle(ColorType.RGB_G, ColorType.RGB_B);
        //}

        [MenuItem("GameObject/UI/ColorPicker/Circle/HSV/HS", false, 11)]
        public static void CreateCircle_HSV_HS(MenuCommand menuCommand)
        {
            CreateCircle(ColorType.HSV_H, ColorType.HSV_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Circle/HSV/HV", false, 11)]
        public static void CreateCircle_HSV_HV(MenuCommand menuCommand)
        {
            CreateCircle(ColorType.HSV_H, ColorType.HSV_V);
        }

        //[MenuItem("GameObject/UI/ColorPicker/Circle/HSV/SV", false, 11)]
        //public static void CreateCircle_HSV_SV(MenuCommand menuCommand)
        //{
        //    CreateCircle(ColorType.HSV_S, ColorType.HSV_V);
        //}

        [MenuItem("GameObject/UI/ColorPicker/Circle/HSL/HS", false, 11)]
        public static void CreateCircle_HSL_HS(MenuCommand menuCommand)
        {
            CreateCircle(ColorType.HSL_H, ColorType.HSL_S);
        }

        [MenuItem("GameObject/UI/ColorPicker/Circle/HSL/HL", false, 11)]
        public static void CreateCircle_HSL_HL(MenuCommand menuCommand)
        {
            CreateCircle(ColorType.HSL_H, ColorType.HSL_L);
        }

        //[MenuItem("GameObject/UI/ColorPicker/Circle/HSL/SL", false, 11)]
        //public static void CreateCircle_HSL_SL(MenuCommand menuCommand)
        //{
        //    CreateCircle(ColorType.HSL_S, ColorType.HSL_L);
        //}

        private static void CreateCircle(ColorType anlge, ColorType distance)
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 150, 150);
                result.name = "Circle_" + anlge + distance.ToString().Substring(distance.ToString().Length - 1);

                // Handle slider area
                RectTransform handleArea = new GameObject("HandleArea").AddComponent<RectTransform>();
                handleArea.SetParent(result.transform, false);
                SetRectTransformSize(handleArea, Vector2.zero, Vector2.one, new Vector2(6, 6));

                // Handle
                RectTransform handleRect = new GameObject("Handle").AddComponent<RectTransform>();
                handleRect.SetParent(handleArea, false);
                SetRectTransformSize(handleRect, 12, 12);
                Image handle = handleRect.gameObject.AddComponent<Image>();
                handle.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

                // Slider
                SliderCircle2D slider = result.AddComponent<SliderCircle2D>();
                slider.handleRect = handleRect;
                slider.targetGraphic = handle;

                // Circle
                GradientCircle circle = result.AddComponent<GradientCircle>();
                circle.ValueType1 = anlge;
                circle.ValueType2 = distance;
                circle.Picker = picker;
            }
        }

        // ***********************************************
        // ******************* Preview *******************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Preview/Opaque", false, 11)]
        public static void CreatePreview_Opaque(MenuCommand menuCommand)
        {
            CreatePreview();
        }

        [MenuItem("GameObject/UI/ColorPicker/Preview/Transparent", false, 11)]
        public static void CreatePreview_Transparent(MenuCommand menuCommand)
        {
            GradientBackground preview = CreatePreview();
            preview[0].AlphaIsFixed = false;
            preview[1].AlphaIsFixed = false;
        }

        [MenuItem("GameObject/UI/ColorPicker/Preview/HalfTransparent", false, 11)]
        public static void CreatePreview_HalfTransparent(MenuCommand menuCommand)
        {
            GradientBackground preview = CreatePreview();
            preview[0].AlphaIsFixed = false;
        }

        [MenuItem("GameObject/UI/ColorPicker/Preview/Seperated", false, 11)]
        public static void CreatePreview_Seperated(MenuCommand menuCommand)
        {
            CreatePreviewAlphaSeperated();
        }

        [MenuItem("GameObject/UI/ColorPicker/Preview/Graphic", false, 11)]
        public static void CreatePreview_Graphic(MenuCommand menuCommand)
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 100, 100);
                result.name = "PreviewGraphic";
                result.AddComponent<Image>();

                PreviewGraphic preview = result.AddComponent<PreviewGraphic>();
                preview.Picker = picker;
            }
        }

        [MenuItem("GameObject/UI/ColorPicker/Preview/Renderer", false, 11)]
        public static void CreatePreview_Renderer(MenuCommand menuCommand)
        {
            ColorPicker picker = GetColorPicker();

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            PreviewRenderer preview = cube.AddComponent<PreviewRenderer>();
            preview.Picker = picker;

        }

        // ***********************************************
        // ****************** Eyedropper *****************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Eyedropper/Button", false, 11)]
        public static void CreateEyeDropper_Button(MenuCommand menuCommand)
        {
            CreateEyedropperButton();
        }

        [MenuItem("GameObject/UI/ColorPicker/Eyedropper/Preview", false, 11)]
        public static void CreateEyeDropper_Preview(MenuCommand menuCommand)
        {
            CreateEyedropperPreview();
        }

        // ***********************************************
        // ******************* Swatches ******************
        // ***********************************************

        [MenuItem("GameObject/UI/ColorPicker/Swatch", false, 11)]
        public static void CreateSwatch(MenuCommand menuCommand)
        {
            ColorPicker picker = GetColorPicker();

            if (CreateGameObject(out GameObject result))
            {
                result.name = "Swatch";
                var swatch = result.AddComponent<ColorSwatch>();
                swatch.Picker = picker;

                AddViewport(swatch);

                var button = AddButton(swatch);

                UnityEventTools.AddVoidPersistentListener(swatch.PresetAdded, button.transform.SetAsLastSibling);
            }
        }

        private static void AddViewport(ColorSwatch parent)
        {
            RectTransform viewport = new GameObject().AddComponent<RectTransform>();
            viewport.SetParent(parent.transform, false);
            viewport.name = "viewport";

            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            parent.Viewport = viewport;

            var layout = viewport.gameObject.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.cellSize = new Vector2(14, 14);
            layout.spacing = new Vector2(2, 2);
        }

        private static Button AddButton(ColorSwatch group)
		{
            GameObject result = new GameObject();
            result.name = "AddPresetBtn";

            var transform = result.AddComponent<RectTransform>();
            transform.SetParent(group.Viewport, false);

            result.AddComponent<Image>();
            PreviewGraphic preview = result.AddComponent<PreviewGraphic>();
            preview.Picker = group.Picker;
            preview.AlphaIsFixed = true;

            var button = result.AddComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, group.Add);

            return button;
        }

        // ***********************************************
        // ***********************************************

        private static void CreateGradientBackground(ColorValueType type)
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, new Vector2(0, 0.2f), new Vector2(1, 0.8f), Vector2.zero);
                result.name = "GradientBackground";
                GradientBackground background = result.AddComponent<GradientBackground>();
                background.Picker = picker;

                switch (type)
                {
                    case ColorValueType.Alpha:
                        background.DisplayCheckboard = true;
                        background[0].Type = GradientBackground.GradientPartType.Color;
                        background[0].FixedAlphaValue = 0;
                        background[1].Type = GradientBackground.GradientPartType.Color;
                        break;
                    case ColorValueType.RGB_R:
                        background[0].Type = GradientBackground.GradientPartType.RGB_R;
                        background[1].Type = GradientBackground.GradientPartType.RGB_R;
                        break;
                    case ColorValueType.RGB_G:
                        background[0].Type = GradientBackground.GradientPartType.RGB_G;
                        background[1].Type = GradientBackground.GradientPartType.RGB_G;
                        break;
                    case ColorValueType.RGB_B:
                        background[0].Type = GradientBackground.GradientPartType.RGB_B;
                        background[1].Type = GradientBackground.GradientPartType.RGB_B;
                        break;
                    case ColorValueType.HSV_H:
                        background[0].Type = GradientBackground.GradientPartType.HSV_H;
                        background[0].FixedValue2 = 1;
                        background[0].FixedValue3 = 1;
                        background[0].Value2IsFixed = true;
                        background[0].Value3IsFixed = true;
                        background[1].Type = GradientBackground.GradientPartType.HSV_H;
                        background[1].FixedValue2 = 1;
                        background[1].FixedValue3 = 1;
                        background[1].Value2IsFixed = true;
                        background[1].Value3IsFixed = true;
                        background.Count = 7;
                        break;
                    case ColorValueType.HSV_S:
                        background[0].Type = GradientBackground.GradientPartType.HSV_S;
                        background[1].Type = GradientBackground.GradientPartType.HSV_S;
                        break;
                    case ColorValueType.HSV_V:
                        background[0].Type = GradientBackground.GradientPartType.HSV_V;
                        background[1].Type = GradientBackground.GradientPartType.HSV_V;
                        break;
                    case ColorValueType.HSL_H:
                        background[0].Type = GradientBackground.GradientPartType.HSL_H;
                        background[0].FixedValue2 = 1;
                        background[0].FixedValue3 = 0.5f;
                        background[0].Value2IsFixed = true;
                        background[0].Value3IsFixed = true;
                        background[1].Type = GradientBackground.GradientPartType.HSL_H;
                        background[1].FixedValue2 = 1;
                        background[1].FixedValue3 = 0.5f;
                        background[1].Value2IsFixed = true;
                        background[1].Value3IsFixed = true;
                        background.Count = 7;
                        break;
                    case ColorValueType.HSL_S:
                        background[0].Type = GradientBackground.GradientPartType.HSL_S;
                        background[1].Type = GradientBackground.GradientPartType.HSL_S;
                        break;
                    case ColorValueType.HSL_L:
                        background.Count = 3;
                        background[0].Type = GradientBackground.GradientPartType.HSL_L;
                        background[1].Type = GradientBackground.GradientPartType.HSL_L;
                        background[2].Type = GradientBackground.GradientPartType.HSL_L;
                        break;
                    default:
                        break;
                }
            }
        }

        private static GradientBackground CreatePreview()
        {
            ColorPicker picker = GetColorPicker();
            
            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 50, 50);
                result.name = "Preview";

                // Preview
                GradientBackground preview = result.AddComponent<GradientBackground>();
                preview.Picker = picker;
                preview.DisplayCheckboard = true;
                preview.Direction = Slider.Direction.BottomToTop;
                preview[0].Type = GradientBackground.GradientPartType.Color;
                preview[1].Type = GradientBackground.GradientPartType.Color;

                return preview;
            }
            return null;
        }

        private static void CreatePreviewAlphaSeperated()
        {
            ColorPicker picker = GetColorPicker();
            
            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 100, 50);
                result.name = "PreviewAlphaSeperated";

                // Opaque
                GradientBackground opaque = new GameObject("Opaque", typeof(RectTransform)).AddComponent<GradientBackground>();
                opaque.rectTransform.SetParent(result.transform, false);
                SetRectTransformSize(opaque.rectTransform, new Vector2(0, 0.1f), Vector2.one, Vector2.zero);
                opaque.BorderSize = 0;
                opaque[0].Type = GradientBackground.GradientPartType.Color;
                opaque[0].AlphaIsFixed = true;
                opaque[1].Type = GradientBackground.GradientPartType.Color;
                opaque[1].AlphaIsFixed = true;
                opaque.Picker = picker;

                // Alpha
                GradientBackground alpha = new GameObject("Alpha", typeof(RectTransform)).AddComponent<GradientBackground>();
                alpha.rectTransform.SetParent(result.transform, false);
                SetRectTransformSize(alpha.rectTransform, Vector2.zero, new Vector2(1, 0.1f), Vector2.zero);
                alpha.BorderSize = 0;
                alpha[0].Type = GradientBackground.GradientPartType.Custom;
                alpha[0].Color = Color.white;
                alpha[1].Type = GradientBackground.GradientPartType.Custom;
                alpha[1].Color = Color.black;
                alpha.Gradient = false;
                alpha.CenterType = GradientBackground.GradientCenterType.Alpha;
                alpha.Picker = picker;
            }
        }

        private static void CreateEyedropperButton()
        {
            ColorPicker picker = GetColorPicker();

            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 50, 20);
                result.name = "Eyedropper";
                Image image = result.AddComponent<Image>();
                Button button = result.AddComponent<Button>();
                ColorEyedropper dropper = result.AddComponent<ColorEyedropper>();

                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = Image.Type.Sliced;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(dropper.Activate));

                dropper.Picker = picker;
            }
        }

        private static void CreateEyedropperPreview()
        {
            GameObject result;
            if (CreateGameObject(out result))
            {
                SetRectTransformSize(result.transform as RectTransform, 150, 150);
                result.name = "EyedropperPreview";
                result.AddComponent<ColorEyedropperPreview>();
            }
        }

        private static ColorPicker GetColorPicker(GameObject go)
        {
            return go?.GetComponentInParent<ColorPicker>();
        }

        private static ColorPicker GetColorPicker()
        {
            return GetColorPicker(Selection.activeGameObject);
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

        private static void SetRectTransformSize(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offset)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offset;
            rect.offsetMax = -offset;
        }

        private static GameObject GetNewObjectRoot()
        {
            GameObject selected = Selection.activeGameObject;

            if (selected != null && selected.GetComponentInParent<Canvas>() != null && EditorApplication.ExecuteMenuItem("GameObject/Create Empty Child"))
            {
                return Selection.activeGameObject;
            }
            else if (EditorApplication.ExecuteMenuItem("GameObject/UI/Image"))
            {
                selected = Selection.activeGameObject;
                GameObject.DestroyImmediate(selected.GetComponent<Image>());
                GameObject.DestroyImmediate(selected.GetComponent<CanvasRenderer>());
                return selected;
            }
            else
            {
                return null;
            }
        }
    }
}

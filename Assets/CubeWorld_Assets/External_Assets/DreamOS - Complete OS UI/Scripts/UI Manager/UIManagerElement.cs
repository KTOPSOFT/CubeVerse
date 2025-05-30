using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    public class UIManagerElement : MonoBehaviour
    {
        // Resources
        public UIManager themeManagerAsset;

        // Options
        public ObjectType objectType;
        public ColorType colorType = ColorType.Primary;
        public FontType fontType = FontType.Regular;
        public bool keepAlphaValue = false;
        public bool useCustomFont = false;

        Image imageObject;
        TextMeshProUGUI textObject;

        public enum ObjectType
        {
            Text,
            Image
        }

        public enum ColorType
        {
            WindowBackground,
            Background,
            Primary,
            Secondary,
            Accent,
            AccentReversed,
            Taskbar
        }

        public enum FontType
        {
            Thin,
            Light,
            Regular,
            Semibold,
            Bold
        }

        void Awake()
        {
            try
            {
                if (themeManagerAsset == null) { themeManagerAsset = Resources.Load<UIManager>("UI Manager/UI Manager"); }

                if (objectType == ObjectType.Image && imageObject == null) { imageObject = gameObject.GetComponent<Image>(); }
                else if (objectType == ObjectType.Text && textObject == null) { textObject = gameObject.GetComponent<TextMeshProUGUI>(); }

                this.enabled = true;

                if (themeManagerAsset.enableDynamicUpdate == false) { UpdateElement(); this.enabled = false; }
            }

            catch { Debug.LogWarning("No <b>UI Manager</b> found.", this); }
        }

        void Update()
        {
            if (themeManagerAsset == null)
                return;

            if (themeManagerAsset.enableDynamicUpdate == true) { UpdateElement(); }
        }

        public void UpdateElement()
        {
            if (objectType == ObjectType.Image && imageObject != null)
            {
                if (keepAlphaValue == false)
                {
                    if (colorType == ColorType.Primary) { imageObject.color = themeManagerAsset.primaryColorDark; }
                    else if (colorType == ColorType.Secondary) { imageObject.color = themeManagerAsset.secondaryColorDark; }
                    else if (colorType == ColorType.WindowBackground) { imageObject.color = themeManagerAsset.windowBGColorDark; }
                    else if (colorType == ColorType.Background) { imageObject.color = themeManagerAsset.backgroundColorDark; }
                    else if (colorType == ColorType.Taskbar) { imageObject.color = themeManagerAsset.taskBarColorDark; }

                    if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Default)
                    {
                        if (colorType == ColorType.Accent) { imageObject.color = themeManagerAsset.highlightedColorDark; }
                        else if (colorType == ColorType.AccentReversed) { imageObject.color = themeManagerAsset.highlightedColorSecondaryDark; }
                    }

                    else if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Custom)
                    {
                        if (colorType == ColorType.Accent) { imageObject.color = themeManagerAsset.highlightedColorCustom; }
                        else if (colorType == ColorType.AccentReversed) { imageObject.color = themeManagerAsset.highlightedColorSecondaryCustom; }
                    }
                }

                else
                {
                    if (colorType == ColorType.WindowBackground)
                        imageObject.color = new Color(themeManagerAsset.windowBGColorDark.r, themeManagerAsset.windowBGColorDark.g, themeManagerAsset.windowBGColorDark.b, imageObject.color.a);
                    else if (colorType == ColorType.Background)
                        imageObject.color = new Color(themeManagerAsset.backgroundColorDark.r, themeManagerAsset.backgroundColorDark.g, themeManagerAsset.backgroundColorDark.b, imageObject.color.a);
                    else if (colorType == ColorType.Primary)
                        imageObject.color = new Color(themeManagerAsset.primaryColorDark.r, themeManagerAsset.primaryColorDark.g, themeManagerAsset.primaryColorDark.b, imageObject.color.a);
                    else if (colorType == ColorType.Secondary)
                        imageObject.color = new Color(themeManagerAsset.secondaryColorDark.r, themeManagerAsset.secondaryColorDark.g, themeManagerAsset.secondaryColorDark.b, imageObject.color.a);

                    if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Default)
                    {
                        if (colorType == ColorType.Accent)
                            imageObject.color = new Color(themeManagerAsset.highlightedColorDark.r, themeManagerAsset.highlightedColorDark.g, themeManagerAsset.highlightedColorDark.b, imageObject.color.a);
                        else if (colorType == ColorType.AccentReversed)
                            imageObject.color = new Color(themeManagerAsset.highlightedColorSecondaryDark.r, themeManagerAsset.highlightedColorSecondaryDark.g, themeManagerAsset.highlightedColorSecondaryDark.b, imageObject.color.a);
                    }

                    else if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Custom)
                    {
                        if (colorType == ColorType.Accent)
                            imageObject.color = new Color(themeManagerAsset.highlightedColorCustom.r, themeManagerAsset.highlightedColorCustom.g, themeManagerAsset.highlightedColorCustom.b, imageObject.color.a);
                        else if (colorType == ColorType.AccentReversed)
                            imageObject.color = new Color(themeManagerAsset.highlightedColorSecondaryCustom.r, themeManagerAsset.highlightedColorSecondaryCustom.g, themeManagerAsset.highlightedColorSecondaryCustom.b, imageObject.color.a);
                    }
                }
            }

            else if (objectType == ObjectType.Text && textObject != null)
            {
                if (keepAlphaValue == false)
                {
                    if (colorType == ColorType.WindowBackground) { textObject.color = themeManagerAsset.windowBGColorDark; }
                    else if (colorType == ColorType.Background) { textObject.color = themeManagerAsset.backgroundColorDark; }
                    else if (colorType == ColorType.Primary) { textObject.color = themeManagerAsset.primaryColorDark; }
                    else if (colorType == ColorType.Secondary) { textObject.color = themeManagerAsset.secondaryColorDark; }

                    if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Default)
                    {
                        if (colorType == ColorType.Accent) { textObject.color = themeManagerAsset.highlightedColorDark; }
                        else if (colorType == ColorType.AccentReversed) { textObject.color = themeManagerAsset.highlightedColorSecondaryDark; }
                    }

                    else if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Custom)
                    {
                        if (colorType == ColorType.Accent) { textObject.color = themeManagerAsset.highlightedColorCustom; }
                        else if (colorType == ColorType.AccentReversed) { textObject.color = themeManagerAsset.highlightedColorSecondaryCustom; }
                    }
                }

                else
                {
                    if (colorType == ColorType.WindowBackground)
                        textObject.color = new Color(themeManagerAsset.windowBGColorDark.r, themeManagerAsset.windowBGColorDark.g, themeManagerAsset.windowBGColorDark.b, textObject.color.a);
                    else if (colorType == ColorType.Background)
                        textObject.color = new Color(themeManagerAsset.backgroundColorDark.r, themeManagerAsset.backgroundColorDark.g, themeManagerAsset.backgroundColorDark.b, textObject.color.a);
                    else if (colorType == ColorType.Primary)
                        textObject.color = new Color(themeManagerAsset.primaryColorDark.r, themeManagerAsset.primaryColorDark.g, themeManagerAsset.primaryColorDark.b, textObject.color.a);
                    else if (colorType == ColorType.Secondary)
                        textObject.color = new Color(themeManagerAsset.secondaryColorDark.r, themeManagerAsset.secondaryColorDark.g, themeManagerAsset.secondaryColorDark.b, textObject.color.a);

                    if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Default)
                    {
                        if (colorType == ColorType.Accent)
                            textObject.color = new Color(themeManagerAsset.highlightedColorDark.r, themeManagerAsset.highlightedColorDark.g, themeManagerAsset.highlightedColorDark.b, textObject.color.a);
                        else if (colorType == ColorType.AccentReversed)
                            textObject.color = new Color(themeManagerAsset.highlightedColorSecondaryDark.r, themeManagerAsset.highlightedColorSecondaryDark.g, themeManagerAsset.highlightedColorSecondaryDark.b, textObject.color.a);
                    }

                    else if (themeManagerAsset.selectedTheme == UIManager.SelectedTheme.Custom)
                    {
                        if (colorType == ColorType.Accent)
                            textObject.color = new Color(themeManagerAsset.highlightedColorCustom.r, themeManagerAsset.highlightedColorCustom.g, themeManagerAsset.highlightedColorCustom.b, textObject.color.a);
                        else if (colorType == ColorType.AccentReversed)
                            textObject.color = new Color(themeManagerAsset.highlightedColorSecondaryCustom.r, themeManagerAsset.highlightedColorSecondaryCustom.g, themeManagerAsset.highlightedColorSecondaryCustom.b, textObject.color.a);
                    }
                }

                if (useCustomFont != true)
                {
                    if (fontType == FontType.Thin) { textObject.font = themeManagerAsset.systemFontThin; }
                    else if (fontType == FontType.Light) { textObject.font = themeManagerAsset.systemFontLight; }
                    else if (fontType == FontType.Regular) { textObject.font = themeManagerAsset.systemFontRegular; }
                    else if (fontType == FontType.Semibold) { textObject.font = themeManagerAsset.systemFontSemiBold; }
                    else if (fontType == FontType.Bold) { textObject.font = themeManagerAsset.systemFontBold; }
                }
            }
        }
    }
}
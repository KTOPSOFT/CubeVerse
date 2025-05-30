using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "New UI Manager Manager", menuName = "DreamOS/New UI Manager")]
    public class UIManager : ScriptableObject
    {
        // Options
        [HideInInspector] public bool enableDynamicUpdate = true;
        [HideInInspector] public bool enableExtendedColorPicker = true;
        [HideInInspector] public bool editorHints = true;
        public SelectedTheme selectedTheme;

        // System Theme Vars
        public Color highlightedColorDark = new Color(255, 255, 255, 255);
        public Color highlightedColorSecondaryDark = new Color(255, 255, 255, 255);
        public Color primaryColorDark = new Color(255, 255, 255, 255);
        public Color secondaryColorDark = new Color(255, 255, 255, 255);
        public Color windowBGColorDark = new Color(255, 255, 255, 255);
        public Color backgroundColorDark = new Color(255, 255, 255, 255);
        public Color taskBarColorDark = new Color(255, 255, 255, 255);

        // Custom Theme Vars
        public Color highlightedColorCustom = new Color(255, 255, 255, 255);
        public Color highlightedColorSecondaryCustom = new Color(255, 255, 255, 255);

        // Fonts
        public TMP_FontAsset systemFontThin;
        public TMP_FontAsset systemFontLight;
        public TMP_FontAsset systemFontRegular;
        public TMP_FontAsset systemFontSemiBold;
        public TMP_FontAsset systemFontBold;

        public enum SelectedTheme
        {
            Default,
            Custom
        }
    }
}
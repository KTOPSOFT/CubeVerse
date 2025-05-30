
namespace AdvancedColorPicker
{
    public enum HexfieldType
    {
        /// <summary>
        /// Short notation for hex colors, excluding aplha
        /// </summary>
        RGB = 1 << 0,

        /// <summary>
        /// Short notation for hex colors, including alpha
        /// </summary>
        RGBA = 1 << 1,

        /// <summary>
        /// Long notation for hex colors, excluding aplha
        /// </summary>
        RRGGBB = 1 << 2,

        /// <summary>
        /// Long notation for hex colors, including aplha
        /// </summary>
        RRGGBBAA = 1 << 3,
    }
}
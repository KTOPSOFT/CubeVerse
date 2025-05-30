using UnityEngine;
using UnityEngine.UI;
using AdvancedColorPicker; // If you want to access any components from the ColorPicker through code, don't forget this line!

[RequireComponent(typeof(Image))]
public class DefaultColorPickerDemoV2 : MonoBehaviour
{
    public RectTransform container;
    public ColorPickerWindow prefab;

    private Image image;
    private ColorPickerWindow instance;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Open()
    {
        // Check if we don't have a colorpicker window already open
        if (instance == null)
        {
            // Open new colorpicker window
            instance = PopupWindow.Open(container, transform as RectTransform, prefab);

            // Set picker's color to ours
            instance.Picker.CurrentColor = GetComponent<Image>().color;

            // Set method to execute when color changes
            instance.Picker.OnColorChanged.AddListener(OnColorChanged);
        }

        // bring this colorpicker window to front
        instance.transform.SetAsLastSibling();
    }

    /// <summary>
    /// This method sets the image color to the color of the ColorPicker
    /// </summary>
    /// <param name="color">The new color of the colorpicker</param>
    private void OnColorChanged(Color color)
    {
        image.color = color;
    }
}

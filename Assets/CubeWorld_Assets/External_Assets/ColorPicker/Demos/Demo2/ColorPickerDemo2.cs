using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A simple script used in the demo to toggle a UI toggle when the 3D object is clicked
/// </summary>
public class ColorPickerDemo2 : MonoBehaviour
{
    public Toggle toggle;

    private void OnMouseUpAsButton()
    {
        if (toggle != null)
            toggle.isOn = !toggle.isOn;
    }
}

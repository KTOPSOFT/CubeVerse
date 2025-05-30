using UnityEngine;
using UnityEngine.EventSystems;
using AdvancedColorPicker; // <-- required if we want to access any scripts of the AdvancedColorPicker package

[RequireComponent(typeof(ColorPreset))] // <-- This makes sure that Unity automatically adds a ColorPreset script to the gameobject whenever we add this script
public class PresetDemo : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	private ColorPreset _preset; // this is where we cache our ColorPreset so we don't have to retrieve it every time we click it

	private void Awake()
	{
		_preset = GetComponent<ColorPreset>();
	}


	// This method gets called by Unity because we implement the IPointerClickHandler, see unity's scripting reference for more information
	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
			case PointerEventData.InputButton.Left:
				_preset.ApplyColorToOriginalPicker(); // If we pressed this with the left mouse button, apply it
				break;
			case PointerEventData.InputButton.Right:
				_preset.Swatch.Remove(_preset); // If we pressed it with the right mouse button, remove it
				break;
			default:
				break;
		}
	}

	// This method get called by Unity because we implement the IPointerDownHandler
	public void OnPointerDown(PointerEventData eventData)
	{
		// If you don't implement IPointerDownHandler, Unity won't invoke OnPointerClick events.
		// As a result we can't remove this empty method without breaking our OnPointerClick logic.
	}
}

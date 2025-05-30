using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedColorPicker
{
	[AddComponentMenu("ColorPicker/Preset")]
	[RequireComponent(typeof(ColorPicker))]
	public class ColorPreset : MonoBehaviour
	{
		private ColorPicker _picker;

		private void Awake()
		{
			_picker = GetComponent<ColorPicker>();
		}

		public Color32 Color { get => _picker.CurrentColor; set => _picker.CurrentColor = value; }

		/// <summary>
		/// The swatch to which this preset belongs, this should be set by the <see cref="ColorSwatch"/> that instantiated a <see cref="ColorPreset"/> prefab.
		/// </summary>
		public ColorSwatch Swatch { get; set; }

		/// <summary>
		/// Applies the color of this preset to the color of the ColorPicker to which the <see cref="Swatch"/> belongs
		/// </summary>
		public void ApplyColorToOriginalPicker()
		{
			Swatch.Picker.CurrentColor = Color;
		}
	}
}
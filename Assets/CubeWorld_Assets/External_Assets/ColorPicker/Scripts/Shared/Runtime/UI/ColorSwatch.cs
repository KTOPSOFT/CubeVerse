using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace AdvancedColorPicker
{
	public class ColorSwatch : MonoBehaviour
	{
		[Serializable] public class PresetChangedEvent : UnityEvent<ColorPreset> { }

		[SerializeField] private ColorPicker _picker;

		[SerializeField] private ColorPreset _presetPrefab;
		[SerializeField] private RectTransform _viewport;

		[Tooltip("name of the color swatch that is automatically loaded on Start")]
		[SerializeField] private string _swatchName = "";

		[SerializeField] private bool _autoSave = true;

		[SerializeField] private PresetChangedEvent _presetAdded = new PresetChangedEvent();
		[SerializeField] private PresetChangedEvent _presetRemoved = new PresetChangedEvent();
		[SerializeField] private PresetChangedEvent _presetEdited = new PresetChangedEvent();


		private List<ColorPreset> _colorSwatches = new List<ColorPreset>();

		public int Count => _colorSwatches.Count;

		public ColorPicker Picker
		{
			get => _picker;
			set => _picker = value;
		}

		public ColorPreset PresetPrefab
		{
			get => _presetPrefab;
			set => _presetPrefab = value;
		}

		public RectTransform Viewport
		{
			get => _viewport;
			set => _viewport = value;
		}

		public bool AutoSave
		{
			get => _autoSave;
			set => _autoSave = value;
		}

		public PresetChangedEvent PresetAdded
		{
			get => _presetAdded;
			set => _presetAdded = value;
		}

		public PresetChangedEvent PresetRemoved
		{
			get => _presetRemoved;
			set => _presetRemoved = value;
		}

		public PresetChangedEvent PresetEdited
		{
			get => _presetEdited;
			set => _presetEdited = value;
		}

		/// <summary>
		/// The name of this current swatch, you can set it using <see cref="SaveSwatch(string, bool)"/> or <see cref="LoadSwatch(string, bool, bool)"/>.
		/// </summary>
		/// <remarks>
		/// You should take note when editing this via code is that you are also responsible for cleaning the PlayerPrefs unused keys.
		/// PlayerPref keys are constructed using <see cref="ColorPickerUtils.GetSwatchKey(string)"/> with this name as parameter.
		/// </remarks>
		public string SwatchName
		{
			get => _swatchName;
		}

		private void Start()
		{
			LoadSwatch(true);
		}

		public void ClearSwatch()
		{
			for (int i = 0; i < _colorSwatches.Count; i++)
			{
				_presetRemoved.Invoke(_colorSwatches[i]);
				Destroy(_colorSwatches[i].gameObject);
			}
			_colorSwatches.Clear();

			if (_autoSave)
				ColorPickerUtils.DeleteSwatch(_swatchName);
		}

		/// <summary>
		/// Returns whether the specified color is present in the current swatch
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public bool Contains(Color32 color)
		{
			return _colorSwatches.Any(x => x.Color.r == color.r && x.Color.g == color.g && x.Color.b == color.b && x.Color.a == color.a);
		}

		/// <summary>
		/// Loads the swatch with name: <see cref="SwatchName"/> from PlayerPrefs
		/// </summary>
		/// <param name="append">If true, appends loaded values to the end, else replaces current values</param>
		public void LoadSwatch(bool append)
		{
			LoadSwatch(_swatchName, append, false);
		}

		/// <summary>
		/// Loads a swatch with given <paramref name="name"/> from playerprefs
		/// </summary>
		/// <param name="name">The name of the swatch to load from PlayerPrefs</param>
		/// <param name="append">If true, appends loaded values to the end, else replaces current values with loaded values</param>
		/// <param name="setName">If true, sets <see cref="SwatchName"/> to loaded swatch name</param>
		public void LoadSwatch(string name, bool append, bool setName)
		{
			Color32[] colors = ColorPickerUtils.LoadSwatch(name);

			LoadSwatch(append, colors);

			if (setName)
			{
				_swatchName = name;
			}
		}

		/// <summary>
		/// Loads given swatch 
		/// </summary>
		/// <param name="append">If true, appends loaded values to the end, else replaces current values with loaded values</param>
		/// <param name="colors">The colors to load</param>
		public void LoadSwatch(bool append, params Color32[] colors)
		{
			// Create new or update presets
			for (int index = 0; index < colors.Length; index++)
			{
				if (append || index >= _colorSwatches.Count)
				{
					AddInternal(colors[index]);
				}
				else
				{
					ReplaceInternal(index, colors[index]);
				}
			}

			// Remove existing presets
			if (!append)
			{
				for (int i = _colorSwatches.Count - 1; i >= colors.Length; i--)
				{
					RemoveAtInternal(i);
				}
			}
		}

		/// <summary>
		/// Saves the swatch with <see cref="SwatchName"/> in PlayerPrefs with the current swatch
		/// </summary>
		public void SaveSwatch()
		{
			SaveSwatch(_swatchName, false);
		}

		/// <summary>
		/// Saves the swatch as given <paramref name="name"/> in PlayerPrefs, overwriting any existing swatches with given name
		/// </summary>
		/// <param name="name">The name of the swatch to save</param>
		/// <param name="setName">If true, sets <see cref="SwatchName"/> to saved swatch name</param>
		public void SaveSwatch(string name, bool setName)
		{
			ColorPickerUtils.SaveSwatch(name, _colorSwatches.Select(x => x.Color).ToArray());

			if (setName)
			{
				_swatchName = name;
			}
		}

		private void AddInternal(Color32 color)
		{
			ColorPreset instance = Instantiate(_presetPrefab);
			instance.Color = color;
			instance.Swatch = this;
			instance.transform.SetParent(_viewport, false);
			_colorSwatches.Add(instance);
			_presetAdded.Invoke(instance);
		}

		/// <summary>
		/// Adds the current color of <see cref="Picker"/> as a preset.
		/// </summary>
		public void Add()
		{
			Add(Picker);
		}

		/// <summary>
		/// Adds the given <paramref name="color"/> as a preset
		/// </summary>
		/// <param name="color"></param>
		public void Add(Color32 color)
		{
			AddInternal(color);

			if (_autoSave)
				SaveSwatch();
		}

		/// <summary>
		/// Adds the current color of given <paramref name="picker"/> as a preset
		/// </summary>
		/// <param name="picker"></param>
		public void Add(ColorPicker picker)
		{
			Add(picker.CurrentColor);
		}

		/// <summary>
		/// Adds given <paramref name="color"/> only if it is not already present in the current swatch
		/// </summary>
		/// <param name="color"></param>
		public void AddIfNew(Color32 color)
		{
			if (!Contains(color))
				Add(color);
		}

		/// <summary>
		/// Adds the current color of given <paramref name="picker"/> only if it is not already present in the current swatch
		/// </summary>
		/// <param name="picker"></param>
		public void AddIfNew(ColorPicker picker)
		{
			AddIfNew(picker.CurrentColor);
		}




		private void ReplaceInternal(int index, Color32 color)
		{
			_colorSwatches[index].Color = color;
			_presetEdited.Invoke(_colorSwatches[index]);
		}

		/// <summary>
		/// Replaces the color of the preset at given <paramref name="index"/> with given <paramref name="color"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="color"></param>
		public void Replace(int index, Color32 color)
		{
			ReplaceInternal(index, color);

			if (_autoSave)
				SaveSwatch();
		}

		/// <summary>
		/// Replaces the color of the preset at given <paramref name="index"/> with the color of the given <paramref name="picker"/>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="picker"></param>
		public void Replace(int index, ColorPicker picker)
		{
			Replace(index, picker.CurrentColor);
		}



		private void RemoveAtInternal(int index)
		{
			_presetRemoved.Invoke(_colorSwatches[index]);
			Destroy(_colorSwatches[index].gameObject);
			_colorSwatches.RemoveAt(index);
		}

		/// <summary>
		/// Removes the preset at given <paramref name="index"/>
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			RemoveAtInternal(index);

			if (_autoSave)
				SaveSwatch();
		}

		/// <summary>
		/// Removes the given <paramref name="preset"/>
		/// </summary>
		/// <param name="preset"></param>
		public void Remove(ColorPreset preset)
		{
			RemoveAt(_colorSwatches.IndexOf(preset));
		}
	}
}
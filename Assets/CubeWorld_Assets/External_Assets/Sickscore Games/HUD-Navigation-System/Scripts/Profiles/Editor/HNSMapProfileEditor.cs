using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Serialization;
#endif
using SickscoreGames.HUDNavigationSystem;

[CustomEditor(typeof(HNSMapProfile))]
public class HNSMapProfileEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HNSMapProfile hudTarget;
	private bool _advanced_, _customLayers_;
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HNS Map Profile";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_MapProfile", typeof(Texture2D));
		showExpandButton = false;

		hudTarget = (HNSMapProfile)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		// update serialized object
		serializedObject.Update ();

		// cache serialized properties
		SerializedProperty _pMapTexture = serializedObject.FindProperty ("MapTexture");
		SerializedProperty _pMapTextureSize = serializedObject.FindProperty ("MapTextureSize");
		SerializedProperty _pMapBackground = serializedObject.FindProperty ("MapBackground");
		SerializedProperty _pMapBounds = serializedObject.FindProperty ("MapBounds");

		// use splash texture
		if (hudTarget.MapTexture != null) {
			EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			Rect rect = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.MaxWidth (256f), GUILayout.MaxHeight (256f));
			GUI.DrawTexture (rect, hudTarget.MapTexture.texture, ScaleMode.ScaleAndCrop, true, 0f);
			GUILayout.FlexibleSpace ();
			EditorGUILayout.EndHorizontal ();
			GUILayout.Space (4); // SPACE
		}

		// CUSTOM LAYERS
		EditorGUILayout.BeginVertical (boxStyle);
		_customLayers_ = EditorGUILayout.Foldout(_customLayers_, "Custom Layers", true, foldoutStyle);
		if (_customLayers_) {
			GUILayout.Space (4); // SPACE

			if (showHelpboxes) {
				EditorGUILayout.HelpBox ("Add custom layers which you can access later via code (e.g. show/hide map overlays)", MessageType.Info);
				GUILayout.Space (4); // SPACE
			}

			// draw custom layers
			DrawCustomLayers ();
		}
		EditorGUILayout.EndVertical ();

		// ADVANCED SETTINGS
		EditorGUILayout.BeginVertical (boxStyle);
		_advanced_ = EditorGUILayout.Foldout(_advanced_, "Advanced Settings", true, foldoutStyle);
		if (_advanced_) {
			GUILayout.Space (4); // SPACE
			EditorGUILayout.HelpBox ("Don't change anything here, unless you know what you're doing!", MessageType.Warning);
			GUILayout.Space (6); // SPACE
			EditorGUILayout.PropertyField (_pMapTexture, new GUIContent ("Map Texture", "The main texture of this map profile."));
			EditorGUILayout.PropertyField (_pMapTextureSize, new GUIContent ("Map Texture Size", "The texture size of the main texture."));
			EditorGUILayout.PropertyField (_pMapBackground, new GUIContent ("Map Background", "Set the background color of the minimap mask."));
			EditorGUILayout.PropertyField (_pMapBounds, new GUIContent ("Map Bounds", "Map Bounds defined by the HNSTextureCreator component."));
		}
		EditorGUILayout.EndVertical ();

		// apply modified properties
		serializedObject.ApplyModifiedProperties ();
	}
	#endregion


	#region Utility Methods
	void DrawCustomLayers ()
	{
		// cache serialized properties
		SerializedProperty _pCustomLayers = serializedObject.FindProperty ("CustomLayers");

		// draw custom layers
		if (_pCustomLayers != null && _pCustomLayers.arraySize >= 0) {
			for (int i = 0; i < _pCustomLayers.arraySize; i++) {
				SerializedProperty _cLayer = _pCustomLayers.GetArrayElementAtIndex (i);
				EditorGUILayout.BeginVertical (boxStyle);
				EditorGUILayout.BeginHorizontal ();
				// CONTENT BEGIN
				if (hudTarget.CustomLayers.Count > 0 && hudTarget.CustomLayers [i].sprite != null) {
					Rect rect = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.MaxWidth (64f), GUILayout.MaxHeight (64f));
					GUI.DrawTexture (rect, hudTarget.CustomLayers [i].sprite.texture, ScaleMode.ScaleAndCrop, true, 0f);
					GUILayout.Space (8); // SPACE
				}

				EditorGUILayout.BeginVertical ();
				EditorGUILayout.PropertyField (_cLayer.FindPropertyRelative ("name"), new GUIContent ("Name"));
				EditorGUILayout.PropertyField (_cLayer.FindPropertyRelative ("sprite"), new GUIContent ("Texture"));
				EditorGUILayout.PropertyField (_cLayer.FindPropertyRelative ("enabled"), new GUIContent ("Enabled"));
				EditorGUILayout.EndVertical ();

				if (GUILayout.Button (new GUIContent ("X", "Delete"), GUILayout.Width (18), GUILayout.Height (18)))
					hudTarget.CustomLayers.RemoveAt (i);
				// CONTENT ENDOF
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();

				// add space after each box
				if (i != _pCustomLayers.arraySize - 1)
					GUILayout.Space (4); // SPACE
			}
		}

		GUILayout.Space (4); // SPACE

		// add element button
		if (GUILayout.Button (new GUIContent ("Add Custom Layer", "Add a new custom layer."), GUILayout.Height (20)))
			hudTarget.CustomLayers.Add (new CustomLayer ());
	}
	#endregion
}

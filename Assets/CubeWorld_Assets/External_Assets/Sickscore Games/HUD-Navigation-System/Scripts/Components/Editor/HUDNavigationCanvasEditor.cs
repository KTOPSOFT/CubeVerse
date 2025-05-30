using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SickscoreGames.HUDNavigationSystem;

[CustomEditor(typeof(HUDNavigationCanvas))]
public class HUDNavigationCanvasEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HUDNavigationCanvas hudTarget;
	private bool _radar_, _compassBar_, _indicator_, _minimap_;
	private bool _hasMissingReferences = true;
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HUD Navigation Canvas";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_Canvas", typeof(Texture2D));
		showHelpboxButton = false;

		hudTarget = (HUDNavigationCanvas)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		// update serialized object
		serializedObject.Update ();

		// cache serialized properties
		SerializedProperty _pRadar = serializedObject.FindProperty ("Radar");
		SerializedProperty _pCompassBar = serializedObject.FindProperty ("CompassBar");
		SerializedProperty _pIndicator = serializedObject.FindProperty ("Indicator");
		SerializedProperty _pMinimap = serializedObject.FindProperty ("Minimap");

		// radar references
		EditorGUILayout.BeginVertical (boxStyle);
		_radar_ = EditorGUILayout.Foldout(_radar_, "Radar References", true, foldoutStyle);
		DrawReferences (_pRadar, _radar_);
		EditorGUILayout.EndVertical ();

		GUILayout.Space (2); // SPACE

		// compass bar references
		EditorGUILayout.BeginVertical (boxStyle);
		_compassBar_ = EditorGUILayout.Foldout(_compassBar_, "Compass Bar References", true, foldoutStyle);
		DrawReferences (_pCompassBar, _compassBar_);
		EditorGUILayout.EndVertical ();

		GUILayout.Space (2); // SPACE

		// indicator references
		EditorGUILayout.BeginVertical (boxStyle);
		_indicator_ = EditorGUILayout.Foldout(_indicator_, "Indicator References", true, foldoutStyle);
		DrawReferences (_pIndicator, _indicator_);
		EditorGUILayout.EndVertical ();

		// minimap references
		EditorGUILayout.BeginVertical (boxStyle);
		_minimap_ = EditorGUILayout.Foldout(_minimap_, "Minimap References", true, foldoutStyle);
		DrawReferences (_pMinimap, _minimap_);
		EditorGUILayout.EndVertical ();

		// check for missing references
		if (_hasMissingReferences)
			EditorGUILayout.HelpBox ("References are missing!", MessageType.Error);
		_hasMissingReferences = false;

		// apply modified properties
		serializedObject.ApplyModifiedProperties ();
	}


	protected override void OnExpandSettings (bool value)
	{
		base.OnExpandSettings (value);
		_radar_ = _compassBar_ = _indicator_ = _minimap_ = value;
	}
	#endregion


	#region Utility Methods
	void DrawReferences (SerializedProperty property, bool drawProperty)
	{		
		if (drawProperty)
			GUILayout.Space (4); // SPACE

		// draw child properties
		string parentPath = property.propertyPath;
		while (property.NextVisible (true) && property.propertyPath.StartsWith (parentPath)) {
			if (drawProperty)
				EditorGUILayout.PropertyField (property);

			// check for missing object reference
			if (property.objectReferenceValue == null)
				_hasMissingReferences = true;
		}
	}
	#endregion
}

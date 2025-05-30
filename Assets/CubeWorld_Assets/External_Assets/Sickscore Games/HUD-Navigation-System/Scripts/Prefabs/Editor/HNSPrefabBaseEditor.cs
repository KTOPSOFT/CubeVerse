using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Serialization;
#endif
using SickscoreGames.HUDNavigationSystem;

public class HNSPrefabBaseEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HNSPrefab hudTarget;
	protected bool _customTransforms_;
	private static readonly string[] _excludedProperties = new string[] {"m_Script", "CustomTransforms"};
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HNS Indicator Prefab";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_Prefab", typeof(Texture2D));
		showExpandButton =  false;

		hudTarget = (HNSPrefab)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		// update serialized object
		serializedObject.Update ();

		// draw inspector
		DrawPropertiesExcluding (serializedObject, _excludedProperties);
		GUILayout.Space (8); // SPACE

		// CUSTOM TRANSFORMS
		EditorGUILayout.BeginVertical (boxStyle);
		_customTransforms_ = EditorGUILayout.Foldout(_customTransforms_, "Custom Transforms", true, foldoutStyle);
		if (_customTransforms_) {
			GUILayout.Space (4); // SPACE

			if (showHelpboxes) {
				EditorGUILayout.HelpBox ("Add custom transforms which you can access later via code (e.g. interaction texts from the example scene)", MessageType.Info);
				GUILayout.Space (4); // SPACE
			}

			// draw custom transforms
			DrawCustomTransforms ();
		}
		EditorGUILayout.EndVertical ();

		// apply modified properties
		serializedObject.ApplyModifiedProperties ();
	}
	#endregion


	#region Utility Methods
	void DrawCustomTransforms ()
	{
		// cache serialized properties
		SerializedProperty _pCustomTransforms = serializedObject.FindProperty ("CustomTransforms");

		// draw custom transforms
		if (_pCustomTransforms != null && _pCustomTransforms.arraySize >= 0) {
			for (int i = 0; i < _pCustomTransforms.arraySize; i++) {
				SerializedProperty _cTransform = _pCustomTransforms.GetArrayElementAtIndex (i);
				EditorGUILayout.BeginVertical (boxStyle);
				EditorGUILayout.BeginHorizontal ();
				// CONTENT BEGIN
				EditorGUILayout.BeginVertical ();
				EditorGUILayout.PropertyField (_cTransform.FindPropertyRelative ("name"), new GUIContent ("Name"));
				EditorGUILayout.PropertyField (_cTransform.FindPropertyRelative ("transform"), new GUIContent ("Transform"));
				EditorGUILayout.EndVertical ();

				if (GUILayout.Button (new GUIContent ("X", "Delete"), GUILayout.Width (18), GUILayout.Height (18)))
					hudTarget.CustomTransforms.RemoveAt (i);
				// CONTENT ENDOF
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();

				// add space after each box
				if (i != _pCustomTransforms.arraySize - 1)
					GUILayout.Space (4); // SPACE
			}
		}

		GUILayout.Space (4); // SPACE

		// add element button
		if (GUILayout.Button (new GUIContent ("Add Custom Transform", "Add a new custom transform."), GUILayout.Height (20)))
			hudTarget.CustomTransforms.Add (new CustomTransform ());
	}
	#endregion
}

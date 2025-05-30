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

[CustomEditor(typeof(HUDNavigationElement))]
public class HUDNavigationElementEditor : HUDNavigationElementBaseEditor
{
	#region Variables
	protected HUDNavigationElement hudTarget;
	private bool _events_;
	#endregion


	#region Main Methods
	protected override void OnEnable ()
	{
		base.OnEnable ();

		editorTitle = "HUD Navigation Element";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_Element", typeof(Texture2D));

		hudTarget = (HUDNavigationElement)target;
	}


	protected override void OnChildInspectorGUI ()
	{
		base.OnChildInspectorGUI ();

		// cache serialized properties
		SerializedProperty _pSettings = serializedObject.FindProperty ("Settings");

		// SETTINGS ASSET
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PropertyField (_pSettings, new GUIContent ("Settings Asset", "(optional) Use a settings asset instead."));
		if (showHelpboxes && GUILayout.Button (new GUIContent ("?", "Instructions to create settings assets."), GUILayout.Width (16), GUILayout.Height (16)))
			EditorUtility.DisplayDialog ("Settings Assets", "Settings Assets are used to share settings between multiple element.\n\nTo create a new settings asset, right-click in the project window and select:\n'Create > " + HNS.PublisherName + " > " + HNS.Name + " > New Element Settings'.", "OK");
		EditorGUILayout.EndHorizontal ();

		// hide settings?
		_hideSettings = _pSettings.objectReferenceValue != null;
		if (!_hideSettings)
			GUILayout.Space (8); // SPACE
	}


	protected override void OnChildEndInspectorGUI ()
	{
		base.OnChildEndInspectorGUI ();

		GUILayout.Space (4); // SPACE

		// cache serialized properties
		SerializedProperty _pOnElementReadyEvent = serializedObject.FindProperty ("OnElementReady");
		SerializedProperty _pOnElementUpdateEvent = serializedObject.FindProperty ("OnElementUpdate");
		SerializedProperty _pOnAppearEvent = serializedObject.FindProperty ("OnAppear");
		SerializedProperty _pOnDisappearEvent = serializedObject.FindProperty ("OnDisappear");
		SerializedProperty _pOnEnterRadiusEvent = serializedObject.FindProperty ("OnEnterRadius");
		SerializedProperty _pOnLeaveRadiusEvent = serializedObject.FindProperty ("OnLeaveRadius");

		// EVENTS
		EditorGUILayout.BeginVertical (boxStyle);
		_events_ = EditorGUILayout.Foldout(_events_, "Element Events", true, foldoutStyle);
		if (_events_) {
			GUILayout.Space (4); // SPACE
			// CONTENT BEGIN
			EditorGUILayout.PropertyField (_pOnElementReadyEvent, new GUIContent ("OnElementReady"), true);
			EditorGUILayout.PropertyField (_pOnElementUpdateEvent, new GUIContent ("OnElementUpdate"), true);
			EditorGUILayout.PropertyField (_pOnAppearEvent, new GUIContent ("OnAppear"), true);
			EditorGUILayout.PropertyField (_pOnDisappearEvent, new GUIContent ("OnDisappear"), true);
			EditorGUILayout.PropertyField (_pOnEnterRadiusEvent, new GUIContent ("OnEnterRadius"), true);
			EditorGUILayout.PropertyField (_pOnLeaveRadiusEvent, new GUIContent ("OnLeaveRadius"), true);
			// CONTENT ENDOF
		}
		EditorGUILayout.EndVertical ();
	}
	#endregion
}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SickscoreGames.HUDNavigationSystem;

[CustomEditor(typeof(HUDNavigationSceneManager))]
public class HUDNavigationSceneManagerEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HUDNavigationSceneManager hudTarget;
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HUD Navigation SceneManager";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_SceneManager", typeof(Texture2D));
		showExpandButton = false;

		hudTarget = (HUDNavigationSceneManager)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		// update serialized object
		serializedObject.Update ();

		// cache serialized properties
		SerializedProperty _pConfigurations = serializedObject.FindProperty ("Configurations");

		// show helpbox
		if (showHelpboxes) {
			EditorGUILayout.HelpBox ("Assign a scene asset and a configuration, which will be applied, when the scene is loaded.", MessageType.Info);
			EditorGUILayout.HelpBox ("To create a new configuration, right click in the project window and select 'Create > Sickscore Games > HUD Navigation System > New Scene Configuration'", MessageType.Info);
		}
		
		// draw configurations
		if (_pConfigurations != null && _pConfigurations.arraySize >= 0) {
			for (int i = 0; i < _pConfigurations.arraySize; i++) {
				SerializedProperty _cConfig = _pConfigurations.GetArrayElementAtIndex (i);
				SerializedProperty _pDisabledInScene = _cConfig.FindPropertyRelative ("_DisabledInScene");

				EditorGUILayout.BeginVertical (boxStyle);
				EditorGUILayout.BeginHorizontal ();
				// CONTENT BEGIN
				EditorGUILayout.BeginVertical ();

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.PropertyField (_pDisabledInScene, new GUIContent ("Disabled In Scene"));
				EditorGUILayout.LabelField ((_pDisabledInScene.boolValue) ? "DISABLED" : "ENABLED", (_pDisabledInScene.boolValue) ? disabledStyle : enabledStyle, GUILayout.Width (100));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.PropertyField (_cConfig.FindPropertyRelative ("_Scene"), new GUIContent ("Scene"));

				if (!_pDisabledInScene.boolValue)
					EditorGUILayout.PropertyField (_cConfig.FindPropertyRelative ("_Config"), new GUIContent ("Config"));
				
				EditorGUILayout.EndVertical ();

				if (GUILayout.Button (new GUIContent ("X", "Delete"), GUILayout.Width (18), GUILayout.Height (18)))
					hudTarget.Configurations.RemoveAt (i);
				// CONTENT ENDOF
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.EndVertical ();

				// add space after each box
				if (i != _pConfigurations.arraySize - 1)
					GUILayout.Space (4); // SPACE
			}
		}

		GUILayout.Space (4); // SPACE

		// add element button
		if (GUILayout.Button (new GUIContent ("Add Configuration", "Add a new configuration entry."), GUILayout.Height (20))) {
			if (hudTarget.Configurations == null || hudTarget.Configurations.Count <= 0)
				hudTarget.Configurations = new List<Configuration> ();
			hudTarget.Configurations.Add (new Configuration ());
		}

		// apply modified properties
		serializedObject.ApplyModifiedProperties ();
	}


	protected override void OnExpandSettings (bool value)
	{
		base.OnExpandSettings (value);
	}
	#endregion


	#region HNSSceneAsset Property Drawer
	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(HNSSceneAsset))]
	public class HNSSceneAssetPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty _pSceneAsset = property.FindPropertyRelative ("_sceneAsset");
			SerializedProperty _pPath = property.FindPropertyRelative ("_path");

			EditorGUI.BeginProperty (position, GUIContent.none, property);
			EditorGUI.BeginChangeCheck ();
			var assignedSceneAsset = EditorGUI.ObjectField (position, label, _pSceneAsset.objectReferenceValue, typeof(SceneAsset), false);
			if (EditorGUI.EndChangeCheck ()) {
				_pSceneAsset.objectReferenceValue = assignedSceneAsset;
				if (_pSceneAsset.objectReferenceValue == null)
					_pPath.stringValue = string.Empty;
			}
			EditorGUI.EndProperty ();
		}
	}
	#endif
	#endregion
}

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

[CustomEditor(typeof(HUDNavigationElementSettings))]
public class HUDNavigationElementSettingsEditor : HUDNavigationElementBaseEditor
{
	#region Variables
	protected HUDNavigationElementSettings hudTarget;
	protected GameObject settingsReference;
	#endregion


	#region Main Methods
	protected override void OnEnable ()
	{
		base.OnEnable ();

		editorTitle = "HUD Navigation Settings";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_Settings", typeof(Texture2D));

		hudTarget = (HUDNavigationElementSettings)target;
	}


	protected override void OnChildInspectorGUI ()
	{
		base.OnChildInspectorGUI ();

		// COPY SETTINGS
		EditorGUILayout.BeginVertical (boxStyle);
		GUILayout.Space (4); // SPACE
		settingsReference = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("Copy From", "Assign the GameObject from which you want to extract the settings."), settingsReference, typeof(GameObject), true);
		if (settingsReference != null) {
			GUILayout.Space (4); // SPACE
			HUDNavigationElement element = settingsReference.GetComponent<HUDNavigationElement> ();
			if (element != null) {
				// show paste button
				if (GUILayout.Button ("Copy Settings", GUILayout.Height (18)))
					hudTarget.CopySettings (element);
			} else {
				// show error message
				EditorGUILayout.HelpBox ("No HUDNavigationElement component found on GameObject.", MessageType.Error);
			}
		}
		GUILayout.Space (4); // SPACE
		EditorGUILayout.EndVertical ();

		GUILayout.Space (8); // SPACE
	}


	protected override void OnChildEndInspectorGUI ()
	{
		base.OnChildEndInspectorGUI ();
	}
	#endregion
}

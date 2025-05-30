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

[CustomEditor(typeof(HNSPlayerController))]
public class HNSPlayerControllerEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HNSPlayerController hudTarget;
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HNS Player Controller";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_PlayerController", typeof(Texture2D));
		showHelpboxButton = showExpandButton = false;

		hudTarget = (HNSPlayerController)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		EditorGUILayout.HelpBox ("This GameObject will be automatically assigned as the Player Controller.", MessageType.Info);
	}


	protected override void OnExpandSettings (bool value)
	{
		base.OnExpandSettings (value);
	}
	#endregion


	#region Utility Methods
	#endregion
}

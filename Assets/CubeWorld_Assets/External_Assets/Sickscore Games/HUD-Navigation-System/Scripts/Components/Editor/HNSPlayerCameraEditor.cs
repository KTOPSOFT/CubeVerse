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

[CustomEditor(typeof(HNSPlayerCamera))]
public class HNSPlayerCameraEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HNSPlayerCamera hudTarget;
	#endregion


	#region Main Methods
	void OnEnable ()
	{
		editorTitle = "HNS Player Camera";
		splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_PlayerCamera", typeof(Texture2D));
		showHelpboxButton = showExpandButton = false;

		hudTarget = (HNSPlayerCamera)target;
	}


	protected override void OnBaseInspectorGUI ()
	{
		EditorGUILayout.HelpBox ("This GameObject will be automatically assigned as the Player Camera.", MessageType.Info);
		if (hudTarget.GetComponent<Camera> () == null)
			EditorGUILayout.HelpBox ("Could not find a camera component on this GameObject!", MessageType.Warning);
	}


	protected override void OnExpandSettings (bool value)
	{
		base.OnExpandSettings (value);
	}
	#endregion


	#region Utility Methods
	#endregion
}

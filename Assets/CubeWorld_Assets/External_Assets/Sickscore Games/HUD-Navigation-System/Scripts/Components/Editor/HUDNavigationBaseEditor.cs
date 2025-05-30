using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using SickscoreGames.HUDNavigationSystem;

public class HUDNavigationBaseEditor : Editor
{
	#region Variables
	public string editorTitle = string.Empty;
	public GUIStyle titleStyle, subtitleStyle, headerStyle, subHeaderStyle, foldoutStyle, richLabelStyle, wrapperStyle, boxStyle, enabledStyle, disabledStyle;
	protected Texture2D splashTexture;
	protected bool expandSettings = false;
	protected bool showHelpboxes = true;

	protected bool showExpandButton = true;
	protected bool showHelpboxButton = true;

	protected string versionText = string.Format("{0} {1}", HNS.Name, HNS.Version);

	private const string _showHelpboxesPrefs = "SickscoreGames_HNS_ShowHelpbox";
	#endregion


	#region Main Methods
	void Awake ()
	{
		showHelpboxes = EditorPrefs.GetBool (_showHelpboxesPrefs, !showHelpboxes);
	}


	public override void OnInspectorGUI ()
	{
		// setup custom styles
		if (titleStyle == null) {
			titleStyle = new GUIStyle (GUI.skin.label);
			titleStyle.fontSize = 20;
			titleStyle.fontStyle = FontStyle.Normal;
			titleStyle.alignment = TextAnchor.MiddleCenter;
		}

		if (subtitleStyle == null) {
			subtitleStyle = new GUIStyle (titleStyle);
			subtitleStyle.fontSize = 11;
			subtitleStyle.fontStyle = FontStyle.Normal;
		}

		if (headerStyle == null) {
			headerStyle = new GUIStyle (GUI.skin.label);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.alignment = TextAnchor.UpperLeft;
		}

		if (subHeaderStyle == null) {
			subHeaderStyle = new GUIStyle (GUI.skin.label);
			subHeaderStyle.fontStyle = FontStyle.Normal;
			subHeaderStyle.alignment = TextAnchor.UpperLeft;
		}

		if (foldoutStyle == null) {
            foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.fontStyle = FontStyle.Bold;
			foldoutStyle.margin.left += 12;
			foldoutStyle.padding.left += 3;
		}

		if (richLabelStyle == null) {
			richLabelStyle = new GUIStyle (GUI.skin.label);
			richLabelStyle.richText = true;
		}

		if (wrapperStyle == null) {
			wrapperStyle = new GUIStyle (GUI.skin.box);
			wrapperStyle.normal.textColor = GUI.skin.label.normal.textColor;
			wrapperStyle.padding = new RectOffset (8, 8, 8, 8);
		}

		if (boxStyle == null) {
			boxStyle = new GUIStyle (GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (enabledStyle == null) {
			enabledStyle = new GUIStyle (GUI.skin.label);
			enabledStyle.fontSize = 12;
			enabledStyle.fontStyle = FontStyle.Bold;
			enabledStyle.alignment = TextAnchor.MiddleCenter;
			enabledStyle.normal.textColor = Color.green;
		}
		if (disabledStyle == null) {
			disabledStyle = new GUIStyle (enabledStyle);
			disabledStyle.normal.textColor = Color.red;
		}

		// SPLASH
		if (splashTexture != null) {
			// use splash texture
			GUILayoutUtility.GetRect (1f, 3f, GUILayout.ExpandWidth (false));
			Rect rect = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.Height (100f));
			GUI.DrawTexture (rect, splashTexture, ScaleMode.ScaleAndCrop, true, 0f);
		} else if (editorTitle != null) {
			// draw editor title instead
			EditorGUILayout.BeginVertical (boxStyle);
			EditorGUILayout.LabelField (editorTitle, titleStyle, GUILayout.Height (40));
			EditorGUILayout.EndVertical ();
		}

		// -- BEGIN CHILD WRAPPER --
		EditorGUILayout.BeginVertical (wrapperStyle);

		// render child content
		OnBaseInspectorGUI ();

		// -- END CHILD WRAPPER --
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical (boxStyle);
		GUILayout.Space (2); // SPACE
		EditorGUILayout.BeginHorizontal ();
		
		// expand / collapse settings
		if (showExpandButton) {
			if (GUILayout.Button (new GUIContent ((expandSettings) ? "-" : "+", "Expand / Collapse all settings."), GUILayout.Width (20), GUILayout.Height (20))) {
				expandSettings = !expandSettings;
				this.OnExpandSettings (expandSettings);
			}
		} else {
			GUILayout.Space (20); // SPACE
		}

		// version info
		EditorGUILayout.LabelField (versionText, subtitleStyle, GUILayout.Height (20));

		// toggle helpboxes
		if (showHelpboxButton) {
			Color bgColor = GUI.backgroundColor;
			GUI.backgroundColor = (showHelpboxes) ? Color.gray : Color.white;
			if (showHelpboxButton && GUILayout.Button (new GUIContent ("?", "(GLOBAL) Toggle helpboxes and detailled informations."), GUILayout.Width (20), GUILayout.Height (20))) {
				showHelpboxes = !showHelpboxes;
				EditorPrefs.SetBool (_showHelpboxesPrefs, showHelpboxes);
			}
			GUI.backgroundColor = bgColor;
		} else {
			GUILayout.Space (20); // SPACE
		}
		EditorGUILayout.EndHorizontal ();
		GUILayout.Space (2); // SPACE
		EditorGUILayout.EndVertical ();

		// set editor dirty
		// EditorUtility.SetDirty (target);
	}


	protected virtual void OnBaseInspectorGUI () {}
	protected virtual void OnExpandSettings (bool value) {}
	#endregion
}

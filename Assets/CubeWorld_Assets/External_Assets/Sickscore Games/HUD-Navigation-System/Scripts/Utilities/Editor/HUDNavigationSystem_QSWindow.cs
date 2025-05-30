using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	public class HUDNavigationSystem_QSWindow : EditorWindow
	{
		#region Variables
		private static HUDNavigationSystem_QSWindow window;
		protected GUIStyle titleStyle, subtitleStyle, wrapperStyle;
		protected Texture2D splashTexture;
		private GameObject goPlayer;
		private Camera goCamera;
		#endregion


		#region Main Methods
		[MenuItem("Window/" + HNS.PublisherName + "/" + HNS.Name + "/Quick Setup Window", false, 0)]
		public static void ShowWindow ()
		{
			window = EditorWindow.GetWindow<HUDNavigationSystem_QSWindow> ("HNS Quick Setup") as HUDNavigationSystem_QSWindow;
			window.minSize = new Vector2 (400, 228);
			window.maxSize = new Vector2 (550, 300);
			window.Show ();
		}


		void OnEnable ()
		{
			splashTexture = (Texture2D)Resources.Load ("Textures/splashTexture_QuickSetup", typeof(Texture2D));
		}


		void OnGUI ()
		{
			// setup custom styles
			if (titleStyle == null) {
				titleStyle = new GUIStyle (GUI.skin.label);
				titleStyle.fontSize = 20;
				titleStyle.fontStyle = FontStyle.Bold;
				titleStyle.alignment = TextAnchor.MiddleCenter;
			}

			if (subtitleStyle == null) {
				subtitleStyle = new GUIStyle (titleStyle);
				subtitleStyle.fontSize = 12;
				subtitleStyle.fontStyle = FontStyle.Italic;
			}

			if (wrapperStyle == null) {
				wrapperStyle = new GUIStyle (GUI.skin.box);
				wrapperStyle.normal.textColor = GUI.skin.label.normal.textColor;
				wrapperStyle.padding = new RectOffset (8, 8, 16, 8);
			}

			// SPLASH
			if (splashTexture != null) {
				GUILayoutUtility.GetRect (1f, 3f, GUILayout.ExpandWidth (false));
				Rect rect = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.Height (100f));
				GUI.DrawTexture (rect, splashTexture, ScaleMode.ScaleAndCrop, true, 0f);
			}

			// -- BEGIN WRAPPER --
			EditorGUILayout.BeginVertical (wrapperStyle);

			// CONTENT
			goPlayer = (GameObject)EditorGUILayout.ObjectField ("Player Controller", goPlayer, typeof(GameObject), true);
			goCamera = (Camera)EditorGUILayout.ObjectField ("Player Camera", goCamera, typeof(Camera), true);

			EditorGUILayout.Separator ();

			GUI.enabled = goPlayer != null && goCamera != null;
			if (GUILayout.Button ("START QUICK SETUP", GUILayout.Height (50)))
				StartQuickSetup ();
			GUI.enabled = true;

			// -- END WRAPPER --
			EditorGUILayout.EndVertical ();
		}
		#endregion


		#region Utility Methods
		void StartQuickSetup ()
		{
			if (goPlayer == null || goCamera == null)
				return;

			// add hud navigation system to scene
			HUDNavigationSystem hudSystem = GameObject.FindObjectOfType<HUDNavigationSystem> ();
			if (hudSystem == null) {
				GameObject hnsGO = new GameObject("[HUD Navigation System]");
				hudSystem = hnsGO.AddComponent<HUDNavigationSystem> ();
			}

			// add scene manager to scene
			HUDNavigationSceneManager sceneManager = GameObject.FindObjectOfType<HUDNavigationSceneManager> ();
			if (sceneManager == null)
				hudSystem.gameObject.AddComponent<HUDNavigationSceneManager> ();

			// assign references
			hudSystem.PlayerController = goPlayer.transform;
			hudSystem.PlayerCamera = goCamera;

			// add hud navigation canvas to scene
			HUDNavigationCanvas hudCanvas = GameObject.FindObjectOfType<HUDNavigationCanvas> ();
			if (hudCanvas == null) {
				// add canvas prefab from assets to scene
				GameObject hudPrefab = Resources.Load ("Prefabs/HUD Navigation Canvas") as GameObject;
				if (hudPrefab != null) {
					GameObject hudGO = Instantiate (hudPrefab) as GameObject;
					hudGO.name = "[HUD Navigation Canvas]";
				}
			}

			// add hns player to player transform
			HNSPlayerController playerController = goPlayer.GetComponent<HNSPlayerController> ();
			if (playerController == null)
				goPlayer.AddComponent<HNSPlayerController> ();

			// add hns camera to player camera
			HNSPlayerCamera playerCamera = goCamera.gameObject.GetComponent<HNSPlayerCamera> ();
			if (playerCamera == null)
				goCamera.gameObject.AddComponent<HNSPlayerCamera> ();

			// console output
			Debug.LogFormat ("'{0}' was successfully added to the scene.", HNS.Name);

			// close editor window
			window.Close ();
		}
		#endregion
	}
}

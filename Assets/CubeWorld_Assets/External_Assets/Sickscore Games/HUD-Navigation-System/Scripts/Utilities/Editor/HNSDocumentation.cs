using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	public class HNSDocumentation : EditorWindow
	{
		#region Variables
		private static HNSDocumentation window;
		protected GUIStyle wrapperStyle;
		protected Texture2D splashTexture;

		private Texture2D btnOnlineTexture, btnOfflineTexture;
		#endregion


		#region Main Methods
		[MenuItem("Window/" + HNS.PublisherName + "/" + HNS.Name + "/Documentation", false, 0)]
		public static void ShowWindow()
		{
			window = EditorWindow.GetWindow<HNSDocumentation>("HNS Documentation") as HNSDocumentation;
			window.minSize = new Vector2(400, 262);
			window.maxSize = new Vector2(500, 280);
			window.Show();
		}


		void OnEnable()
		{
			splashTexture = (Texture2D)Resources.Load("Textures/splashTexture_Documentation", typeof(Texture2D));
			btnOnlineTexture = (Texture2D)Resources.Load("Textures/buttonTexture_DocsOnline", typeof(Texture2D));
			btnOfflineTexture = (Texture2D)Resources.Load("Textures/buttonTexture_DocsOffline", typeof(Texture2D));
		}


		void OnGUI()
		{
			// setup custom styles
			if (wrapperStyle == null)
			{
				wrapperStyle = new GUIStyle(GUI.skin.box);
				wrapperStyle.normal.textColor = GUI.skin.label.normal.textColor;
				wrapperStyle.padding = new RectOffset(8, 8, 16, 16);
			}

			// SPLASH
			if (splashTexture != null)
			{
				GUILayoutUtility.GetRect(1f, 3f, GUILayout.ExpandWidth(false));
				Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100f));
				GUI.DrawTexture(rect, splashTexture, ScaleMode.ScaleAndCrop, true, 0f);
			}

			// -- BEGIN WRAPPER --
			EditorGUILayout.BeginHorizontal(wrapperStyle, GUILayout.ExpandWidth(true));

			// CONTENT
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button(new GUIContent(btnOnlineTexture), GUILayout.Width(128), GUILayout.Height(128)))
				ShowDocumentationOnline ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button(new GUIContent(btnOfflineTexture), GUILayout.Width(128), GUILayout.Height(128)))
				ShowDocumentationOffline ();
			GUILayout.FlexibleSpace ();

			// -- END WRAPPER --
			EditorGUILayout.EndVertical();

			GUILayout.Space(8); // SPACE
		}
		#endregion


		#region Utility Methods
		void ShowDocumentationOnline()
		{
			// open online documentation
			Application.OpenURL("http://docs.sickscore.games/hud-navigation-system/");

			// close editor window
			window.Close();
		}


		void ShowDocumentationOffline()
		{
			// open online documentation
			string publisherPath = Path.Combine(Application.dataPath, HNS.PublisherName);
			string assetPath = Path.Combine(publisherPath, HNS.Name.Replace(' ', '-'));
			string docsPath = Path.Combine(assetPath, "Documentation.pdf");
			if (File.Exists(docsPath)) {
				// highlight profile in project window
				EditorUtility.FocusProjectWindow();
				string relativeDocsPath = docsPath.Substring(docsPath.IndexOf("Assets"));
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(relativeDocsPath) as Object;
			} else if (EditorUtility.DisplayDialog("Documentation missing!", "Offline documentation could not be found!", "View Online", "Cancel"))
				ShowDocumentationOnline();

			// close editor window
			window.Close();
		}
		#endregion
	}
}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[InitializeOnLoad]
	public class HNSEditorIcons
	{
		#region Variables
		private static Texture2D texturePanel;
		#endregion


		#region Main Methods
		static HNSEditorIcons ()
		{
			EditorApplication.hierarchyWindowItemOnGUI += HNSIcon;
			EditorApplication.hierarchyWindowItemOnGUI += HNSCanvasIcon;
		}


		static void HNSIcon (int instanceId, Rect selectionRect)
		{
			CheckInstance<HUDNavigationSystem> (instanceId, selectionRect, "hns_icon");
		}


		static void HNSCanvasIcon (int instanceId, Rect selectionRect)
		{
			CheckInstance<HUDNavigationCanvas> (instanceId, selectionRect, "hns_icon_canvas");
		}
		#endregion


		#region Utility Methods
		private static void CheckInstance<T> (int instanceId, Rect selectionRect, string iconName) where T : UnityEngine.Object
		{
			GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			if (go == null) return;
			if (go.GetComponent<T> () != null)
				DrawIcon(iconName, selectionRect);
		}


		private static void DrawIcon (string name, Rect rect)
		{
			Rect texRect = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);
			Texture2D tex = (Texture2D)Resources.Load("Textures/Icons/" + name);
			if (tex != null)
				GUI.DrawTexture (texRect, tex);
		}
		#endregion
	}
}

using System.IO;
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
	public class HNSMapProfile : ScriptableObject
	{
		#region Variables
		public Sprite MapTexture;
		public Vector2 MapTextureSize;
		public Color MapBackground = Color.black;
		public Bounds MapBounds = new Bounds (Vector3.zero, Vector3.one);

		[HideInInspector]
		public List<CustomLayer> CustomLayers = new List<CustomLayer> ();
		#endregion


		#region Main Methods
		public void Init (Sprite mapTexture, Vector2 mapTextureSize, Color mapBackground, Bounds mapBounds)
		{
			this.MapTexture = mapTexture;
			this.MapTextureSize = new Vector2 ((int)mapTextureSize.x, (int)mapTextureSize.y);
			this.MapBackground = mapBackground;
			this.MapBounds = mapBounds;
		}


		/// <summary>
		/// Gets a custom layer.
		/// </summary>
		/// <returns>Custom layer gameobject.</returns>
		/// <param name="name">Unique Name.</param>
		public GameObject GetCustomLayer (string name)
		{
			CustomLayer custom = CustomLayers.FirstOrDefault (cl => cl.name.Equals (name));
			if (custom != null)
				return custom.instance;

			return null;
		}
		#endregion
	}


	#region Subclasses
	[System.Serializable]
	public class CustomLayer
	{
		[Tooltip("Enter a unique name for this layer.")]
		public string name;
		[Tooltip("Assign the sprite texture you want to add.")]
		public Sprite sprite;
		[Tooltip("If checked, this layer will be enabled by default.")]
		public bool enabled = false;

		[HideInInspector]
		public GameObject instance;
	}
	#endregion


	public static class HNSMapProfileUtilities
	{
		#if UNITY_EDITOR
		public static void CreateProfile (string mapTexturePath, Vector2 mapTextureSize, Color mapBackground, Bounds mapBounds)
		{
			// check given path
			if (mapTexturePath.Length <= 0)
				return;
			
			// get map texture
			Sprite mapTexture = AssetDatabase.LoadAssetAtPath<Sprite> (mapTexturePath);

			// create map profile
			HNSMapProfile profile = ScriptableObject.CreateInstance<HNSMapProfile> ();
			profile.Init (mapTexture, mapTextureSize, mapBackground, mapBounds);

			// create asset & save changes
			string path = Path.Combine (Path.GetDirectoryName (mapTexturePath), Path.GetFileNameWithoutExtension (mapTexturePath).Replace ("_Map", "")).Replace ('\\', '/');
			string profilePath = AssetDatabase.GenerateUniqueAssetPath (path + "_Profile.asset");
			AssetDatabase.CreateAsset (profile, profilePath);
			AssetDatabase.SaveAssets ();

			// highlight profile in project window
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = profile;
		}
		#endif
	}
}

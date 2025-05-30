using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	#if (UNITY_EDITOR)
	[ExecuteInEditMode]
	public class HNSTextureCreator : MonoBehaviour
	{
		#region Variables
		public string MapName = "New_Map";

		public _TextureSize TextureSize = _TextureSize._2048;
		public Color Background = Color.black;

		public GameObject FitToObject;
		public float ObjectBoundsMultiplier = 1f;

		public bool PreviewShowBounds = true;
		public Color PreviewBoundsColor = new Color (0f, 0f, 1f, .3f);


		[HideInInspector]
		public Bounds MapBounds = new Bounds (Vector3.zero, new Vector3 (500f, 100f, 500f));

		[HideInInspector]
		public Camera RenderCamera;

		[HideInInspector]
		public RenderTexture CameraRenderTexture;

		public bool _isBusy = false;
		#endregion


		#region Main Methods
		[MenuItem("Window/" + HNS.PublisherName + "/" + HNS.Name + "/Map Texture Creator", false, 11)]
		public static void InitTextureCreator ()
		{
			// find / instantiate TextureCreator
			HNSTextureCreator textureCreator = FindObjectOfType<HNSTextureCreator> ();
			if (textureCreator == null)
				textureCreator = new GameObject ("HNS MapTextureCreator").AddComponent<HNSTextureCreator> ();

			// set TextureCreator as active gameobject
			Selection.activeGameObject = textureCreator.gameObject;
		}


		protected virtual void OnEnable ()
		{
			// prevent issues in play mode
			if (Application.isPlaying)
				DestroyImmediate (this.gameObject);
		}


		protected virtual void OnDisable ()
		{
			// destroy preview camera
			if (RenderCamera != null)
				DestroyImmediate (RenderCamera.gameObject);

			// destroy render texture
			if (CameraRenderTexture != null)
				DestroyImmediate (CameraRenderTexture);
		}


		public virtual void FitToBounds (GameObject boundsObj)
		{
			if (boundsObj == null)
				return;

			// store object's position / scale
			Vector3 objPosition = boundsObj.transform.position;
			Vector3 objScale = boundsObj.transform.localScale;

			// get bounds from object
			Terrain terrain = boundsObj.GetComponent<Terrain> ();
			if (terrain != null) {
				// use terrain size
				objPosition += terrain.terrainData.size / 2f;
				objScale = terrain.terrainData.size;
			} else {
				// get mesh renderer
				MeshRenderer meshRenderer = boundsObj.GetComponent<MeshRenderer> ();
				if (meshRenderer != null) {
					// use mesh renderer bounds
					objPosition = meshRenderer.bounds.center;
					objScale = meshRenderer.bounds.size;
				} else {
					// throw error if object has no mesh renderer
					Debug.LogErrorFormat ("No MeshRenderer found on object '{0}'", boundsObj.name);
					return;
				}
			}

			// multiply bounds scale
			if (ObjectBoundsMultiplier != 0f)
				objScale = new Vector3 (objScale.x * ObjectBoundsMultiplier, objScale.y, objScale.z * ObjectBoundsMultiplier);

			// update bounds center/size
			MapBounds.center = objPosition;
			MapBounds.size = objScale;
		}


		public virtual void CreateRenderCamera ()
		{
			// create render camera
			if (RenderCamera == null) {
				// create camera gameobject
				GameObject cameraGO = new GameObject ("HNS MapTextureCreator Camera", typeof(Camera));
				cameraGO.transform.rotation = Quaternion.Euler (90f, 0f, 0f);
				cameraGO.transform.SetParent (this.transform);
				RenderCamera = cameraGO.GetComponent<Camera> ();
			}

			// setup render camera
			RenderCamera.clearFlags = CameraClearFlags.SolidColor;
			RenderCamera.backgroundColor = Background;
			RenderCamera.orthographic = true;
			RenderCamera.depth = -100f;
			
			// update orthographic size
			RenderCamera.orthographicSize = Mathf.Min (MapBounds.size.x, MapBounds.size.z) / 2f;
			if (MapBounds.size.x < MapBounds.size.z)
				RenderCamera.orthographicSize = Mathf.Max (MapBounds.size.x, MapBounds.size.z) / 2f;

			// update camera position
			RenderCamera.transform.position = new Vector3 (MapBounds.center.x, MapBounds.max.y + .1f, MapBounds.center.z);

			// update clipping planes
			RenderCamera.nearClipPlane = .1f;
			RenderCamera.farClipPlane = RenderCamera.transform.position.y - MapBounds.min.y + .1f;

			// update camera background
			RenderCamera.backgroundColor = Background;

			// make sure it always has a target texture
			if (RenderCamera.targetTexture == null && CameraRenderTexture != null)
				RenderCamera.targetTexture = CameraRenderTexture;
		}


		public virtual void DestroyRenderCamera ()
		{
			if (RenderCamera == null)
				return;
			
			DestroyImmediate (RenderCamera.gameObject);
		}


		public virtual void UpdateRenderTexture ()
		{
			// check if we need to update the render texture
			Vector2 renderSize = GetTotalTextureSize ();
			if (CameraRenderTexture == null || CameraRenderTexture.width != renderSize.x || CameraRenderTexture.height != renderSize.y) {
				// release render texture from camera
				if (RenderCamera != null && RenderCamera.targetTexture != null)
					RenderCamera.targetTexture.Release ();

				// create render texture with new dimensions
				CameraRenderTexture = new RenderTexture ((int)renderSize.x, (int)renderSize.y, 24, RenderTextureFormat.ARGB32);

				// assign new render texture to camera
				if (RenderCamera != null)
					RenderCamera.targetTexture = CameraRenderTexture;
			}
		}


		public void CreateMapTexture ()
		{
			_isBusy = true;
			StartCoroutine (CreateMapTextureRoutine ());
		}


		public Vector2 GetTotalTextureSize ()
		{
			Vector3 intMapBounds = new Vector3 ((int)MapBounds.size.x, (int)MapBounds.size.y, (int)MapBounds.size.z);
			int min = Mathf.Min ((int)intMapBounds.x, (int)intMapBounds.z);
			int max = Mathf.Max ((int)intMapBounds.x, (int)intMapBounds.z);
			int textureSize = (int)TextureSize;
			int smallSide = Mathf.RoundToInt (textureSize / ((float)max / (float)min));

			Vector2 finalSize = new Vector2 (textureSize, smallSide);
			if (intMapBounds.x < intMapBounds.z)
				finalSize = new Vector2 (smallSide, textureSize);

			return finalSize;
		}


		public void FixPotDimensions ()
		{
			if (MapBounds.size.x < MapBounds.size.z)
				MapBounds.size = new Vector3 (MapBounds.size.z, MapBounds.size.y, MapBounds.size.z);
			else
				MapBounds.size = new Vector3 (MapBounds.size.x, MapBounds.size.y, MapBounds.size.x);
		}
		#endregion


		#region Utility Methods
		IEnumerator CreateMapTextureRoutine ()
		{
			// create render camera
			CreateRenderCamera ();

			// update render texture
			UpdateRenderTexture ();

			// create map texture
			string mapTexturePath = GetTexturePath ();
			Vector2 mapTextureSize = GetTotalTextureSize ();
			Texture2D mapTexture = new Texture2D ((int)mapTextureSize.x, (int)mapTextureSize.y, TextureFormat.RGB24, false);

			// TextureCreator
			try {
				while (_isBusy) {
					// manually render camera view
					RenderCamera.Render ();

					// set active render texture
					RenderTexture.active = CameraRenderTexture;

					// read pixels from current tile
					mapTexture.ReadPixels (new Rect (Vector2.zero, mapTextureSize), 0, 0);

					// apply changes to final texture
					mapTexture.Apply ();

					// unset active render texture
					RenderTexture.active = null;

					// write final map texture
					byte[] bytes = mapTexture.EncodeToPNG ();
					using (Stream stream = File.Create (mapTexturePath))
						stream.Write (bytes, 0, bytes.Length);
					
					_isBusy = false;
				}
			} catch (System.Exception ex) {
				Debug.LogException (ex);
				yield break;
			} finally {
				RenderTexture.active = null;
				DestroyRenderCamera ();
				_isBusy = false;
			}

			// refresh asset database
			AssetDatabase.Refresh ();

			string relativeTexturePath = mapTexturePath.Substring (mapTexturePath.IndexOf ("Assets"));
			TextureImporter textureImporter = AssetImporter.GetAtPath (relativeTexturePath) as TextureImporter;
			if (textureImporter != null) {
				// set texture as UI sprite
				if (textureImporter.textureType != TextureImporterType.Sprite)
					textureImporter.textureType = TextureImporterType.Sprite;

				// update texture size
				if (textureImporter.maxTextureSize != (int)TextureSize)
					textureImporter.maxTextureSize = (int)TextureSize;

				// clamp texture
				if (textureImporter.wrapMode != TextureWrapMode.Clamp)
					textureImporter.wrapMode = TextureWrapMode.Clamp;

				// disable POT scaling
				if (mapTextureSize.x != mapTextureSize.y && textureImporter.npotScale != TextureImporterNPOTScale.None)
					textureImporter.npotScale = TextureImporterNPOTScale.None;

				// save changes
				textureImporter.SaveAndReimport ();

				// create map profile
				HNSMapProfileUtilities.CreateProfile (relativeTexturePath, mapTextureSize, Background, MapBounds);

				// refresh asset database
				AssetDatabase.Refresh ();
			} else {
				Debug.LogError ("MapTextureCreator couldn't update the texture import settings. Please adjust the texture size manually within the texture import settings.");
			}

			// finish operation
			EditorUtility.DisplayDialog ("HNS MapTextureCreator", string.Format ("Successfully created profile '{0}':\n\n{1}", MapName, mapTexturePath), "OK");

			// destroy render camera
			DestroyRenderCamera ();

			yield return null;
		}


		string GetTexturePath ()
		{
			// check for empty filename
			if (MapName.Length <= 0)
				MapName = "Unnamed";
			
			// check path and create directory
			string path = Path.Combine (Application.dataPath, "HNS MapTextureCreator/" + MapName).Replace ('\\', '/');
			if (!System.IO.Directory.Exists (path)) {
				System.IO.Directory.CreateDirectory (path);
				AssetDatabase.Refresh ();
			}

			// return final path and filename
			return string.Format ("{0}/{1}_{2}_Map.png", path, MapName, System.DateTime.Now.ToString ("yyyyMMddHHmmss"));
		}


		void OnDrawGizmos ()
		{
			if (Selection.activeGameObject != this.gameObject)
				return;

			// draw map bounds
			if (PreviewShowBounds) {
				Gizmos.color = PreviewBoundsColor;
				Gizmos.DrawCube (MapBounds.center, MapBounds.size);
			}
		}
		#endregion


		#region Subclasses
		[System.Serializable]
		public enum _TextureSize { _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8192 = 8192 };
		#endregion
	}
	#endif
}

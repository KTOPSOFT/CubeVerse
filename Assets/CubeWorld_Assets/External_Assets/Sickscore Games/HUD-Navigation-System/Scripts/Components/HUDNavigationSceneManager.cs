using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[AddComponentMenu (HNS.Name + "/HNS Scene Manager"), DisallowMultipleComponent]
	public class HUDNavigationSceneManager : MonoBehaviour
	{
		private static HUDNavigationSceneManager _Instance;
		public static HUDNavigationSceneManager Instance {
			get {
				if (_Instance == null) {
					_Instance = FindObjectOfType<HUDNavigationSceneManager> ();
				}
				return _Instance;
			}
		}


		#region Variables
		public List<Configuration> Configurations;

		private HUDNavigationSystem _HUDNavigationSystem;
		#endregion


		#region Main Methods
		void OnEnable ()
		{
			SceneManager.activeSceneChanged += OnSceneChanged;
		}


		void OnDisable ()
		{
			SceneManager.activeSceneChanged -= OnSceneChanged;
		}


		void Awake ()
		{
			// destroy duplicate instances
			if (_Instance != null) {
				Destroy (this.gameObject);
				return;
			}

			_Instance = this;
		}


		void Start ()
		{
            // assign references
			if (_HUDNavigationSystem == null)
				_HUDNavigationSystem = HUDNavigationSystem.Instance;

            // dont destroy on load
			if (_HUDNavigationSystem != null && _HUDNavigationSystem.KeepAliveOnLoad)
				DontDestroyOnLoad (this.gameObject);
		}
		#endregion


		#region Utility Methods
		void OnSceneChanged (Scene scene, Scene nextScene)
		{
			// return if no configuration is assigned
			if (Configurations.Count <= 0) {
				Debug.LogWarning("[HNS SceneManager] Could't find any scene configuration!");
				return;
			}

			// get configuration matching currently active scene
			Configuration config = Configurations.Where (c => c._Scene != null && c._Config != null && c._Scene.path.Equals (nextScene.path)).FirstOrDefault ();
			HNSSceneConfiguration sceneConfig = config._Config;
			if (sceneConfig == null && !config._DisabledInScene) {
				Debug.Log("[HNS SceneManager] Configuration is missing for current scene!");
				return;
			}

			// check/assign references
			if (_HUDNavigationSystem == null) {
				_HUDNavigationSystem = HUDNavigationSystem.Instance;
				if (_HUDNavigationSystem == null) {
					Debug.LogError ("[HNS SceneManager] HUDNavigationSystem not found in scene!");
					this.enabled = false;
					return;
				}
			}

			// enable/disable HNS within this scene
			_HUDNavigationSystem.EnableSystem (!config._DisabledInScene);

			// apply configuration
			if (sceneConfig != null)
				_HUDNavigationSystem.ApplySceneConfiguration (sceneConfig);
		}
		#endregion
	}


	#region Subclasses
	[System.Serializable]
	public struct Configuration
	{
		public HNSSceneAsset _Scene;
		public bool _DisabledInScene;
		public HNSSceneConfiguration _Config;
	}
	#endregion


	#region CustomScene Wrapper
	/// <summary>
	/// Wrapper to serialize SceneAsset references.
	/// 
	/// Special Thanks To:
	/// https://gist.github.com/JohannesMP/ec7d3f0bcf167dab3d0d3bb480e0e07b
	/// </summary>
	[System.Serializable]
	public class HNSSceneAsset : ISerializationCallbackReceiver
	{
		#if UNITY_EDITOR
		[SerializeField]private Object _sceneAsset;
		bool IsValidSceneAsset {
			get {
				if (_sceneAsset == null)
					return false;
				
				return _sceneAsset.GetType ().Equals (typeof(SceneAsset));
			}
		}
		#endif


		[SerializeField]private string _path = string.Empty;
		public string path {
			get {
				#if UNITY_EDITOR
				return ScenePathFromAsset ();
				#else
				return _path;
				#endif
			}
			set {
				_path = value;
				#if UNITY_EDITOR
				_sceneAsset = SceneAssetFromPath ();
				#endif
			}
		}


		public static implicit operator string (HNSSceneAsset sceneReference)
		{
			return sceneReference.path;
		}


		public void OnBeforeSerialize ()
		{
			#if UNITY_EDITOR
			if (!IsValidSceneAsset && !string.IsNullOrEmpty (_path)) {
				_sceneAsset = SceneAssetFromPath ();
				if (_sceneAsset == null)
					_path = string.Empty;
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty ();
			} else {
				_path = ScenePathFromAsset ();
			}
			#endif
		}


		public void OnAfterDeserialize ()
		{
			#if UNITY_EDITOR
			EditorApplication.update += HandleAfterDeserialize;
			#endif
		}


		#if UNITY_EDITOR
		private SceneAsset SceneAssetFromPath ()
		{
			if (string.IsNullOrEmpty (_path))
				return null;
			
			return AssetDatabase.LoadAssetAtPath<SceneAsset> (_path);
		}


		private string ScenePathFromAsset ()
		{
			if (_sceneAsset == null)
				return string.Empty;
			
			return AssetDatabase.GetAssetPath (_sceneAsset);
		}


		private void HandleAfterDeserialize ()
		{
			EditorApplication.update -= HandleAfterDeserialize;
			if (IsValidSceneAsset)
				return;

			if (!string.IsNullOrEmpty (_path)) {
				_sceneAsset = SceneAssetFromPath ();
				if (_sceneAsset == null)
					_path = string.Empty;
				if (!Application.isPlaying)
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty ();
			}
		}
		#endif
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[AddComponentMenu (HNS.Name + "/HUD Navigation Element")]
	public class HUDNavigationElement : MonoBehaviour
	{
		#region Variables
		// MISC
		public HUDNavigationElementSettings Settings;
		public HNSPrefabs Prefabs = new HNSPrefabs ();

		// RADAR SETTINGS
		public bool hideInRadar = false;
		public bool ignoreRadarRadius = false;
		public bool ignoreRadarScaling = false;
		public bool ignoreRadarFading = false;
		public bool rotateWithGameObject = true;
		public bool useRadarHeightSystem = true;

		// COMPASS BAR SETTINGS
		public bool hideInCompassBar = false;
		public bool ignoreCompassBarRadius = false;
		public bool useCompassBarDistanceText = true;
		public string compassBarDistanceTextFormat = "{0}m";

		// INDICATOR SETTINGS
		public bool showIndicator = true;
		public bool showOffscreenIndicator = true;
		public bool ignoreIndicatorRadius = true;
		public bool ignoreIndicatorHideDistance = false;
		public bool ignoreIndicatorScaling = false;
		public bool ignoreIndicatorFading = false;
		public bool useIndicatorDistanceText = true;
		public bool showOffscreenIndicatorDistance = false;
		public string indicatorOnscreenDistanceTextFormat = "{0}m";
		public string indicatorOffscreenDistanceTextFormat = "{0}";

		// MINIMAP
		public bool hideInMinimap = false;
		public bool ignoreMinimapRadius = false;
		public bool ignoreMinimapScaling = false;
		public bool ignoreMinimapFading = false;
		public bool rotateWithGameObjectMM = true;
		public bool useMinimapHeightSystem = true;

		// EVENTS
		public NavigationElementEvent OnElementReady = new NavigationElementEvent ();
		public NavigationUpdateEvent OnElementUpdate = new NavigationUpdateEvent();
		public NavigationTypeEvent OnAppear = new NavigationTypeEvent ();
		public NavigationTypeEvent OnDisappear = new NavigationTypeEvent ();
		public NavigationTypeEvent OnEnterRadius = new NavigationTypeEvent ();
		public NavigationTypeEvent OnLeaveRadius = new NavigationTypeEvent ();


		[HideInInspector]
		public bool IsActive = true;


		[HideInInspector]
		public HNSRadarPrefab Radar;
		[HideInInspector]
		public HNSCompassBarPrefab CompassBar;
		[HideInInspector]
		public HNSIndicatorPrefab Indicator;
		[HideInInspector]
		public HNSMinimapPrefab Minimap;


		[HideInInspector]
		public bool IsInRadarRadius;
		[HideInInspector]
		public bool IsInCompassBarRadius;
		[HideInInspector]
		public bool IsInIndicatorRadius;
		[HideInInspector]
		public bool IsInMinimapRadius;


		protected bool _isInitialized = false;
		#endregion


		#region Main Methods
		protected virtual void Start ()
		{
			// disable, if navigation system is missing
			if (HUDNavigationSystem.Instance == null) {
				Debug.LogError ("HUDNavigationSystem not found in scene!");
				this.enabled = false;
				return;
			}

			// initialize settings
			InitializeSettings ();

			// initialize components
			Initialize ();
		}


		protected virtual void OnEnable ()
		{
			if (!_isInitialized)
				return;
			
			Initialize ();
		}


		protected virtual void OnDisable ()
		{
			// remove element from the navigation system
			if (HUDNavigationSystem.Instance != null)
				HUDNavigationSystem.Instance.RemoveNavigationElement (this);

			// destroy all marker references
			if (Radar != null)
				Destroy (Radar.gameObject);
			if (CompassBar != null)
				Destroy (CompassBar.gameObject);
			if (Indicator != null)
				Destroy (Indicator.gameObject);
			if (Minimap != null)
				Destroy (Minimap.gameObject);
		}


		protected virtual void Refresh ()
		{
			this.enabled = false;

			// reset markers
			Radar = null;
			CompassBar = null;
			Indicator = null;
			Minimap = null;

			// create marker references
			CreateMarkerReferences ();

			this.enabled = true;
		}
		#endregion


		#region Utility Methods
		protected virtual void InitializeSettings ()
		{
			if (Settings == null)
				return;

			// misc
			this.Prefabs = Settings.Prefabs;

			// radar settings
			this.hideInRadar = Settings.hideInRadar;
			this.ignoreRadarRadius = Settings.ignoreRadarRadius;
			this.ignoreRadarScaling = Settings.ignoreRadarScaling;
			this.ignoreRadarFading = Settings.ignoreRadarFading;
			this.rotateWithGameObject = Settings.rotateWithGameObject;
			this.useRadarHeightSystem = Settings.useRadarHeightSystem;

			// compass bar settings
			this.hideInCompassBar = Settings.hideInCompassBar;
			this.ignoreCompassBarRadius = Settings.ignoreCompassBarRadius;
			this.useCompassBarDistanceText = Settings.useCompassBarDistanceText;
			this.compassBarDistanceTextFormat = Settings.compassBarDistanceTextFormat;

			// indicator settings
			this.showIndicator = Settings.showIndicator;
			this.showOffscreenIndicator = Settings.showOffscreenIndicator;
			this.ignoreIndicatorRadius = Settings.ignoreIndicatorRadius;
			this.ignoreIndicatorHideDistance = Settings.ignoreIndicatorHideDistance;
			this.ignoreIndicatorScaling = Settings.ignoreIndicatorScaling;
			this.ignoreIndicatorFading = Settings.ignoreIndicatorFading;
			this.useIndicatorDistanceText = Settings.useIndicatorDistanceText;
			this.showOffscreenIndicatorDistance = Settings.showOffscreenIndicatorDistance;
			this.indicatorOnscreenDistanceTextFormat = Settings.indicatorOnscreenDistanceTextFormat;
			this.indicatorOffscreenDistanceTextFormat = Settings.indicatorOffscreenDistanceTextFormat;

			// minimap settings
			this.hideInMinimap = Settings.hideInMinimap;
			this.ignoreMinimapRadius = Settings.ignoreMinimapRadius;
			this.ignoreMinimapScaling = Settings.ignoreMinimapScaling;
			this.ignoreMinimapFading = Settings.ignoreMinimapFading;
			this.rotateWithGameObjectMM = Settings.rotateWithGameObjectMM;
			this.useMinimapHeightSystem = Settings.useMinimapHeightSystem;
		}


		protected virtual void Initialize ()
		{
			// create marker references
			CreateMarkerReferences ();

			// add element to the navigation system
			if (HUDNavigationSystem.Instance != null)
				HUDNavigationSystem.Instance.AddNavigationElement (this);

			// set as initialized
			_isInitialized = true;

			// invoke events
			OnElementReady.Invoke (this);
		}


		protected virtual void CreateMarkerReferences ()
		{
			CreateRadarMarker ();
			CreateCompassBarMarker ();
			CreateIndicatorMarker ();
			CreateMinimapMarker ();
		}


		void CreateRadarMarker ()
		{
			if (Prefabs.RadarPrefab == null)
				return;

			// create radar gameobject
			GameObject radarGO = Instantiate (Prefabs.RadarPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			radarGO.transform.SetParent (HUDNavigationCanvas.Instance.Radar.ElementContainer, false);
			radarGO.SetActive (false);

			// assign radar prefab
			Radar = radarGO.GetComponent<HNSRadarPrefab> ();
		}


		void CreateCompassBarMarker ()
		{
			if (Prefabs.CompassBarPrefab == null)
				return;

			// create compass bar gameobject
			GameObject compassBarGO = Instantiate (Prefabs.CompassBarPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			compassBarGO.transform.SetParent (HUDNavigationCanvas.Instance.CompassBar.ElementContainer, false);
			compassBarGO.SetActive (false);

			// assign compass bar prefab
			CompassBar = compassBarGO.GetComponent<HNSCompassBarPrefab> ();
		}


		void CreateIndicatorMarker ()
		{
			if (Prefabs.IndicatorPrefab == null)
				return;

			// create indicator gameobject
			GameObject indicatorGO = Instantiate (Prefabs.IndicatorPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			indicatorGO.transform.SetParent (HUDNavigationCanvas.Instance.Indicator.ElementContainer, false);
			indicatorGO.SetActive (false);

			// assign indicator prefab
			Indicator = indicatorGO.GetComponent<HNSIndicatorPrefab> ();
		}


		void CreateMinimapMarker ()
		{
			if (Prefabs.MinimapPrefab == null)
				return;

			// create minimap gameobject
			GameObject minimapGO = Instantiate (Prefabs.MinimapPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			minimapGO.transform.SetParent (HUDNavigationCanvas.Instance.Minimap.ElementContainer, false);
			minimapGO.SetActive (false);

			// assign minimap prefab
			Minimap = minimapGO.GetComponent<HNSMinimapPrefab> ();
		}
		#endregion
	}


	#region Subclasses
	[System.Serializable]
	public class HNSPrefabs
	{
		public HNSRadarPrefab RadarPrefab;
		public HNSCompassBarPrefab CompassBarPrefab;
		public HNSIndicatorPrefab IndicatorPrefab;
		public HNSMinimapPrefab MinimapPrefab;
	}


	[System.Serializable]
	public enum NavigationElementType { Radar, CompassBar, Indicator, Minimap };


	[System.Serializable]
	public class NavigationElementEvent : UnityEvent<HUDNavigationElement> {}


	[System.Serializable]
	public class NavigationUpdateEvent : UnityEvent<Vector3, Vector3, float> {}


	[System.Serializable]
	public class NavigationTypeEvent : UnityEvent<HUDNavigationElement, NavigationElementType> {}
	#endregion
}

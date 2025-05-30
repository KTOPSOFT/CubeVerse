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
	[AddComponentMenu (HNS.Name + "/" + HNS.Name), DisallowMultipleComponent]
	public class HUDNavigationSystem : MonoBehaviour
	{
		private static HUDNavigationSystem _Instance;
		public static HUDNavigationSystem Instance {
			get {
				if (_Instance == null) {
					_Instance = FindObjectOfType<HUDNavigationSystem> ();
				}
				return _Instance;
			}
		}


		#region Variables
		// SYSTEM
		public bool isEnabled // use the EnableSystem(bool) method to change this value.
		{
			get { return _isEnabled; }
			private set { _isEnabled = value; }
		}
		[SerializeField]private bool _isEnabled = true;

		// REFERENCES
		public Camera PlayerCamera;
		public Transform PlayerController;
		public _RotationReference RotationReference = _RotationReference.Camera;
		public _UpdateMode UpdateMode = _UpdateMode.LateUpdate;
		public bool KeepAliveOnLoad = true;

		// RADAR
		[Tooltip("Enable, if you want to use the radar feature.")]
		public bool useRadar = true;
		[Tooltip("Select the radar mode you want to use.")]
		public RadarModes radarMode = RadarModes.RotateRadar;
		[Tooltip("Define the radar zoom. Change value to zoom the radar. Set to 1 for default radar zoom.")]
		public float radarZoom = 1f;
		[Tooltip("Define the radar radius. Elements outside this radius will be displayed on the border of the radar.")]
		public float radarRadius = 50f;
		[Tooltip("Define the maximum radar radius. Elements outside this radius will be hidden.")]
		public float radarMaxRadius = 75f;
		[Tooltip("Enable, if you want to scale radar elements, when they're about to disappear from the radar.")]
		public bool useRadarScaling = true;
		[Tooltip("Define the radar scale distance. Radar elements will be scaled, when close to the radar max radius. Must be smaller or equal to the radar max radius.")]
		public float radarScaleDistance = 10f;
		[Tooltip("Minimum scale of the radar element. Set value to 1, if you don't want your element to scale.")]
		public float radarMinScale = .35f;
		[Tooltip("Enable, if you want to fade radar elements, when they're about to disappear from the radar.")]
		public bool useRadarFading = true;
		[Tooltip("Define the radar fade distance. Radar elements will be faded, when close to the radar max radius. Must be smaller or equal to the radar max radius.")]
		public float radarFadeDistance = 5f;
		[Tooltip("Minimum opacity of the radar elements. Set value to 0, to completely fade-out the element.")]
		public float radarMinFade = 0f;
		[Tooltip("Enable, if you want to show arrows pointing upwards/downwards if the element is physically above or below a certain distance.")]
		public bool useRadarHeightSystem = true;
		[Tooltip("Minimum distance upwards to activate the element's ABOVE arrow.")]
		public float radarDistanceAbove = 10f;
		[Tooltip("Minimum distance downwards to activate the element's BELOW arrow.")]
		public float radarDistanceBelow = 10f;
		[Tooltip("(DEBUG) Enable to show the radar's height gizmos.")]
		public bool showRadarHeightGizmos = false;
		[SerializeField]protected Vector2 radarHeightGizmoSize = new Vector2 (100f, 100f);
		[SerializeField]protected Color radarHeightGizmoColor = new Color (0f, 0f, 1f, .4f);

		// COMPASS BAR
		[Tooltip("Enable, if you want to use the compass bar feature.")]
		public bool useCompassBar = true;
		[Tooltip("Define the compass radius. Elements that don't ignore the radius will be hidden outside this radius.")]
		public float compassBarRadius = 150f;

		// INDICATOR
		[Tooltip("Enable, if you want to use the indicator feature. Must be separately enabled on each element.")]
		public bool useIndicators = true;
		[Tooltip("Define the indicator radius. Indicators that don't ignore the radius will be hidden outside this radius.")]
		public float indicatorRadius = 25f;
		[Tooltip("Define the distance below which the indicator will automatically be hidden. (0 = no auto-hide)")]
		public float indicatorHideDistance = 3f;
		[Tooltip("Enable, if you want to use an offscreen indicator, when the element is not on screen.")]
		public bool useOffscreenIndicators = true;
		[Tooltip("Increase this value to move the indicators further away from the screen borders.")]
		public float indicatorOffscreenBorder = .075f;
		[Tooltip("Enable, if you want to scale the indicator by distance and within defined radius.")]
		public bool useIndicatorScaling = true;
		[Tooltip("Define the indicator scale radius. Indicator will scale when inside this radius. Must be smaller or equal to indicator radius.")]
		public float indicatorScaleRadius = 15f;
		[Tooltip("Minimum scale of the indicator. Set value to 1, if you don't want your indicator to scale.")]
		public float indicatorMinScale = .8f;
		[Tooltip("Enable, if you want to fade the indicator by distance and within defined radius.")]
		public bool useIndicatorFading = true;
		[Tooltip("Define the indicator fade radius. Indicator will fade when inside this radius. Must be smaller or equal to indicator radius.")]
		public float indicatorFadeRadius = 15f;
		[Tooltip("Minimum opacity of the indicator. Set value to 0, to completely fade-out the indicator.")]
		public float indicatorMinFade = 0f;

		// MINIMAP
		[Tooltip("Enable, if you want to use the minimap feature.")]
		public bool useMinimap = true;
		[Tooltip("Assign the map profile for your minimap.")]
		public HNSMapProfile minimapProfile;
		[HideInInspector]public HNSMapProfile currentMinimapProfile;
		[Tooltip("Select the minimap mode you want to use.")]
		public MinimapModes minimapMode = MinimapModes.RotatePlayer;
		[Tooltip("Define the minimap scale. Change value to zoom the minimap.")]
		public float minimapScale = .25f;
		[Tooltip("Define the minimap radius. Elements will be displayed on the border of the minimap, depending on the minimap scale.")]
		public float minimapRadius = 75f;
		[Tooltip("Enable, if you want to scale minimap elements, when they're about to disappear from the minimap.")]
		public bool useMinimapScaling = true;
		[Tooltip("Define the minimap scale distance. Minimap elements will be scaled, when close to the minimap radius. Must be smaller or equal to the minimap radius.")]
		public float minimapScaleDistance = 10f;
		[Tooltip("Minimum scale of the minimap element. Set value to 1, if you don't want your element to scale.")]
		public float minimapMinScale = .35f;
		[Tooltip("Enable, if you want to fade minimap elements, when they're about to disappear from the minimap.")]
		public bool useMinimapFading = true;
		[Tooltip("Define the minimap fade distance. Minimap elements will be faded, when close to the minimap radius. Must be smaller or equal to the minimap radius.")]
		public float minimapFadeDistance = 5f;
		[Tooltip("Minimum opacity of the minimap elements. Set value to 0, to completely fade-out the element.")]
		public float minimapMinFade = 0f;
		[Tooltip("(DEBUG) Enable to show the minimap bounds gizmos.")]
		public bool showMinimapBounds = true;
		[SerializeField]protected Color minimapBoundsGizmoColor = new Color (0f, 1f, 0f, .85f);
		[Tooltip("Enable, if you want to show arrows pointing upwards/downwards if the element is physically above or below a certain distance.")]
		public bool useMinimapHeightSystem = true;
		[Tooltip("Minimum distance upwards to activate the element's ABOVE arrow.")]
		public float minimapDistanceAbove = 10f;
		[Tooltip("Minimum distance downwards to activate the element's BELOW arrow.")]
		public float minimapDistanceBelow = 10f;
		[Tooltip("(DEBUG) Enable to show the minimap's height gizmos.")]
		public bool showMinimapHeightGizmos = false;
		[SerializeField]protected Vector2 minimapHeightGizmoSize = new Vector2 (100f, 100f);
		[SerializeField]protected Color minimapHeightGizmoColor = new Color (0f, 0f, 1f, .4f);


		[HideInInspector]
		public List<HUDNavigationElement> NavigationElements;

		private HUDNavigationCanvas _HUDNavigationCanvas;
		#endregion


		#region Main Methods
		void Awake ()
		{
			// destroy duplicate instances
			if (_Instance != null) {
				Destroy (this.gameObject);
				return;
			}

			// dont destroy on load
			if (KeepAliveOnLoad)
			    DontDestroyOnLoad (this.gameObject);

			_Instance = this;
		}


		protected virtual void Start ()
		{
			// assign references
			if (_HUDNavigationCanvas == null) {
				_HUDNavigationCanvas = HUDNavigationCanvas.Instance;

				// check if HUDNavigationCanvas exists
				if (_HUDNavigationCanvas == null) {
					Debug.LogError ("HUDNavigationCanvas not found in scene!");
					this.enabled = false;
					return;
				}
			}

			// enable the system
			EnableSystem(isEnabled);

			// init all components
			InitAllComponents ();
		}


		void Update ()
		{
			if (UpdateMode == _UpdateMode.Update)
				UpdateAllComponents ();
		}


		void LateUpdate ()
		{
			if (UpdateMode == _UpdateMode.LateUpdate)
				UpdateAllComponents ();
		}


		/// <summary>
		/// Enable / Disable the entire system at runtime.
		/// </summary>
		/// <param name="value">value</param>
		public void EnableSystem (bool value)
		{
			// enable/disable system
			isEnabled = value;

			// enable/disable HNS canvas if value has changed
			if (_HUDNavigationCanvas != null && isEnabled != _HUDNavigationCanvas.isEnabled)
				_HUDNavigationCanvas.EnableCanvas (isEnabled);

			// try to automatically assign the player camera and transform if possible
			if (PlayerCamera == null) {
				HNSPlayerCamera hnsCamera = GameObject.FindObjectOfType<HNSPlayerCamera> ();
				if (hnsCamera != null)
					PlayerCamera = hnsCamera.GetComponent<Camera> ();
				else
					PlayerCamera = Camera.main;
				if (PlayerCamera == null)
					Debug.LogError("[HUDNavigationSystem] Player camera unassigned. Assign camera to resume system!");
			}
			if (PlayerController == null) {
				HNSPlayerController hnsTransform = GameObject.FindObjectOfType<HNSPlayerController> ();
				if (hnsTransform != null)
					PlayerController = hnsTransform.gameObject.transform;
				if (PlayerController == null)
					Debug.LogError("[HUDNavigationSystem] Player transform unassigned. Assign transform to resume system!");
			}

			// refresh references
			RefreshPlayerReferences ();
		}


		/// <summary>
		/// Change the currently assigned player camera at runtime.
		/// </summary>
		/// <param name="playerCamera">Player Camera.</param>
		public void ChangePlayerCamera (Camera playerCamera)
		{
			if (playerCamera == null || playerCamera == PlayerCamera)
				return;

			PlayerCamera = playerCamera;
			RefreshPlayerReferences ();
		}


		/// <summary>
		/// Change the currently assigned player controller at runtime.
		/// </summary>
		/// <param name="playerController">Player Controller.</param>
		public void ChangePlayerController (Transform playerController)
		{
			if (playerController == null || playerController == PlayerController)
				return;

			PlayerController = playerController;
			RefreshPlayerReferences ();
		}


		/// <summary>
		/// Add a navigation element to the collection.
		/// </summary>
		/// <param name="element">Element.</param>
		public void AddNavigationElement (HUDNavigationElement element)
		{
			if (element == null)
				return;

			// add element, if it doesn't exist yet
			if (!NavigationElements.Contains (element))
				NavigationElements.Add (element);
		}


		/// <summary>
		/// Remove a navigation element from the collection.
		/// </summary>
		/// <param name="element">Element.</param>
		public void RemoveNavigationElement (HUDNavigationElement element)
		{
			if (element == null)
				return;

			// remove element from list
			NavigationElements.Remove (element);
		}


		/// <summary>
		/// Enable / Disable the radar feature at runtime.
		/// </summary>
		/// <param name="value">value</param>
		public void EnableRadar (bool value)
		{
			if (useRadar != value) {
				useRadar = value;
				_HUDNavigationCanvas.ShowRadar (value);
			}
		}


		/// <summary>
		/// Enable / Disable the compass bar feature at runtime.
		/// </summary>
		/// <param name="value">value</param>
		public void EnableCompassBar (bool value)
		{
			if (useCompassBar != value) {
				useCompassBar = value;
				_HUDNavigationCanvas.ShowCompassBar (value);
			}
		}


		/// <summary>
		/// Enable / Disable the indicator feature at runtime.
		/// </summary>
		/// <param name="value">value</param>
		public void EnableIndicators (bool value)
		{
			if (useIndicators != value) {
				useIndicators = value;
				_HUDNavigationCanvas.ShowIndicators (value);
			}
		}


		/// <summary>
		/// Enable / Disable the minimap feature at runtime.
		/// </summary>
		/// <param name="value">value</param>
		public void EnableMinimap (bool value)
		{
			if (useMinimap != value) {
				useMinimap = value;
				_HUDNavigationCanvas.ShowMinimap (value);
			}
		}
		#endregion


		#region Utility Methods
		void InitAllComponents ()
		{
			if (_HUDNavigationCanvas == null)
				return;

			// init radar
			if (useRadar) {
				_HUDNavigationCanvas.InitRadar ();

				// make sure max radius is greater than radius
				if (radarMaxRadius < radarRadius)
					radarMaxRadius = radarRadius;
			} else {
				_HUDNavigationCanvas.ShowRadar (false);
			}

			// init compass bar
			if (useCompassBar)
				_HUDNavigationCanvas.InitCompassBar ();
			else
				_HUDNavigationCanvas.ShowCompassBar (false);

			// init indicators
			if (useIndicators)
				_HUDNavigationCanvas.InitIndicators ();
			else
				_HUDNavigationCanvas.ShowIndicators (false);
			
			// init minimap
			if (useMinimap && minimapProfile != null)
				_HUDNavigationCanvas.InitMinimap (minimapProfile);
			else
				_HUDNavigationCanvas.ShowMinimap (false);
		}


		protected virtual void UpdateAllComponents ()
		{
			// return, if system is disabled
			if (!isEnabled)
				return;

			// return, if references are missing
			if (PlayerCamera == null || PlayerController == null)
				return;
			
			// update navigation elements
			UpdateNavigationElements ();

			// get rotation reference
			Transform rotationReference = GetRotationReference ();

			// update radar
			if (useRadar)
				_HUDNavigationCanvas.UpdateRadar (rotationReference, radarMode);

			// update compass bar
			if (useCompassBar)
				_HUDNavigationCanvas.UpdateCompassBar (rotationReference);

			// update minimap
			if (useMinimap && minimapProfile != null && PlayerController != null)
				_HUDNavigationCanvas.UpdateMinimap (rotationReference, minimapMode, PlayerController, minimapProfile, minimapScale);
		}


		protected void UpdateNavigationElements ()
		{
			if (_HUDNavigationCanvas == null || NavigationElements.Count <= 0)
				return;

			// update navigation elements
			foreach (HUDNavigationElement element in NavigationElements) {
				if (element == null)
					continue;

				// check if element is active
				if (!element.IsActive) {
					// disable all marker instances
					element.SetMarkerActive (NavigationElementType.Radar, false);
					element.SetMarkerActive (NavigationElementType.CompassBar, false);
					element.SetMarkerActive (NavigationElementType.Minimap, false);
					element.SetIndicatorActive (false);

					// skip the element
					continue;
				}

				// cache element values
				Vector3 _worldPos = element.GetPosition ();
				Vector3 _screenPos = PlayerCamera.WorldToScreenPoint (_worldPos);
				float _distance = element.GetDistance (PlayerController.transform);

				// update radar element
				if (useRadar && element.Radar != null)
					UpdateRadarElement (element, _screenPos, _distance);

				// update compass bar element
				if (useCompassBar && element.CompassBar != null)
					UpdateCompassBarElement (element, _screenPos, _distance);

				// update indicator element
				if (useIndicators && element.Indicator != null)
					UpdateIndicatorElement (element, _screenPos, _distance);

				// update minimap element
				if (useMinimap && element.Minimap != null && currentMinimapProfile != null)
					UpdateMinimapElement (element, _screenPos, _distance);

				// element update event
				element.OnElementUpdate.Invoke(_worldPos, _screenPos, _distance);
			}
		}


		protected Transform GetRotationReference ()
		{
			return (RotationReference == _RotationReference.Camera) ? PlayerCamera.transform : PlayerController;
		}


		void OnDrawGizmos ()
		{
			#if UNITY_EDITOR
			if (PlayerController == null || Selection.activeGameObject != this.gameObject)
				return;
			
			// draw height system debug gizmos
			bool _radarHeightGizmos = useRadarHeightSystem && showRadarHeightGizmos;
			bool _minimapHeightGizmos = useMinimapHeightSystem && showMinimapHeightGizmos;
			if (_radarHeightGizmos || _minimapHeightGizmos) {
				Vector3 playerPos = PlayerController.position;

				// draw radar height planes
				if (_radarHeightGizmos) {
					Gizmos.color = radarHeightGizmoColor;
					Gizmos.DrawCube (playerPos + (Vector3.up * radarDistanceAbove), new Vector3(radarHeightGizmoSize.x, .01f, radarHeightGizmoSize.y));
					Gizmos.DrawCube (playerPos - (Vector3.up * radarDistanceBelow), new Vector3(radarHeightGizmoSize.x, .01f, radarHeightGizmoSize.y));
				}

				// draw minimap height planes
				if (_minimapHeightGizmos) {
					Gizmos.color = minimapHeightGizmoColor;
					Gizmos.DrawCube (playerPos + (Vector3.up * minimapDistanceAbove), new Vector3(minimapHeightGizmoSize.x, .01f, minimapHeightGizmoSize.y));
					Gizmos.DrawCube (playerPos - (Vector3.up * minimapDistanceBelow), new Vector3(minimapHeightGizmoSize.x, .01f, minimapHeightGizmoSize.y));
				}
			}

			// draw map bounds
			if (showMinimapBounds && minimapProfile != null) {
				Gizmos.color = minimapBoundsGizmoColor;
				Gizmos.DrawWireCube (minimapProfile.MapBounds.center, minimapProfile.MapBounds.size);
			}
			#endif
		}


		void RefreshPlayerReferences ()
		{
			if (_HUDNavigationCanvas == null)
				return;

			// check for missing / newly assigned player references
			if (PlayerCamera == null || PlayerController == null)
				_HUDNavigationCanvas.EnableCanvas (false); // keep system enabled, but disable canvas
			else if (isEnabled && !_HUDNavigationCanvas.isEnabled)
				_HUDNavigationCanvas.EnableCanvas (true); // re-enable canvas, because all missing references were assigned
		}
		#endregion


		#region Radar Methods
		void UpdateRadarElement (HUDNavigationElement element, Vector3 screenPos, float distance)
		{
			float _scaledRadius = radarRadius * radarZoom;
			float _scaledMaxRadius = radarMaxRadius * radarZoom;
			float _rawDistance = distance;

			// check if element is hidden within the radar
			if (element.hideInRadar) {
				element.SetMarkerActive (NavigationElementType.Radar, false);
				return;
			}

			// check distance
			if (distance > _scaledRadius) {
				// invoke events
				if (element.IsInRadarRadius) {
					element.IsInRadarRadius = false;
					element.OnLeaveRadius.Invoke (element, NavigationElementType.Radar);
				}

				// check max distance
				if (distance > _scaledMaxRadius && !element.ignoreRadarRadius) {
					element.SetMarkerActive (NavigationElementType.Radar, false);
					return;
				}

				// set scaled distance when out of range
				distance = _scaledRadius;
			} else {
				// invoke events
				if (!element.IsInRadarRadius) {
					element.IsInRadarRadius = true;
					element.OnEnterRadius.Invoke (element, NavigationElementType.Radar);
				}
			}

			// rotate marker within radar with gameobject?
			Transform rotationReference = GetRotationReference ();
			if (radarMode == RadarModes.RotateRadar) {
				element.Radar.PrefabRect.rotation = Quaternion.identity;
				if (element.rotateWithGameObject)
					element.Radar.Icon.transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, -element.transform.eulerAngles.y + rotationReference.eulerAngles.y));
			} else {
				if (element.rotateWithGameObject)
					element.Radar.Icon.transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, -element.transform.eulerAngles.y));
			}

			// keep marker icon identity rotation?
			if (!element.rotateWithGameObject)
				element.Radar.Icon.transform.rotation = Quaternion.identity;

			// update marker values
			float invertedDistance = radarMaxRadius - _rawDistance;
			element.SetMarkerScale (useRadarScaling && !element.ignoreRadarScaling && !element.ignoreRadarRadius, invertedDistance, radarScaleDistance, radarMinScale, element.Radar.PrefabRect);
			element.SetMarkerFade (useRadarFading && !element.ignoreRadarFading && !element.ignoreRadarRadius, invertedDistance, radarFadeDistance, radarMinFade, element.Radar.PrefabCanvasGroup);
			element.SetMarkerActive (NavigationElementType.Radar, true);

			// calculate marker position
			Vector3 posOffset = element.GetPositionOffset (PlayerController.position);
			Vector3 markerPos = new Vector3 (posOffset.x, posOffset.z, 0f);
			markerPos.Normalize ();
			markerPos *= (distance / _scaledRadius) * (_HUDNavigationCanvas.Radar.ElementContainer.GetRadius () - element.GetIconRadius (NavigationElementType.Radar));

			// set marker position
			element.SetMarkerPosition (NavigationElementType.Radar, markerPos);

			// handle marker's above/below arrows
			bool heightSystemConditions = useRadarHeightSystem && element.useRadarHeightSystem && element.IsInRadarRadius;
			element.ShowRadarAboveArrow (heightSystemConditions && -posOffset.y < -radarDistanceAbove);
			element.ShowRadarBelowArrow (heightSystemConditions && -posOffset.y > radarDistanceBelow);
		}
		#endregion


		#region CompassBar Methods
		void UpdateCompassBarElement (HUDNavigationElement element, Vector3 screenPos, float distance)
		{			
			// check if element is hidden within the compass bar
			if (element.hideInCompassBar) {
				element.SetMarkerActive (NavigationElementType.CompassBar, false);
				return;
			}

			// check distance
			if (distance > compassBarRadius && !element.ignoreCompassBarRadius) {
				element.SetMarkerActive (NavigationElementType.CompassBar, false);

				// invoke events
				if (element.IsInCompassBarRadius) {
					element.IsInCompassBarRadius = false;
					element.OnLeaveRadius.Invoke (element, NavigationElementType.CompassBar);
				}
				return;
			}

			// invoke events
			if (!element.IsInCompassBarRadius) {
				element.IsInCompassBarRadius = true;
				element.OnEnterRadius.Invoke (element, NavigationElementType.CompassBar);
			}

			// set marker position
			if (screenPos.z <= 0) {
				// hide marker and skip element
				element.SetMarkerActive (NavigationElementType.CompassBar, false);
				return;
			}

			// show compass bar distance?
			element.ShowCompassBarDistance ((int)distance);

			// set marker active
			element.SetMarkerActive (NavigationElementType.CompassBar, true);

			// set marker position
			element.SetMarkerPosition (NavigationElementType.CompassBar, screenPos, _HUDNavigationCanvas.CompassBar.ElementContainer);
		}
		#endregion


		#region Indicator Methods
		protected virtual void UpdateIndicatorElement (HUDNavigationElement element, Vector3 screenPos, float distance)
		{
			if (useIndicators && element.showIndicator) {
				// check indicator distance
				if ((distance > indicatorRadius && !element.ignoreIndicatorRadius)) {
					element.SetIndicatorActive (false);

					// invoke events
					if (element.IsInIndicatorRadius) {
						element.IsInIndicatorRadius = false;
						element.OnLeaveRadius.Invoke (element, NavigationElementType.Indicator);
					}
				} else {
					// check if element is visible on screen
					bool _isElementOnScreen = element.IsVisibleOnScreen (screenPos);
					if (!_isElementOnScreen) {
						if (useOffscreenIndicators && element.showOffscreenIndicator) {
							// flip if indicator is behind us
							if (screenPos.z < 0f) {
								screenPos.x = Screen.width - screenPos.x;
								screenPos.y = Screen.height - screenPos.y;
							}

							// calculate off-screen position/rotation
							Vector3 screenCenter = new Vector3 (Screen.width, Screen.height, 0f) / 2f;
							screenPos -= screenCenter;
							float angle = Mathf.Atan2 (screenPos.y, screenPos.x);
							angle -= 90f * Mathf.Deg2Rad;
							float cos = Mathf.Cos (angle);
							float sin = -Mathf.Sin (angle);
							float cotangent = cos / sin;
							float offset = Mathf.Min (screenCenter.x, screenCenter.y);
							offset = Mathf.Lerp (0f, offset, indicatorOffscreenBorder);
							Vector3 screenBounds = screenCenter - new Vector3 (offset, offset, 0f);
							float boundsY = (cos > 0f) ? screenBounds.y : -screenBounds.y;
							screenPos = new Vector3 (boundsY / cotangent, boundsY, 0f);

							// when out of bounds, get point on appropriate side
							if (screenPos.x > screenBounds.x) // out => right
								screenPos = new Vector3 (screenBounds.x, screenBounds.x * cotangent, 0f);
							else if (screenPos.x < -screenBounds.x) // out => left
								screenPos = new Vector3 (-screenBounds.x, -screenBounds.x * cotangent, 0f);
							screenPos += screenCenter;

							// update indicator rotation
							element.SetIndicatorOffscreenRotation (Quaternion.Euler (0f, 0f, angle * Mathf.Rad2Deg));
						} else {
							// hide indicator offscreen
							element.SetIndicatorActive (false);
							return;
						}
					}

					// show indicator distance?
					element.ShowIndicatorDistance (_isElementOnScreen, (int)distance);

					// set indicator on/offscreen
					element.SetIndicatorOnOffscreen (_isElementOnScreen);

					// update indicator values
					element.SetIndicatorPosition (screenPos, _HUDNavigationCanvas.Indicator.ElementContainer);
					element.SetMarkerScale (useIndicatorScaling && !element.ignoreIndicatorScaling, distance, indicatorScaleRadius, indicatorMinScale, element.Indicator.PrefabRect);
					element.SetMarkerFade (useIndicatorFading && !element.ignoreIndicatorFading, distance, indicatorFadeRadius, indicatorMinFade, element.Indicator.PrefabCanvasGroup);
					element.SetIndicatorActive ((indicatorHideDistance > 0f && !element.ignoreIndicatorHideDistance) ? distance > indicatorHideDistance : true);

					// invoke events
					if (!element.IsInIndicatorRadius) {
						element.IsInIndicatorRadius = true;
						element.OnEnterRadius.Invoke (element, NavigationElementType.Indicator);
					}
				}
			} else {
				element.SetIndicatorActive (false);
			}
		}
		#endregion


		#region Minimap Methods
		void UpdateMinimapElement (HUDNavigationElement element, Vector3 screenPos, float distance)
		{
			float _rawDistance = distance;

			// check if element is hidden within the minimap
			if (element.hideInMinimap) {
				element.SetMarkerActive (NavigationElementType.Minimap, false);
				return;
			}

			// check distance
			if (distance > minimapRadius) {
				// invoke events
				if (element.IsInMinimapRadius) {
					element.IsInMinimapRadius = false;
					element.OnLeaveRadius.Invoke (element, NavigationElementType.Minimap);
				}

				// hide element
				if (!element.ignoreMinimapRadius) {
					element.SetMarkerActive (NavigationElementType.Minimap, false);
					return;
				}
			} else {
				// invoke events
				if (!element.IsInMinimapRadius) {
					element.IsInMinimapRadius = true;
					element.OnEnterRadius.Invoke (element, NavigationElementType.Minimap);
				}
			}

			// rotate marker within minimap with gameobject?
			if (element.rotateWithGameObjectMM)
				element.Minimap.Icon.transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, -element.transform.eulerAngles.y));
			else
				element.Minimap.Icon.transform.rotation = Quaternion.identity;

			// update marker values
			float invertedDistance = minimapRadius - _rawDistance;
			element.SetMarkerScale (useMinimapScaling && !element.ignoreMinimapScaling && !element.ignoreMinimapRadius, invertedDistance, minimapScaleDistance, minimapMinScale, element.Minimap.PrefabRect);
			element.SetMarkerFade (useMinimapFading && !element.ignoreMinimapFading && !element.ignoreMinimapRadius, invertedDistance, minimapFadeDistance, minimapMinFade, element.Minimap.PrefabCanvasGroup);
			element.SetMarkerActive (NavigationElementType.Minimap, true);

			// calculate marker position
			Vector2 unitScale = minimapProfile.GetMapUnitScale ();
			Vector3 posOffset = element.GetPositionOffset (PlayerController.position);
			Vector3 markerPos = new Vector3 (posOffset.x * unitScale.x, posOffset.z * unitScale.y, 0f) * minimapScale;

			// adjust marker position when using minimap rotation mode
			if (minimapMode == MinimapModes.RotateMinimap)
				markerPos = PlayerController.MinimapRotationOffset (markerPos);

			// always keep marker within minimap rect
			bool outOfBounds;
			markerPos = _HUDNavigationCanvas.Minimap.ElementContainer.KeepInRectBounds(markerPos, out outOfBounds);

            // set marker position
            element.SetMarkerPosition (NavigationElementType.Minimap, markerPos);

			// handle marker's above/below arrows
			bool heightSystemConditions = useMinimapHeightSystem && element.useMinimapHeightSystem && element.IsInMinimapRadius && !outOfBounds;
			element.ShowMinimapAboveArrow (heightSystemConditions && -posOffset.y < -minimapDistanceAbove);
			element.ShowMinimapBelowArrow (heightSystemConditions && -posOffset.y > minimapDistanceBelow);
		}
		#endregion


		#region Configuration Methods
		/// <summary>
		/// Applies a new scene configuration.
		/// </summary>
		/// <param name="config">Scene Configuration.</param>
		public void ApplySceneConfiguration (HNSSceneConfiguration config)
		{
			if (config == null)
				return;

			// override radar settings
			if (config.overrideRadarSettings) {
				// radar usage changed?
				if (_HUDNavigationCanvas != null && this.useRadar != config.useRadar)
					_HUDNavigationCanvas.ShowRadar (config.useRadar);
				
				this.useRadar = config.useRadar;
				this.radarMode = config.radarMode;
				this.radarZoom = config.radarZoom;
				this.radarRadius = config.radarRadius;
				this.radarMaxRadius = config.radarMaxRadius;
				this.useRadarScaling = config.useRadarScaling;
				this.radarScaleDistance = config.radarScaleDistance;
				this.radarMinScale = config.radarMinScale;
				this.useRadarFading = config.useRadarFading;
				this.radarFadeDistance = config.radarFadeDistance;
				this.radarMinFade = config.radarMinFade;
				this.useRadarHeightSystem = config.useRadarHeightSystem;
				this.radarDistanceAbove = config.radarDistanceAbove;
				this.radarDistanceBelow = config.radarDistanceBelow;
			}

			// override compass bar settings
			if (config.overrideCompassBarSettings) {
				// compass bar usage changed?
				if (_HUDNavigationCanvas != null && this.useCompassBar != config.useCompassBar)
					_HUDNavigationCanvas.ShowCompassBar (config.useCompassBar);
				
				this.useCompassBar = config.useCompassBar;
				this.compassBarRadius = config.compassBarRadius;
			}

			// override indicator settings
			if (config.overrideIndicatorSettings) {
				// indicator usage changed?
				if (_HUDNavigationCanvas != null && this.useIndicators != config.useIndicators)
					_HUDNavigationCanvas.ShowIndicators (config.useIndicators);
				
				this.useIndicators = config.useIndicators;
				this.indicatorRadius = config.indicatorRadius;
				this.indicatorHideDistance = config.indicatorHideDistance;
				this.useOffscreenIndicators = config.useOffscreenIndicators;
				this.indicatorOffscreenBorder = config.indicatorOffscreenBorder;
				this.useIndicatorScaling = config.useIndicatorScaling;
				this.indicatorScaleRadius = config.indicatorScaleRadius;
				this.indicatorMinScale = config.indicatorMinScale;
				this.useIndicatorFading = config.useIndicatorFading;
				this.indicatorFadeRadius = config.indicatorFadeRadius;
				this.indicatorMinFade = config.indicatorMinFade;
			}

			// override minimap settings
			if (config.overrideMinimapSettings) {
				// assign new minimap profile
				if (this.currentMinimapProfile != config.minimapProfile) {
					this.currentMinimapProfile = config.minimapProfile;
					if (_HUDNavigationCanvas != null)
						_HUDNavigationCanvas.SetMinimapProfile (this.currentMinimapProfile);
				}

				// minimap usage changed?
				if (_HUDNavigationCanvas != null && this.useMinimap != config.useMinimap)
					_HUDNavigationCanvas.ShowMinimap (config.useMinimap);

				this.useMinimap = config.useMinimap;
				this.minimapMode = config.minimapMode;
				this.minimapScale = config.minimapScale;
				this.minimapRadius = config.minimapRadius;
				this.useMinimapScaling = config.useMinimapScaling;
				this.minimapScaleDistance = config.minimapScaleDistance;
				this.minimapMinScale = config.minimapMinScale;
				this.useMinimapFading = config.useMinimapFading;
				this.minimapFadeDistance = config.minimapFadeDistance;
				this.minimapMinFade = config.minimapMinFade;
				this.showMinimapBounds = config.showMinimapBounds;
				this.useMinimapHeightSystem = config.useMinimapHeightSystem;
				this.minimapDistanceAbove = config.minimapDistanceAbove;
				this.minimapDistanceBelow = config.minimapDistanceBelow;
			}
		}
		#endregion
	}


	#region Subclasses
	[System.Serializable]
	public enum _RotationReference
	{
		Camera, Controller
	}


	[System.Serializable]
	public enum _UpdateMode
	{
		Update, LateUpdate
	}


	[System.Serializable]
	public enum RadarModes
	{
		RotateRadar, RotatePlayer
	}


	[System.Serializable]
	public enum MinimapModes
	{
		RotateMinimap, RotatePlayer
	}
	#endregion
}

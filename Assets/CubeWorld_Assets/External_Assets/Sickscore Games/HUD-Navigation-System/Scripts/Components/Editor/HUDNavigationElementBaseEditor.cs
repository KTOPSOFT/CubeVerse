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

public class HUDNavigationElementBaseEditor : HUDNavigationBaseEditor
{
	#region Variables
	protected HUDNavigationSystem hudNavigationSystem;
	protected bool _hideSettings;
	private bool _radar_, _compassBar_, _indicator_, _minimap_;
	#endregion


	#region Main Methods
	protected virtual void OnEnable ()
	{
		hudNavigationSystem = FindObjectOfType<HUDNavigationSystem> ();
	}


	protected override void OnBaseInspectorGUI ()
	{
		// update serialized object
		serializedObject.Update ();

		// cache serialized properties
		SerializedProperty _pPrefabs = serializedObject.FindProperty ("Prefabs");
		SerializedProperty _pRadarPrefab = _pPrefabs.FindPropertyRelative ("RadarPrefab");
		SerializedProperty _pCompassBarPrefab = _pPrefabs.FindPropertyRelative ("CompassBarPrefab");
		SerializedProperty _pIndicatorPrefab = _pPrefabs.FindPropertyRelative ("IndicatorPrefab");
		SerializedProperty _pMinimapPrefab = _pPrefabs.FindPropertyRelative ("MinimapPrefab");

		SerializedProperty _pHideInRadar = serializedObject.FindProperty ("hideInRadar");
		SerializedProperty _pIgnoreRadarRadius = serializedObject.FindProperty ("ignoreRadarRadius");
		SerializedProperty _pIgnoreRadarScaling = serializedObject.FindProperty("ignoreRadarScaling");
		SerializedProperty _pIgnoreRadarFading = serializedObject.FindProperty("ignoreRadarFading");
		SerializedProperty _pRotateWithGameObject = serializedObject.FindProperty ("rotateWithGameObject");
		SerializedProperty _pUseRadarHeightSystem = serializedObject.FindProperty ("useRadarHeightSystem");

		SerializedProperty _pHideInCompassBar = serializedObject.FindProperty ("hideInCompassBar");
		SerializedProperty _pIgnoreCompassBarRadius = serializedObject.FindProperty ("ignoreCompassBarRadius");
		SerializedProperty _pUseCompassBarDistanceText = serializedObject.FindProperty ("useCompassBarDistanceText");
		SerializedProperty _pCompassBarDistanceTextFormat = serializedObject.FindProperty ("compassBarDistanceTextFormat");

		SerializedProperty _pShowIndicator = serializedObject.FindProperty ("showIndicator");
		SerializedProperty _pShowOffscreenIndicator = serializedObject.FindProperty ("showOffscreenIndicator");
		SerializedProperty _pIgnoreIndicatorRadius = serializedObject.FindProperty ("ignoreIndicatorRadius");
		SerializedProperty _pIgnoreIndicatorHideDistance = serializedObject.FindProperty ("ignoreIndicatorHideDistance");
		SerializedProperty _pIgnoreIndicatorScaling = serializedObject.FindProperty ("ignoreIndicatorScaling");
		SerializedProperty _pIgnoreIndicatorFading = serializedObject.FindProperty ("ignoreIndicatorFading");
		SerializedProperty _pUseIndicatorDistanceText = serializedObject.FindProperty ("useIndicatorDistanceText");
		SerializedProperty _pShowOffscreenIndicatorDistance = serializedObject.FindProperty ("showOffscreenIndicatorDistance");
		SerializedProperty _pIndicatorOnscreenDistanceTextFormat = serializedObject.FindProperty ("indicatorOnscreenDistanceTextFormat");
		SerializedProperty _pIndicatorOffscreenDistanceTextFormat = serializedObject.FindProperty ("indicatorOffscreenDistanceTextFormat");

		SerializedProperty _pHideInMinimap = serializedObject.FindProperty ("hideInMinimap");
		SerializedProperty _pIgnoreMinimapRadius = serializedObject.FindProperty ("ignoreMinimapRadius");
		SerializedProperty _pIgnoreMinimapScaling = serializedObject.FindProperty("ignoreMinimapScaling");
		SerializedProperty _pIgnoreMinimapFading = serializedObject.FindProperty("ignoreMinimapFading");
		SerializedProperty _pRotateWithGameObjectMM = serializedObject.FindProperty ("rotateWithGameObjectMM");
		SerializedProperty _pUseMinimapHeightSystem = serializedObject.FindProperty ("useMinimapHeightSystem");

		// render child content
		OnChildInspectorGUI ();

		// hide if settings asset is used
		if (!_hideSettings) {
			// PREFABS
			EditorGUILayout.BeginVertical (boxStyle);
			EditorGUILayout.PropertyField (_pRadarPrefab, new GUIContent ("Radar Prefab", "Assign a radar prefab, if you want to use this feature."));
			EditorGUILayout.PropertyField (_pCompassBarPrefab, new GUIContent ("CompassBar Prefab", "Assign a compass bar prefab, if you want to use this feature."));
			EditorGUILayout.PropertyField (_pIndicatorPrefab, new GUIContent ("Indicator Prefab", "Assign an indicator prefab, if you want to use this feature."));
			EditorGUILayout.PropertyField (_pMinimapPrefab, new GUIContent ("Minimap Prefab", "Assign a minimap prefab, if you want to use this feature."));
			EditorGUILayout.EndVertical ();

			GUILayout.Space (8); // SPACE

			// render child prepend content
			OnChildPrependInspectorGUI();

			// RADAR SETTINGS
			if (_pRadarPrefab.objectReferenceValue != null) {
				SerializedObject _sRadarObject = new SerializedObject (_pRadarPrefab.objectReferenceValue);

				EditorGUILayout.BeginVertical (boxStyle);
				_radar_ = EditorGUILayout.Foldout(_radar_, "Radar Settings", true, foldoutStyle);
				if (_radar_) {
					GUILayout.Space (4); // SPACE
					// CONTENT BEGIN
					EditorGUILayout.PropertyField (_pHideInRadar, new GUIContent ("Hide In Radar", "If enabled, this element will be hidden within the radar."));
					EditorGUILayout.PropertyField (_pIgnoreRadarRadius, new GUIContent ("Ignore Radius", "Enable, if this element should always be visible within the radar. Useful for e.g. quest markers."));
					EditorGUILayout.PropertyField (_pIgnoreRadarScaling, new GUIContent("Ignore Radar Scaling", "Enable, if this element should ignore the radar scaling feature."));
					EditorGUILayout.PropertyField (_pIgnoreRadarFading, new GUIContent("Ignore Radar Fading", "Enable, if this element should ignore the radar fading feature."));
					EditorGUILayout.PropertyField (_pRotateWithGameObject, new GUIContent ("Rotate With GameObject", "If enabled, this element will rotate within the radar depending on it's gameobject's rotation."));

					// height system settings
					GUILayout.Space (4); // SPACE
					EditorGUILayout.BeginVertical (boxStyle);
					_pUseRadarHeightSystem.boolValue = EditorGUILayout.ToggleLeft (new GUIContent ("Enable Height System", "Enable if you want to use the height system on this element."), _pUseRadarHeightSystem.boolValue, subHeaderStyle);
					if (_pUseRadarHeightSystem.boolValue) {
						if (hudNavigationSystem != null && !hudNavigationSystem.useRadarHeightSystem) {
							GUILayout.Space (4); // SPACE
							EditorGUILayout.HelpBox ("The Height System is deactivated!\nEnable this feature on the HUDNavigationSystem component.", MessageType.Warning);
						}
						if (_pRadarPrefab.objectReferenceValue == null) {
							GUILayout.Space (4); // SPACE
							EditorGUILayout.HelpBox ("Radar Prefab is missing!", MessageType.Error);
						} else {
							if (_sRadarObject.FindProperty ("ArrowAbove") == null) {
								GUILayout.Space (4); // SPACE
								EditorGUILayout.HelpBox ("Radar Prefab has no ArrowAbove reference assigned!", MessageType.Warning);
							}
							if (_sRadarObject.FindProperty ("ArrowBelow") == null) {
								GUILayout.Space (4); // SPACE
								EditorGUILayout.HelpBox ("Radar Prefab has no ArrowBelow reference assigned!", MessageType.Warning);
							}
						}
					}
					EditorGUILayout.EndVertical ();

					if (showHelpboxes) {
						if (_pHideInRadar.boolValue)
							EditorGUILayout.HelpBox ("Element will be hidden within the radar.", MessageType.Info);
						if (_pIgnoreRadarRadius.boolValue)
							EditorGUILayout.HelpBox ("The element will always be rendered within the radar, independent of the actual distance.", MessageType.Info);
						if (_pIgnoreRadarScaling.boolValue)
							EditorGUILayout.HelpBox("The element will not be affected by the radar scaling feature.", MessageType.Info);
						if (_pIgnoreRadarFading.boolValue)
							EditorGUILayout.HelpBox("The element will not be affected by the radar fading feature.", MessageType.Info);
						if (_pRotateWithGameObject.boolValue)
							EditorGUILayout.HelpBox ("Element within the radar will rotate depending on it's gameobject's rotation.", MessageType.Info);
					}
					// CONTENT ENDOF
				}
				EditorGUILayout.EndVertical ();
			}

			// COMPASS BAR SETTINGS
			if (_pCompassBarPrefab.objectReferenceValue != null) {
				SerializedObject _sCompassBarObject = new SerializedObject (_pCompassBarPrefab.objectReferenceValue);

				EditorGUILayout.BeginVertical (boxStyle);
				_compassBar_ = EditorGUILayout.Foldout(_compassBar_, "Compass Bar Settings", true, foldoutStyle);
				if (_compassBar_) {
					GUILayout.Space (4); // SPACE
					// CONTENT BEGIN
					EditorGUILayout.PropertyField (_pHideInCompassBar, new GUIContent ("Hide In Compass Bar", "If enabled, this element will be hidden within the compass bar."));
					if (_pHideInCompassBar.boolValue && showHelpboxes) {
						EditorGUILayout.HelpBox ("Element will be hidden within the compass bar.", MessageType.Info);
					} else {
						EditorGUILayout.PropertyField (_pIgnoreCompassBarRadius, new GUIContent ("Ignore Radius", "Enable, if this element should always be visible within the compass bar. Useful for e.g. quest markers."));
						if (_pIgnoreCompassBarRadius.boolValue && showHelpboxes)
							EditorGUILayout.HelpBox ("The element will always be rendered within the compass bar, independent of the actual distance.", MessageType.Info);
					}

					// distance text settings
					GUILayout.Space (4); // SPACE
					EditorGUILayout.BeginVertical (boxStyle);
					_pUseCompassBarDistanceText.boolValue = EditorGUILayout.ToggleLeft (new GUIContent ("Enable Distance Text", "Enable to show a text with the actual distance next to the element."), _pUseCompassBarDistanceText.boolValue, subHeaderStyle);
					if (_pUseCompassBarDistanceText.boolValue) {
						GUILayout.Space (4); // SPACE
						EditorGUILayout.PropertyField (_pCompassBarDistanceTextFormat, new GUIContent ("Text Format", "Define a text format for the distance. {0} will be replaced with the actual distance."));
						if (_pCompassBarPrefab.objectReferenceValue == null)
							EditorGUILayout.HelpBox ("CompassBar Prefab is missing!", MessageType.Error);
						else if (_sCompassBarObject.FindProperty ("DistanceText") == null)
							EditorGUILayout.HelpBox ("CompassBar Prefab has no distance text reference assigned!", MessageType.Warning);
					}
					EditorGUILayout.EndVertical ();
					// CONTENT ENDOF
				}
				EditorGUILayout.EndVertical ();
			}

			// INDICATOR SETTINGS
			if (_pIndicatorPrefab.objectReferenceValue != null) {
				SerializedObject _sIndicatorObject = new SerializedObject (_pIndicatorPrefab.objectReferenceValue);

				EditorGUILayout.BeginVertical (boxStyle);
				_indicator_ = EditorGUILayout.Foldout(_indicator_, "Indicator Settings", true, foldoutStyle);
				if (_indicator_) {
					GUILayout.Space (4); // SPACE
					// CONTENT BEGIN
					EditorGUILayout.PropertyField (_pShowIndicator, new GUIContent ("Show Indicator", "Enable, if you want to use the indicator feature on this element. Needs to be enabled on the HUD Navigation System component."));
					if (_pShowIndicator.boolValue) {
						EditorGUILayout.PropertyField (_pShowOffscreenIndicator, new GUIContent ("Show Offscreen Indicator", "Enable, if you want to use the offscreen indicator feature on this element. Needs to be enabled on the HUD Navigation System component."));

						if (hudNavigationSystem != null) {
							if (!hudNavigationSystem.useIndicators)
								EditorGUILayout.HelpBox ("Indicators are deactivated!\nEnable this feature on the HUDNavigationSystem component.", MessageType.Warning);
							if (_pShowOffscreenIndicator.boolValue && !hudNavigationSystem.useOffscreenIndicators)
								EditorGUILayout.HelpBox ("Offscreen Indicators are deactivated!\nEnable this feature on the HUDNavigationSystem ", MessageType.Warning);
						}

						EditorGUILayout.PropertyField (_pIgnoreIndicatorRadius, new GUIContent ("Ignore Radius", "Enable, if the indicator of this element should always be visible. Useful for e.g. quest markers."));
						EditorGUILayout.PropertyField (_pIgnoreIndicatorHideDistance, new GUIContent ("Ignore Hide Distance", "Enable, if the indicator of this element should ignore the hide distance. Useful for e.g. pickup items."));
						EditorGUILayout.PropertyField (_pIgnoreIndicatorScaling, new GUIContent ("Ignore Distance Scaling", "Enable, if the indicator should ignore the distance scaling feature. Useful for e.g. pickup items."));
						EditorGUILayout.PropertyField (_pIgnoreIndicatorFading, new GUIContent ("Ignore Distance Fading", "Enable, if the indicator should ignore the distance fading feature. Useful for e.g. POI's or quest markers."));
						if (showHelpboxes) {
							if (_pIgnoreIndicatorRadius.boolValue)
								EditorGUILayout.HelpBox ("The indicator will always be rendered, independent of the actual distance.", MessageType.Info);
							if (_pIgnoreIndicatorHideDistance.boolValue)
								EditorGUILayout.HelpBox ("The indicator will still be rendered, even if the player is close to it (hide distance).", MessageType.Info);
							if (_pIgnoreIndicatorScaling.boolValue)
								EditorGUILayout.HelpBox ("The indicator will not be affected by the distance scaling feature.", MessageType.Info);
							if (_pIgnoreIndicatorFading.boolValue)
								EditorGUILayout.HelpBox ("The indicator will not be affected by the distance fading feature.", MessageType.Info);
						}

						// distance text settings
						GUILayout.Space (4); // SPACE
						EditorGUILayout.BeginVertical (boxStyle);
						_pUseIndicatorDistanceText.boolValue = EditorGUILayout.ToggleLeft (new GUIContent ("Enable Distance Text", "Enable to show a text with the actual distance next to the element."), _pUseIndicatorDistanceText.boolValue, subHeaderStyle);
						if (_pUseIndicatorDistanceText.boolValue) {
							GUILayout.Space (4); // SPACE
							EditorGUILayout.PropertyField (_pShowOffscreenIndicatorDistance, new GUIContent ("Show Offscreen Distance", "Enable, if you want to show the offscreen distance text."));
							EditorGUILayout.PropertyField (_pIndicatorOnscreenDistanceTextFormat, new GUIContent ("Onscreen Text Format", "Define a text format for the onscreen distance. {0} will be replaced with the actual distance."));
							if (_pShowOffscreenIndicatorDistance.boolValue)
								EditorGUILayout.PropertyField (_pIndicatorOffscreenDistanceTextFormat, new GUIContent ("Offscreen Text Format", "Define a text format for the offscreen distance. {0} will be replaced with the actual distance."));
							
							if (_pIndicatorPrefab.objectReferenceValue == null) {
								EditorGUILayout.HelpBox ("Indicator Prefab is missing!", MessageType.Error);
							} else {
								if (_sIndicatorObject.FindProperty ("OnscreenDistanceText") == null)
									EditorGUILayout.HelpBox ("Indicator Prefab has no onscreen distance text reference assigned!", MessageType.Warning);
								if (_pShowOffscreenIndicatorDistance.boolValue && _sIndicatorObject.FindProperty ("OffscreenDistanceText") == null)
									EditorGUILayout.HelpBox ("Indicator Prefab has no offscreen distance text reference assigned!", MessageType.Warning);
							}
						}
						EditorGUILayout.EndVertical ();
					}
					// CONTENT ENDOF
				}
				EditorGUILayout.EndVertical ();
			}

			// MINIMAP SETTINGS
			if (_pMinimapPrefab.objectReferenceValue != null) {
				SerializedObject _sMinimapObject = new SerializedObject (_pMinimapPrefab.objectReferenceValue);

				EditorGUILayout.BeginVertical (boxStyle);
				_minimap_ = EditorGUILayout.Foldout(_minimap_, "Minimap Settings", true, foldoutStyle);
				if (_minimap_) {
					GUILayout.Space (4); // SPACE
					// CONTENT BEGIN
					EditorGUILayout.PropertyField (_pHideInMinimap, new GUIContent ("Hide In Minimap", "If enabled, this element will be hidden within the minimap."));
					EditorGUILayout.PropertyField (_pIgnoreMinimapRadius, new GUIContent ("Ignore Radius", "Enable, if this element should always be visible within the minimap. Useful for e.g. quest markers."));
					EditorGUILayout.PropertyField (_pIgnoreMinimapScaling, new GUIContent("Ignore Minimap Scaling", "Enable, if this element should ignore the minimap scaling feature."));
					EditorGUILayout.PropertyField (_pIgnoreMinimapFading, new GUIContent("Ignore Minimap Fading", "Enable, if this element should ignore the minimap fading feature."));
					EditorGUILayout.PropertyField (_pRotateWithGameObjectMM, new GUIContent ("Rotate With GameObject", "If enabled, this element will rotate within the minimap depending on it's gameobject's rotation."));

					// height system settings
					GUILayout.Space (4); // SPACE
					EditorGUILayout.BeginVertical (boxStyle);
					_pUseMinimapHeightSystem.boolValue = EditorGUILayout.ToggleLeft (new GUIContent ("Enable Height System", "Enable if you want to use the height system on this element."), _pUseMinimapHeightSystem.boolValue, subHeaderStyle);
					if (_pUseMinimapHeightSystem.boolValue) {
						if (hudNavigationSystem != null && !hudNavigationSystem.useMinimapHeightSystem) {
							GUILayout.Space (4); // SPACE
							EditorGUILayout.HelpBox ("The Height System is deactivated!\nEnable this feature on the HUDNavigationSystem component.", MessageType.Warning);
						}
						if (_pMinimapPrefab.objectReferenceValue == null) {
							GUILayout.Space (4); // SPACE
							EditorGUILayout.HelpBox ("Minimap Prefab is missing!", MessageType.Error);
						} else {
							if (_sMinimapObject.FindProperty ("ArrowAbove") == null) {
								GUILayout.Space (4); // SPACE
								EditorGUILayout.HelpBox ("Minimap Prefab has no ArrowAbove reference assigned!", MessageType.Warning);
							}
							if (_sMinimapObject.FindProperty ("ArrowBelow") == null) {
								GUILayout.Space (4); // SPACE
								EditorGUILayout.HelpBox ("Minimap Prefab has no ArrowBelow reference assigned!", MessageType.Warning);
							}
						}
					}
					EditorGUILayout.EndVertical ();

					if (showHelpboxes) {
						if (_pHideInMinimap.boolValue)
							EditorGUILayout.HelpBox ("Element will be hidden within the minimap.", MessageType.Info);
						if (_pIgnoreMinimapRadius.boolValue)
							EditorGUILayout.HelpBox ("The element will always be rendered within the minimap, independent of the actual distance.", MessageType.Info);
						if (_pIgnoreMinimapScaling.boolValue)
							EditorGUILayout.HelpBox("The element will not be affected by the minimap scaling feature.", MessageType.Info);
						if (_pIgnoreMinimapFading.boolValue)
							EditorGUILayout.HelpBox("The element will not be affected by the minimap fading feature.", MessageType.Info);
						if (_pRotateWithGameObjectMM.boolValue)
							EditorGUILayout.HelpBox ("Element within the minimap will rotate depending on it's gameobject's rotation.", MessageType.Info);
					}
					// CONTENT ENDOF
				}
				EditorGUILayout.EndVertical ();
			}

			// render child append content
			OnChildAppendInspectorGUI();
		}

		// render child end content
		OnChildEndInspectorGUI ();

		// apply modified properties
		serializedObject.ApplyModifiedProperties ();
	}


	protected override void OnExpandSettings (bool value)
	{
		base.OnExpandSettings (value);
		_radar_ = _compassBar_ = _indicator_ = _minimap_ = value;
	}


	protected virtual void OnChildInspectorGUI () {}
	protected virtual void OnChildPrependInspectorGUI () {}
	protected virtual void OnChildAppendInspectorGUI () {}
	protected virtual void OnChildEndInspectorGUI () {}
	#endregion
}

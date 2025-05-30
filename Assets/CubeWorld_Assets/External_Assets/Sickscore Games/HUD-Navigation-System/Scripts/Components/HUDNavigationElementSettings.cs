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
	[CreateAssetMenu (fileName="New Element Settings", menuName=HNS.PublisherName+"/"+HNS.Name+"/New Element Settings")]
	public class HUDNavigationElementSettings : ScriptableObject
	{
		#region Variables
		// MISC
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
		#endregion


		#region Main Methods
		public void CopySettings (HUDNavigationElement element)
		{
			if (element == null)
				return;
			
			// misc
			this.Prefabs = element.Prefabs;

			// radar settings
			this.hideInRadar = element.hideInRadar;
			this.ignoreRadarRadius = element.ignoreRadarRadius;
			this.ignoreRadarScaling = element.ignoreRadarScaling;
			this.ignoreRadarFading = element.ignoreRadarFading;
			this.rotateWithGameObject = element.rotateWithGameObject;
			this.useRadarHeightSystem = element.useRadarHeightSystem;

			// compass bar settings
			this.hideInCompassBar = element.hideInCompassBar;
			this.ignoreCompassBarRadius = element.ignoreCompassBarRadius;
			this.useCompassBarDistanceText = element.useCompassBarDistanceText;
			this.compassBarDistanceTextFormat = element.compassBarDistanceTextFormat;

			// indicator settings
			this.showIndicator = element.showIndicator;
			this.showOffscreenIndicator = element.showOffscreenIndicator;
			this.ignoreIndicatorRadius = element.ignoreIndicatorRadius;
			this.ignoreIndicatorHideDistance = element.ignoreIndicatorHideDistance;
			this.ignoreIndicatorScaling = element.ignoreIndicatorScaling;
			this.ignoreIndicatorFading = element.ignoreIndicatorFading;
			this.useIndicatorDistanceText = element.useIndicatorDistanceText;
			this.showOffscreenIndicatorDistance = element.showOffscreenIndicatorDistance;
			this.indicatorOnscreenDistanceTextFormat = element.indicatorOnscreenDistanceTextFormat;
			this.indicatorOffscreenDistanceTextFormat = element.indicatorOffscreenDistanceTextFormat;

			// minimap settings
			this.hideInMinimap = element.hideInMinimap;
			this.ignoreMinimapRadius = element.ignoreMinimapRadius;
			this.ignoreMinimapScaling = element.ignoreMinimapScaling;
			this.ignoreMinimapFading = element.ignoreMinimapFading;
			this.rotateWithGameObjectMM = element.rotateWithGameObjectMM;
			this.useMinimapHeightSystem = element.useMinimapHeightSystem;
		}
		#endregion
	}
}

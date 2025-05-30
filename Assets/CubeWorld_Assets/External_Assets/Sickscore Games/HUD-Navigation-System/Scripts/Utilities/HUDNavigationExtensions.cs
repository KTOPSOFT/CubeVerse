using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	public static class HUDNavigationExtensions
	{
		#region Extension Methods
		public static float GetDistance (this HUDNavigationElement element, Transform other)
		{
			return Vector2.Distance (new Vector2 (element.transform.position.x, element.transform.position.z), new Vector2 (other.position.x, other.position.z));
		}


		public static Vector3 GetPosition (this HUDNavigationElement element)
		{
			return element.transform.position;
		}


		public static Vector3 GetPositionOffset (this HUDNavigationElement element, Vector3 otherPosition)
		{
			return element.transform.position - otherPosition;
		}


		public static float GetRadius (this RectTransform rect)
		{
			Vector3[] arr = new Vector3[4];
			rect.GetLocalCorners (arr);
			float _radius = Mathf.Abs (arr [0].y);
			if (Mathf.Abs (arr [0].x) < Mathf.Abs (arr [0].y))
				_radius = Mathf.Abs (arr [0].x);

			return _radius;
		}


		public static Vector3 KeepInRectBounds (this RectTransform rect, Vector3 markerPos, out bool outOfBounds)
		{
			Vector3 oldPos = markerPos;
			markerPos = Vector3.Min (markerPos, rect.rect.max);
			markerPos = Vector3.Max (markerPos, rect.rect.min);

			outOfBounds = oldPos != markerPos;

			return markerPos;
		}


		public static float GetIconRadius (this HUDNavigationElement element, NavigationElementType elementType)
		{
			float radius = (elementType == NavigationElementType.Radar) ? element.Radar.PrefabRect.sizeDelta.x : element.Minimap.PrefabRect.sizeDelta.x;
			return radius / 2f;
		}


		public static bool IsVisibleOnScreen (this HUDNavigationElement element, Vector3 screenPos)
		{
			return screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
		}


		public static void SetMarkerPosition (this HUDNavigationElement element, NavigationElementType elementType, Vector3 markerPos, RectTransform parentRect = null)
		{
			// set marker position
			if (elementType == NavigationElementType.Radar)
				element.Radar.transform.localPosition = markerPos;
			else if (elementType == NavigationElementType.CompassBar)
				element.CompassBar.transform.position = new Vector3 (markerPos.x + parentRect.localPosition.x, parentRect.position.y, 0f);
			else if (elementType == NavigationElementType.Minimap)
				element.Minimap.transform.localPosition = markerPos;
		}


		public static void SetMarkerActive (this HUDNavigationElement element, NavigationElementType elementType, bool value)
		{
			// get marker gameobject
			GameObject markerGO = null;
			switch (elementType) {
				case NavigationElementType.Radar:
					markerGO = element.Radar.gameObject;
					break;
				case NavigationElementType.CompassBar:
					markerGO = element.CompassBar.gameObject;
					break;
				case NavigationElementType.Minimap:
					markerGO = element.Minimap.gameObject;
					break;
				default:
					break;
			}

			// set marker gameobject active/inactive
			if (markerGO != null) {
				// only update if value has changed
				if (value != markerGO.activeSelf) {
					// invoke events
					if (value) // appeared
						element.OnAppear.Invoke (element, elementType);
					else // disappeared
						element.OnDisappear.Invoke (element, elementType);

					// set active state
					markerGO.gameObject.SetActive (value);
				}
			}
		}

		public static void SetMarkerScale (this HUDNavigationElement element, bool scalingEnabled, float distance, float scaleDistance, float minScale, RectTransform prefabRect)
		{
			if (!scalingEnabled || prefabRect == null)
				return;

			// set marker scale
			float scale = (distance - 1f) / (scaleDistance - 1f);
			prefabRect.localScale = Vector2.Lerp(Vector2.one * minScale, Vector2.one, Mathf.Clamp01(scale));
		}

		public static void SetMarkerFade (this HUDNavigationElement element, bool fadingEnabled, float distance, float fadeDistance, float minFade, CanvasGroup canvasGroup)
		{
			if (!fadingEnabled || canvasGroup == null)
				return;

			// set marker fade
			float fade = (distance - 1f) / (fadeDistance - 1f);
			canvasGroup.alpha = Mathf.Lerp(1f * minFade, 1f, Mathf.Clamp01(fade));
		}
		#endregion


		#region Radar Extension Methods
		public static void ShowRadarAboveArrow (this HUDNavigationElement element, bool value)
		{
			if (element.Radar.ArrowAbove == null)
				return;

			// only update if value has changed
			if (value != element.Radar.ArrowAbove.gameObject.activeSelf)
				element.Radar.ArrowAbove.gameObject.SetActive (value);
		}


		public static void ShowRadarBelowArrow (this HUDNavigationElement element, bool value)
		{
			if (element.Radar.ArrowBelow == null)
				return;

			// only update if value has changed
			if (value != element.Radar.ArrowBelow.gameObject.activeSelf)
				element.Radar.ArrowBelow.gameObject.SetActive (value);
		}
		#endregion


		#region CompassBar Extension Methods
		public static void ShowCompassBarDistance (this HUDNavigationElement element, int distance = 0)
		{
			if (element.CompassBar.DistanceText == null)
				return;

			// only update if value has changed
			bool useDistanceText = element.useCompassBarDistanceText;
			if (useDistanceText != element.CompassBar.DistanceText.gameObject.activeSelf)
				element.CompassBar.DistanceText.gameObject.SetActive (useDistanceText);

			// update distance text if active
			if (useDistanceText) // TODO add TextMeshPro support?
				element.CompassBar.DistanceText.text = string.Format (element.compassBarDistanceTextFormat, distance);
		}
		#endregion


		#region Indicator Extension Methods
		public static void SetIndicatorActive (this HUDNavigationElement element, bool value)
		{
			// only update, if value has changed
			if (value != element.Indicator.gameObject.activeSelf) {
				// invoke events
				if (value) // appeared
					element.OnAppear.Invoke (element, NavigationElementType.Indicator);
				else // disappeared
					element.OnDisappear.Invoke (element, NavigationElementType.Indicator);

				// set indicator active/inactive
				element.Indicator.gameObject.SetActive (value);
			}
		}


		public static void ShowIndicatorDistance (this HUDNavigationElement element, bool onScreen, int distance = 0)
		{
			// show/hide distance text
			Text distanceText = (onScreen) ? element.Indicator.OnscreenDistanceText : element.Indicator.OffscreenDistanceText;
			if (distanceText != null) {
				bool showDistance = (onScreen) ? element.useIndicatorDistanceText : element.useIndicatorDistanceText && element.showOffscreenIndicatorDistance;

				// only update if value has changed
				if (showDistance != distanceText.gameObject.activeSelf)
					distanceText.gameObject.SetActive (showDistance);

				// update distance text if active
				if (showDistance) // TODO add TextMeshPro support?
					distanceText.text = string.Format ((onScreen) ? element.indicatorOnscreenDistanceTextFormat : element.indicatorOffscreenDistanceTextFormat, distance);
			}
		}


		public static void SetIndicatorOnOffscreen (this HUDNavigationElement element, bool value)
		{
			// show/hide onscreen rect
			if (element.Indicator.OnscreenRect != null) {
				// only update, if value has changed
				if (value != element.Indicator.OnscreenRect.gameObject.activeSelf)
					element.Indicator.OnscreenRect.gameObject.SetActive (value);
			}

			// show/hide offscreen rect
			if (element.Indicator.OffscreenRect != null) {
				// only update, if value has changed
				if (!value != element.Indicator.OffscreenRect.gameObject.activeSelf)
					element.Indicator.OffscreenRect.gameObject.SetActive (!value);
			}
		}


		public static void SetIndicatorPosition (this HUDNavigationElement element, Vector3 indicatorPos, RectTransform parentRect = null)
		{
			// set indicator position
			element.Indicator.transform.position = (parentRect != null) ? new Vector3 (indicatorPos.x + parentRect.localPosition.x, indicatorPos.y + parentRect.localPosition.y, 0f) : indicatorPos;
		}


		public static void SetIndicatorOffscreenRotation (this HUDNavigationElement element, Quaternion rotation)
		{
			// set indicator offscreen pointer rotation
			if (element.Indicator.OffscreenPointer != null)
				element.Indicator.OffscreenPointer.transform.rotation = rotation;
		}
		#endregion


		#region Minimap Extension Methods
		public static Vector3 MinimapRotationOffset (this Transform _transform, Vector3 position)
		{
			Vector2 offset = position.x * new Vector2 (_transform.right.x, -_transform.right.z);
			offset += position.y * new Vector2 (-_transform.forward.x, _transform.forward.z);
			return offset;
		}


		public static void ShowMinimapAboveArrow (this HUDNavigationElement element, bool value)
		{
			if (element.Minimap.ArrowAbove == null)
				return;

			// only update if value has changed
			if (value != element.Minimap.ArrowAbove.gameObject.activeSelf)
				element.Minimap.ArrowAbove.gameObject.SetActive (value);
		}


		public static void ShowMinimapBelowArrow (this HUDNavigationElement element, bool value)
		{
			if (element.Minimap.ArrowBelow == null)
				return;

			// only update if value has changed
			if (value != element.Minimap.ArrowBelow.gameObject.activeSelf)
				element.Minimap.ArrowBelow.gameObject.SetActive (value);
		}
		#endregion


		#region MapProfile Extension Methods
		public static Vector2 GetMapUnitScale (this HNSMapProfile profile)
		{
			return new Vector2 (profile.MapTextureSize.x / profile.MapBounds.size.x, profile.MapTextureSize.y / profile.MapBounds.size.z);
		}


		public static float GetMapAspect (this HNSMapProfile profile)
		{
			return (float)profile.MapTextureSize.x / (float)profile.MapTextureSize.y;
		}
		#endregion
	}
}

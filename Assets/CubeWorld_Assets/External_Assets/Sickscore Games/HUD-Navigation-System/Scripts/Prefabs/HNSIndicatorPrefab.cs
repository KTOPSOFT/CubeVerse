using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[AddComponentMenu (HNS.Name + "/HNS Indicator Prefab")]
	public class HNSIndicatorPrefab : HNSPrefab
	{
		#region Variables
		[Header("Onscreen")]
		[Tooltip("Assign the transform for the onscreen marker.")]
		public RectTransform OnscreenRect;
		[Tooltip("Assign an onscreen image component.")]
		public Image OnscreenIcon;

		[Header("Offscreen")]
		[Tooltip("Assign the transform for the offscreen marker.")]
		public RectTransform OffscreenRect;
		[Tooltip("Assign the transform which should rotate towards the offscreen element.")]
		public RectTransform OffscreenPointer;
		[Tooltip("Assign an offscreen image component.")]
		public Image OffscreenIcon;

		[Header("Distance Text")]
		[Tooltip("(optional) Assign an onscreen distance text component.")]
		public Text OnscreenDistanceText;
		[Tooltip("(optional) Assign an offscreen distance text component.")]
		public Text OffscreenDistanceText;
		#endregion


		#region Main Methods
		protected override void OnEnable ()
		{
			base.OnEnable ();

			// assign/add canvas group (for element fading)
			PrefabCanvasGroup = GetComponent<CanvasGroup> ();
			if (PrefabCanvasGroup == null) {
				PrefabCanvasGroup = gameObject.AddComponent<CanvasGroup> ();
				PrefabCanvasGroup.interactable = PrefabCanvasGroup.blocksRaycasts = false;
			}
		}


		/// <summary>
		/// Change the color of the offscreen indicator icon.
		/// </summary>
		/// <param name="color">Color.</param>
		public void ChangeOffscreenIconColor (Color color)
		{
			if (OffscreenIcon != null)
				OffscreenIcon.color = color;
		}
		#endregion


		#region Override Methods
		/// <summary>
		/// Change the color of the onscreen indicator icon.
		/// </summary>
		/// <param name="color">Color.</param>
		public override void ChangeIconColor (Color color)
		{
			base.ChangeIconColor (color);
			if (OnscreenIcon != null)
				OnscreenIcon.color = color;
		}
		#endregion
	}
}

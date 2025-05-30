using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[AddComponentMenu (HNS.Name + "/HNS Radar Prefab")]
	public class HNSRadarPrefab : HNSPrefab
	{
		#region Variables
		[Header("Icon")]
		[Tooltip("Assign an image component.")]
		public Image Icon;

		[Header("Height Arrows")]
		[Tooltip("Assign the above arrow image component.")]
		public Image ArrowAbove;
		[Tooltip("Assign the above arrow image component.")]
		public Image ArrowBelow;
		#endregion


		#region Main Methods
		protected override void OnEnable ()
		{
			base.OnEnable();

			// assign/add canvas group (for element fading)
			PrefabCanvasGroup = GetComponent<CanvasGroup>();
			if (PrefabCanvasGroup == null)
			{
				PrefabCanvasGroup = gameObject.AddComponent<CanvasGroup>();
				PrefabCanvasGroup.interactable = PrefabCanvasGroup.blocksRaycasts = false;
			}
		}
		#endregion


		#region Override Methods
		/// <summary>
		/// Change the color of the radar icon.
		/// </summary>
		/// <param name="color">Color.</param>
		public override void ChangeIconColor (Color color)
		{
			base.ChangeIconColor (color);
			if (Icon != null)
				Icon.color = color;
		}
		#endregion
	}
}

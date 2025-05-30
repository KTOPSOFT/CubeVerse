using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[AddComponentMenu (HNS.Name + "/HNS CompassBar Prefab")]
	public class HNSCompassBarPrefab : HNSPrefab
	{
		#region Variables
		[Header("Icon")]
		[Tooltip("Assign an image component.")]
		public Image Icon;

		[Header("Distance Text")]
		[Tooltip("Assign the distance text component.")]
		public Text DistanceText;
		#endregion


		#region Main Methods
		#endregion


		#region Override Methods
		/// <summary>
		/// Change the color of the compass bar icon.
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

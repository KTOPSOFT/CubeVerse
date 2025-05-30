using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	[RequireComponent(typeof(Text))]
	public class HUDCurrentDegrees : MonoBehaviour
	{
		#region Variables
		protected Text text;
		#endregion


		#region Main Methods
		void Awake ()
		{
			text = GetComponent<Text> ();
		}


		void Update ()
		{
			text.text = ((int)HUDNavigationCanvas.Instance.CompassBarCurrentDegrees).ToString ();
		}
		#endregion
	}
}

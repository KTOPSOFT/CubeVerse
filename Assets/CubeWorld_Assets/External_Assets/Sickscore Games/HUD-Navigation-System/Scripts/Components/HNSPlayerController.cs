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
	[AddComponentMenu (HNS.Name + "/HNS Player Controller")]
	public class HNSPlayerController : MonoBehaviour
	{
		#region Variables
		#endregion


		#region Main Methods
		void Start ()
		{
			if (HUDNavigationSystem.Instance != null)
				HUDNavigationSystem.Instance.ChangePlayerController (this.gameObject.transform);
		}
		#endregion


		#region Utility Methods
		#endregion
	}


	#region Subclasses
	#endregion
}

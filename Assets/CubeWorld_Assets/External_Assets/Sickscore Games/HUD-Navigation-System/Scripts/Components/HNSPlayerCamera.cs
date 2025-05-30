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
	[AddComponentMenu (HNS.Name + "/HNS Player Camera")]
	public class HNSPlayerCamera : MonoBehaviour
	{
		#region Variables
		#endregion


		#region Main Methods
		void Start ()
		{
			if (HUDNavigationSystem.Instance != null) {
				Camera camera = gameObject.GetComponent<Camera> ();
				HUDNavigationSystem.Instance.ChangePlayerCamera (camera);
			}
		}
		#endregion


		#region Utility Methods
		#endregion
	}


	#region Subclasses
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	public class HUDKeepRotation : MonoBehaviour
	{
		#region Variables
		#endregion


		#region Main Methods
		private Transform _transform;
		private Quaternion _rotation;

		void Awake ()
		{
			_transform = this.transform;
			_rotation = _transform.rotation;
		}


		void LateUpdate ()
		{
			_transform.rotation = _rotation;
		}
		#endregion


		#region Utility Methods
		#endregion


		#region Subclasses
		#endregion
	}
}

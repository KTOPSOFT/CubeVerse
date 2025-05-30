using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SickscoreGames;

namespace SickscoreGames.HUDNavigationSystem
{
	public class HNSPrefab : MonoBehaviour
	{
		#region Variables
		[HideInInspector]
		public List<CustomTransform> CustomTransforms = new List<CustomTransform> ();


		[HideInInspector]
		public RectTransform PrefabRect;

		[HideInInspector]
		public CanvasGroup PrefabCanvasGroup;
		#endregion


		#region Main Methods
		protected virtual void OnEnable ()
		{
			// assign own rect transform
			PrefabRect = GetComponent<RectTransform> ();
		}


		/// <summary>
		/// Gets a custom transform.
		/// </summary>
		/// <returns>Custom transform.</returns>
		/// <param name="name">Unique Name.</param>
		public Transform GetCustomTransform (string name)
		{
			CustomTransform custom = CustomTransforms.FirstOrDefault (ct => ct.name.Equals (name));
			if (custom != null)
				return custom.transform;
			
			return null;
		}


		public virtual void ChangeIconColor (Color color) {}
		#endregion
	}


	#region Subclasses
	[System.Serializable]
	public class CustomTransform
	{
		[Tooltip("Enter a unique name for this transform.")]
		public string name;
		[Tooltip("Assign the transform you want to add.")]
		public Transform transform;
	}
	#endregion
}

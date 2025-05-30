using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SickscoreGames.HUDNavigationSystem; // MANDATORY !!

public class ExampleCallbackScript : MonoBehaviour
{
	#region Example Methods
	public void ChangeIndicatorColors (HUDNavigationElement element)
	{
		// EXAMPLE:
		// => Change the indicator colors of the element
		if (element.Indicator != null) {
			// onscreen icon color
			if (element.Indicator.OnscreenIcon != null)
				element.Indicator.OnscreenIcon.color = Color.magenta;
			
			// offscreen icon color
			if (element.Indicator.OffscreenIcon != null)
				element.Indicator.OffscreenIcon.color = Color.magenta;
		}
	}


	public void OnElementAppeared (HUDNavigationElement element, NavigationElementType type)
	{
		Debug.LogFormat ("{0} element of {1} appeared.", type, element.name);
	}


	public void OnElementDisappeared (HUDNavigationElement element, NavigationElementType type)
	{
		Debug.LogFormat ("{0} element of {1} disappeared.", type, element.name);
	}


	public void OnElementEnterRadius (HUDNavigationElement element, NavigationElementType type)
	{
		Debug.LogFormat ("{0} element of {1} entered radius.", type, element.name);
	}


	public void OnElementLeaveRadius (HUDNavigationElement element, NavigationElementType type)
	{
		Debug.LogFormat ("{0} element of {1} left radius.", type, element.name);
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SickscoreGames.HUDNavigationSystem;
using SickscoreGames.ExampleScene;

public class ExampleInteractions : MonoBehaviour
{
	#region Variables
	public LayerMask layerMask = 1 << 0;
	public float interactionDistance = 4f;

	private RaycastHit hit;
	private Transform pickupText;
	private Transform interactionText;
	private HUDNavigationSystem _HUDNavigationSystem;
	#endregion


	#region Main Methods
	void Start ()
	{
		_HUDNavigationSystem = HUDNavigationSystem.Instance;
	}


	void Update ()
	{
		HandleKeyInput ();
		HandleItemPickUp ();
		HandlePrismColorChange ();
	}
	#endregion


	#region Utility Methods
	void HandleKeyInput ()
	{
		// update radar zoom / indicator border input
		if (Input.GetKey (KeyCode.X) && _HUDNavigationSystem.radarZoom < 5f)
			_HUDNavigationSystem.radarZoom += .0175f;
		else if (Input.GetKey (KeyCode.C) && _HUDNavigationSystem.radarZoom > .25f)
			_HUDNavigationSystem.radarZoom -= .0175f;
		else if (Input.GetKey (KeyCode.V) && _HUDNavigationSystem.indicatorOffscreenBorder < .7f)
			_HUDNavigationSystem.indicatorOffscreenBorder += .01f;
		else if (Input.GetKey (KeyCode.B) && _HUDNavigationSystem.indicatorOffscreenBorder > .07f)
			_HUDNavigationSystem.indicatorOffscreenBorder -= .01f;
		else if (Input.GetKey (KeyCode.N) && _HUDNavigationSystem.minimapScale > .06f)
			_HUDNavigationSystem.minimapScale -= .0075f;
		else if (Input.GetKey (KeyCode.M) && _HUDNavigationSystem.minimapScale < .35f)
			_HUDNavigationSystem.minimapScale += .0075f;

		// update feature enable / disable input
		if (Input.GetKeyDown (KeyCode.H))
			_HUDNavigationSystem.EnableSystem (!_HUDNavigationSystem.isEnabled);
		if (Input.GetKeyDown (KeyCode.Alpha1))
			_HUDNavigationSystem.EnableRadar (!_HUDNavigationSystem.useRadar);
		if (Input.GetKeyDown (KeyCode.Alpha2))
			_HUDNavigationSystem.EnableCompassBar (!_HUDNavigationSystem.useCompassBar);
		if (Input.GetKeyDown (KeyCode.Alpha3))
			_HUDNavigationSystem.EnableIndicators (!_HUDNavigationSystem.useIndicators);
		if (Input.GetKeyDown (KeyCode.Alpha4))
			_HUDNavigationSystem.EnableMinimap (!_HUDNavigationSystem.useMinimap);

		// toggle radar / minimap mode
		if (Input.GetKeyDown (KeyCode.Alpha5))
			_HUDNavigationSystem.radarMode = (_HUDNavigationSystem.radarMode == RadarModes.RotateRadar) ? RadarModes.RotatePlayer : RadarModes.RotateRadar;
		if (Input.GetKeyDown (KeyCode.Alpha6))
			_HUDNavigationSystem.minimapMode = (_HUDNavigationSystem.minimapMode == MinimapModes.RotateMinimap) ? MinimapModes.RotatePlayer : MinimapModes.RotateMinimap;

		// toggle minimap custom layers
		if (Input.GetKeyDown (KeyCode.Alpha7) && _HUDNavigationSystem.currentMinimapProfile != null) {
			GameObject blackWhiteLayer = _HUDNavigationSystem.currentMinimapProfile.GetCustomLayer ("exampleLayer");
			if (blackWhiteLayer != null)
				blackWhiteLayer.SetActive (!blackWhiteLayer.activeSelf);
		}

		// toggle day/night scene
		if (Input.GetKeyUp (KeyCode.Return)) {
			if (SceneManager.GetActiveScene ().buildIndex == 0)
				SceneManager.LoadScene (1);
			else
				SceneManager.LoadScene (0);
		}
	}


	void HandleItemPickUp ()
	{
		if (!_HUDNavigationSystem.isEnabled)
			return;
		
		// check for pickup items
		if (Physics.Raycast (transform.position, transform.TransformDirection (Vector3.forward), out hit, interactionDistance, layerMask) && hit.collider.name.Contains ("PickUp")) {
			// get HUD navigation element component
			HUDNavigationElement element = hit.collider.gameObject.GetComponent<HUDNavigationElement> ();
			if (element != null) {
				// show pickup text
				if (element.Indicator != null) {
					pickupText = element.Indicator.GetCustomTransform ("pickupText");
					if (pickupText != null)
						pickupText.gameObject.SetActive (true);
				}

				// wait for interaction input and destroy gameobject
				if (Input.GetKeyDown (KeyCode.E))
					Destroy (element.gameObject);
			}
		} else {
			// reset pickup text
			if (pickupText != null) {
				pickupText.gameObject.SetActive (false);
				pickupText = null;
			}
		}
	}


	void HandlePrismColorChange ()
	{
		if (!_HUDNavigationSystem.isEnabled)
			return;
		
		// check for colored prisms
		if (Physics.Raycast (transform.position, transform.TransformDirection (Vector3.forward), out hit, interactionDistance, layerMask) && hit.collider.name.Contains ("Prism")) {
			// get HUD navigation element component
			HUDNavigationElement element = hit.collider.gameObject.GetComponentInChildren<HUDNavigationElement> ();
			if (element != null) {
				// show interaction text
				if (element.Indicator != null) {
					interactionText = element.Indicator.GetCustomTransform ("interactionText");
					if (interactionText != null)
						interactionText.gameObject.SetActive (true);
				}

				// wait for interaction input and change prism color
				if (Input.GetKeyDown (KeyCode.E)) {
					// generate random color
					Color randomColor = Random.ColorHSV (0f, 1f, 1f, 1f, .5f, 1f);

					// change prism color
					ChangePrismColor (element, randomColor);
				}
			}
		} else {
			// reset interaction text
			if (interactionText != null) {
				interactionText.gameObject.SetActive (false);
				interactionText = null;
			}
		}
	}


	public void SetInitialPrismColor (HUDNavigationElement element)
	{
		// get renderer from prism
		Renderer prismRenderer = element.transform.parent.GetComponent<Renderer> ();
		if (prismRenderer != null)
			ChangePrismColor (element, prismRenderer.material.color);
	}


	static void ChangePrismColor (HUDNavigationElement element, Color elementColor)
	{
		// change radar color
		if (element.Radar != null)
			element.Radar.ChangeIconColor (elementColor);

		// change compass bar color
		if (element.CompassBar != null)
			element.CompassBar.ChangeIconColor (elementColor);

		// change indicator colors
		if (element.Indicator != null) {
			element.Indicator.ChangeIconColor (elementColor);
			element.Indicator.ChangeOffscreenIconColor (elementColor);
		}

		// change minimap color
		if (element.Minimap != null)
			element.Minimap.ChangeIconColor (elementColor);

		// change prism material color
		Renderer prismRenderer = element.transform.parent.GetComponent<Renderer> ();
		if (prismRenderer != null)
			prismRenderer.material.color = new Color (elementColor.r, elementColor.g, elementColor.b, prismRenderer.material.color.a);

		// change prism light (Night Scene)
		Light prismLight = element.transform.parent.gameObject.GetComponentInChildren<Light> ();
		if (prismLight != null)
			prismLight.color = new Color (elementColor.r, elementColor.g, elementColor.b);
	}
	#endregion
}

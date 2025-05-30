using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SickscoreGames.ExampleScene
{
	public class ExampleMouseLook : MonoBehaviour
	{
		#region Variables
		public float sensitivityX = 3f;
		public float sensitivityY = 3f;

		public Vector2 rotationLimitsX = new Vector2 (-360f, 360f);
		public Vector2 rotationLimitsY = new Vector2 (-60f, 60f);
		public float rotationSmooth = 8f;

		private Quaternion rotationOrigin;
		private float currentRotationX, currentRotationY = 0f;
		#endregion


		#region Main Methods
		void Awake ()
		{
			rotationOrigin = transform.localRotation;
		}


		void Update ()
		{
			// get input
			float mouseX = Input.GetAxis ("Mouse X");
			float mouseY = Input.GetAxis ("Mouse Y");

			// calculate and apply rotations
			if (axes == RotationAxes.MouseX) {
				currentRotationX += mouseX * sensitivityX;
				currentRotationX = this.ClampAngle (currentRotationX, rotationLimitsX.x, rotationLimitsX.y);
				Quaternion rotationX = Quaternion.AngleAxis (currentRotationX, Vector3.up);
				transform.localRotation = Quaternion.Lerp (transform.localRotation, rotationOrigin * rotationX, rotationSmooth * Time.deltaTime);
			} else {
				currentRotationY += mouseY * sensitivityY;
				currentRotationY = this.ClampAngle (currentRotationY, rotationLimitsY.x, rotationLimitsY.y);
				Quaternion rotationY = Quaternion.AngleAxis (-currentRotationY, Vector3.right);
				transform.localRotation = Quaternion.Lerp (transform.localRotation, rotationOrigin * rotationY, rotationSmooth * Time.deltaTime);
			}
		}
		#endregion


		#region Utility Methods
		float ClampAngle (float angle, float min, float max)
		{
			angle %= 360f;
			if (angle < -360f)
				angle += 360f;
			if (angle > 360f)
				angle -= 360f;
			
			return Mathf.Clamp (angle, min, max);
		}
		#endregion


		#region Subclasses
		public enum RotationAxes { MouseX, MouseY }
		public RotationAxes axes = RotationAxes.MouseX;
		#endregion
	}
}

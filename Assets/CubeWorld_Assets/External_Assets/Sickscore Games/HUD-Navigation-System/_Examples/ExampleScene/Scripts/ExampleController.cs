using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SickscoreGames.ExampleScene
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public class ExampleController : MonoBehaviour
	{
		#region Variables
		public float walkSpeed = 8f;
		public float runSpeed = 12f;
		public float jumpHeight = 3.25f;
		public float gravity = 28f;

		private Transform _transform;
		private Rigidbody _rigidbody;
		private bool isGrounded;
		#endregion


		#region Main Methods
		void Awake ()
		{
			// assign components
			_transform = this.transform;
			_rigidbody = GetComponent<Rigidbody> ();

			// setup rigidbody
			_rigidbody.freezeRotation = true;
			_rigidbody.useGravity = false;
		}


		void FixedUpdate ()
		{
			// check if grounded
			if (isGrounded) {
				// directional input
				Vector3 targetVelocity = new Vector3 (Input.GetAxis ("Horizontal"), 0f, Input.GetAxis ("Vertical"));
				float moveSpeed = (Input.GetKey (KeyCode.LeftShift)) ? runSpeed : walkSpeed;
				targetVelocity = _transform.TransformDirection (targetVelocity) * moveSpeed;

				// calculate velocity and max velocity change
				Vector3 velocity = _rigidbody.linearVelocity;
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.x = Mathf.Clamp (velocityChange.x, -8f, 8f);
				velocityChange.z = Mathf.Clamp (velocityChange.z, -8f, 8f);
				velocityChange.y = 0f;
				_rigidbody.AddForce (velocityChange, ForceMode.VelocityChange);

				// jump input
				if (Input.GetKeyDown (KeyCode.Space))
					_rigidbody.linearVelocity = new Vector3 (velocity.x, CalculateJumpVerticalSpeed (), velocity.z);
			}

			// apply force to rigidbody
			_rigidbody.AddForce (new Vector3 (0f, -gravity * _rigidbody.mass, 0f));

			isGrounded = false;
		}
		#endregion


		#region Utility Methods
		void OnCollisionStay ()
		{
			isGrounded = true;    
		}


		float CalculateJumpVerticalSpeed ()
		{
			return Mathf.Sqrt (2f * jumpHeight * gravity);
		}
		#endregion
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SickscoreGames.ExampleScene
{
	public class ExampleResetPlayer : MonoBehaviour
	{
		#region Variables
		public Transform spawnPoint;
		#endregion


		#region Main Methods
		void OnTriggerEnter (Collider other)
		{
			if (other.gameObject.tag == "Player") {
				// reset position
				other.gameObject.transform.position = spawnPoint.position;

				// reset velocity
				Rigidbody rBody = other.gameObject.GetComponent<Rigidbody> ();
				if (rBody != null)
					rBody.linearVelocity = other.transform.forward * 5f;
			}
		}
		#endregion
	}
}

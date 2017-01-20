using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Interactable : MonoBehaviour {
		public float playerLookTimer { get; set; }

		//

		public virtual void Interact() {
			Debug.Log("Interact with " + name);
		}
	}

}
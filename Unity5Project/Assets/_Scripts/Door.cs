using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Door : Interactable {
		[Header("Door")]
		public float openAngle = 100f;
		public float openTime = 1f;
		public GameObject[] doors;
		public GameObject[] removeAfterwards;

		//

		public override void Interact(Player player) {
			base.Interact(player);
			if (doors != null && doors.Length > 0) {
				LeanTween.rotateAround(doors[0], Vector3.up, openAngle, openTime);
				LeanTween.rotateAround(doors[1], Vector3.up, -openAngle, openTime * 1.1f);
			}
			else {
				LeanTween.rotateAround(gameObject, Vector3.up, openAngle, openTime);
			}

			if (removeAfterwards != null) { for (int i = 0; i < removeAfterwards.Length; ++i) { Destroy(removeAfterwards[i]); } }
		}
	}

}
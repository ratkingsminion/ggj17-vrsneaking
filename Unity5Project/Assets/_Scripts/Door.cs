using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Door : Interactable {
		[Header("Door")]
		public float openAngle = 100f;
		public float openTime = 1f;
		public GameObject message;
		public GameObject[] doors;
		public GameObject[] removeAfterwards;
		public bool needsKey;
		public Base.Sound snd;
		//
		bool closed;

		//

		void Awake() {
			if (message != null) { message.gameObject.SetActive(false); }
			if (needsKey) { closed = true; }
		}

		public override void Interact(Player player) {
			base.Interact(player);
			if (needsKey && closed) {
				if (player.keys <= 0) {
					message.gameObject.SetActive(true);
					MovementEffects.Timing.CallDelayed(openTime, () => {
						if (message != null && message.gameObject != null) { message.gameObject.SetActive(false); }
					});
					return;
				}
				player.keys--; Score.Change();
				closed = false;
			}
			if (snd != null) { snd.Play(transform); }
			if (doors != null && doors.Length > 0) {
				LeanTween.rotateAround(doors[0], Vector3.up, openAngle, openTime);
				LeanTween.rotateAround(doors[1], Vector3.up, -openAngle, openTime * 1.1f);
			}
			else {
				LeanTween.rotateAround(gameObject, Vector3.up, openAngle, openTime);
			}

			if (GetComponent<Collider>() != null) { GetComponent<Collider>().enabled = false; }
			if (removeAfterwards != null) { for (int i = 0; i < removeAfterwards.Length; ++i) { Destroy(removeAfterwards[i]); } }
		}
	}

}
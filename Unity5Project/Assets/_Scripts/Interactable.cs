using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Interactable : MonoBehaviour {
		[Header("Interactable")]
		public float lookTime = 1f;
		//
		protected float lookTimer;
		protected bool curLookingAt;
		protected bool runningLookCoroutine;
		protected bool mayLookAt = true;

		//

		public void LookingAt() {
			if (!mayLookAt) { return; }
			if (!runningLookCoroutine) { MovementEffects.Timing.RunCoroutine(LookingAtCR()); }
			curLookingAt = true;
		}

		IEnumerator<float> LookingAtCR() {
			runningLookCoroutine = true;
			//lookTimer = lookTime;
			while (true) {
				//print(lookTimer + " " + curLookingAt + " " + runningLookCoroutine + " " + mayLookAt);
				if (curLookingAt) {
					lookTimer += Time.deltaTime;
					if (lookTimer >= lookTime) {
						Interact(Main.Inst.player);
						mayLookAt = false;
						MovementEffects.Timing.CallDelayed(1f, () => { mayLookAt = true; });
						break;
					}
				}
				else {
					lookTimer -= Time.deltaTime * 2.5f;
					if (lookTimer <= 0f) { break; }
				}
				curLookingAt = false;
				yield return 0f;
			}
			lookTimer = 0f;
			runningLookCoroutine = false;
		}

		public virtual void Interact(Player player) {
			Debug.Log("Interact with " + name);
		}
	}

}
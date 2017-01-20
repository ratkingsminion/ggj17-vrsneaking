using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Player : MonoBehaviour {
		public float lookAtSeconds = 1f;
		public Transform pointer; // for testing mostly
		public GvrHead head;
		public Transform camTrans;
		//
		List<Interactable> interactables = new List<Interactable>();

		//

		void Start() {
		}

		void Update() {
			RaycastHit hitInfo;
			bool hit = Physics.Raycast(new Ray(camTrans.position, camTrans.forward), out hitInfo, Main.Inst.roomSize * 0.75f);
			if (hit) {
				var interactable = hitInfo.transform.GetComponent<Interactable>();
				if (interactable != null) { LookingAt(interactable); }
			}
			ShowPointer(hit, hitInfo);
			UpdateLookingAts();
		}

		//

		void LookingAt(Interactable inter) {
			if (inter.playerLookTimer <= 0f) {
				interactables.Add(inter);
			}
			inter.playerLookTimer += Time.deltaTime;
			print("yay " + inter.playerLookTimer);
			if (inter.playerLookTimer > lookAtSeconds) {
				inter.Interact();
				inter.playerLookTimer = -1f;
				interactables.Remove(inter);
			}
		}

		void ShowPointer(bool show, RaycastHit hit) {
			if (show != pointer.gameObject.activeSelf) {
				pointer.gameObject.SetActive(show);
			}
			if (show) {
				pointer.position = hit.point;
			}
		}

		void UpdateLookingAts() {
			for (int i = interactables.Count - 1; i >= 0; --i) {
				var inter = interactables[i];
				inter.playerLookTimer -= Time.deltaTime * 0.5f;
				if (inter.playerLookTimer <= 0f) {
					interactables.RemoveAt(i);
				}
			}
		}
	}

}
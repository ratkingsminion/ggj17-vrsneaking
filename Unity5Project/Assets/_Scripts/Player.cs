using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Player : MonoBehaviour {
		public float lookAtSeconds = 1f;
		public Transform pointer; // for testing mostly
		public GvrHead head;
		public Transform camTrans;
		public float moveSpeed = 5f;
		public Transform walkArrow;
		//
		List<Interactable> interactables = new List<Interactable>();
		bool moving;

		//

		void Start() {
		}

		void Update() {
			// looking at something.
			RaycastHit hitInfo;
			Ray ray = new Ray(camTrans.position, camTrans.forward);
			bool hit = Physics.Raycast(ray, out hitInfo, Main.Inst.roomSize * 0.65f);
			if (hit) {
				var interactable = hitInfo.transform.GetComponent<Interactable>();
				if (interactable != null) { interactable.LookingAt(); }
			}
			ShowPointer(hit, hitInfo);
			// walking.
			ray = new Ray(camTrans.position, Quaternion.Euler(0f, camTrans.eulerAngles.y, 0f) * Vector3.forward);
			if (!Physics.Raycast(ray, Main.Inst.roomSize)) {
				var quat = Quaternion.FromToRotation(Vector3.forward, ray.direction);
				var yaw = Mathf.Round(quat.eulerAngles.y / 90f) * 90f;
				quat.eulerAngles = new Vector3(0f, yaw, 0f);
				walkArrow.gameObject.SetActive(true);
				walkArrow.rotation = quat;
				if (GvrViewer.Instance.Triggered) {
					// move now
					Move(walkArrow.forward * Main.Inst.roomSize);
				}
			}
			else {
				walkArrow.gameObject.SetActive(false);
			}
		}

		//

		public void Move(Vector3 dir) {
			if (moving) { return; }
			moving = true;
			LeanTween.move(gameObject, transform.position + dir, dir.magnitude / moveSpeed)
				.setEase(LeanTweenType.easeInOutSine)
				.setOnComplete(() => moving = false);
		}

		public void Collect(GameObject go, float time) {
			Vector3 startPos = go.transform.position;
			Quaternion startRot = go.transform.localRotation;
			Vector3 startScale = go.transform.localScale;
			Quaternion randomRot = Random.rotationUniform; // TODO?
			LeanTween.value(0f, 1f, time)
				.setOnUpdate(f => {
					go.transform.position = Vector3.Lerp(startPos, head.transform.position + new Vector3(0f, -0.35f, 0f), f);
					go.transform.localRotation = Quaternion.Slerp(startRot, randomRot, f);
					go.transform.localScale = Vector3.Lerp(startScale, new Vector3(0.05f, 0.05f, 0.05f), f);
				})
				.setEase(LeanTweenType.easeInSine)
				.setOnComplete(() => go.SetActive(false));
		}

		//

		void ShowPointer(bool show, RaycastHit hit) {
			if (show != pointer.gameObject.activeSelf) {
				pointer.gameObject.SetActive(show);
			}
			if (show) {
				pointer.position = hit.point;
			}
		}
	}

}
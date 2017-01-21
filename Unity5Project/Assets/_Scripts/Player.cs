using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Player : MonoBehaviour {
		public float lookAtSeconds = 1f;
		public float mapMinAngle = 20f;
		public Transform pointer; // for testing mostly
		public Waitsymbol waitSymbol;
		public GvrHead head;
		public Transform camTrans;
		public float moveSpeed = 5f;
		public Transform walkArrow;
		//
		public bool moving { get; private set; }
		public Base.Position2 curPos { get; private set; }

		//

		void Start() {
			curPos = Base.Position2.RoundedVector(new Vector2(transform.position.x, transform.position.z) / Main.Inst.tileSize);
		}

		void Update() {
			// looking at something.
			RaycastHit hitInfo;
			Ray ray = new Ray(camTrans.position, camTrans.forward);
			bool hit = Physics.Raycast(ray, out hitInfo, Main.Inst.tileSize * 0.65f);
			var lookTimeFactor = 0f;
			if (hit) {
				var interactable = hitInfo.transform.GetComponent<Interactable>();
				if (interactable != null) {
					interactable.LookingAt();
					lookTimeFactor = interactable.GetFactor();
				}
			}
			ShowPointer(hit, hitInfo);
			waitSymbol.SetFactor(lookTimeFactor);
			// walking.
			ray = new Ray(camTrans.position, Quaternion.Euler(0f, camTrans.eulerAngles.y, 0f) * Vector3.forward);
			if (!Physics.Raycast(ray, Main.Inst.tileSize)) {
				var quat = Quaternion.FromToRotation(Vector3.forward, ray.direction);
				var yaw = Mathf.Round(quat.eulerAngles.y / 90f) * 90f;
				quat.eulerAngles = new Vector3(0f, yaw, 0f);
				walkArrow.gameObject.SetActive(!moving);
				walkArrow.rotation = quat;
				if (GvrViewer.Instance.Triggered) {
					// move now
					Move(walkArrow.forward * Main.Inst.tileSize);
				}
			}
			else {
				walkArrow.gameObject.SetActive(false);
			}
			// looking down
			Map.Seen(Vector3.Angle(camTrans.forward, Vector3.down) < mapMinAngle);
		}

		//

		public void Move(Vector3 dir) {
			if (moving) { return; }
			Map.HideCross();
			moving = true;
			var targetPos = transform.position + dir;
			var nextPos = Base.Position2.RoundedVector(new Vector2(targetPos.x, targetPos.z) / Main.Inst.tileSize);
			Main.Inst.UpdateVisibility(nextPos, false);
			LeanTween.move(gameObject, targetPos, dir.magnitude / moveSpeed)
				.setEase(LeanTweenType.easeInOutSine)
				.setOnComplete(() => {
					moving = false;
					curPos = nextPos;
					Map.SetCross(curPos);
					Main.Inst.UpdateVisibility(curPos, true);
					Main.Inst.VisitRoom(curPos);
				});
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
			pointer.gameObject.SetActive(show);
			if (show) {
				pointer.position = hit.point;
			}
			// move and show the waitsymbol
			var showWait = waitSymbol.GetFactor() > 0f;
			if (showWait) {
				waitSymbol.transform.position = camTrans.position + (pointer.position - camTrans.position).normalized * 3f;
				waitSymbol.transform.LookAt(camTrans);
			}
			waitSymbol.gameObject.SetActive(showWait);
		}
	}

}
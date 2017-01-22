using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Player : MonoBehaviour {
		public float lookAtSeconds = 1f;
		public float mapMinAngle = 20f;
		public float scoreMinAngle = 30f;
		public Transform pointer; // for testing mostly
		public Waitsymbol waitSymbol;
		public GvrHead head;
		public Transform camTrans;
		public float moveSpeed = 5f;
		public Transform walkArrow;
		public Transform scoreBoard;
		//
		public bool moving { get; private set; }
		public bool allowInput { get; set; }
		public Base.Position2 curPos { get; private set; }
		public Base.Position2 lastPos { get; private set; }

		//

		void Awake() {
			allowInput = true;
		}

		void Start() {
			lastPos = curPos = Base.Position2.RoundedVector(new Vector2(transform.position.x, transform.position.z) / Main.Inst.tileSize);
			var curRoom = Main.Inst.GetTile(curPos).rooms[0]; // TODO BUG?
			curRoom.FadeRevealersOut(0f, 1f);
		}

		void Update() {
			if (!allowInput) {
				HidePointers();
				return;
			}

			// looking at something.
			RaycastHit hitInfo;
			Ray ray = new Ray(camTrans.position, camTrans.forward);
			bool hit = Physics.Raycast(ray, out hitInfo, Main.Inst.tileSize * 0.75f);
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
			//
			var angleDown = Vector3.Angle(camTrans.forward, Vector3.down);
			var angleUp = Vector3.Angle(camTrans.forward, Vector3.up);
			// looking down
			Map.Seen(angleDown < mapMinAngle);
			// score board
			Score.Seen(head.transform.localEulerAngles.y, angleUp < scoreMinAngle);
		}

		//

		public void Move(Vector3 dir) {
			if (moving) { return; }
			Map.HideCross();
			moving = true;
			var targetPos = transform.position + dir;
			var nextPos = Base.Position2.RoundedVector(new Vector2(targetPos.x, targetPos.z) / Main.Inst.tileSize);

			Main.Inst.UpdateVisibility(nextPos, false);
			if (curPos == Main.Inst.winFromPos && nextPos == Main.Inst.winToPos) {
				// way of the winner
				LeanTween.move(gameObject, transform.position + dir * 0.5f, dir.magnitude / moveSpeed)
					.setEase(LeanTweenType.easeInSine)
					.setOnComplete(() => {
						Main.Inst.Win();
					});
			}
			else {
				// normal moving
				Main.Inst.Step(dir, targetPos);
				float time = dir.magnitude / moveSpeed;
				LeanTween.move(gameObject, targetPos, time)
					.setEase(LeanTweenType.easeInOutSine)
					.setOnComplete(() => {
						moving = false;
						lastPos = curPos;
						curPos = nextPos;
						Map.SetCross(curPos);
						Main.Inst.UpdateVisibility(curPos, true);
						Main.Inst.VisitRoom(curPos);
					});

				var curRoom = Main.Inst.GetTile(curPos).rooms[0]; // TODO BUG?
				var nextRoom = Main.Inst.GetTile(nextPos).rooms[0]; // TODO BUG?
				if (curRoom != nextRoom) {
					curRoom.FadeRevealersIn(time * 0.5f, time * 0.5f);
					nextRoom.FadeRevealersOut(0f, time * 0.5f);
				}
			}
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

		void HidePointers() {
			pointer.gameObject.SetActive(false);
			waitSymbol.gameObject.SetActive(false);
		}

		void ShowPointer(bool show, RaycastHit hit) {
			pointer.gameObject.SetActive(show);
			if (show) {
				pointer.position = hit.point;
				pointer.transform.LookAt(camTrans);
			}
			// move and show the waitsymbol
			var showWait = waitSymbol.GetFactor() > 0f;
			if (showWait) {
				waitSymbol.transform.position = camTrans.position + (pointer.position - camTrans.position).normalized * 3f;
				waitSymbol.transform.LookAt(camTrans);
			}
			waitSymbol.gameObject.SetActive(showWait);
			pointer.gameObject.SetActive(show && !showWait);
		}
	}

}
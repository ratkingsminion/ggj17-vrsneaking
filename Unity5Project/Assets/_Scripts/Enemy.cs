using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Enemy : MonoBehaviour {
		public float smoothRotTime = 0.5f; // = 90f;
		public float moveTime = 2f; // = 3f;
		public Base.Position2[] moves;
		public Transform upAndDown;
		public float amplitude = 0.1f;
		public float frequency = 10f;
		public Color lightColor = Color.white;
		public float lightRangeMax = 8f;
		public float lightIntensityMax = 5f;
		public Vector3 lightOffset = Vector3.up * 2.5f;
		public Transform[] follows;
		//
		public Base.Position2 curPos { get; private set; }
		public Base.Position2 targetPos { get; private set; }
		public bool moving { get; private set; }
		Vector3 targetWorldPos, startUpAndDownPos;
		int curIndex;
		float curRotVel;
		bool visible = true;
		float upAndDownTime;
		int lightIndex;
		Light[] lights;
		Vector3[] followOffsets;

		//

		void Start() {
			var pos = transform.position;
			curPos = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / Main.Inst.tileSize);
			if (moves.Length > 0) {
				targetPos = curPos + moves[curIndex];
			}
			else {
				targetPos = curPos;
			}
			targetWorldPos = new Vector3(targetPos.x, 0f, targetPos.y) * Main.Inst.tileSize;
			startUpAndDownPos = upAndDown.localPosition;
			upAndDownTime = Random.value;
			lights = new Light[2];
			lights[0] = new GameObject("ENEMY LIGHT 0").AddComponent<Light>();
			lights[1] = new GameObject("ENEMY LIGHT 1").AddComponent<Light>();
			lights[0].color = lights[1].color = lightColor;
			lights[0].range = lights[1].range = 0f;
			lights[0].intensity = lights[1].intensity = 0f;
			lights[0].enabled = lights[1].enabled = false;
			if (moves.Length == 0) {
				lights[0].enabled = true;
				lights[0].transform.position = targetWorldPos + lightOffset;
				lights[0].range = lightRangeMax;
				lights[0].intensity = lightIntensityMax;
			}
			followOffsets = new Vector3[follows.Length];
			for (int i = follows.Length - 1; i >= 0; --i) {
				followOffsets[i] = follows[i].position - transform.position;
				follows[i].SetParent(null);
			}
		}

		void OnDisable() {
			for (int i = follows.Length - 1; i >= 0; --i) {
				if (follows[i] != null) {
					follows[i].gameObject.SetActive(false);
				}
			}
		}

		void Update() {
			var pos = transform.position;
			curPos = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / Main.Inst.tileSize);
			SetVisibility();

			// up and down
			upAndDownTime += Time.deltaTime;
			upAndDown.localPosition = startUpAndDownPos + new Vector3(0f, Mathf.Sin(upAndDownTime * frequency) * amplitude, 0f);

			if (moves.Length > 0) {
				// rotate towards goal
				var dir = new Vector3(moves[curIndex].x, 0f, moves[curIndex].y).normalized;
				var angle = Vector3.Angle(transform.forward, dir);
				transform.eulerAngles = new Vector3(0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y, ref curRotVel, smoothRotTime), 0f);

				// move to next tile
				if (!moving && Vector3.Angle(transform.forward, dir) <= 0.05f) {
					if (!Main.Inst.EnemyWillCollideWith(this)) {
						moving = true;
						lightIndex = 1 - lightIndex;
						lights[lightIndex].enabled = true;
						lights[lightIndex].transform.position = targetWorldPos + lightOffset;
						// light goes on
						LeanTween.value(0f, 1f, moveTime * 0.35f)
							.setEase(LeanTweenType.easeInOutSine)
							.setOnUpdate(f => {
								lights[lightIndex].range = f * lightRangeMax;
								lights[lightIndex].intensity = f * lightIntensityMax;
							});
						// light goes off
						LeanTween.value(0f, 1f, moveTime * 1f)
							.setEase(LeanTweenType.easeInOutSine)
							.setOnUpdate(f => {
								lights[1 - lightIndex].range = (1f - f) * lightRangeMax;
								lights[1 - lightIndex].intensity = (1f - f) * lightIntensityMax;
							})
							.setOnComplete(() => {
								lights[1 - lightIndex].enabled = false;
							});
						LeanTween.move(gameObject, targetWorldPos, moveTime)
							.setEase(LeanTweenType.easeInOutSine)
							.setOnComplete(() => {
								curIndex = (curIndex + 1) % moves.Length;
								targetPos = curPos + moves[curIndex];
								targetWorldPos = new Vector3(targetPos.x, 0f, targetPos.y) * Main.Inst.tileSize;
								moving = false;
							});
					}
				}
			}

			// check player distance
			var playerDir = Main.Inst.player.transform.position - transform.position;
			var maxDist = Main.Inst.tileSize * 0.4f;
			var sqrDist = playerDir.sqrMagnitude;
			if (sqrDist < maxDist * maxDist) {
				var playerAngle = Mathf.Clamp(Vector3.Angle(transform.forward, playerDir.normalized), 33f, 100f);
				var factor = 33f + ((playerAngle - 33f) / (100f - 33f));
				var distance = 1f * (1f - factor) + maxDist * factor;
				if (maxDist < distance * distance) { Main.Inst.Die(); }
			}
			/*
			var playerPos = Main.Inst.player.transform.position;
			var pPos = Base.Position2.RoundedVector(new Vector2(playerPos.x, playerPos.z) / Main.Inst.tileSize);
			if ((!moving && pPos == curPos) || (pPos == curPos && Vector3.Distance(pos, playerPos) < 0.5f) || (moving && pPos == curPos && targetPos == pPos) || (moving && pPos == curPos && (targetPos == Main.Inst.player.curPos || targetPos == Main.Inst.player.lastPos))) {
				Main.Inst.Die();
			}
			*/

				// something follows
				for (int i = follows.Length - 1; i >= 0; --i) {
				follows[i].position = followOffsets[i] + transform.position;
			}
		}

		void SetVisibility() {
			bool newVisible = Main.Inst.TileVisible(curPos);
			if (newVisible != visible) {
				for (var iter = GetComponentsInChildren<Renderer>(true).GetEnumerator(); iter.MoveNext();) {
					((Renderer)(iter.Current)).enabled = newVisible;
				}
				visible = newVisible;
			}
		}

		//

#if UNITY_EDITOR
		void OnDrawGizmos() {
		}
		void OnDrawGizmosSelected() {
			if (!moving && moves != null) {
				var ts = 8f; //  Main.Inst.tileSize;
				Gizmos.color = Color.red;
				var pos = transform.position;
				var cp = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / ts);
				for (int i = 0; i < moves.Length; ++i) {
					var m = moves[(curIndex + i) % moves.Length];
					var cpc = Vector3.up * 0.5f + new Vector3(cp.x, 0f, cp.y) * ts;
					Gizmos.DrawSphere(cpc, 0.3f);
					Gizmos.DrawRay(cpc, new Vector3(m.x, 0f, m.y) * ts * 0.485f);
					cp += m;
				}
			}
		}
#endif
	}

}
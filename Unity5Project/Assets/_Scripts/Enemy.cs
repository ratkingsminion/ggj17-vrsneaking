using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Enemy : MonoBehaviour {
		public float smoothRotTime = 0.5f; // = 90f;
		public float moveTime = 2f; // = 3f;
		public Base.Position2[] moves;
		//
		Base.Position2 curPos;
		Vector3 targetWorldPos;
		int curIndex;
		bool moving;
		float curRotVel;
		bool visible = true;

		//

		void Start() {
			var pos = transform.position;
			curPos = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / Main.Inst.tileSize);
			var targetPos = curPos + moves[curIndex];
			targetWorldPos = new Vector3(targetPos.x, 0f, targetPos.y) * Main.Inst.tileSize;
		}

		void Update() {
			var pos = transform.position;
			curPos = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / Main.Inst.tileSize);
			SetVisibility();

			// rotate towards goal
			var dir = new Vector3(moves[curIndex].x, 0f, moves[curIndex].y).normalized;
			var angle = Vector3.Angle(transform.forward, dir);
			transform.eulerAngles = new Vector3(0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y, ref curRotVel, smoothRotTime), 0f);

			if (!moving && Vector3.Angle(transform.forward, dir) <= 0.05f) {
				moving = true;
				LeanTween.move(gameObject, targetWorldPos, moveTime)
					.setEase(LeanTweenType.easeInOutSine)
					.setOnComplete(() => {
						curIndex = (curIndex + 1) % moves.Length;
						var targetPos = curPos + moves[curIndex];
						targetWorldPos = new Vector3(targetPos.x, 0f, targetPos.y) * Main.Inst.tileSize;
						moving = false;
					});
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
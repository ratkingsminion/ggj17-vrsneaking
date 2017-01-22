using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Score : MonoBehaviour {
		static TMPro.TextMeshPro tmp;
		static Transform trans;
		public static int score { get; private set; }
		static float fadeInSpeed = 2f;
		static float fadeOutSpeed = 2f;

		//

		public static void Deactivate() {
			trans.gameObject.SetActive(false);
		}

		void Awake() {
			trans = transform;
			tmp = GetComponentInChildren<TMPro.TextMeshPro>(true);
			score = 0;
			tmp.alpha = 0f;
		}

		void Start() {
			AddScore(0);
		}

		public static void Seen(float yaw, bool seen) {
			trans.localEulerAngles = new Vector3(0f, yaw, 0f);
			float a = tmp.alpha;
			if (seen) { a += Time.deltaTime * fadeInSpeed; }
			else { a -= Time.deltaTime * fadeOutSpeed; }
			tmp.alpha = Mathf.Clamp01(a);
		}

		public static void AddScore(int value) {
			score += value;
			tmp.text = "$ " + score + "  \nKeys: " + Main.Inst.player.keys;
		}

		public static void Change() {
			tmp.text = "$ " + score + "  \nKeys: " + Main.Inst.player.keys;
		}
	}

}
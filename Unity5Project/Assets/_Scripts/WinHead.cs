using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class WinHead : MonoBehaviour {
		public TMPro.TextMeshPro endMsg;
		public Transform endStuff;
		public float wobbleSpeed = 1f;
		public float wobbleStrength = 5f;
		public float maxTime = 10f;
		//
		float timer;

		//

		void Start() {
			gameObject.SetActive(false);
		}

		void Update() {
			endStuff.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * wobbleSpeed) * wobbleStrength);
			timer += Time.deltaTime;
			if (timer > maxTime || GvrViewer.Instance.Triggered) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
			}
		}

		public void EndMessage(string myLoot, string lootOverall) {
			endMsg.text = endMsg.text.Replace("[myloot]", myLoot).Replace("[all]", lootOverall);
		}
	}

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class EndHead : MonoBehaviour {
		public Transform head;
		public float headWobbleSpeed = 1f;
		public float headWobbleStrength = 0.05f;
		public float headWobbleRotate = 4f;
		public float maxTime = 5f;
		//
		Vector3 headStartPos, headNextPos;
		Quaternion headStartRot;
		bool ending;
		float timer;

		//

		void Start() {
			headNextPos = headStartPos = head.localPosition;
			headStartRot = head.localRotation;
			gameObject.SetActive(false);
		}

		void Update() {
			timer += Time.deltaTime;
			if ((timer > maxTime || GvrViewer.Instance.Triggered) && !ending) {
				ending = true;
				LeanTween.scale(head.gameObject, Vector3.one * 0.001f, 0.75f)
					.setOnComplete(() => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
			}

			head.localPosition = Vector3.MoveTowards(head.localPosition, headNextPos, Time.deltaTime * headWobbleSpeed);
			if ((head.localPosition - headNextPos).sqrMagnitude < 0.00001f) {
				headNextPos = headStartPos + Random.insideUnitSphere * headWobbleStrength;
				head.localRotation = Quaternion.RotateTowards(headStartRot, Random.rotationUniform, headWobbleRotate);
			}
		}
	}

}
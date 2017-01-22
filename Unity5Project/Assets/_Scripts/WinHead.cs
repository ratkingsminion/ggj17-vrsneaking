using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class WinHead : MonoBehaviour {
		public Transform endStuff;
		public float wobbleSpeed = 1f;
		public float wobbleStrength = 5f;

		void Start() {

		}

		void Update() {
			endStuff.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * wobbleSpeed) * wobbleStrength);
		}
	}

}
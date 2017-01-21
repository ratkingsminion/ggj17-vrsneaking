using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Main : MonoBehaviour {
		public static Main Inst { get; private set; }
		//
		public Player player;
		//
		public float roomSize = 8f;

		//

		void Awake() {
			Inst = this;
		}

		void Start() {
			print("vr mode enabled " + GvrViewer.Instance.VRModeEnabled);
#if UNITY_EDITOR
			print("viewer type " + GvrViewer.Instance.ViewerType);
#endif
		}

		void Update() {
			if (GvrViewer.Instance != null) {

				if (GvrViewer.Instance.VRModeEnabled && GvrViewer.Instance.Triggered) {
					// do stuff in vr
					//GameObject vrLauncher = GvrViewer.Instance.GetComponentInChildren<GvrHead>().gameObject;
					//print(vrLauncher.name);
				}
				if (GvrViewer.Instance.Triggered) {
					//print("triggered!");
					//GvrViewer.Instance.VRModeEnabled = !GvrViewer.Instance.VRModeEnabled;
				}
			}
		}
	}

}
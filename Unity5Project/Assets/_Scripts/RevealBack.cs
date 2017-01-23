using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class RevealBack : MonoBehaviour {
		Color32[] colors32;
		Mesh mesh;

		void Start() {
			var mr = GetComponentInChildren<MeshFilter>();
			mesh = mr.mesh;
			colors32 = new Color32[mesh.vertexCount];
			for (int i = 0; i < mesh.vertexCount; ++i) {
				colors32[i] = new Color32(2,2,2, 255);
			}
			mesh.colors32 = colors32;
			mr.mesh = mesh;
		}

		public void SetAlpha(float a) {
			if (mesh == null) { return; }
			byte aa = (byte)Mathf.RoundToInt(a * 255f);
			for (int i = 0; i < mesh.vertexCount; ++i) {
				colors32[i].a = aa;
			}
			mesh.colors32 = colors32;
		}
	}

}
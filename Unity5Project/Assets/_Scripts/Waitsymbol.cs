using UnityEngine;
using System.Collections;

namespace RatKing {
	public class Waitsymbol : MonoBehaviour {
		public int sections = 10;
		public float size = 0.5f;
		public Material material;
		public Color visible = Color.white;
		public Color invisible = Color.black;
		//
		Mesh mesh;
		Color[] colors;
		float curFactor;

		//

		void CreateMesh() {
			if (mesh != null)
				return;

			// create mesh
			mesh = new Mesh();
			MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
			MeshFilter mf = gameObject.AddComponent<MeshFilter>();

			Vector3[] vertices = new Vector3[3 * sections];
			Vector3[] normals = new Vector3[3 * sections];
			Vector2[] uv = new Vector2[3 * sections];
			colors = new Color[3 * sections];
			int[] triangles = new int[3 * sections];

			float pi2 = Mathf.PI * 2.0f;
			for (int i = 0; i < sections; ++i) {
				int i3 = i * 3;
				float fa = (float)i / (float)sections;
				float fb = (float)(i + 1) / (float)sections;
				vertices[i3 + 0] = Vector3.zero;
				vertices[i3 + 1] = new Vector3(Mathf.Sin(fb * pi2) * 0.5f, Mathf.Cos(fb * pi2) * 0.5f, 0f) * size;
				vertices[i3 + 2] = new Vector3(Mathf.Sin(fa * pi2) * 0.5f, Mathf.Cos(fa * pi2) * 0.5f, 0f) * size;
				normals[i3 + 0] = normals[i3 + 1] = normals[i3 + 2] = Vector3.forward;
				uv[i3 + 0] = Vector2.zero;
				uv[i3 + 1] = Vector2.up;
				uv[i3 + 1] = Vector2.one;
				colors[i3 + 0] = colors[i3 + 1] = colors[i3 + 2] = invisible;
				triangles[i3 + 0] = i3 + 0;
				triangles[i3 + 1] = i3 + 1;
				triangles[i3 + 2] = i3 + 2;
			}

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.colors = colors;
			mesh.triangles = triangles;

			mr.material = material;
			mf.sharedMesh = mesh;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = false;
		}

		public void SetFactor(float f) {
			CreateMesh();

			int max = Mathf.RoundToInt(sections * f);
			if (Mathf.RoundToInt(sections * curFactor) == max)
				return;

			curFactor = f;
			for (int i = 0; i < sections; ++i) {
				Color c = (i < max) ? visible : invisible;
				colors[i * 3 + 0] = colors[i * 3 + 1] = colors[i * 3 + 2] = c;
			}
			mesh.colors = colors;
		}

		public float GetFactor() {
			return curFactor;
		}
	}
}
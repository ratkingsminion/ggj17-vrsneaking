using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Map : MonoBehaviour {
		public Material material;
		//
		public static Base.Position2[] checkTileDirs = new[] { new Base.Position2(0, 1), new Base.Position2(1, 0), new Base.Position2(0, -1), new Base.Position2(-1, 0) };
		public static Vector3[] checkDirs = new[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
		static List<Base.Position2> tiles;
		static Base.Position2 min, max;
		enum ConnectionType { Wall, Door, None }
		static List<ConnectionType[]> tileConns;
		//
		static float fadeInSpeed = 2f;
		static float fadeOutSpeed = 1f;
		static Material mat;
		static float tileWidth = 0.3f;
		static Mesh mesh;
		static List<Vector3> vertices;
		static List<Vector3> normals;
		static List<Vector2> uv;
		static List<int> triangles;
		static Transform trans;
		static Vector3 transStartPos;
		static Vector3 crossPos;
		static Vector3 moveCrossPosDamp;

		//

		public static void Deactivate() {
			trans.gameObject.SetActive(false);
		}

		void Awake() {
			mesh = null;
			mat = null;
			trans = transform;
			transStartPos = trans.localPosition;
			CreateMesh();
			// transform.position += new Vector3(max.x - min.x, 0f, max.y - min.y) * tileWidth * 0.5f;
			SetCross(Base.Position2.zero);
		}

		public static void HideCross() {
			vertices[0] = new Vector3(0f, 0.05f, 0f);
			vertices[1] = new Vector3(0f, 0.05f, 0f);
			vertices[2] = new Vector3(0f, 0.05f, 0f);
			vertices[3] = new Vector3(0f, 0.05f, 0f);
			mesh.SetVertices(vertices);
		}

		public static void SetCross(Base.Position2 pos) {
			crossPos = new Vector3(pos.x, 0f, pos.y);
			float tw2 = tileWidth * 0.5f;
			vertices[0] = new Vector3(-tw2 + tileWidth * pos.x, 0.05f, +tw2 + tileWidth * pos.y);
			vertices[1] = new Vector3(+tw2 + tileWidth * pos.x, 0.05f, +tw2 + tileWidth * pos.y);
			vertices[2] = new Vector3(+tw2 + tileWidth * pos.x, 0.05f, -tw2 + tileWidth * pos.y);
			vertices[3] = new Vector3(-tw2 + tileWidth * pos.x, 0.05f, -tw2 + tileWidth * pos.y);
			mesh.SetVertices(vertices);
		}

		public static void Seen(bool seen) {
			//transform.localPosition = new Vector3(max.x - min.x, 0.1f, max.y - min.y) * tileWidth * 0.5f;
			trans.localPosition = Vector3.SmoothDamp(trans.localPosition, transStartPos - crossPos * tileWidth, ref moveCrossPosDamp, 0.2f);
			float a = mat.color.a;
			if (seen) { a += Time.deltaTime * fadeInSpeed; }
			else { a -= Time.deltaTime * fadeOutSpeed; }
			mat.color = Base.Helpers.Colors.WithAlpha(mat.color, Mathf.Clamp01(a));
		}

		public static void AddTile(Base.Position2 pos) {
			AddMeshFace(tiles.IndexOf(pos));
		}

		static void AddMeshFace(int i) {
			float tw2 = tileWidth * 0.5f;
			if (i < 0 || i >= tiles.Count) { return; }
			float x = tiles[i].x * tileWidth, y = tiles[i].y * tileWidth;
			var v = new Vector3(x, 0f, y);
			var vc = vertices.Count;
			vertices.Add(v); vertices.Add(new Vector3(x - tw2, 0f, y + tw2)); vertices.Add(new Vector3(x + tw2, 0f, y + tw2));
			vertices.Add(v); vertices.Add(new Vector3(x + tw2, 0f, y + tw2)); vertices.Add(new Vector3(x + tw2, 0f, y - tw2));
			vertices.Add(v); vertices.Add(new Vector3(x + tw2, 0f, y - tw2)); vertices.Add(new Vector3(x - tw2, 0f, y - tw2));
			vertices.Add(v); vertices.Add(new Vector3(x - tw2, 0f, y - tw2)); vertices.Add(new Vector3(x - tw2, 0f, y + tw2));
			for (int n = 0; n < 12; ++n) { normals.Add(Vector3.up); triangles.Add(vc + n); }
			float d, it = 1f/4f, t = 0.01f;
			d = (float)tileConns[i][0] * it; uv.Add(new Vector2(d + 0.5f * it, 0.5f)); uv.Add(new Vector2(d + t, t)); uv.Add(new Vector2(d - t + it, t));
			d = (float)tileConns[i][1] * it; uv.Add(new Vector2(d + 0.5f * it, 0.5f)); uv.Add(new Vector2(d - t + it, t)); uv.Add(new Vector2(d - t + it, 1f - t));
			d = (float)tileConns[i][2] * it; uv.Add(new Vector2(d + 0.5f * it, 0.5f)); uv.Add(new Vector2(d - t + it, 1f - t)); uv.Add(new Vector2(d + t, 1f - t));
			d = (float)tileConns[i][3] * it; uv.Add(new Vector2(d + 0.5f * it, 0.5f)); uv.Add(new Vector2(d + t, 1f - t)); uv.Add(new Vector2(d + t, t));

			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetUVs(0, uv);
			mesh.SetTriangles(triangles, 0);
		}

		void CreateMesh() {
			if (mesh != null)
				return;

			// create mesh
			mesh = new Mesh();
			MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
			MeshFilter mf = gameObject.AddComponent<MeshFilter>();
			vertices = new List<Vector3>();
			normals = new List<Vector3>();
			uv = new List<Vector2>();
			triangles = new List<int>();

			float tw2 = tileWidth * 0.5f;
			vertices.Add(new Vector3(-tw2, 0.05f, +tw2)); vertices.Add(new Vector3(+tw2, 0.05f, +tw2));
			vertices.Add(new Vector3(+tw2, 0.05f, -tw2)); vertices.Add(new Vector3(-tw2, 0.05f, -tw2));
			for (int n = 0; n < 4; ++n) { normals.Add(Vector3.up); }
			triangles.Add(0); triangles.Add(1); triangles.Add(2);
			triangles.Add(2); triangles.Add(3); triangles.Add(0);
			float d = 3f/4f, it = 1f/4f, t = 0.01f;
			uv.Add(new Vector2(d + t, t)); uv.Add(new Vector2(d - t + it, t));
			uv.Add(new Vector2(d - t + it, 1f - t)); uv.Add(new Vector2(d + t, 1f - t));

			mr.material = mat = new Material(material);
			mat.color = Base.Helpers.Colors.WithAlpha(mat.color, 0f);
			mf.sharedMesh = mesh;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = false;
		}

		public static void MapTheWorld(float tileSize) {
			var mask = LayerMask.GetMask("Default");
			tiles = new List<Base.Position2>();
			tileConns = new List<ConnectionType[]>();
			var checkPositions = new List<Vector3>();
			var checkedTiles = new List<Base.Position2>();
			checkPositions.Add(Vector3.up);
			while (checkPositions.Count > 0) {
				var curIndex = checkPositions.Count - 1;
				var cur = checkPositions[curIndex];
				checkPositions.RemoveAt(curIndex);
				var tilePos = Base.Position2.RoundedVector(new Vector2(cur.x, cur.z) / tileSize);
				checkedTiles.Add(tilePos);
				if (Physics.Raycast(new Ray(cur, Vector3.down), 2f, mask)) {
					// has floor
					tiles.Add(tilePos);
					if (tilePos.x < min.x) { min.x = tilePos.x; } if (tilePos.y < min.y) { min.y = tilePos.y; }
					if (tilePos.x > max.x) { max.x = tilePos.x; } if (tilePos.y > max.y) { max.y = tilePos.y; }
					var conns = new ConnectionType[4];
					for (int i = 0; i < 4; ++i) {
						if (!Physics.Raycast(new Ray(cur, checkDirs[i]), tileSize, mask)) {
							if (!checkedTiles.Contains(tilePos + checkTileDirs[i])) {
								checkPositions.Add(cur + checkDirs[i] * tileSize);
							}
							var delta = checkDirs[(i + 1) % 4] * tileSize * 0.2f;
							if (!Physics.Raycast(new Ray(cur + delta, checkDirs[i]), tileSize * 0.8f, mask) && !Physics.Raycast(new Ray(cur + delta, checkDirs[i]), tileSize * 0.8f, mask)) {
								conns[i] = ConnectionType.None;
							}
							else {
								conns[i] = ConnectionType.Door;
							}
						}
						else {
							conns[i] = ConnectionType.Wall;
						}
					}
					tileConns.Add(conns);
				}
			}
		}

#if UNITY_EDITOR
		/*
		void OnDrawGizmos() {
			if (tiles != null) {
				for (int i = 0; i < tiles.Count; ++i) {
					Gizmos.DrawCube(new Vector3(tiles[i].x, 0f, tiles[i].y) * 0.5f, Vector3.one * 0.4f);
				}
			}
		}
		*/
#endif
	}

}
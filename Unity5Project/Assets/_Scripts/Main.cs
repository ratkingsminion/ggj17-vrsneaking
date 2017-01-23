using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Main : MonoBehaviour {
		public static Main Inst { get; private set; }
		//
		public AudioClip music;
		public GameObject[] hideOnStart;
		public Player player;
		public MeshFilter blacknessFilter;
		public float tileSize = 8f;
		public Transform levelParent;
		public Transform pillarParent;
		public EndHead deadHead;
		public WinHead winHead;
		public Transform endContent;
		public Material rainMaterial;
		public float rainSpeed = 1f;
		public Base.Sound soundDeath;
		[Header("Lightning")]
		public float lgtnMinTime = 10f;
		public float lgtnMaxTime = 30f;
		public float lgtnMinDuration = 0.5f, lgtnMaxDuration = 3f;
		public Color lgtnAmbient = Color.white;
		public Light lgtnLightningLight;
		public Base.Sound soundThunder;
		//
		Dictionary<Base.Position2, Tile> tilesByPos = new Dictionary<Base.Position2, Tile>();
		public List<Enemy> enemies { get; private set; }
		public List<Room> rooms { get; private set; }
		//
		HashSet<Room> curVisibleRooms = new HashSet<Room>();
		HashSet<Room> newVisibleRooms = new HashSet<Room>();
		bool dead, won;
		List<Radio.Direction> endDirections;
		int endDirectionsIdx = -1;
		bool endDoorShown;
		Vector2 rainStartUV, rainCurUV;
		int lootCount;
		Color nrmlAmbient;
		float lgtnTimer;
		//
		public Base.Position2 winFromPos { get; private set; }
		public Base.Position2 winToPos { get; private set; }

		//

		void Awake() {
			Inst = this;
			//
			enemies = new List<Enemy>();
			rooms = new List<Room>();
			Map.MapTheWorld(tileSize);
			endContent.gameObject.SetActive(false);
			rainCurUV = rainStartUV = rainMaterial.GetTextureOffset("_MainTex");
			//
			// enemies
			for (var iter = levelParent.GetComponentsInChildren<Enemy>(true).GetEnumerator(); iter.MoveNext();) {
				var enemy = (Enemy)(iter.Current);
				enemies.Add(enemy);
			}
			// loot
			lootCount = 0;
			for (var iter = levelParent.GetComponentsInChildren<Loot>(true).GetEnumerator(); iter.MoveNext();) {
				var loot = (Loot)(iter.Current);
				lootCount += loot.value;
			}
			// tiles
			for (var iter = levelParent.GetComponentsInChildren<Tile>().GetEnumerator(); iter.MoveNext();) {
				var tile = (Tile)(iter.Current);
				var pos = tile.transform.position;
				tile.pos = Base.Position2.RoundedVector(new Vector2(pos.x, pos.z) / tileSize);
				tilesByPos[tile.pos] = tile;

			}
			// rooms
			for (var iter = levelParent.GetComponentsInChildren<Room>().GetEnumerator(); iter.MoveNext();) {
				var room = (Room)(iter.Current);
				rooms.Add(room);
				for (var iter_tiles = room.tiles.GetEnumerator(); iter_tiles.MoveNext();) {
					((Tile)(iter_tiles.Current)).AddRoom(room);
				}
				room.Activate(false);
			}
			// pillars
			for (var iter = pillarParent.GetEnumerator(); iter.MoveNext();) {
				var pillar = (Transform)(iter.Current);
				if (pillar == pillarParent) { continue; }
				var pos = pillar.position;
				var p2 = Base.Position2.RoundedVector(new Vector2(pos.x + tileSize * 0.5f, pos.z + tileSize * 0.5f) / tileSize);
				var tiles = new[] { p2, p2 - new Base.Position2(1, 0), p2 - new Base.Position2(0, 1), p2 - new Base.Position2(1, 1) };
				for (int i = 0; i < 4; ++i) {
					if (tilesByPos.ContainsKey(tiles[i])) { tilesByPos[tiles[i]].AddPillar(pillar.gameObject); }
				}
				pillar.gameObject.SetActive(false);
			}
			//
			var m = blacknessFilter.mesh;
			var t = m.triangles;
			for (int i = t.Length - 1; i >= 0; i -= 3) {
				var s = t[i]; t[i] = t[i - 1]; t[i - 1] = s;
			}
			m.triangles = t;
			blacknessFilter.mesh = m;
			blacknessFilter.gameObject.SetActive(false);
			//
			deadHead.gameObject.SetActive(true);
			winHead.gameObject.SetActive(true);
		}

		void OnDestroy() {
			LeanTween.cancelAll();
			rainMaterial.SetTextureOffset("_MainTex", rainStartUV);
		}

		void Start() {
#if UNITY_EDITOR
			lgtnTimer = 1f;
#else
			lgtnTimer = Random.Range(lgtnMinTime, lgtnMaxTime);
#endif
			lgtnLightningLight.enabled = false;
			nrmlAmbient = RenderSettings.ambientLight;
			for (int i = 0; i < hideOnStart.Length; ++i) {
				hideOnStart[i].SetActive(false);
			}
			//print("vr mode enabled " + GvrViewer.Instance.VRModeEnabled);
#if UNITY_EDITOR
			//print("viewer type " + GvrViewer.Instance.ViewerType);
#endif
			var pos0 = new Base.Position2(0, 0);
			UpdateVisibility(pos0, false);
			Main.Inst.VisitRoom(pos0);
			//
			Base.Music.Play(music, 0.5f, 2f);
		}

		void Update() {			
			if (GvrViewer.Instance != null) {
#if UNITY_EDITOR
				if (Input.GetKeyDown(KeyCode.Escape)) {
					GvrViewer.Instance.VRModeEnabled = !GvrViewer.Instance.VRModeEnabled;
				}

#endif
				if (GvrViewer.Instance.BackButtonPressed) {
					Application.Quit();
				}
			}
			if (dead || won) { return; }


			lgtnTimer -= Time.deltaTime;
			if (lgtnTimer < 0f) {
				Lightning();
			}

			rainCurUV.y += Time.deltaTime * rainSpeed;
			rainMaterial.SetTextureOffset("_MainTex", rainCurUV);
		}

		//

		public Tile GetTile(Base.Position2 pos) {
			if (tilesByPos.ContainsKey(pos)) {
				return tilesByPos[pos];
			}
			return null;
		}

		public bool EnemyWillCollideWith(Enemy checker) {
			for (var iter = enemies.GetEnumerator(); iter.MoveNext();) {
				var e = iter.Current;
				if (e == checker) { continue; }
				if ((!e.moving && e.curPos == checker.targetPos) || (e.moving && e.targetPos == checker.targetPos)) {
					return true;
				}
			}
			return false;
		}

		//

		void Lightning() {
			var duration = Random.Range(lgtnMinDuration, lgtnMaxDuration);
			lgtnTimer = Random.Range(lgtnMinTime, lgtnMaxTime) + duration;
			float r = Random.value;
			LeanTween.value(0f, 1f, duration)
				.setOnUpdate((float f) => {
					lgtnLightningLight.enabled = Base.Helpers.Randomness.SimplexNoise.NormalizedNoise(10f, (r + f) * 10f, 0f) > 0.4f;
					if (Base.Helpers.Randomness.SimplexNoise.NormalizedNoise(0.6f, (r - f) * 10f, -700f) > 0.5f) { lgtnLightningLight.transform.Rotate(0f, Random.Range(0f, 180f), 0f, Space.World); }
					RenderSettings.ambientLight = Color.Lerp(lgtnAmbient, nrmlAmbient, f);
				})
				.setOnComplete(() => {
					lgtnLightningLight.enabled = false;
					RenderSettings.ambientLight = nrmlAmbient;
				});
			MovementEffects.Timing.CallDelayed(Random.Range(1f, 2f), () => soundThunder.Play());
		}

		//

		public void SetEndSteps(List<Radio.Direction> directions) {
			if (endDoorShown) { return; }
			endDirections = new List<Radio.Direction>(directions);
			endDirectionsIdx = 0;
		}

		public void Step(Vector3 dir, Vector3 targetPos) {
			if (endDoorShown || endDirectionsIdx < 0) { return; }
			var ddir = Base.Position3.RoundedVector(dir.normalized);
			Radio.Direction rdir = Radio.Direction.East;
			if (ddir.x == 0 && ddir.z == 1) { rdir = Radio.Direction.North; }
			else if (ddir.x == 0 && ddir.z == -1) { rdir = Radio.Direction.South; }
			else if (ddir.x == 1 && ddir.z == 0) { rdir = Radio.Direction.East; }
			else if (ddir.x == -1 && ddir.z == 0) { rdir = Radio.Direction.West; }
			else { rdir = Radio.Direction.NONE; } // you have to start again :(
			if (rdir != endDirections[endDirectionsIdx]) {
				endDirectionsIdx = -1;
#if UNITY_EDITOR
				Debug.Log("sequence aborted");
#endif
				return;
			}
			endDirectionsIdx++;

			if (endDirectionsIdx >= endDirections.Count - 1) {
#if UNITY_EDITOR
				Debug.Log("show end");
#endif
				var lastDir = Map.checkTileDirs[(int)(endDirections[endDirections.Count - 1])];
				ddir.x = lastDir.x; ddir.z = lastDir.y;
				// show end doors, hide unnecessary stuff
				endDoorShown = true;
				var ray = new Ray(targetPos, ddir.ToVector());
				RaycastHit hitInfo;
				for (int i = -10; i <= 10; ++i) {
					var startPos = targetPos + Map.checkDirs[((int)rdir + 1) % 4] * (i / 10f) * tileSize * 0.5f;
					for (int j = 1; j <= 12; ++j) {
						ray.origin = startPos + Vector3.up * (j / 12f) * 3.5f;
#if UNITY_EDITOR
						Debug.DrawRay(ray.origin, ray.direction * tileSize * 0.55f, Color.cyan, 10f);
#endif
						if (Physics.Raycast(ray, out hitInfo, tileSize * 0.55f)) {
							hitInfo.transform.gameObject.SetActive(false);
						}
					}
				}
				endContent.position = targetPos;
				endContent.Rotate(0f, 90f * ((2 + (int)rdir) % 4), 0f);
				endContent.gameObject.SetActive(true);
				winFromPos = Base.Position2.RoundedVector(new Vector2(targetPos.x, targetPos.z) / tileSize);
				winToPos = winFromPos + new Base.Position2(ddir.x, ddir.z);
				endDirectionsIdx = -1;
			}
		}

		//

		public void Win() {
			if (won) { return; }
			won = true;
			for (var iter = levelParent.GetComponentsInChildren<Renderer>().GetEnumerator(); iter.MoveNext();) {
				((Renderer)(iter.Current)).gameObject.SetActive(false);
			}
			Base.Music.Play(null, 0.3f, 1f);
			Map.Deactivate();
			Score.Deactivate();
			player.allowInput = false;
			winHead.gameObject.SetActive(true);
			winHead.EndMessage(Score.score.ToString(), lootCount.ToString());
			blacknessFilter.gameObject.SetActive(true);
		}

		//

		public void Die() {
			if (dead) { return; }
			dead = true;
			//MovementEffects.Timing.CallDelayed(0.5f, () => DieThen());
			DieThen();
		}

		void DieThen() {
			Base.Music.Play(null, 0.3f, 0.01f);
			soundDeath.Play();
			for (var iter = levelParent.GetComponentsInChildren<Enemy>().GetEnumerator(); iter.MoveNext();) {
				((Enemy)(iter.Current)).gameObject.SetActive(false);
			}
			Map.Deactivate();
			Score.Deactivate();
			player.allowInput = false;
			deadHead.gameObject.SetActive(true);
			blacknessFilter.gameObject.SetActive(true);
		}

		public bool TileVisible(Base.Position2 pos) {
			if (tilesByPos.ContainsKey(pos)) {
				var t = tilesByPos[pos];
				return t.visible;
			}
			return false;
		}

		public void VisitRoom(Base.Position2 pos) {
			if (tilesByPos.ContainsKey(pos)) {
				var t = tilesByPos[pos];
				for (var iter_rooms = t.rooms.GetEnumerator(); iter_rooms.MoveNext();) {
					var room = iter_rooms.Current;
					if (!room.visited) {
						for (var iter_tiles = room.tiles.GetEnumerator(); iter_tiles.MoveNext();) {
							Map.AddTile(iter_tiles.Current.pos);
						}
						room.visited = true;
					}
				}
			}
		}

		public void UpdateVisibility(Base.Position2 pos, bool removeOld) {
			if (removeOld) {
				for (var iter = curVisibleRooms.GetEnumerator(); iter.MoveNext();) {
					((Room)(iter.Current)).Activate(false);
				}
			}

			if (tilesByPos.ContainsKey(pos)) {
				newVisibleRooms.Clear();
				var t = tilesByPos[pos];
				// all rooms of this tile (usually only one)
				for (var iter_rooms = t.rooms.GetEnumerator(); iter_rooms.MoveNext();) {
					var room = iter_rooms.Current;
					newVisibleRooms.Add(room);
					// all rooms connected with the rooms
					for (var iter_connections = room.connections.GetEnumerator(); iter_connections.MoveNext();) {
						var r = iter_connections.Current;
						newVisibleRooms.Add(r);
					}
				}

				for (var iter = newVisibleRooms.GetEnumerator(); iter.MoveNext();) {
					iter.Current.Activate(true);
				}

				//curVisibleRooms = new Room[newVisibleRooms.Count];
				//newVisibleRooms.CopyTo(curVisibleRooms);
				if (removeOld) { curVisibleRooms.Clear(); }
				curVisibleRooms.UnionWith(newVisibleRooms);
			}
		}

		//

#if UNITY_EDITOR
		void OnDrawGizmos() {
			if (curVisibleRooms != null) {
				for (var iter = curVisibleRooms.GetEnumerator(); iter.MoveNext();) {
					for (var itter = ((Room)(iter.Current)).tiles.GetEnumerator(); itter.MoveNext();) {
						var t = itter.Current.transform;
						Gizmos.DrawWireSphere(t.position, 2f);
					}
				}
			}
		}
#endif

	}

}
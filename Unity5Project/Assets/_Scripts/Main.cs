using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Main : MonoBehaviour {
		public static Main Inst { get; private set; }
		//
		public Player player;
		public float tileSize = 8f;
		public Transform levelParent;
		public Transform pillarParent;
		//
		Dictionary<Base.Position2, Tile> tilesByPos = new Dictionary<Base.Position2, Tile>();
		List<Room> rooms = new List<Room>();
		//
		HashSet<Room> curVisibleRooms = new HashSet<Room>();
		HashSet<Room> newVisibleRooms = new HashSet<Room>();

		//

		void Awake() {
			Inst = this;
			Map.MapTheWorld(tileSize);
			//
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
			for (var iter = pillarParent.GetComponentsInChildren<Transform>().GetEnumerator(); iter.MoveNext();) {
				var pillar = (Transform)(iter.Current);
				if (pillar == pillarParent) { continue; }
				var pos = pillar.position;
				var p2 = Base.Position2.RoundedVector(new Vector2(pos.x + tileSize * 0.5f, pos.z + tileSize * 0.5f) / tileSize);
				var tiles = new[] { p2, p2 - new Base.Position2(1, 0), p2 - new Base.Position2(0, 1), p2 - new Base.Position2(1, 1) };
				for (int i = 0; i < 4; ++i) {
					if (tilesByPos.ContainsKey(tiles[i])) { tilesByPos[tiles[i]].AddPillar(pillar.gameObject); }
				}
			}
		}

		void Start() {
			print("vr mode enabled " + GvrViewer.Instance.VRModeEnabled);
#if UNITY_EDITOR
			print("viewer type " + GvrViewer.Instance.ViewerType);
#endif
			var pos0 = new Base.Position2(0, 0);
			UpdateVisibility(pos0, false);
			Main.Inst.VisitRoom(pos0);
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
		}

		//

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
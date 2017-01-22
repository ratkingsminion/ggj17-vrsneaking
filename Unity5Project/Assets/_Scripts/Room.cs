using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Room : MonoBehaviour {
		public List<Room> connections;
		public List<Tile> tiles;
		//
		public bool visited { get; set; }
		public bool visible { get; set; }

		//

		public void FadeRevealersIn(float delay, float time) {
			LeanTween.value(0f, 1f, time)
				.setDelay(delay)
				.setOnUpdate(f => {
					for (var iter = tiles.GetEnumerator(); iter.MoveNext();) {
						for (var iter_revealer = iter.Current.GetComponentsInChildren<RevealBack>().GetEnumerator(); iter_revealer.MoveNext();) {
							((RevealBack)(iter_revealer.Current)).SetAlpha(f);
						}
					}
				});
		}

		public void FadeRevealersOut(float delay, float time) {
			LeanTween.value(1f, 0f, time)
				.setDelay(delay)
				.setOnUpdate(f => {
					for (var iter = tiles.GetEnumerator(); iter.MoveNext();) {
						for (var iter_revealer = iter.Current.GetComponentsInChildren<RevealBack>().GetEnumerator(); iter_revealer.MoveNext();) {
							((RevealBack)(iter_revealer.Current)).SetAlpha(f);
						}
					}
				});
		}

		public void Activate(bool activate) {
			//print(activate + " " + name);
			// show or hide all tiles
			visible = activate;
			for (var iter = tiles.GetEnumerator(); iter.MoveNext();) {
				var tile = iter.Current;
				tile.gameObject.SetActive(activate);
				tile.visible = activate;
				if (tile.pillars != null) {
					for (var iter_pillars = tile.pillars.GetEnumerator(); iter_pillars.MoveNext();) {
						iter_pillars.Current.gameObject.SetActive(activate);
					}
				}
			}
		}

		//

#if UNITY_EDITOR
		void OnDrawGizmos() {
			/*Gizmos.color = Color.yellow * 0.8f;
			Gizmos.DrawSphere(transform.position, 0.6f);
			if (connections != null) {
				var start = transform.position + Vector3.up * 0.5f;
				Gizmos.color = Color.yellow;
				for (int i = 0; i < connections.Count; ++i) {
					if (connections[i]==null) { continue; }
					Gizmos.DrawRay(start, (connections[i].transform.position - start) * 0.75f);
				}
			}*/
			if (tiles != null) {
				Gizmos.color = Color.green;
				for (int i = 0; i < tiles.Count; ++i) {
					if (tiles[i] == null) { continue; }
					Gizmos.DrawWireCube(tiles[i].transform.position + new Vector3(0f, 2.5f, 0f), new Vector3(8f, 5f, 8f));
				}
			}
		}
		void OnDrawGizmosSelected() {
			if (tiles != null) {
				Gizmos.color = Color.green * 0.5f;
				for (int i = 0; i < tiles.Count; ++i) {
					if (tiles[i] == null) { continue; }
					Gizmos.DrawCube(tiles[i].transform.position + new Vector3(0f, 2.5f, 0f), 0.99f * new Vector3(8f, 5f, 8f));
				}
			}
		}
#endif
	}

}
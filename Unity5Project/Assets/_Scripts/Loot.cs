using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Loot : Interactable {
		[Header("Loot")]
		public float takeTime = 0.5f;
		public int value = 50;
		public int keys = 0;
		public Base.Sound snd;

		//
		
		public override void Interact(Player player) {
			base.Interact(player);
			if (snd != null) { snd.Play(); }
			player.Collect(gameObject, takeTime);
			if (value > 0) { Score.AddScore(value); }
			if (keys > 0) { player.keys++; Score.Change(); }
			mayLookAt = false;
			for (var iter = GetComponentsInChildren<Collider>(true).GetEnumerator(); iter.MoveNext(); ) {
				((Collider)(iter.Current)).enabled = false;
			}
		}
	}

}
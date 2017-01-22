using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Loot : Interactable {
		[Header("Loot")]
		public float takeTime = 0.5f;
		public int value = 50;
		
		public override void Interact(Player player) {
			base.Interact(player);
			player.Collect(gameObject, takeTime);
			Score.Add(value);
			mayLookAt = false;
		}
	}

}
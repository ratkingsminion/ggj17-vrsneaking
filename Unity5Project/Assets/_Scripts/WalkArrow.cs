using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class WalkArrow : Interactable {
		public override void Interact(Player player) {
			base.Interact(player);
			player.Move(transform.forward * Main.Inst.roomSize);
		}
	}

}
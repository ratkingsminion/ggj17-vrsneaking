using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class VRChange : Interactable {
		public override void Interact(Player player) {
			base.Interact(player);
			GvrViewer.Instance.VRModeEnabled = !GvrViewer.Instance.VRModeEnabled;
		}
	}

}
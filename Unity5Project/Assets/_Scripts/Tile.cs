using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Tile : MonoBehaviour {
		public List<GameObject> pillars { get; private set; } // pillars are at the sides, can be shared...
		public List<Room> rooms { get; private set; }
		public bool visible { get; set; }
		public Base.Position2 pos { get; set; }

		//

		public void AddRoom(Room room) {
			if (rooms == null) { rooms = new List<Room>(); }
			rooms.Add(room);
		}

		public void AddPillar(GameObject pillar) {
			if (pillars == null) { pillars = new List<GameObject>(); }
			pillars.Add(pillar);
		}
	}

}
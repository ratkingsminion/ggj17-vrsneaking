using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RatKing {

	public class Radio : Interactable {
		[Header("Radio")]
		public Base.Sound soundNormal;
		public Base.Sound[] directionSounds;
		public enum Direction { North, East, South, West, NONE }
		public List<Direction> directions;
		//
		Base.Sound curSoundNormal;

		//

		void Start() {
			if (soundNormal != null) { curSoundNormal = soundNormal.Play(transform); }
		}

		public override void Interact(Player player) {
			base.Interact(player);

			if (curSoundNormal != null) {
				LeanTween.value(1f, 0f, 0.5f)
					.setOnUpdate(f => curSoundNormal.SetVolume(f))
					.setOnComplete(() => {
						curSoundNormal.Stop();
						Sequence();
					});
			}
		}

		void Sequence() {
			MovementEffects.Timing.RunCoroutine(SequenceCR());
		}

		IEnumerator<float> SequenceCR() {
			int idx = 0;
			Main.Inst.SetEndSteps(directions);
			while (idx < directions.Count) {
				var msg = directionSounds[(int)directions[idx]].Play(transform);
				yield return MovementEffects.Timing.WaitForSeconds(msg.GetLength() + 0.1f);
				idx++;
			}
			if (soundNormal != null) { curSoundNormal = soundNormal.Play(transform); }
			mayLookAt = true;
		}
	}

}
using UnityEngine;
using System.Collections.Generic;
using World.Entities;

namespace World.Tiles {
	class Factory : Structure {

		[Header("What does this factory produce?")]
		public Entity entity;

		private void OnEnable() {

		}

		private void OnDisable() {
		}

		private void OnMouseUp() {
		}

		public void Spawn() {
			if (GetComponent<TileController>().controlledBy == null) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}
			GameManager.instance.GetCurrentPlayer().GrabEntity(GetComponent<TileController>());
		}
	}
}
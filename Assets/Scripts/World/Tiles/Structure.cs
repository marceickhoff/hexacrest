using UnityEngine;

namespace World.Tiles {
	class Structure : MonoBehaviour {

		private void OnEnable() {
			GameManager.OnTurnStart += ConsumeResources;
			GameManager.OnTurnStart += ProduceResources;
		}

		private void OnDisable() {
			GameManager.OnTurnStart -= ConsumeResources;
			GameManager.OnTurnStart -= ProduceResources;
		}

		public void ProduceResources(PlayerController player) {
			var tileController = GetComponent<TileController>();
			if (tileController != null && tileController.GetOwner() != null && tileController.GetOwner().Equals(player) && tileController.controlledBy != null) {
				player.inventory.Give(tileController.tile.foodProduction, Inventory.Resource.Food);
				player.inventory.Give(tileController.tile.woodProduction, Inventory.Resource.Wood);
				player.inventory.Give(tileController.tile.ironProduction, Inventory.Resource.Iron);
				player.inventory.Give(tileController.tile.stoneProduction, Inventory.Resource.Stone);
			}
		}

		public void ConsumeResources(PlayerController player) {
			var tileController = GetComponent<TileController>();
			if (tileController != null && tileController.GetOwner() != null && tileController.GetOwner().Equals(player) && tileController.controlledBy != null) {
				player.inventory.Take(tileController.tile.foodConsumption, Inventory.Resource.Food);
				player.inventory.Take(tileController.tile.woodConsumption, Inventory.Resource.Wood);
				player.inventory.Take(tileController.tile.ironConsumption, Inventory.Resource.Iron);
				player.inventory.Take(tileController.tile.stoneConsumption, Inventory.Resource.Stone);
			}
		}
	}
}
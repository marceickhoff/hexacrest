using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace World.Tiles {
	class Generic : MonoBehaviour {

		public GameObject genericMenu;

		private void OnEnable() {

		}

		private void OnDisable() {
		}

		/*public void OnMouseDown() {
			if (GameManager.instance.GetCurrentPlayer().CarriesEntities() || TileController.PreviewModeActive()) return;
			ShowMenu();
		}*/

		public void ShowMenu() {
			if (TileController.GetFocus().HasEntities() || TileController.GetFocus().GetOccupant() == null || !TileController.GetFocus().GetOccupant().Equals(GameManager.instance.GetCurrentPlayer())) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}

			var canBuild = TileController.GetFocus().IsRoad() || TileController.GetFocus().tile.connectsToRoad;
			if (TileController.GetFocus() != null && !canBuild) {
				foreach (var neighbor in TileController.GetFocus().GetNeighbors()) {
					if (neighbor.IsRoad() || neighbor.tile.connectsToRoad) {
						canBuild = true;
						break;
					}
				}
			}
			if (canBuild) {
				UIManager.SetCurrentPanel(genericMenu);
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}

		public void BuildRoad() {
			var player = GameManager.instance.GetCurrentPlayer();
			var tile = TileController.GetFocus();
			if (!tile.IsRoad() && tile.GetOccupant().Equals(player) && player.inventory.Take(1, Inventory.Resource.Stone)) {
				tile.SetRoad(true);
				UIManager.instance.PlayTileSound(tile, UIManager.instance.soundBash);
				UIManager.ClosePanel();
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}

		public void BuildSettlement() {
			var player = GameManager.instance.GetCurrentPlayer();
			var tile = TileController.GetFocus();
			foreach (var neighbor in tile.GetNeighbors()) {
				if (neighbor.IsStructure()) {
					UIManager.instance.PlayUISound(UIManager.instance.soundError);
					return;
				}
			}
			if (tile.GetOccupant().Equals(player)) {
				var newTile = TileController.Build(tile, GameManager.instance.world.GetTileController("settlement"), player);
				if (tile.oldAssociatedFacilites.Count > 0) {
					foreach (var facility in tile.oldAssociatedFacilites) {
						newTile.AddFacility(facility);
					}
				}
				UIManager.ClosePanel();
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}
	}
}
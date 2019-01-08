using UnityEngine;

namespace World.Tiles {
	class Field : MonoBehaviour {
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
			if (TileController.GetFocus().GetOccupant() == null || !TileController.GetFocus().GetOccupant().Equals(GameManager.instance.GetCurrentPlayer())) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}
			var canBuild = false;
			if (TileController.GetFocus() != null) {
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
			tile.controlledBy.RemoveFacility(tile);
			tile = TileController.Replace(tile, GameManager.instance.world.GetTileController("empty"));
			TileController.UpdateRealms();
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
			if (tile.controlledBy != null) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}
			foreach (var neighbor in tile.GetNeighbors()) {
				if (neighbor.tile.connectsToRoad) {
					UIManager.instance.PlayUISound(UIManager.instance.soundError);
					return;
				}
			}
			if (tile.GetOccupant().Equals(player)) {
				TileController.Build(tile, GameManager.instance.world.GetTileController("settlement"), player);
				UIManager.ClosePanel();
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}
	}
}
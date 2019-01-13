using UnityEngine;
using UnityEngine.Experimental.UIElements;
using System.Collections.Generic;

namespace World.Tiles {
	class Settlement : Structure {

		public GameObject settlementMenu;
		public GameObject settlementBuildMenu;

		private void OnEnable() {

		}

		private void OnDisable() {
		}

		/*private void OnMouseDown() {
			ShowMainMenu();
		}*/

		public void ShowMenu() {
			if (GameManager.instance.GetCurrentPlayer().CarriesEntities() || TileController.GetFocus().GetOccupant() == null || TileController.PreviewModeActive() || !TileController.GetFocus().GetOccupant().Equals(GameManager.instance.GetCurrentPlayer())) return;
			UIManager.SetCurrentPanel(settlementMenu);
		}

		public void ShowBuildMenu() {
			UIManager.SetCurrentPanel(settlementBuildMenu);
		}

		public void Build(TileController tileController) {
			var player = GameManager.instance.GetCurrentPlayer();
			if (!player.inventory.Has(
				tileController.tile.buildingCostFood,
				tileController.tile.buildingCostWood,
				tileController.tile.buildingCostIron,
				tileController.tile.buildingCostStone
			)) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}
			UIManager.ClosePanel();
			List<TileController> allowed = new List<TileController>();
			foreach (var candidate in TileController.GetFocus().GetNeighbors()) {
				var nextToMatch = true;

				if (tileController.tile.builindRestrictionsNextTo.Count > 0) {
					foreach (var neighbor in candidate.GetNeighbors()) {
						nextToMatch = false;
						foreach (var restriction in tileController.tile.builindRestrictionsNextTo) {
							if (neighbor.tile.Equals(restriction)) {
								nextToMatch = true;
								break;
							}
						}
						if (nextToMatch) break;
					}
				}
				if ((!TileController.GetFocus().HasEntities() || (TileController.GetFocus().HasEntities() && !candidate.tile.obstructed)) &&
				    (candidate.controlledBy == null || (candidate.controlledBy != null && candidate.controlledBy.Equals(TileController.GetFocus()))) &&
				    candidate.GetOccupant().Equals(player) &&
				    nextToMatch &&
				    tileController.tile.builingRestrictionsOn.Contains(candidate.tile) &&
				    !(!tileController.tile.connectsToRoad && candidate.IsRoad())) {
					allowed.Add(candidate);
				}
			}
			TileController.StartPreviewMode(tileController, allowed);
		}

		public void UpgradeTo(TileController tileController) {
			var facilites = TileController.GetFocus().GetFacilities();
			var index = TileController.GetFocus().transform.GetSiblingIndex();
			var upgradedTile = TileController.Build(TileController.GetFocus(), tileController, GameManager.instance.GetCurrentPlayer());
			upgradedTile.transform.SetSiblingIndex(index);
			if (upgradedTile) {
				UIManager.ClosePanel();
				foreach (var facility in facilites) {
					upgradedTile.AddFacility(facility);
				}
				TileController.UpdateRealms();
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}
	}
}
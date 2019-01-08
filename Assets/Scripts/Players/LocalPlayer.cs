using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.EventSystems;
using World;
using World.Tiles;

namespace Players {
	public class LocalPlayer : PlayerController {
		private TileController _hoveredTile;
		private TileController _focusedTile;

		void Update() {
			if (Input.GetButtonDown("Cancel")) {
				if (TileController.PreviewModeActive()) {
					TileController.CancelPreview();
				}
				if (CarriesEntities()) {
					CancelEntityPlacement();
				}
				UIManager.ClosePanel();
			}
			if (!EventSystem.current.IsPointerOverGameObject()) {
				var ray = GameManager.instance.cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					var tileController = hit.transform.gameObject.GetComponent<TileController>();
					if (tileController != null) {
						if (!tileController.Equals(_hoveredTile)) {
							tileController.SetHover();
							if (_hoveredTile != null) _hoveredTile.UnsetHover();
							_hoveredTile = tileController;
						}
						if (Input.GetButtonDown("Select") && HasTurn()) {
							tileController.SetFocus();
							if (TileController.PreviewModeActive()) {
								if (TileController.CanApplyPreview(tileController)) {
									TileController.ApplyPreview(tileController);
									return;
								}
								else {
									UIManager.instance.PlayUISound(UIManager.instance.soundError);
								}
							}
							_focusedTile = tileController;
							var settlement = tileController.GetComponent<Settlement>();
							var factory = tileController.GetComponent<Factory>();
							var generic = tileController.GetComponent<Generic>();
							var field = tileController.GetComponent<Field>();
							if (CarriesEntities()) {
								if (GetEntitySource() != null && GetEntitySource().Equals(tileController) && (tileController.HasEntities() || tileController.IsFactory())) {
									Debug.Log("Grab");
									GrabEntity(tileController);
								}
								else if (GetEntitySource() != null && GetEntitySource().Equals(tileController) && !tileController.HasEntities()) {
									Debug.Log("Cancel");
									CancelEntityPlacement();
								}
								else {
									Debug.Log("Place");
									PlaceEntities(tileController);
								}
							}
							else if (tileController.HasEntities() || tileController.IsFactory()) {
								Debug.Log("Grab");
								GrabEntity(tileController);
							}
							else {
								if (settlement != null) {
									settlement.ShowMenu();
									return;
								}
								if (generic != null) {
									generic.ShowMenu();
									return;
								}
								if (field != null) {
									field.ShowMenu();
									return;
								}
							}
						}
					}
				}
			}
		}
	}
}
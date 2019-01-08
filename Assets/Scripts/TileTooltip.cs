using UnityEngine;
using World;

public class TileTooltip : MonoBehaviour {

	public Tile tile;

	public void Show() {
		string additional = "";
		if (tile.constructionScore > 0) {
			additional += "+"+tile.constructionScore+" score";
		}
		if (tile.foodStorage > 0 && tile.woodStorage > 0 && tile.ironStorage > 0 && tile.stoneStorage > 0) {
			additional += "\n+"+tile.foodStorage+" storage capacity";
		}
		UIManager.instance.ShowTooltip(tile.title, tile.description, tile.buildingCostFood, tile.buildingCostWood, tile.buildingCostIron, tile.buildingCostStone, additional);
	}

	public void Hide() {
		UIManager.instance.HideTooltip();
	}
}

using UnityEngine;

namespace World.Entities {
	public class Entity : MonoBehaviour{

		[Header("Building cost")]
		[Tooltip("")] public int buildingCostFood;
		[Tooltip("")] public int buildingCostWood;
		[Tooltip("")] public int buildingCostIron;
		[Tooltip("")] public int buildingCostStone;
		
		[Header("Movement cost")]
		[Tooltip("")] public int movementCostFood;
		[Tooltip("")] public int movementCostWood;
		[Tooltip("")] public int movementCostIron;
		[Tooltip("")] public int movementCostStone;

		public PlayerController owner;
	}
}
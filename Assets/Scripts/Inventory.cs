using System.Collections.Generic;
using UnityEngine;

public class Inventory {


	public enum Resource {
		Food,
		Wood,
		Iron,
		Stone
	}

	public PlayerController player;

	public delegate void InventoryChanged(Inventory inventory);
	public static event InventoryChanged OnInventoryChanged;

	public delegate void CapacityChanged(Inventory inventory);
	public static event CapacityChanged OnCapacityChanged;

	private Dictionary<Resource, int> _inventory = new Dictionary<Resource, int>() {{Resource.Food, 0}, {Resource.Wood, 0}, {Resource.Iron, 0}, {Resource.Stone, 0}};
	private Dictionary<Resource, int> _inventoryCapacity = new Dictionary<Resource, int>() {{Resource.Food, 0}, {Resource.Wood, 0}, {Resource.Iron, 0}, {Resource.Stone, 0}}; //TODO

	public int Get(Resource resource) {
		return _inventory[resource];
	}

	public int GetCapacity(Resource resource) {
		return _inventoryCapacity[resource];
	}

	public bool Give(int amount, Resource resource) {
		if (amount == 0) return true;
		if (amount < 0) return Take(Mathf.Abs(amount), resource);
		_inventory[resource] += amount;
		if (_inventory[resource] + amount > _inventoryCapacity[resource]) {
			_inventory[resource] = _inventoryCapacity[resource];
		}
		if (OnInventoryChanged != null && player.HasTurn()) OnInventoryChanged(this);
		return true;
	}

	public bool Take(int amount, Resource resource) {
		if (amount == 0) return true;
		if (amount < 0) return Give(Mathf.Abs(amount), resource);
		if (_inventory[resource] - amount >= 0) {
			_inventory[resource] -= amount;
			if (OnInventoryChanged != null && player.HasTurn()) OnInventoryChanged(this);
			return true;
		}
		return false;
	}

	public void AddCapacity(int amount, Resource resource) {
		if (amount == 0) return;
		_inventoryCapacity[resource] += amount;
		if (_inventoryCapacity[resource] < 0) _inventoryCapacity[resource] = 0;
		if (_inventory[resource] > _inventoryCapacity[resource]) {
			Take(_inventory[resource] - _inventoryCapacity[resource], resource);
		}
		if (OnInventoryChanged != null && player.HasTurn()) OnInventoryChanged(this);
		if (OnCapacityChanged != null && player.HasTurn()) OnCapacityChanged(this);
	}

	public bool Has(int amountFood, int amountWood, int amountIron, int amountStone) {
		return _inventory[Resource.Food] >= amountFood && _inventory[Resource.Wood] >= amountWood && _inventory[Resource.Iron] >= amountIron && _inventory[Resource.Stone] >= amountStone;
	}
}
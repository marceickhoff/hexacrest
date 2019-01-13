using System;
using System.Collections;
using System.Collections.Generic;
using Players;
using UnityEngine;
using World;
using World.Entities;
using World.Tiles;
using NetworkPlayer = Players.NetworkPlayer;

public abstract class PlayerController : MonoBehaviour {
	public List<TileController> ownedTiles;
	public List<Unit> ownedUnits;
	private Hashtable _protocol; //"undo stack"


	public delegate void PlayerDone(PlayerController player);
	public static event PlayerDone OnPlayerDone;

	public delegate void EntitiesChanged(PlayerController player);
	public static event EntitiesChanged OnEntitiesChanged;

	public Inventory inventory = new Inventory();

	public Color color = Color.white;

	private Stack<Entity> _carriedEntities = new Stack<Entity>();
	private TileController _entitySource;
	private List<Entity> _ownedEntities = new List<Entity>();

	public Vector3 cameraPosition;
	public Vector3 cameraPositionFocus;

	public bool defeated;

	[HideInInspector] public int score;

	void Start() {
		inventory.player = this;
	}

	void Update() {
		foreach (var entity in _carriedEntities) {
			entity.transform.position = Input.mousePosition;
		}
	}

	public bool CarriesEntities() {
		return _carriedEntities.Count > 0;
	}

	public int CarriedEntitiyCount() {
		return _carriedEntities.Count;
	}

	public TileController GetEntitySource() {
		return _entitySource;
	}

	public bool IsLocal() {
		return GetComponent<LocalPlayer>() != null;
	}

	public bool IsAI() {
		return GetComponent<AIPlayer>() != null;
	}

	public bool IsNetwork() {
		return GetComponent<NetworkPlayer>() != null;
	}

	public void Kill() {
		try {
			var entity = _carriedEntities.Pop();
			_ownedEntities.Remove(entity);
			Destroy(entity.gameObject);
		}
		catch (InvalidOperationException e) {
			// No carried units
		}
	}

	private bool CanGrabEntity(Entity entity) {
		return inventory.Has(entity.movementCostFood, entity.movementCostWood, entity.movementCostIron, entity.movementCostStone) && entity.moved == false;
	}

	private bool CanCreateEntity(Entity entity) {
		return inventory.Has(entity.buildingCostFood, entity.buildingCostWood, entity.buildingCostIron, entity.buildingCostStone);
	}

	public void GrabEntity(TileController source) {
		if (source.IsFactory() && source.GetOwner().Equals(GameManager.instance.GetCurrentPlayer())) {
			var factory = source.GetComponent<Factory>();
			if (factory == null || !HasTurn() || !source.GetOwner().Equals(this))
				return;
			if (!CanCreateEntity(factory.entity) || _carriedEntities.Count >= TileController.maxEntitiesPerTile) {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
				return;
			}
			_entitySource = source;
			var entity = Instantiate(factory.entity);
			_ownedEntities.Add(entity);
			foreach (var material in entity.GetComponent<Renderer>().materials) {
				if (material.name.StartsWith("Player")) {
					material.color = color;
					break;
				}
			}
			entity.owner = this;
			entity.gameObject.SetActive(false);
			_carriedEntities.Push(entity);
			if (OnEntitiesChanged != null) OnEntitiesChanged(this);
			inventory.Take(entity.buildingCostFood, Inventory.Resource.Food);
			inventory.Take(entity.buildingCostWood, Inventory.Resource.Wood);
			inventory.Take(entity.buildingCostIron, Inventory.Resource.Iron);
			inventory.Take(entity.buildingCostStone, Inventory.Resource.Stone);
		}
		else {
			_entitySource = source;
			if (_entitySource != null && !_entitySource.Equals(source) ||
			    _carriedEntities.Count >= TileController.maxEntitiesPerTile ||
			    !source.GetEntityOwner().Equals(GameManager.instance.GetCurrentPlayer())) {
				return;
			}
			Entity entity = null;
			foreach (var e in source.GetEntities()) {
				if (!e.moved) {
					entity = e;
					break;
				}
			}
			if (entity != null && CanGrabEntity(entity)) {
				entity = source.RemoveEntity();
				entity.gameObject.SetActive(false);
				inventory.Take(entity.movementCostFood, Inventory.Resource.Food);
				inventory.Take(entity.movementCostWood, Inventory.Resource.Wood);
				inventory.Take(entity.movementCostIron, Inventory.Resource.Iron);
				inventory.Take(entity.movementCostStone, Inventory.Resource.Stone);
				_carriedEntities.Push(entity);
				if (OnEntitiesChanged != null) OnEntitiesChanged(this);
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}
		//Debug.Log(name+" carries "+_carriedEntities.Count+" entities");
	}

	public Entity GetEntityType() {
		try {
			return _carriedEntities.Peek();
		}
		catch (InvalidOperationException e) {
			return null;
		}
	}

	public void CancelEntityPlacement() {
		if (!CarriesEntities()) return;
		if (_entitySource.IsFactory()) {
			while (CarriesEntities()) {
				var entity = _carriedEntities.Pop();
				_ownedEntities.Remove(entity);
				inventory.Give(entity.buildingCostFood, Inventory.Resource.Food);
				inventory.Give(entity.buildingCostWood, Inventory.Resource.Wood);
				inventory.Give(entity.buildingCostIron, Inventory.Resource.Iron);
				inventory.Give(entity.buildingCostStone, Inventory.Resource.Stone);
				Destroy(entity.gameObject);
			}
		}
		else {
			inventory.Give(GetEntityType().movementCostFood * CarriedEntitiyCount(), Inventory.Resource.Food);
			inventory.Give(GetEntityType().movementCostWood * CarriedEntitiyCount(), Inventory.Resource.Wood);
			inventory.Give(GetEntityType().movementCostIron * CarriedEntitiyCount(), Inventory.Resource.Iron);
			inventory.Give(GetEntityType().movementCostStone * CarriedEntitiyCount(), Inventory.Resource.Stone);
			var tempSrc = _entitySource;
			_entitySource = null;
			Entity tempEntity;
			var tempEntities = new List<Entity>();
			try {
				while (tempEntity = _carriedEntities.Pop()) {
					tempEntities.Add(tempEntity);
				}
			}
			catch (InvalidOperationException e) {
			}


			foreach (var entity in tempEntities) {
				_carriedEntities.Push(entity);
			}
			PlaceEntities(tempSrc);
			foreach (var entity in tempEntities) {
				entity.moved = false;
			}
		}
		if (OnEntitiesChanged != null) OnEntitiesChanged(this);
		//Debug.Log(name+" carries "+_carriedEntities.Count+" entities");
	}

	public void PlaceEntities(TileController target) {
		if (!CarriesEntities()) return;
		if (
			!HasTurn() ||
			(_entitySource != null && !target.GetNeighbors().Contains(_entitySource)) ||
			(target.tile.obstructed && !target.IsRoad() && target.GetOwner() != null && target.GetOwner().Equals(this)) ||
			(target.tile.obstructed && !target.IsRoad() && target.GetOwner() == null)
		) {
			UIManager.instance.PlayUISound(UIManager.instance.soundError);
			return;
		}
		if (target.Equals(_entitySource)) {
			CancelEntityPlacement();
			return;
		}
		var player = GameManager.instance.GetCurrentPlayer();
		if (target.tile.healthPoints <= CarriedEntitiyCount()) {
			if (target.IsStructure() && target.variantDestroyed != null) {
				score += target.tile.destructionScore;
				UIManager.instance.UpdatePlayers(GameManager.instance.GetPlayers());
				var enemyPlayer = target.GetOwner();
				enemyPlayer.inventory.AddCapacity(-target.tile.foodStorage, Inventory.Resource.Food);
				enemyPlayer.inventory.AddCapacity(-target.tile.woodStorage, Inventory.Resource.Wood);
				enemyPlayer.inventory.AddCapacity(-target.tile.ironStorage, Inventory.Resource.Iron);
				enemyPlayer.inventory.AddCapacity(-target.tile.stoneStorage, Inventory.Resource.Stone);
				var facilities = target.oldAssociatedFacilites;
				if (target.IsFacility()) {
					target.controlledBy.RemoveFacility(target);
				}
				target.SetOwner(null);
				TileController.UpdateRealms();
				target = TileController.Replace(target, target.variantDestroyed);
				target.oldAssociatedFacilites = facilities;
				target.UpdateOutline();

				TileController.UpdateRealms();

				var defeat = true;
				foreach (var tile in enemyPlayer.ownedTiles) {
					if (tile.tile.name.Equals("settlement") || tile.tile.name.Equals("village") || tile.tile.name.Equals("city")) {
						defeat = false;
						break;
					}
				}
				if (defeat) {
					enemyPlayer.Lose();
				}
			}
			while (CarriesEntities()) {
				try {
					if (target.HasEnemyEntities()) {
						var enemy = target.GetEntityOwner();
						if (target.GetEntityCount() == _carriedEntities.Count) {
							if (UnityEngine.Random.value >= .5f) {
								var enemyEntity = target.RemoveEntity();
								enemy._ownedEntities.Remove(enemyEntity);
								Destroy(enemyEntity.gameObject);
								player.score++;
								continue;
							}
							else {
								Kill();
								continue;
							}
						}
						else {
							var enemyEntity = target.RemoveEntity();
							enemy._ownedEntities.Remove(enemyEntity);
							Destroy(enemyEntity.gameObject);
							player.score++;
							Kill();
							continue;
						}
					}
					if (target.GetEntityCount() < TileController.maxEntitiesPerTile) {
						var entity = _carriedEntities.Pop();
						entity.gameObject.SetActive(true);
						target.AddEntity(entity);
						entity.moved = true;
					}
					else break;
				}
				catch (InvalidOperationException e) {
					break;
				}
			}
			GameManager.instance.CheckWinningConditions();
			UIManager.instance.UpdatePlayers(GameManager.instance.GetPlayers());
		}
		else {
			UIManager.instance.PlayUISound(UIManager.instance.soundError);
		}
		if (!CarriesEntities()) {
			_entitySource = null;
		}
		if (OnEntitiesChanged != null) OnEntitiesChanged(this);
	}

	void OnEnable() {
		GameManager.OnTurnStart += TurnStart;
		/*TileController.OnGrabEntity += GrabEntity;
		TileController.OnPlaceEntities += PlaceEntities;*/
	}

	void OnDisable() {
		GameManager.OnTurnStart -= TurnStart;
		/*TileController.OnGrabEntity -= GrabEntity;
		TileController.OnPlaceEntities -= PlaceEntities;*/
	}

	/*public List<Structure> GetOwnedStructures() {
		return _ownedStructures;
	}*/

	public bool HasTurn() {
		return GameManager.instance.GetCurrentPlayer().Equals(this);
	}

	public void TurnStart(PlayerController player) {
		if (player.Equals(this)) {
			foreach (var entity in _ownedEntities) {
				entity.moved = false;
			}
			//Debug.Log("Hi! I'm player "+name+" and it's my turn!");
		}
	}

	/*public void Build(Structure structure, TileController position) {
		if (!hasTurn) return;
		_ownedStructures.Add(structure);
	}*/

	/*public void BuildStructure(TileController target, TileController structure) {
		TileController.Build(target, structure);
	}*/

	public void BuildRoad(TileController target) {
		if (target.GetOccupant().Equals(this) || target.GetOwner().Equals(this)) {
			target.SetRoad(true);
		}
	}

	/*public void Destroy(Structure structure) {
		if (!hasTurn) return;
		_ownedStructures.Remove(structure);
	}*/

	public void Kill(Entity entity, int amount) {
		Destroy(_carriedEntities.Pop().gameObject);
	}

	/*public void GainStructure(Structure structure) {
		_ownedStructures.Add(structure);
	}

	public void LoseStructure(Structure structure) {
		_ownedStructures.Remove(structure);
	}*/

	public void Undo() {
		if (!HasTurn()) return;
	}

	public void Done() {
		if (!HasTurn()) return;
		//send protocol to game manager, then:
		//_protocol.Clear();
		if (CarriesEntities()) {
			CancelEntityPlacement();
		}
		UIManager.ClosePanel();
		if (OnPlayerDone != null) OnPlayerDone(this);
	}

	public void Win() {
		//Won the game
	}

	public void Surrender() {
		if (!HasTurn()) return;
		//Quit the game
	}

	public void Lose() {
		defeated = true;
		GameManager.instance.CheckWinningConditions();
		UIManager.instance.UpdatePlayers(GameManager.instance.GetPlayers());
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Players;
using QuickOutline;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Entities;
using World.Tiles;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace World {
	public class TileController : MonoBehaviour {
		private static TileController _lastCreated; //Last created tile.
		private static TileController _origin; //First created tile.
		private static Hashtable _all = new Hashtable(); //List of all tiles.

		public Tile tile; //The tile data.
		private PlayerController _owner; //The owner is the player that build this tile. Only structures/buildable tiles can be owned.
		private PlayerController _occupant; //The occupant is the player in whose realm this tile currently lies.

		private static TileController _previewOrigin; //Tile that was initially selected
		private static TileController _preview; //Tile for preview mode.
		private static TileController _previewInstance; //Instance of preview tile.
		private static List<TileController> _previewArea; //Tile that support preview.

		private bool _isRoad; //This does not mean that roads are visible, merely that this tile acts as if it were a road (i.e. transfers resources etc.). It may still be obstructed to units.
		private Hashtable _roads = new Hashtable(); //This is about actually visible road decals.

		[HideInInspector] public Outline outline; //Outline element

		private List<TileController> _associatedFacilites = new List<TileController>();
		public TileController controlledBy;
		public List<TileController> oldAssociatedFacilites = new List<TileController>();


		private int _currentHighlightPower = 0;

		private static readonly List<Vector3> _entitySlots = new List<Vector3> {
			new Vector3(-.1f, .344f, -.1f), 
			new Vector3(-.1f, .344f,  .1f),
			new Vector3( .1f, .344f, -.1f),
			new Vector3( .1f, .344f,  .1f)
		};

		public static readonly int maxEntitiesPerTile = _entitySlots.Count;

		public GameObject roadDecal;

		private static List<TileController> _highlightedTiles = new List<TileController>();
		private static TileController _focus;

		[HideInInspector] public bool isHighlightedHover;
		[HideInInspector] public bool isHighlightedCursor;
		[HideInInspector] public bool isHighlightedNeutral;
		[HideInInspector] public bool isHighlightedPositive;
		[HideInInspector] public bool isHighlightedNegative;

		[Header("Tile Variants")]
		public TileController variantDefault;
		public TileController variantDestroyed;
		public TileController variantRoad;
		private TileController _original; //Saves the default tile variant


		private Stack<Entity> _entities = new Stack<Entity>();

		public delegate void GrabEntity(TileController tileController);
		public static event GrabEntity OnGrabEntity;

		public delegate void PlaceEntities(TileController tileController);
		public static event PlaceEntities OnPlaceEntities;

		public bool AddFacility(TileController facility) {
			if (_associatedFacilites.Count >= tile.maxNeighboringFacilites) return false;
			_associatedFacilites.Add(facility);
			oldAssociatedFacilites.Add(facility);
			facility.controlledBy = this;
			return true;
		}

		public void RemoveFacility(TileController facility) {
			_associatedFacilites.Remove(facility);
			oldAssociatedFacilites.Remove(facility);
			facility.controlledBy = null;
		}

		public List<TileController> GetFacilities() {
			return _associatedFacilites;
		}

		public int GetEntityCount() {
			return _entities.Count;
		}

		public bool AddEntity(Entity entity) {
			if (_entities.Count <= maxEntitiesPerTile) {
				_entities.Push(entity);
				entity.transform.parent = transform;
				entity.transform.position = transform.position + _entitySlots[GetEntityCount() - 1];
				var source = GameManager.instance.GetCurrentPlayer().GetEntitySource();
				if (source != null) {
					foreach (var direction in Direction.List()) {
						if (GetNeighbor(direction).Equals(source)) {
							entity.transform.rotation = direction.Angle();
							break;
						}
					}
				}
				return true;
			}
			return false;
		}

		public Entity GetEntityType() {
			return HasEntities() ? _entities.Peek () : null;
		}

		public Entity RemoveEntity() {
			Entity entity;
			try {
				entity = _entities.Pop();
			}
			catch (InvalidOperationException e) {
				entity = null;
			}
			return entity;
		}

		/// <summary>
		///   <para>Starts preview mode for a given tile.</para>
		///   <para>Preview mode will temporarily place a tile in the world.</para>
		/// </summary>
		/// <param name="preview">Tile to preview.</param>
		/// <param name="previewArea"></param>
		public static void StartPreviewMode(TileController preview, List<TileController> previewArea) {
			_previewOrigin = GetFocus();
			_previewArea = previewArea;
			_preview = preview;
			_previewInstance = Instantiate(_preview);
			_previewInstance.GetComponent<MeshCollider>().enabled = false; //Disable collider to hide preview tile from mouse events
			_previewInstance.gameObject.SetActive(false);
			foreach(var areaTile in _previewOrigin.GetNeighbors()) {
				if (_previewArea.Contains(areaTile) && CanApplyPreview(areaTile)) {
					areaTile.MarkNeutral();
				}
				else {
					areaTile.MarkNegative();
				}
			}
		}

		public static void CancelPreview() {
			foreach(var areaTile in _previewOrigin.GetNeighbors()) {
				areaTile.UnmarkNeutral();
				areaTile.UnmarkNegative();
				ClearPreview(areaTile);
			}
			Destroy(_previewInstance.gameObject);
			_preview = _previewInstance = _previewOrigin = null;
			_previewArea.Clear();
		}

		/*public static void EndPreviewMode() {
			Destroy(_previewInstance.gameObject);
			_preview = _previewInstance = _previewOrigin = null;
			foreach(var areaTile in _previewArea) {
				areaTile.UnmarkNeutral();
			}
			_previewArea.Clear();
		}*/

		public static bool CanApplyPreview(TileController target) {
			return _previewArea.Contains(target) && (
				       _previewOrigin._associatedFacilites.Count < _previewOrigin.tile.maxNeighboringFacilites ||
				       (_previewOrigin._associatedFacilites.Count >= _previewOrigin.tile.maxNeighboringFacilites && target.IsStructure() && _previewInstance.tile.builingRestrictionsOn.Contains(target.tile))
		   );
		}

		public static void ApplyPreview(TileController target) {
			target.GetComponent<MeshRenderer>().enabled = true;
			if (CanApplyPreview(target)) {
				_preview.transform.rotation = Quaternion.identity;
				var newTile = Build(target, _preview, GameManager.instance.GetCurrentPlayer());
				if (target.IsFacility()) {
					_previewOrigin.RemoveFacility(target);
				}
				_previewOrigin.AddFacility(newTile);
				foreach (var direction in Direction.List()) {
					if (newTile.tile.name.Contains("mine") && newTile.GetNeighbor(direction).tile.name.Contains("mountain")) {
						newTile.transform.LookAt(target.GetNeighbor(direction.Opposite()).transform.position, Vector3.up);
						newTile.transform.rotation = Quaternion.Euler(newTile.transform.rotation.eulerAngles + Quaternion.Euler(0f, -90f, 0f).eulerAngles);
						break;
					}
					if (!newTile.tile.name.Contains("mine") && newTile.GetNeighbor(direction).Equals(_previewOrigin)) {
						newTile.transform.rotation = Direction.Angle(direction.Opposite());
						break;
					}
				}
				CancelPreview();
			}
			else {
				UIManager.instance.PlayUISound(UIManager.instance.soundError);
			}
		}

		public static bool PreviewModeActive() {
			return _previewInstance != null;
		}

		public static void MovePreview(TileController target) {
			if (CanApplyPreview(target) && !target.Equals(_previewInstance) && _previewArea.Contains(target)) {
				if (_previewInstance.IsStructure() && _previewOrigin.IsSettlement()) {
					foreach (var direction in Direction.List()) {
						if (_previewInstance.tile.name.Contains("mine") && target.GetNeighbor(direction).tile.name.Contains("mountain")) {
							_previewInstance.transform.LookAt(target.GetNeighbor(direction.Opposite()).transform.position, Vector3.up);
							_previewInstance.transform.rotation = Quaternion.Euler(_previewInstance.transform.rotation.eulerAngles + Quaternion.Euler(0f, -90f, 0f).eulerAngles);
							break;
						}
						if (!_previewInstance.tile.name.Contains("mine") && target.GetNeighbor(direction).Equals(_previewOrigin)) {
							_previewInstance.transform.rotation = Direction.Angle(direction.Opposite());
							break;
						}
					}
				}
				_previewInstance.gameObject.SetActive(true);
				UIManager.instance.PlayTileSound(target, UIManager.instance.soundPlop);
				_previewInstance.transform.position = target.transform.position;
				target.GetComponent<MeshRenderer>().enabled = false;
				/*if (_blueprint.tile.connectsToRoad) {
						foreach (var neighbor in GetNeighbors()) {
							neighbor.UpdateRoadDecals();
						}
					}*/
			}
			else {
				_previewInstance.gameObject.SetActive(false);
			}
		}

		public static void ClearPreview(TileController target) {
			if (_previewInstance != null && !target.Equals(_previewInstance)) {
				/*if (_blueprint.tile.connectsToRoad) {
					foreach (var neighbor in GetNeighbors()) {
						neighbor.UpdateRoadDecals();
					}
				}*/
				target.GetComponent<MeshRenderer>().enabled = true;
			}
		}

		/// <summary>
		///   <para>Returns the currently focused tile.</para>
		/// </summary>
		public static TileController GetFocus() {
			return _focus;
		}

		/// <summary>
		///   <para>Returns the current owner of this tile.</para>
		/// </summary>
		public PlayerController GetOwner() {
			return _owner;
		}

		/// <summary>
		///   <para>Returns the current occupant of this tile.</para>
		/// </summary>
		public PlayerController GetOccupant() {
			return _occupant;
		}

		/// <summary>
		///   <para>Changes the focused tile.</para>
		/// </summary>
		/// <param name="tileController">Tile to focus.</param>
		public static void MoveFocus(TileController tileController) {
			if (_focus != null) {
				_focus.isHighlightedCursor = false;
				_focus.UpdateOutline();
			}
			tileController.isHighlightedCursor = true;
			tileController.UpdateOutline();
			_focus = tileController;
		}

		public bool HasEnemyEntities() {
			return HasEntities() && !GetEntityType().owner.Equals(GameManager.instance.GetCurrentPlayer());
		}

		/// <summary>
		///   <para>Disables focus.</para>
		/// </summary>
		public static void DisableFocus() {
			_focus.isHighlightedCursor = false;
			_focus.UpdateOutline();
			_focus = null;
		}

		/// <summary>
		///   <para>Draws outlines around realms.</para>
		/// </summary>
		public void UpdateOutline() {
			if (outline == null) return;
			Color newColor = new Color();
			Outline.Mode newOutlineMode = Outline.Mode.OutlineAll;
			if (_owner != null) {
				newColor = _owner.color;
				newOutlineMode = Outline.Mode.OutlineVisible;
				outline.OutlineWidth = 10f;
			}
			if (_occupant != null) {
				newColor = _occupant.color;
				newOutlineMode = Outline.Mode.OutlineVisible;
				outline.OutlineWidth = 10f;
			}
			if (isHighlightedHover) {
				newColor = GameManager.instance.highlightHoverColor;
				outline.OutlineWidth = 10f;
				newOutlineMode = Outline.Mode.OutlineAll;
			}
			if (isHighlightedCursor) {
				newColor = GameManager.instance.highlightClickColor;
				outline.OutlineWidth = 10f;
				newOutlineMode = Outline.Mode.OutlineAll;
			}
			if (isHighlightedNeutral) {
				newColor = GameManager.instance.highlightNeutralColor;
				outline.OutlineWidth = 0f;
				newOutlineMode = Outline.Mode.OutlineAndSilhouette;
			}
			if (isHighlightedNegative) {
				newColor = GameManager.instance.highlightNegativeColor;
				outline.OutlineWidth = 0f;
				newOutlineMode = Outline.Mode.OutlineAndSilhouette;
			}


			if (!newColor.Equals(default(Color))) {
				outline.enabled = true;
				outline.OutlineColor = newColor;
				outline.OutlineMode = newOutlineMode;
			}
			else {
				outline.enabled = false;
			}
		}

		public void UnsetHover() {
			if (Equals(_previewInstance) || !tile.selectable) return;
			if (PreviewModeActive()) ClearPreview(this);
			isHighlightedHover = false;
			UpdateOutline();
		}

		public void MarkNeutral() {
			isHighlightedNeutral = true;
			UpdateOutline ();
		}

		public void MarkNegative() {
			isHighlightedNegative = true;
			UpdateOutline();
		}

		public void UnmarkNeutral() {
			isHighlightedNeutral = false;
			UpdateOutline ();
		}

		public void UnmarkNegative() {
			isHighlightedNegative = false;
			UpdateOutline();
		}

		/// <summary>
		///   <para>When this tile gains hover focus.</para>
		/// </summary>
		public void SetHover() {
			if (Equals(_previewInstance) || !tile.selectable || EventSystem.current.IsPointerOverGameObject()) return;
			if (PreviewModeActive()) MovePreview(this);
			isHighlightedHover = true;
			UpdateOutline();
		}

		/// <summary>
		///   <para>When this tile loses hover focus.</para>
		/// </summary>
		/*public void OnMouseExit() {
			if (Equals(_previewInstance) || !tile.selectable) return;
			if (PreviewModeActive()) ClearPreview(this);
			isHighlightedHover = false;
			UpdateOutline();
		}*/

		/// <summary>
		///   <para>When this tile is clicked.</para>
		/// </summary>
		public void SetFocus() {
			if (!tile.selectable || EventSystem.current.IsPointerOverGameObject()) return;
			MoveFocus(this);
			/*var player = GameManager.instance.GetCurrentPlayer();
			if (player.CarriesEntities()) {
				if ((HasEntities() && !GetEntityOwner().Equals(player)) || (IsStructure() && !_owner.Equals(player))) {
					player.Attack(this);
				}
				else if (IsFactory() &&) {
					if ()
				}
				else {

				}
			}*/
		}

		public PlayerController GetEntityOwner() {
			return HasEntities() ? _entities.Peek().owner : null;
		}

		public bool IsStructure() {
			return GetComponent<Structure>() != null;
		}

		public bool IsSettlement() {
			return GetComponent<Settlement>() != null;
		}

		public bool IsFacility() {
			return GetComponent<Structure>() != null && GetComponent<Settlement>() == null;
		}

		public bool IsFactory() {
			return GetComponent<Factory>() != null;
		}

		public bool HasEntities() {
			return _entities.Count > 0;
		}


		/// <summary>
		///   <para>Sets the owner of this tile and updates realms.</para>
		/// </summary>
		/// <param name="player">New owner.</param>
		public void SetOwner(PlayerController player) {
			if (player == null) {
				if (_owner == null) return;
				_owner.ownedTiles.Remove(this);
				_owner = null;
				_occupant = null;
			}
			else {
				_owner = player;
				_occupant = player;
				_owner.ownedTiles.Add(this);
			}
			UpdateRealms();
		}

		public void SetOccupant(PlayerController player) {
			_occupant = player;
		}

		/// <summary>
		///   <para>Checks tiles for ownership and updates occupation radiuses.</para>
		/// </summary>
		public static void UpdateRealms() {
			foreach (TileController ownableTile in GetAll(Tile.ownable)) {
				foreach (var realmTile in ownableTile.GetArea(ownableTile.tile.occupyRadius)) {
					if (ownableTile._owner == null) {
						realmTile._occupant = null;
					}
					else if (realmTile._occupant == null && (realmTile._owner == null || realmTile._owner.Equals(ownableTile._owner))) {
						realmTile._occupant = ownableTile._owner;
					}
					realmTile.UpdateOutline();
				}
			}
			GameManager.instance.CheckWinningConditions();
			UIManager.instance.UpdatePlayers(GameManager.instance.GetPlayers());
		}

		/// <summary>
		///   <para>Creates a new tile at the specified world coordinates.</para>
		/// </summary>
		/// <param name="paint">Tile to create.</param>
		/// <param name="position">Global position.</param>
		/// <param name="tileType">Tile type this tile will be associated with.</param>
		/// <param name="index">New tile's position in the associated tile type's list.</param>
		public static TileController Create(TileController paint, Vector3 position, string tileType, int index) {
			var newTile = Instantiate(paint, GameManager.instance.world.transform);
			//if (newTile.tile.selectable) {
				newTile.outline = Instantiate(GameManager.instance.cursor, newTile.transform);
				newTile.outline.transform.position = Vector3.zero;
			//}
			/*var tilePrototype = Instantiate(GameManager.instance.tilePrototype);
			var mFilter = tilePrototype.GetComponent<MeshFilter>();
			var mRenderer = tilePrototype.GetComponent<MeshRenderer>();
			var tController = tilePrototype.GetComponent<TileController>();
			mFilter.sharedMesh = paint.tile.defaultModel.GetComponent<MeshFilter>().sharedMesh;
			mRenderer.sharedMaterials = paint.tile.defaultModel.GetComponent<MeshRenderer>().sharedMaterials;
			tController.tile = paint.tile;
			tController.outline = tilePrototype.GetComponentInChildren<Outline>();*/
			newTile.transform.position = position;
			newTile.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 6) % 6 * 60f, 0f);
			newTile.name = tileType;
			if (newTile.tile.connectsToRoad) {
				foreach (var neighbor in newTile.GetNeighbors()) {
					neighbor.UpdateRoadDecals();
				}
			}
			if (!_all.ContainsKey(tileType)) {
				_all.Add(tileType, new List<TileController>());
			}
			if (_all[tileType].GetType() == typeof(List<TileController>)) {
				var list = _all[tileType] as List<TileController>;
				if (list != null) list.Insert(index, newTile);
			}
			if (_origin == null) _origin = newTile;
			_lastCreated = newTile;
			return _lastCreated;
		}

		/// <summary>
		///   <para>Creates a new tile at the specified world coordinates.</para>
		/// </summary>
		/// <param name="paint">Tile to create.</param>
		/// <param name="position">Global position.</param>
		/// <param name="index">New tile's position in the associated tile type's list.</param>
		public static TileController Create(TileController paint, Vector3 position, int index) {
			return Create(paint, position, paint.tile.name, index);
		}

		/// <summary>
		///   <para>Creates a new tile at the specified world coordinates.</para>
		/// </summary>
		/// <param name="paint">Tile to create.</param>
		/// <param name="position">Global position.</param>
		/// <param name="tileType">Tile type this tile will be associated with.</param>
		public static TileController Create(TileController paint, Vector3 position, string tileType) {
			return Create(paint, position, tileType, GetAll(paint.tile.name).Count);
		}

		/// <summary>
		///   <para>Creates a new tile at the specified world coordinates.</para>
		/// </summary>
		/// <param name="paint">Tile to create.</param>
		/// <param name="position">Global position.</param>
		public static TileController Create(TileController paint, Vector3 position) {
			return Create(paint, position, paint.tile.name);
		}

		/// <summary>
		///   <para>Replaces an existing tile.</para>
		/// </summary>
		/// <param name="original">Original tile controller.</param>
		/// <param name="replacement">Replacement tile.</param>
		/// <param name="tileType"></param>
		public static TileController Replace(TileController original, TileController replacement, string tileType) {
			if (!original.isActiveAndEnabled) return _lastCreated;
			if (original.name == replacement.name) {
				_lastCreated = Create(replacement, original.transform.position, tileType, GetAll(original.name).FindIndex(tc => tc.name.Equals(original.name)));
			}
			else {
				var list = _all[original.name] as List<TileController>;
				_all.Remove(original.name);
				if (list != null) list.Remove(original);
				_all.Add(original.name, list);
				_lastCreated = Create(replacement, original.transform.position, tileType);
			}
			//_lastCreated.transform.parent = transform;
			_lastCreated.name = tileType;
			if (original.Equals(_origin)) _origin = _lastCreated;
			original.gameObject.SetActive(false);
			foreach (var entity in original._entities) {
				_lastCreated.AddEntity(entity);
			}
			Destroy(original.gameObject);
			return _lastCreated;
		}

		/// <summary>
		///   <para>Replaces an existing tile.</para>
		/// </summary>
		/// <param name="original">Original tile controller.</param>
		/// <param name="replacement">Replacement tile.</param>
		public static TileController Replace(TileController original, TileController replacement) {
			return Replace(original, replacement, replacement.tile.name);
		}

		public static TileController Build(TileController position, TileController structure, PlayerController player) {
			if (player.inventory.Has(structure.tile.buildingCostFood, structure.tile.buildingCostWood, structure.tile.buildingCostIron, structure.tile.buildingCostStone)) {
				player.inventory.Take(structure.tile.buildingCostFood, Inventory.Resource.Food);
				player.inventory.Take(structure.tile.buildingCostWood, Inventory.Resource.Wood);
				player.inventory.Take(structure.tile.buildingCostIron, Inventory.Resource.Iron);
				var oldCapacityFood = position.tile.foodStorage;
				var oldCapacityWood = position.tile.woodStorage;
				var oldCapacityIron = position.tile.ironStorage;
				var oldCapacityStone = position.tile.stoneStorage;
				var building = Replace(position, structure);
				if (building.tile.sound != null) {
					var sound = Instantiate(building.tile.sound);
					sound.transform.parent = building.transform;
					sound.time = Random.Range(0, sound.clip.length);
				}

				if (building.tile.buildSound == null) {
					UIManager.instance.PlayUISound(UIManager.instance.soundBash);
				}
				else {
					UIManager.instance.PlayUISound(building.tile.buildSound);
				}
				if (building.tile.connectsToRoad) {
					foreach (var neighbor in building.GetNeighbors()) {
						neighbor.UpdateRoadDecals();
					}
				}
				player.score += building.tile.constructionScore;
				building.SetOwner(player);
				player.inventory.AddCapacity(building.tile.foodStorage - oldCapacityFood, Inventory.Resource.Food);
				player.inventory.AddCapacity(building.tile.woodStorage - oldCapacityWood, Inventory.Resource.Wood);
				player.inventory.AddCapacity(building.tile.ironStorage - oldCapacityIron, Inventory.Resource.Iron);
				player.inventory.AddCapacity(building.tile.stoneStorage - oldCapacityStone, Inventory.Resource.Stone);
				UpdateRealms();
				UIManager.instance.UpdatePlayers(GameManager.instance.GetPlayers());
				GameManager.instance.CheckWinningConditions();
				return building;
			}
			return null;
		}

		/// <summary>
		///   <para>Returns a list of all existing tile controllers in this world.</para>
		/// </summary>
		public static List<TileController> GetAll() {
			var all = new List<TileController>();
			foreach (DictionaryEntry dictList in _all) {
				if (dictList.Value.GetType() == typeof(List<TileController>)) {
					var tileListe = dictList.Value as List<TileController>;
					if (tileListe != null) {
						all.AddRange(tileListe);
					}
				}
			}
			return all;
		}

		/// <summary>
		///   <para>Returns a list of all existing tile controllers in this world that match a specified tile type.</para>
		/// </summary>
		/// <param name="tileType">Tile type.</param>
		public static List<TileController> GetAll(string tileType) {
			var all = new List<TileController>();
			if (_all.ContainsKey(tileType) && _all[tileType].GetType() == typeof(List<TileController>)) {
				all =_all[tileType] as List<TileController>;
			}
			return all;
		}

		/// <summary>
		///   <para>Returns a list of all existing tile controllers in this world that match the specified tile types.</para>
		/// </summary>
		/// <param name="tileTypes">Tile types.</param>
		public static List<TileController> GetAll(IEnumerable<string> tileTypes) {
			var all = new List<TileController>();
			foreach (var tileType in tileTypes) {
				all.AddRange(GetAll(tileType));
			}
			return all;
		}

		/// <summary>
		///   <para>Returns the last created or changed tile.</para>
		/// </summary>
		public static TileController GetLast() {
			return _lastCreated;
		}

		/// <summary>
		///   <para>Returns the first ever created tile (should be located at Vector3.zero).</para>
		/// </summary>
		public static TileController GetOrigin() {
			return _origin;
		}

		/// <summary>
		///   <para>Checks if there are any neighboring tiles.</para>
		/// </summary>
		public bool HasNeighbor() {
			return GetNeighbors().Count > 0;
		}

		/// <summary>
		///   <para>Checks if there is a neighboring tile in the specified direction.</para>
		/// </summary>
		/// <param name="direction"></param>
		public bool HasNeighbor(Direction direction) {
			return GetNeighbor(direction) != null;
		}

		/// <summary>
		///   <para>Gets the local position of a potential neighboring tile in the specified direction.</para>
		/// </summary>
		/// <param name="direction"></param>
		public Vector3 GetNeighborPosition(Direction direction) {
			return direction.vector;
		}

		/// <summary>
		///   <para>Gets the world position of a potential neighboring tile in the specified direction.</para>
		/// </summary>
		/// <param name="direction"></param>
		public Vector3 GetNeighborPositionAbsolute(Direction direction) {
			return gameObject.transform.position + direction.vector;
		}

		/// <summary>
		///   <para>Gets the neighboring tile controller in the specified direction if it exists. Uses ray casting to detect tiles.</para>
		/// </summary>
		/// <param name="direction"></param>
		public TileController GetNeighbor(Direction direction) {
			var center = gameObject.transform.position;
			var rayVector = GetNeighborPosition(direction);
			RaycastHit hit;
			return Physics.Raycast(center, rayVector, out hit, Tile.diameter) ? hit.transform.gameObject.GetComponent<TileController>() : null;
		}

		/// <summary>
		///   <para>Gets all neighboring tile controllers. Uses ray casting to detect tiles.</para>
		/// </summary>
		public List<TileController> GetNeighbors() {
			var neighbors = new List<TileController>();
			foreach (var direction in Direction.List()) {
				if (HasNeighbor(direction)) {
					neighbors.Add(GetNeighbor(direction));
				}
			}
			return neighbors;
		}

		/// <summary>
		///   <para>Gets all tile controllers in a specified radius.</para>
		/// </summary>
		/// <param name="radius">Radius.</param>
		/// <param name="filters">Tile type filters.</param>
		public List<TileController> GetArea(int radius, string[] filters) {
			var area = new List<TileController>();
			if (radius == 1) area = GetNeighbors();
			else {
				var totalStackSize = 1;
				for (var i = 1; i < radius; i++) {
					totalStackSize += Mathf.RoundToInt(Mathf.Pow(0, i) + 6 * i);
				}
				var frontier = new Queue<TileController>();
				frontier.Enqueue(this);
				while (frontier.Count > 0 && totalStackSize > 0) {
					var current = frontier.Dequeue();
					var neighbors = current.GetNeighbors();
					foreach (var next in neighbors) {
						if (!area.Contains(next)) {
							frontier.Enqueue(next);
							area.Add(next);
							totalStackSize--;
						}
					}
				}
			}
			area.Add(this); //Adding center piece
			var filteredArea = new List<TileController>();
			if (filters.Length > 0) {
				foreach (var tileController in area) {
					if (filters.Contains (tileController.tile.name)) {
						filteredArea.Add (tileController);
					}
				}
			}
			else filteredArea = area;
			return filteredArea;
		}

		/// <summary>
		///   <para>Gets all tile controllers in a specified radius.</para>
		/// </summary>
		/// <param name="radius">Radius.</param>
		/// <param name="filter">Tile type filter.</param>
		public List<TileController> GetArea(int radius, string filter) {
			return GetArea(radius, new[] {filter});
		}

		/// <summary>
		///   <para>Gets all tile controllers in a specified radius.</para>
		/// </summary>
		/// <param name="radius">Radius.</param>
		public List<TileController> GetArea(int radius) {
			return GetArea(radius, new string[] {});
		}

		/// <summary>
		///   <para>Changes this tiles model.</para>
		/// </summary>
		/// <param name="newModel">New model.</param>
		public void ChangeModel(GameObject newModel) {
			var mesh = gameObject.GetComponent<MeshFilter>().mesh = newModel.GetComponent<MeshFilter>().sharedMesh;
			gameObject.GetComponent<MeshRenderer>().materials = newModel.GetComponent<MeshRenderer>().sharedMaterials;
			gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
		}

		/// <summary>
		///   <para>Updates road decals on tile according to neighboring tiles.</para>
		/// </summary>
		public void UpdateRoadDecals() {
			foreach (var direction in Direction.List()) {
				if (_isRoad) {
					if (HasNeighbor(direction)) {
						if (GetNeighbor(direction)._isRoad || GetNeighbor(direction).tile.connectsToRoad) {
							if (_roads.ContainsKey(direction)) {
								var road = _roads[direction] as GameObject;
								if (road != null) road.SetActive(true);
							}
							else if ( roadDecal != null) {
								var newRoad = Instantiate(roadDecal, transform);
								newRoad.transform.position = new Vector3(transform.position.x, .25f, transform.position.z);
								newRoad.transform.rotation = direction.Opposite().Angle();
								_roads.Add(direction, newRoad);
							}
						}
						else {
							if (_roads.ContainsKey(direction)) {
								var road = _roads[direction] as GameObject;
								if (road != null) road.SetActive(false);
							}
						}
					}
				}
				else {
					if (_roads.ContainsKey(direction)) {
						var road = _roads[direction] as GameObject;
						if (road != null) road.SetActive(false);
					}
				}
			}
		}

		/// <summary>
		///   <para>Adds or removes a road on this tile.</para>
		/// </summary>
		/// <param name="state">Enable or disable road.</param>
		public void SetRoad(bool state) {
			var roadTile = this;
			if (variantRoad == null && roadDecal == null) {
				return;
			}
			if (state && variantRoad != null && variantRoad.roadDecal != null) {
				roadTile = Replace(this, variantRoad);
				UpdateRealms();
				roadTile._original = this;
				roadTile.SetRoad(true);
			}
			if (!state && roadTile._original != null) {
				roadTile = Replace(this, roadTile._original);
				UpdateRealms();
				roadTile.SetRoad(false);
			}
			roadTile._isRoad = state;
			roadTile.UpdateRoadDecals();
			foreach (var neighbor in roadTile.GetNeighbors()) {
				neighbor.UpdateRoadDecals();
			}
		}

		/// <summary>
		///   <para>Checks if this tile is a road.</para>
		/// </summary>
		public bool IsRoad() {
			return _isRoad;
		}

		/// <summary>
		///   <para>Highlights this tile visually.</para>
		/// </summary>
		public void Highlight(Color color, int power) {
			if (_currentHighlightPower <= power) {
				_currentHighlightPower = power;
				outline.enabled = true;
				outline.OutlineColor = color;
				_highlightedTiles.Add(this);
			}
		}

		public void Highlight(Color color) {
			Highlight(color, 0);
		}

		/// <summary>
		///   <para>Un-highlights this tile visually.</para>
		/// </summary>
		public void UnHighlight(int power) {
			if (_currentHighlightPower <= power) {
				outline.enabled = false;
				outline.OutlineColor = new Color();
				_highlightedTiles.Remove(this);
			}
		}

		public static void UnHighlightAll(int power) {
			foreach (var tile in _highlightedTiles) {
				if (tile._currentHighlightPower <= power) {
					tile.outline.enabled = false;
					tile.outline.OutlineColor = new Color();
				}
			}
		}

		public void UnHighlight() {
			UnHighlight(_currentHighlightPower);
		}
	}
}

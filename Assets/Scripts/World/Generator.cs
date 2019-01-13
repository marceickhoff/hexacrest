using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace World {
	public class Generator : MonoBehaviour {
		
		public int size = 50;
		public int forestCount = 60;
		public int mountainCount = 20;
		public int lakeCount = 10;
		public string randomSeed;

		private List<TileController> _playerSpawns = new List<TileController>();
		private WorldController _world;

		/// <summary>
		///   <para>Generates a random world.</para>
		/// </summary>
		public void Generate() {
			_world = GetComponent<WorldController>();
			SetRandomSeed();

			var playerCount = GameManager.instance.GetPlayers().Count;
			CreateContinent(_world.GetTileController("empty"));
			CreateForests(playerCount * forestCount, _world.GetTileController("forest"), 0, 2);
			CreateLakes(playerCount * lakeCount, _world.GetTileController("waterLake"),  0, 2);
			CreateMountains(playerCount * mountainCount, _world.GetTileController("mountain"), 0, 2);
			CreateOcean(size);
			ClearPaths();
			PrepareSpawnAreas();
			TileController.UpdateRealms();
			DistributeSoundSources("waterOcean", 4);
			DistributeSoundSources("forest", 5);
			DistributeSoundSources("mountain", 4);
		}

		/// <summary>
		///   <para>Initializes the random number generator with the given seed.</para>
		/// </summary>
		public void SetRandomSeed() {
			if (randomSeed == null) randomSeed = "";
			if (randomSeed.Equals("")) {
				for (var i = 0; i < 8; i++) {
					randomSeed += Random.Range(0, 9);
				}
			}
			Random.InitState(randomSeed.GetHashCode());
		}

		/// <summary>
		///   <para>Makes mountains that are surrounded by mountains twice as big.</para>
		/// </summary>
		public void ScaleMountains() {
			var mountains = TileController.GetAll("mountain");
			foreach (var mountain in mountains) {
				var sourrounded = true;
				foreach (var neighbor in mountain.GetNeighbors()) {
					if (!mountain.tile.name.Equals(neighbor.tile.name)) {
						sourrounded = false;
						break;
					}
				}
				if (sourrounded) {
					mountain.transform.localScale = new Vector3(2,2,2);
					mountain.transform.Translate( new Vector3(0, -.25f, 0));
				}
			}
		}

		/// <summary>
		///   <para>Creates a square ocean of the given size including outer static and inner non-static ocean.</para>
		/// </summary>
		/// <param name="oceanSize">Size of the inner non-static ocean.</param>
		private void CreateOcean(int oceanSize) {
			var waterTile = _world.GetTileController("waterOcean");
			var waterDeepTile = _world.GetTileController("waterOceanDeep");
			var existingTiles = TileController.GetAll();
			var oceanQueue = new Queue<TileController>();
			foreach (var tile in existingTiles) {
				foreach (var direction in Direction.List()) {
					if (!tile.HasNeighbor(direction)) {
						oceanQueue.Enqueue(TileController.Create(waterTile, tile.GetNeighborPositionAbsolute(direction)));
					}
				}
			}
			var extendedOceanSize = oceanSize + 50;
			while (oceanQueue.Count > 0) {
				var oceanTile = oceanQueue.Dequeue();
				foreach (var direction in Direction.List()) {
					if (oceanTile.HasNeighbor(direction)) continue;
					var newOceanTilePosition = oceanTile.GetNeighborPositionAbsolute(direction);
					if (Mathf.Abs(newOceanTilePosition.x * 2) <= Tile.diameter * (oceanSize - 1) &&
						Mathf.Abs(newOceanTilePosition.z * 2) <= Tile.height * (oceanSize - 1)) {
						oceanQueue.Enqueue(TileController.Create(waterTile, newOceanTilePosition));
					}
					else if (Mathf.Abs(newOceanTilePosition.x * 2) <= Tile.diameter * (extendedOceanSize - 1) &&
					    Mathf.Abs(newOceanTilePosition.z * 2) <= Tile.height * (extendedOceanSize - 1)) {
						var newOceanTile = TileController.Create(waterDeepTile, newOceanTilePosition);
						var meshRenderer = newOceanTile.GetComponent<MeshRenderer>();
						meshRenderer.receiveShadows = false;
						meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
						Destroy(newOceanTile.GetComponent<MeshCollider>());
						oceanQueue.Enqueue(newOceanTile);
					}
				}
			}
		}

		/*/// <summary>
		///   <para>Creates shallow oceans around land and makes deep ocean tiles static.</para>
		/// </summary>
		public void CreateShallowOceans() {
			var oceanTiles = TileController.GetAll ("waterOceanDeep");
			var replaceTiles = new List<TileController> ();
			foreach (var tile in oceanTiles) {
				foreach (var neighbor in tile.GetNeighbors()) {
					if (!neighbor.name.Equals ("waterOceanDeep")) {
						foreach (var areaTile in tile.GetArea(10)) {
							if (areaTile.name.Equals("waterOceanDeep") && !replaceTiles.Contains (areaTile)) {
								replaceTiles.Add (areaTile);
							}
						}
					}
				}
			}
			foreach (var tile in replaceTiles) {
				TileController.Replace (tile, _world.GetTileController("waterOceanShallow"));
			}
			foreach (var tile in TileController.GetAll("waterOceanDeep")) {
				// Make static and other stuff to increase performance
				var meshRenderer = tile.GetComponent<MeshRenderer>();
				meshRenderer.receiveShadows = false;
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
				Destroy(tile.GetComponent<BoxCollider>());
				tile.gameObject.isStatic = true;
			}
		}*/

		/// <summary>
		///   <para>Creates a natural looking continent fit for the given player count.</para>
		/// </summary>
		/// <param name="paint">Tile to draw continent with.</param>
		private void CreateContinent(TileController paint) {
			var playerCount = GameManager.instance.GetPlayers().Count;
			TileController.Create(_world.GetTileController("empty"), Vector3.zero); //Origin
			var playerDirections = Direction.Spread(playerCount);
			var paths = new List<TileController>();
			/*foreach (var direction in playerDirections) {
				paths.AddRange(DrawSquigglyLine(TileController.GetOrigin(), _world.GetTileController("empty"), new List<string> {
						"waterOcean",
						"empty"
					},  direction, size / 4 + Mathf.RoundToInt(Mathf.Pow(playerCount, 1.5f))));
				var settlement = TileController.Replace(TileController.GetLast(), _world.GetTileController("settlement"));
				_playerSpawns.Add(settlement);
			}*/
			var settlement = _world.GetTileController("settlement");
			foreach (var direction in playerDirections) {
				paths.AddRange(DrawSquigglyLine(TileController.GetOrigin(), _world.GetTileController("empty"), new List<string> {
						"waterOcean",
						"empty"
					},  direction, size / 4 + Mathf.RoundToInt(Mathf.Pow(playerCount, 1.5f))));
				/*while (TileController.GetLast().GetArea(settlement.tile.occupyRadius * 2).Contains(_world.GetTileController("settlement"))) {
					Debug.Log("shifting spawn a bit...");
					paths.AddRange(DrawSquigglyLine(TileController.GetLast(), _world.GetTileController("empty"), new List<string> {
							"waterOcean",
							"empty"
					},  direction, settlement.tile.occupyRadius));
				}*/
				var playerSpawn = TileController.Replace(TileController.GetLast(), _world.GetTileController("settlement"));
				_playerSpawns.Add(playerSpawn);
			}
			TileController.UpdateRealms();
			foreach (var t in paths) {
				DrawLand(t, paint, Random.Range(1, 20), .1f);
			}
			//CreateShallowOceans (); // Disabled for now to increase generation speed.
		}

		/// <summary>
		///   <para>Creates random forests.</para>
		/// </summary>
		/// <param name="count">Forest count.</param>
		/// <param name="paint">Tile to draw forest with.</param>
		/// <param name="sizeMin">Minimum size.</param>
		/// <param name="sizeMax">Maximum size.</param>
		private void CreateForests(int count, TileController paint, int sizeMin, int sizeMax) {
			int index;
			var tiles = TileController.GetAll("empty");
			for (int i = 0; i <= count; i++) {
				index = Random.Range(0, tiles.Count);
				var start = tiles[index];
				var lineTileControllers = DrawSquigglyLine(start, paint, new List<string> {
					"empty",
					"forest"
				}, Direction.Random(), 5);
				foreach (var tile in lineTileControllers) {
					var forestSize = Random.Range(sizeMin, sizeMax + 1);
					DrawCircle(tile, paint, new List<string> {
						"empty",
						"forest"
					}, forestSize, .33f);
				}
			}
		}

		/// <summary>
		///   <para>Creates random lakes.</para>
		/// </summary>
		/// <param name="count">Lake count.</param>
		/// <param name="paint">Tile to draw lakes with.</param>
		/// <param name="sizeMin">Minimum thickness.</param>
		/// <param name="sizeMax">Maximum thickness.</param>
		private void CreateLakes(int count, TileController paint, int sizeMin, int sizeMax) {
			var canvas = new List<string> {
				"empty",
				"forest",
				"waterLake"
			};
			var tiles = TileController.GetAll(canvas);
			for (int i = 0; i <= count; i++) {
				var index = Random.Range(0, tiles.Count);
				var start = tiles[index];
				var lineTileControllers = DrawSquigglyLine(start, paint, canvas, Direction.Random(), 3);
				foreach (var tile in lineTileControllers) {
					DrawCircle(tile, paint, canvas, Random.Range(sizeMin, sizeMax), .33f);
				}
			}
		}

		/// <summary>
		///   <para>Creates random mountain ranges.</para>
		/// </summary>
		/// <param name="count">Mountain range count.</param>
		/// <param name="paint">Tile to draw mountains with.</param>
		/// <param name="sizeMin">Minimum thickness.</param>
		/// <param name="sizeMax">Maximum thickness.</param>
		private void CreateMountains(int count, TileController paint, int sizeMin, int sizeMax) {
			int index;
			var canvas = new List<string> {
				"empty",
				"forest",
				"mountain"
			};
			var tiles = TileController.GetAll(canvas);
			for (int i = 0; i <= count; i++) {
				index = Random.Range(0, tiles.Count);
				var start = tiles[index];
				var lineTileControllers = DrawSquigglyLine(start, paint, canvas, Direction.Random(), 5);
				foreach (var tile in lineTileControllers) {
					DrawCircle(tile, paint, canvas, Random.Range(sizeMin, sizeMax), .33f);
				}
			}
		}

		/// <summary>
		///   <para>Uses pathfinding to ensure a connection between all players. If no path exists, then it will create one by force.</para>
		/// </summary>
		private void ClearPaths() {
			var pathfinder = new Pathfinder();
			var connections = new List<HashSet<TileController>>();
			var lastSpawn = _playerSpawns[0];
			for (int i = 1; i < _playerSpawns.Count; i++) {
				var connection = new HashSet<TileController>();
				connection.Add(lastSpawn);
				connection.Add(_playerSpawns[i]);
				lastSpawn = _playerSpawns[i];
				connections.Add(connection);
			}
			foreach (var connection in connections) {
				var tiles = new List<TileController>();
				foreach (var tile in connection) {
					tiles.Add(tile);
				}
				var path = pathfinder.GetPath(tiles[0], tiles[1], Tile.accessible);
				if (path.Count == 0) {
					path = pathfinder.GetPath(tiles[0], tiles[1]);
					foreach (var tileController in path) {
						if (!Tile.blocking.Contains(tileController.tile.name)) continue;
						var adjacentEmptyCount = 0;
						var adjacentForestCount = 0;
						foreach (var neighbor in tileController.GetNeighbors()) {
							if (neighbor.tile.name.Equals("empty")) adjacentEmptyCount++;
							if (neighbor.tile.name.Equals("forest")) adjacentForestCount++;
						}
						if (adjacentForestCount > adjacentEmptyCount) {
							TileController.Replace(tileController, _world.GetTileController("forest"));
						}
						else {
							TileController.Replace(tileController, _world.GetTileController("empty"));
						}
					}
				}

				/*TileController last = null;
				foreach (var tileController in path) {
					if (last != null) {
						Debug.DrawLine(last.transform.position, tileController.transform.position, Color.red, 10f, false);
					}
					last = tileController;
				}*/
			}
		}

		/// <summary>
		///   <para>Makes sure the given spawn area is fair and has access to all resources.</para>
		/// </summary>
		private void PrepareSpawnAreas() {
			var players = GameManager.instance.GetPlayers();
			for (var player = 0; player < players.Count; player++) {
				var directions = Direction.Spread (2);
				var forest = _playerSpawns[player].GetNeighbor(directions[0]);
				var empty = _playerSpawns[player].GetNeighbor(directions[1]);
				DrawCircle (forest, _world.GetTileController("forest"), Tile.natural, 1, 1);
				DrawCircle (empty, _world.GetTileController("empty"), Tile.natural, 1, 1);
				var woodcutter = TileController.Replace(_playerSpawns[player].GetNeighbor(directions[0]), _world.GetTileController("woodcutter"));
				woodcutter.SetOwner(players[player]);
				_playerSpawns[player].AddFacility(woodcutter);

				// START creating mountain
				var mountainDirection = Direction.Random();
				if (_playerSpawns[player].GetNeighbor(mountainDirection).tile.name.StartsWith("water")) {
					TileController.Replace(_playerSpawns[player].GetNeighbor(mountainDirection), _world.GetTileController("empty"));
				}
				if (_playerSpawns[player].GetNeighbor(mountainDirection).tile.name.StartsWith("water")) {
					TileController.Replace(_playerSpawns[player].GetNeighbor(mountainDirection).GetNeighbor(mountainDirection), _world.GetTileController("empty"));
				}
				TileController.Replace(_playerSpawns[player].GetNeighbor(mountainDirection).GetNeighbor(mountainDirection).GetNeighbor(mountainDirection), _world.GetTileController("mountain"));
				// END creating mountain

				var fieldCandidates = directions[1].Near();
				fieldCandidates.Add(directions[1]);
				var newFieldLocation = _playerSpawns[player].GetNeighbor(fieldCandidates[Random.Range(0, fieldCandidates.Count)]);
				var newField = TileController.Replace(newFieldLocation, _world.GetTileController("field"));
				newField.SetOwner(players[player]);
				_playerSpawns[player].AddFacility(newField);
				var settlement = TileController.GetAll("settlement")[player];
				if (settlement.tile.sound != null) {
					var sound = Instantiate(settlement.tile.sound);
					sound.transform.parent = settlement.transform;
					sound.transform.localPosition = Vector3.zero;
					sound.time = Random.Range(0, sound.clip.length);
				}
				settlement.SetOwner(players[player]);
				foreach (var areaTile in settlement.GetArea(settlement.tile.occupyRadius)) {
					areaTile.influencedBy.Add(settlement);
				}
				players[player].inventory.AddCapacity(settlement.tile.foodStorage, Inventory.Resource.Food);
				players[player].inventory.AddCapacity(settlement.tile.woodStorage, Inventory.Resource.Wood);
				players[player].inventory.AddCapacity(settlement.tile.ironStorage, Inventory.Resource.Iron);
				players[player].inventory.AddCapacity(settlement.tile.stoneStorage, Inventory.Resource.Stone);
				players[player].cameraPosition = settlement.transform.position + new Vector3(0f, 5f, -4f);
				settlement.UpdateRoadDecals();
			}
		}

		/// <summary>
		///   <para>Distributes sound sources of all sound-enabled tiles evenly across the world.</para>
		/// </summary>
		/// <param name="tileType">Tile type.</param>
		/// <param name="distance">Minimum distance between sound sources of the given tile type.</param>
		private void DistributeSoundSources(string tileType, int distance) {
			foreach (var tile in TileController.GetAll(tileType)) {
				if (tile.tile.sound == null) continue;
				var allowSound = true;
				var middleOfTheOcean = true;


				//Start harcode for only doing shore tiles if ocean
				if (tileType.Contains(tileType)) {
					var nearLand = false;
					foreach (var neighbor in tile.GetNeighbors()) {
						if (!neighbor.name.Equals(tileType)) {
							nearLand = true;
						}
					}
					if (!nearLand) continue;
				}
				//End hardcode


				foreach (var areaTile in tile.GetArea(distance)) {
					if (areaTile.GetComponentInChildren<AudioSource>() != null) {
						allowSound = false;
					}
					if (!Tile.water.Contains(areaTile.tile.name)) {
						middleOfTheOcean = false;
					}
				}
				if (allowSound && !middleOfTheOcean && tile.tile.sound != null) {
					var sound = Instantiate(tile.tile.sound);
					sound.transform.parent = tile.transform;
					sound.time = Random.Range(0, sound.clip.length);
					sound.transform.position = tile.transform.position;
				}
			}
		}

		/// <summary>
		///   <para>Draws a squiggly line into a specific direction.</para>
		/// </summary>
		/// <param name="start">Start tile.</param>
		/// <param name="paint">Tile to draw line with.</param>
		/// <param name="canvas">Tile types on which the line can be drawn.</param>
		/// <param name="direction">Direction of the line.</param>
		/// <param name="length">Length of the line.</param>
		List<TileController> DrawSquigglyLine(TileController start, TileController paint, List<string> canvas, Direction direction, int length) {
			List<TileController> tiles = new List<TileController>();
			Direction randomDirection;
			while (length > 0) {
				randomDirection = direction.Near()[Random.Range(0, direction.Near().Count)];
				var neighbor = start.GetNeighbor(randomDirection);
				if (neighbor == null) {
					tiles.Add(TileController.Create(paint, start.GetNeighborPositionAbsolute(randomDirection)));
					start = TileController.GetLast();
				}
				else if (canvas.Contains(neighbor.tile.name) && !tiles.Contains(neighbor)) {
					tiles.Add(TileController.Replace(neighbor, paint));
					start = TileController.GetLast();
				}
				else break;
				length--;
			}
			return tiles;
		} List<TileController> DrawSquigglyLine(TileController start, TileController paint, string canvas, Direction direction, int length) {
			return DrawSquigglyLine(start, paint, new List<string> {canvas}, direction, length);
		}

		/// <summary>
		///   <para>Replaces adjacent tiles.</para>
		/// </summary>
		/// <param name="tiles">Tiles to replace the neighbors of.</param>
		/// <param name="paint">Replacement.</param>
		/// <param name="canvas">Tile types which can be replaced.</param>
		/// <param name="density">Randomly skip tiles [0..1] (0 = skip all, 1 = skip none).</param>
		List<TileController> DrawNeighbors(List<TileController> tiles, TileController paint, List<string> canvas, float density) {
			List<TileController> newTiles = new List<TileController>();
			foreach (var tile in tiles) {
				foreach (var direction in Direction.List()) {
					var neighbor = tile.GetNeighbor(direction);
					if (neighbor == null) {
						newTiles.Add(TileController.Create(paint, tile.GetNeighborPositionAbsolute(direction)));
					}
					else if (neighbor != null && canvas.Contains(neighbor.tile.name) && Random.value <= density) {
						newTiles.Add(TileController.Replace(neighbor, paint));
					}
				}
			}
			return newTiles;
		} List<TileController> DrawNeighbors(List<TileController> tiles, TileController paint, string canvas, float density) {
			return DrawNeighbors(tiles, paint, new List<string> {canvas}, density);
		} List<TileController> DrawNeighbors(TileController tiles, TileController paint, List<string> canvas, float density) {
			return DrawNeighbors(new List<TileController> {tiles}, paint, canvas, density);
		} List<TileController> DrawNeighbors(TileController tiles, TileController paint, string canvas, float density) {
			return DrawNeighbors(new List<TileController> {tiles}, paint, canvas, density);
		} List<TileController> DrawNeighbors(TileController tiles, TileController paint, float density) {
			return DrawNeighbors(new List<TileController> {tiles}, paint, "", density);
		} List<TileController> DrawNeighbors(TileController tiles, TileController paint) {
			return DrawNeighbors(new List<TileController> {tiles}, paint, "", 1);
		}

		/// <summary>
		///   <para>Draws a, well, technically more of a hexagon, of the given radius.</para>
		/// </summary>
		/// <param name="center">Center tile.</param>
		/// <param name="paint">Tile to draw circle with.</param>
		/// <param name="canvas">Tile types on which the circle can be drawn.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="density">Randomly skip tiles [0..1] (0 = skip all, 1 = skip none).</param>
		List<TileController> DrawCircle(TileController center, TileController paint, List<string> canvas, int radius, float density) {
			List<TileController> tiles = new List<TileController>();
			List<TileController> newTiles = new List<TileController>();
			TileController.Replace (center, paint); //Replace center
			newTiles.Add(center);
			tiles.AddRange(newTiles);
			for (var i = 0; i < radius; i++) {
				newTiles = DrawNeighbors(newTiles, paint, canvas, density);
				tiles.AddRange(newTiles);
			}
			return tiles;
		} List<TileController> DrawCircle(TileController center, TileController paint, string canvas, int radius, float density) {
			return DrawCircle(center, paint, new List<string> {canvas}, radius, density);
		}

		/// <summary>
		///   <para>Draws a natural-looking land around a given origin tile.</para>
		/// </summary>
		/// <param name="origin">Origin tile.</param>
		/// <param name="paint">Tile to draw land with.</param>
		/// <param name="radius">Approximate radius.</param>
		/// <param name="breakProbability">Randomly stop drawing [0..1] (0 = never stop, 1 = stop immediately). This makes the land look more natural and can cause bays and peninsulas.</param>
		List<TileController> DrawLand(TileController origin, TileController paint, int radius, float breakProbability) {
			radius--;
			var tiles = new List<TileController>();
			if (Random.value <= breakProbability) {
				return tiles;
			}
			foreach (var direction in Direction.List()) {
				if (!origin.HasNeighbor(direction)) {
					tiles.Add(TileController.Create(paint, origin.GetNeighborPositionAbsolute(direction)));
				}
				else if (origin.GetNeighbor(direction).tile.name.Contains("water")) {
					tiles.Add(TileController.Replace(origin.GetNeighbor(direction), paint));
				}
			}
			if (radius > 0) {
				foreach (var tile in tiles) {
					DrawLand(tile, paint, radius, breakProbability);
				}
			}
			return tiles;
		}
	}
}

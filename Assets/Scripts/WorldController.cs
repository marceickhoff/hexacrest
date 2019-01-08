using UnityEngine;
using System.Collections.Generic;
using World;

public class WorldController : MonoBehaviour {
	public List<TileController> tiles = new List<TileController>();
	private Vector2 _bounds = Vector2.zero;

	public delegate void Ready();
	public static event Ready OnReady;

	/// <summary>
	///   <para>Starts the world generator if one exists.</para>
	/// </summary>
	void Start() {
		var generator = GetComponent<Generator>();
		if (generator != null) {
			generator.Generate();
		}
		_bounds = MeasureBounds();
		if (OnReady != null) OnReady();
	}

	private Vector2 MeasureBounds() {
		float boundX = 0;
		float boundZ = 0;
		foreach (var tile in TileController.GetAll()) {
			if (!tile.name.Equals("waterOceanDeep")) {
				var absX = Mathf.Abs(tile.transform.position.x);
				var absZ = Mathf.Abs(tile.transform.position.z);
				if (absX > boundX) {
					boundX = absX;
				}
				if (absZ > boundZ) {
					boundZ = absZ;
				}
			}
		}
		return _bounds = new Vector2(boundX, boundZ);
	}

	public Vector2 Bounds() {
		return _bounds;
	}

	/// <summary>
	///   <para>Returns the tile controller prototype for a specific tile type. Selects a random one if there are multiple tile controllers for the given tile type.</para>
	/// </summary>
	/// <param name="tileType">Tile type.</param>
	public TileController GetTileController(string tileType) {
		List<TileController> matches = new List<TileController>();
		foreach (var tileController in tiles) {
			if (tileController.tile.name.Equals(tileType)) {
				matches.Add(tileController);
			}
		}
		return matches[Random.Range(0, matches.Count)];
	}
}


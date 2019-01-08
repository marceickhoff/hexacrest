using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World {

	[CreateAssetMenu(fileName = "New Hexagon Tile", menuName = "Hexagon Tile")]
	public class Tile : ScriptableObject {
		public static readonly float diameter = 1;
		public static readonly float height = diameter * Mathf.Sin(60 * Mathf.PI / 180);

		public string title;
		[TextArea] public string description;

		[Header("Sounds")]
		[Tooltip("Ambient sound")] public AudioSource sound;
		[Tooltip("Will be played once when this tile is built")] public AudioClip buildSound;
		[Tooltip("Will be played once when this tile is destoryed")] public AudioClip destroySound;

		[Header("Parameters")]
		[Tooltip("Is this tile selectable?")] public bool selectable;
		[Tooltip("Can units be placed on this tile?")] public bool obstructed;
		[Tooltip("Does this tile connects to neighboring roads? This also means that this tile can be built on tiles with road")] public bool connectsToRoad;
		[Tooltip("Defines the radius around the tile which will be occupied by this tile's owner")] public int occupyRadius = 0;
		[Range(0,6)] public int maxNeighboringFacilites;

		[Header("Gameplay")]
		public int healthPoints;
		public int constructionScore;
		public int destructionScore;

		[Header("Storage capacity")]
		[Tooltip("Defines how much food storage this tile adds to the player's inventory")] public int foodStorage;
		[Tooltip("Defines how much wood storage this tile adds to the player's inventory")] public int woodStorage;
		[Tooltip("Defines how much iron storage this tile adds to the player's inventory")] public int ironStorage;
		[Tooltip("Defines how much stone storage this tile adds to the player's inventory")] public int stoneStorage;

		[Header("Production per round")]
		[Tooltip("Defines how much food this tile produces each round")] public int foodProduction;
		[Tooltip("Defines how much wood this tile produces each round")] public int woodProduction;
		[Tooltip("Defines how much iron this tile produces each round")] public int ironProduction;
		[Tooltip("Defines how much stone this tile produces each round")] public int stoneProduction;

		[Header("Consumption per round")]
		[Tooltip("Defines how much food this tile consumes each round")] public int foodConsumption;
		[Tooltip("Defines how much wood this tile consumes each round")] public int woodConsumption;
		[Tooltip("Defines how much iron this tile consumes each round")] public int ironConsumption;
		[Tooltip("Defines how much stone this tile consumes each round")] public int stoneConsumption;

		[Header("Building cost")]
		[Tooltip("")] public int buildingCostFood;
		[Tooltip("")] public int buildingCostWood;
		[Tooltip("")] public int buildingCostIron;
		[Tooltip("")] public int buildingCostStone;

		[Header("Building Restrictions")]
		public List<Tile> builingRestrictionsOn = new List<Tile>();
		public List<Tile> builindRestrictionsNextTo = new List<Tile>();


		public enum Type {
			natural, //Naturally generated tiles
			water, //Water tiles
			accessible, //Tiles that can be accessed by players
			structure //Tiles that can be built by players
		}
		[Header("Tile Types")]
		[Tooltip("Tile types this tile will be associated with")] public Type[] types;

		public bool Is(Type type) {
			return types.Contains(type);
		}



		public static readonly List<string> natural = new List<string> {
			"waterLake",
			"waterOcean",
			"waterOceanDeep",
			"empty",
			"forest",
			"mountain"
		};

		public static readonly List<string> water = new List<string> {
			"waterLake",
			"waterOcean",
			"waterOceanDeep"
		};

		public static readonly List<string> blocking = new List<string> {
			"waterLake",
			"waterOcean",
			"waterOceanDeep",
			"mountain"
		};

		public static readonly List<string> accessible = new List<string> {
			"empty",
			"forest",
			"settlement",
			"village",
			"city",
			"field"
		};

		public static readonly List<string> ownable = new List<string> {
			"settlement",
			"village",
			"city"
		};
	}
}
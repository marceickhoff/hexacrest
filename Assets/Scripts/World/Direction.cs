using System.Collections.Generic;
using UnityEngine;

namespace World {
	public struct Direction {
		private readonly int _id;
		public readonly Vector3 vector;

		public static readonly Direction north = new Direction(0, new Vector3(Tile.height, 0f, 0f));
		public static readonly Direction northEast = new Direction(1, new Vector3(Tile.height / 2, 0f, Tile.diameter * .75f));
		public static readonly Direction southEast = new Direction(2, new Vector3(Tile.height / -2, 0f, Tile.diameter * .75f));
		public static readonly Direction south = 	new Direction(3, new Vector3(Tile.height * -1, 0f, 0f));
		public static readonly Direction southWest = new Direction(4, new Vector3(Tile.height / -2, 0f, Tile.diameter * -.75f));
		public static readonly Direction northWest = new Direction(5, new Vector3(Tile.height / 2, 0f, Tile.diameter * -.75f));

		/// <summary>
		///   <para>Returns a list with all directions.</para>
		/// </summary>
		private static readonly List<Direction> _directionList = new List<Direction> {
			north, northEast, southEast, south, southWest, northWest
		};

		/// <summary>
		///   <para>Creates a new direction.</para>
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="targetVector">Target vector.</param>
		private Direction(int id, Vector3 targetVector) {
			_id = id;
			vector = targetVector;
		}

		/// <summary>
		///   <para>Returns the angle between two directions.</para>
		/// </summary>
		/// <param name="direction1"></param>
		/// <param name="direction2"></param>
		public static Quaternion Angle(Direction direction1, Direction direction2) {
			return Quaternion.Euler(0f, Vector3.Angle(direction1.vector, direction2.vector), 0f);
		}

		/// <summary>
		///   <para>Returns the rotation of the given direction relative to north.</para>
		/// </summary>
		/// <param name="direction"></param>
		public static Quaternion Angle(Direction direction) {
			return Angle(direction, north);
		}

		/// <summary>
		///   <para>Returns the opposite direction of a given direction.</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		public static Direction Opposite(Direction origin) {
			return _directionList[(origin._id + 3) % _directionList.Count];
		}

		/// <summary>
		///   <para>Returns the direction that is [offset] positions away from the origin (clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		/// <param name="offset">Offset.</param>
		public static Direction Offset(Direction origin, int offset) {
			return _directionList[(origin._id + offset % _directionList.Count + _directionList.Count) % _directionList.Count];
		}

		/// <summary>
		///   <para>Returns all directions that are between 1 and [size] or less positions away from the origin (clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		/// <param name="size">Range size.</param>
		public static List<Direction> Range(Direction origin, int size) {
			size = Mathf.Clamp(size, _directionList.Count * - 1, _directionList.Count);
			var directions = new List<Direction>();
			for (int i = 0; i < size; i++) {
				directions.Add(origin.Offset(i));
			}
			return directions;
		}

		/// <summary>
		///   <para>Returns a list containing the next [range] directions (clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		/// <param name="range">Range size.</param>
		public static List<Direction> Next(Direction origin, int range) {
			return origin.Range(range);
		}

		/// <summary>
		///   <para>Returns a list containing the previous [range] directions (counter-clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		/// <param name="range">Range size.</param>
		public static List<Direction> Prev(Direction origin, int range) {
			return origin.Range(range * -1);
		}

		/// <summary>
		///   <para>Returns the next direction (clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		public static Direction Next(Direction origin) {
			return origin.Offset(1);
		}

		/// <summary>
		///   <para>Returns the previous direction (counter-clockwise).</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		public static Direction Prev(Direction origin) {
			return origin.Offset(-1);
		}

		/// <summary>
		///   <para>Returns the 2 neighboring directions.</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		public static List<Direction> Near(Direction origin) {
			return new List<Direction>{
				origin.Prev(),
				origin.Next()
			};
		}

		/// <summary>
		///   <para>Returns a list containing the 2 neighboring directions and their neighbors.</para>
		/// </summary>
		/// <param name="origin">Origin direction.</param>
		public static List<Direction> Far(Direction origin) {
			var directions = new List<Direction>();
			directions.AddRange(origin.Range(-2));
			directions.AddRange(origin.Range(2));
			return directions;
		}

		/// <summary>
		///   <para>Returns the angle of this direction relative to north.</para>
		/// </summary>
		public Quaternion Angle() {
			return Quaternion.Euler(0f, _id * -60, 0f);
		}

		/// <summary>
		///   <para>Returns the opposite direction of this direction.</para>
		/// </summary>
		public Direction Opposite() {
			return Opposite(this);
		}

		/// <summary>
		///   <para>Returns the direction that is [offset] positions away from this direction (clockwise).</para>
		/// </summary>
		/// <param name="offset">Offset.</param>
		public Direction Offset(int offset) {
			return Offset(this, offset);
		}

		/// <summary>
		///   <para>Returns all directions that are between 1 and [size] or less positions away from ths direction (clockwise).</para>
		/// </summary>
		/// <param name="size">Range size.</param>
		public List<Direction> Range(int size) {
			return Range(this, size);
		}

		/// <summary>
		///   <para>Returns a list containing the next [range] directions (clockwise).</para>
		/// </summary>
		/// <param name="range">Range size.</param>
		public List<Direction> Next(int range) {
			return Next(this, range);
		}

		/// <summary>
		///   <para>Returns a list containing the previous [range] directions (counter-clockwise).</para>
		/// </summary>
		/// <param name="range">Range size.</param>
		public List<Direction> Prev(int range) {
			return Prev(this, range);
		}

		/// <summary>
		///   <para>Returns the next direction (clockwise).</para>
		/// </summary>
		public Direction Next() {
			return Next(this);
		}

		/// <summary>
		///   <para>Returns the previous direction (counter-clockwise).</para>
		/// </summary>
		public Direction Prev() {
			return Prev(this);
		}

		/// <summary>
		///   <para>Returns the 2 neighboring directions.</para>
		/// </summary>
		public List<Direction> Near() {
			return Near(this);
		}

		/// <summary>
		///   <para>Returns a list containing the 2 neighboring directions and their neighbors.</para>
		/// </summary>
		public List<Direction> Far() {
			return Far(this);
		}

		/// <summary>
		///   <para>Returns a list with a specified number of directions that are approximately equally distant from each other.</para>
		/// </summary>
		/// <param name="directionCount">Amount of directions to be spread [0..6].</param>
		/// <param name="origin">Origin direction (spread starts here).</param>
		public static List<Direction> Spread(int directionCount, Direction origin) {
			var directions = new List<Direction>();
			var remaining = _directionList.Count % directionCount;
			if (remaining != 0) {
				directionCount = _directionList.Count - directionCount;
				for (var i = 0; i < _directionList.Count; i++) {
					if (i % (_directionList.Count / directionCount) != 0) {
						directions.Add(_directionList[i].Offset(origin._id - 1));
					}
				}
			}
			else {
				for (var i = 0; i < _directionList.Count; i++) {
					if (i % (_directionList.Count / directionCount) == 0) {
						directions.Add(_directionList[i].Offset(origin._id));
					}
				}
			}
			return directions;
		}

		/// <summary>
		///   <para>Spreads a specified number of directions evenly around this direction.</para>
		/// </summary>
		/// <param name="directionCount">Amount of directions to be spread [0..6].</param>
		public static List<Direction> Spread(int directionCount) {
			return Spread(directionCount, Random());
		}

		/// <summary>
		///   <para>Returns a list with all directions.</para>
		/// </summary>
		public static List<Direction> List() {
			return _directionList;
		}

		/// <summary>
		///   <para>Returns a random direction.</para>
		/// </summary>
		public static Direction Random() {
			return _directionList[UnityEngine.Random.Range(0, _directionList.Count)];
		}
	}
}
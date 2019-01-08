using System.Collections.Generic;
using UnityEngine;

namespace World {
	public class Pathfinder {
		public List<TileController> GetPath(TileController origin, TileController target, List<string> allowedTiles) {
			var path = new List<TileController>();
			var frontier = new Queue<TileController>();
			frontier.Enqueue(origin);

			IDictionary<TileController, TileController> last = new Dictionary<TileController, TileController>();
			last[origin] = null;

			while (frontier.Count > 0) {
				var current = frontier.Dequeue();
				if (current == target) {
					while (!current.Equals(origin)) {
						path.Add(current);
						current = last[current];
					}
					path.Add(origin);
					break;
				}
				foreach (var next in current.GetNeighbors()) {
					if (allowedTiles.Count > 0 && !allowedTiles.Contains(next.name) || last.ContainsKey(next)) continue;
					frontier.Enqueue(next);
					last[next] = current;
				}
			}
			return path;
		}

		public List<TileController> GetPath(TileController origin, TileController target) {
			return GetPath(origin, target, new List<string>());
		}
	}
}
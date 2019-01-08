using System.Collections.Generic;
using UnityEngine;

namespace World {
	public abstract class Unit : MonoBehaviour {
		private int _size = 1;
		public int range = 1;
		private TileController _position;

		public int GetSize() {
			return _size;
		}

		public TileController GetPosition() {
			return _position;
		}

		public int Add(int amount = 1) {
			_size += amount;
			if (_size <= 0) {
				_size = 0;
				gameObject.SetActive(false);
			}
			return _size;
		}

		public int Remove(int amount = 1) {
			return Add(amount * -1);
		}

		public List<TileController> GetTilesInRange() {
			return new List<TileController>();
		}

		public bool Move(TileController target) {
			_position = target;
			if (GetTilesInRange().Contains(target)) {
				return true;
			}
			return false;
		}

		public bool IsAlive() {
			return gameObject.activeSelf;
		}

		public void Attack(Unit opponent) {
			while (IsAlive()) {
				/*if (is structure && is owned by enemy) {
					Remove();
					if (!IsAlive()) return;
					opponent.Remove();
				}
				else {
					opponent.Remove();
					if (!opponent.IsAlive()) return;
					Remove();
				}
				*/
			}
		}
	}
}
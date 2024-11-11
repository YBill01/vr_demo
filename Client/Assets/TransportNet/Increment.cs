using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Increment {

	private static uint _current;
	private static List<uint> _free = new List<uint>();
	private static List<uint> _bans = new List<uint>();

	public static void Reset() {
		_current = 0; _free.Clear(); _bans.Clear();
	}

	public static uint Value {
		get {
			if (_free.Count == 0) {

				while (true) {
					if (!_bans.Contains(_current)) {
						break;
					} else {
						_current++;
					}
				}
				_current++;
				return _current - 1;
			} else {
				return _free.GetLast(true);
			}
		}
		set {
			_bans.Remove(value);
			_free.AddWithoutDoubles(value);
		}
	}

	public static void AddBan(params uint[] values) {
		for (int i = 0; i < values.Length; i++) {
			_bans.AddWithoutDoubles(values[i]);
		}
	}

}

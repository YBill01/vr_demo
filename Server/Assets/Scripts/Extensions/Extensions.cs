using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System;

public static class Extensions {


	public static bool AddWithoutDoubles<T>(this List<T> to, T what) {
		if (!to.Contains(what)) {
			to.Add(what);
			return true;
		}
		return false;
	}

	public static bool IsOneOf<T>(this T _obj, params T[] _objects) {
		return _objects.Contains(_obj);
	}
	public static bool IsOneOf<T>(this T _obj, List<T> _objects) {
		return _objects.Contains(_obj);
	}
	public static bool IsOneOf<T>(this T[] _oneOfThis, params T[] _objects) {
		foreach (var obj in _oneOfThis) {
			if (_objects.Contains(obj)) {
				return true;
			}
		}
		return false;
	}

	public static bool Contains<T>(this List<T> _obj, params T[] _objects) {
		for (int i = 0; i < _objects.Length; i++) {
			if (_obj.Contains(_objects[i])) return true;
		}
		return false;
	}
	public static bool ContaitsWhere<T>(this List<T> lst, Predicate<T> filter) where T : class {
		return lst.Find(x => filter(x)) != null;
	}
	public static bool ContaitsWhere<T>(this List<T> lst, Predicate<T> filter, out T result) where T : class {
		result = lst.Find(x => filter(x));
		return result != null;
	}
	public static bool ContaitsWhere<T>(this T[] array, Predicate<T> filter) {
		foreach (var el in array) {
			if (filter(el)) {
				return true;
			}
		}
		return false;
	}
	public static bool ContaitsWhere<T>(this T[] array, Predicate<T> filter, out T result) {
		foreach (var el in array) {
			if (filter(el)) {
				result = el;
				return true;
			}
		}
		result = default;
		return false;
	}
	public static void MoveTo<T>(this List<T> _from, List<T> _to, bool _clearFrom = true) {
		_to.AddRange(_from);
		if (_clearFrom) {
			_from.Clear();
		}
	}
	public static T GetLast<T>(this List<T> _from, bool remove = false) {
		if (_from.Count <= 0) {
			return default;
		}
		T obj = _from[_from.Count - 1];
		if (remove) {
			_from.RemoveAt(_from.Count - 1);
		}
		return obj;
	}

	public static List<T> CastBox<T>(this BoxCollider castReferance, Predicate<T> filter) {
		List<T> components = new List<T>();
		Vector3 castSize = castReferance.size;
		Vector3 multiplyer = castReferance.transform.parent == null ? castReferance.transform.localScale : castReferance.transform.parent.localScale;
		Quaternion rotation = castReferance.transform.parent == null ? castReferance.transform.rotation : castReferance.transform.rotation * castReferance.transform.parent.rotation;
		castSize = new Vector3(castSize.x * multiplyer.x, castSize.y * multiplyer.y, castSize.z * multiplyer.z) / 2f;
		foreach (var hit in Physics.OverlapBox(castReferance.transform.TransformPoint(castReferance.center), castSize, rotation)) {
			if (hit.gameObject.TryGetComponent<T>(out var component) && filter(component)) {
				components.Add(component);
			}
		}
		return components.Distinct().ToList();
	}

	public static List<Transform> Transforms(this Transform parent) {
		List<Transform> result = new List<Transform>();
		FindIn(parent);

		void FindIn(Transform target) {
			result.Add(target);
			foreach (Transform t in target) {
				result.Add(t);
				if (t.childCount > 0) {
					FindIn(t);
				}
			}
		}
		return result;
	}

	public static void Disabe(this GraphicRaycaster raycaster) {
		raycaster.enabled = false;
	}

	public static void Enable(this GraphicRaycaster raycaster) {
		raycaster.enabled = true;
	}

	public static Vector3 Round(this Vector3 target, int r) {
		return new Vector3((float)Math.Round(target.x, r), (float)Math.Round(target.y, r), (float)Math.Round(target.z, r));
	}

	public static string Replace(this string s, char[] separators, string newVal) {
		string[] temp;

		temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
		return String.Join(newVal, temp);
	}

}

public static class NetworkExtensions {


	public static byte[] GetNetworkPosition(this Transform transform) {

		byte[] result = new byte[16];

		Array.Copy(BitConverter.GetBytes(transform.position.x), 0, result, 0, 4);
		Array.Copy(BitConverter.GetBytes(transform.position.y), 0, result, 4, 4);
		Array.Copy(BitConverter.GetBytes(transform.position.z), 0, result, 8, 4);
		Array.Copy(BitConverter.GetBytes(transform.eulerAngles.y), 0, result, 12, 4);

		return result;

	}

	public static void SetNetworkPosition(this Transform transform, byte[] netPosition, int startIndex = 0) {

		transform.position = new Vector3(
			BitConverter.ToSingle(netPosition, startIndex),
			BitConverter.ToSingle(netPosition, startIndex + 4),
			BitConverter.ToSingle(netPosition, startIndex + 8)
		);
		transform.eulerAngles = new Vector3(
			0,
			BitConverter.ToSingle(netPosition, startIndex + 12),
			0
		);

	}

	public static BitArray EcodeInput() {
		// TODO - normal input
		var array = new BitArray(8);
		array[0] = Input.GetKey(KeyCode.W) || Input.GetKeyDown(KeyCode.W);
		array[1] = Input.GetKey(KeyCode.A) || Input.GetKeyDown(KeyCode.A);
		array[2] = Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.S);
		array[3] = Input.GetKey(KeyCode.D) || Input.GetKeyDown(KeyCode.D);
		return array;
	
	}

	public static Vector3 DecodeInput(this BitArray array) {

		return new Vector3 {
			z = array[0] && !array[2] ?  1 : array[2] && !array[0] ? -1 : 0,
			x = array[1] && !array[3] ? -1 : array[3] && !array[1] ?  1 : 0,
			y = 0
		};

	}

	public static void DecodeToBytes(this Vector3 vector, ref byte[] array, int startWriteIndex) {

		Array.Copy(BitConverter.GetBytes(vector.x), 0, array, startWriteIndex, 4);
		Array.Copy(BitConverter.GetBytes(vector.y), 0, array, startWriteIndex + 4, 4);
		Array.Copy(BitConverter.GetBytes(vector.z), 0, array, startWriteIndex + 8, 4);

	}
	public static Vector3 EncodeFromBytes(this byte[] array, int startReadingIndex) {

		return new Vector3(
			BitConverter.ToSingle(array, startReadingIndex),
			BitConverter.ToSingle(array, startReadingIndex + 4),
			BitConverter.ToSingle(array, startReadingIndex + 8)
		);

	}

}
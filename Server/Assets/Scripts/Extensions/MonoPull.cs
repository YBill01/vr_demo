
using System.Collections.Generic;
using UnityEngine;

public class MonoPull<T> where T : MonoBehaviour {

	public MonoPull(T prefab, Transform spawnParent, bool useWorldSpace) {
		_prefab = prefab; _spawnParent = spawnParent; _useWorldSase = useWorldSpace;
		_prefab.gameObject.SetActive(false);
	}

	private T _prefab;
	private bool _useWorldSase;
	private Transform _spawnParent;

	private List<T> _pull = new List<T>();
	private List<T> _active = new List<T>();

	public void Clear(bool destroyAll = false) {
		if (destroyAll) {
			_pull.ForEach(x => UnityEngine.Object.Destroy(x.gameObject));
			_active.ForEach(x => UnityEngine.Object.Destroy(x.gameObject));
			_pull.Clear(); _active.Clear();
		} else {
			_active.ForEach(x => x.gameObject.SetActive(false));
			_active.MoveTo(_pull);
		}
	}
	public T GetItem(Transform newParent = null) {
		T obj = null;
		_pull.RemoveAll(x => x == null);
		if (_pull.Count > 0) {
			obj = _pull.GetLast(true);
		} else {
			obj = UnityEngine.Object.Instantiate(_prefab, _spawnParent, _useWorldSase);
		}
		_active.Add(obj);
		if (newParent != null) {
			obj.transform.SetParent(newParent);
		}
		obj.gameObject.SetActive(true);
		obj.transform.SetAsLastSibling();
		return obj;
	}
	public void ReturnItem(T item) {
		_active.Remove(item);
		_pull.AddWithoutDoubles(item);
		item.gameObject.SetActive(false);
	}
	public void ReturnItem(List<T> items) {
		foreach (var item in items) {
			if (item != null) {
				_active.Remove(item);
				_pull.AddWithoutDoubles(item);
				item.gameObject.SetActive(false);
			}
		}
	}
}


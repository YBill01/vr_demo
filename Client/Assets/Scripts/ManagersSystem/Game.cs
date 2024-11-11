using System;
using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	[SerializeField]
	private List<SceneManager> _sceneManagers = new List<SceneManager>();

	private static Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();

	private static List<IUpdate> _updatable = new List<IUpdate>();
	private static List<ILateUpdate> _lateUpdatable = new List<ILateUpdate>();
	private static List<IFixedUpdate> _fixedUpdatable = new List<IFixedUpdate>();

	private void Awake() {

		_managers.Clear();
		_updatable.Clear();
		_lateUpdatable.Clear();
		_fixedUpdatable.Clear();

		foreach (var m in _sceneManagers) {
			_managers[m.GetType()] = m;
		}

	}

	public static void OnInitialize() {
		foreach (var m in _managers.Values) {
			m.OnInitialize();
		}
	}

	public static void OnStart() {
		foreach (var m in _managers.Values) {
			m.OnStart();
		}
	}

	public static void OnFinalSetup() {
		foreach (var m in _managers.Values) {

			if (m is IUpdate upd) {
				_updatable.Add(upd);
			}
			if (m is ILateUpdate lateUpd) {
				_lateUpdatable.Add(lateUpd);
			}
			if (m is IFixedUpdate fixUpd) {
				_fixedUpdatable.Add(fixUpd);
			}

			m.Final();
		}
	}

	public static T GetManager<T>() where T : class, IManager {
		if (_managers.TryGetValue(typeof(T), out var value)) {
			return value as T;
		}
		return null;
	}

	public static T AddManager<T>(T manager) where T : class, IManager {
		_managers[typeof(T)] = manager; return manager;
	}

	#region Updating

	#region Subscribing

	public static void Updating_Subscribe(IUpdate obj) {
		if (!_updatable.Contains(obj)) {
			_updatable.Add(obj);
		}
	}
	public static void Updating_Subscribe(ILateUpdate obj) {
		if (!_lateUpdatable.Contains(obj)) {
			_lateUpdatable.Add(obj);
		}
	}
	public static void Updating_Subscribe(IFixedUpdate obj) {
		if (!_fixedUpdatable.Contains(obj)) {
			_fixedUpdatable.Add(obj);
		}
	}

	public static void Updating_Unsubscribe(IUpdate obj) {
		_updatable.Remove(obj);
	}
	public static void Updating_Unsubscribe(ILateUpdate obj) {
		_lateUpdatable.Remove(obj);
	}
	public static void Updating_Unsubscribe(IFixedUpdate obj) {
		_fixedUpdatable.Remove(obj);
	}

	#endregion

	private void Update() {
		if (_updatable.Count > 0) {
			for (int i = _updatable.Count-1; i >= 0; i--) {
				_updatable[i]?.Update(Time.unscaledDeltaTime);
			}
		}
	}
	private void LateUpdate() {
		if (_lateUpdatable.Count > 0) {
			for (int i = _lateUpdatable.Count - 1; i >= 0; i--) {
				_lateUpdatable[i]?.LateUpdate(Time.unscaledDeltaTime);
			}
		}
	}
	private void FixedUpdate() {
		if (_fixedUpdatable.Count > 0) {
			for (int i = _fixedUpdatable.Count - 1; i >= 0; i--) {
				_fixedUpdatable[i]?.FixedUpdate(Time.fixedDeltaTime);
			}
		}
	}

	#endregion

}

public abstract class Manager : IManager {
	public virtual void OnInitialize() { }
	public virtual void OnStart() { }
	public virtual void Final() { }
}

public abstract class SceneManager : MonoBehaviour, IManager {
	public virtual void OnInitialize() { }
	public virtual void OnStart() { }
	public virtual void Final() { }
}

public interface IManager {
	void OnInitialize();
	void OnStart();
	void Final();
}

public interface IUpdate {
	void Update(float timeDelta);
}

public interface ILateUpdate {
	void LateUpdate(float timeDelta);
}

public interface IFixedUpdate {
	void FixedUpdate(float timeDelta);
}

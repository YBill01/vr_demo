
using System.Collections;
using UnityEngine;
using System;

/// Более управляемые курутины
public class CoroutineBehavior : MonoBehaviour {

	private static CoroutineBehavior _instance;

	public static CoroutineWrapper StartCoroutine(IEnumerator coroutine, Action endCallabck = null) {
		if (_instance == null) {
			GameObject go = new GameObject("CoroutineCenter");
			_instance = go.AddComponent<CoroutineBehavior>();
			DontDestroyOnLoad(go);
		}
		var cor = new CoroutineWrapper(coroutine, _instance, endCallabck);
		cor.Start();
		return cor;
	}



	#region Regular coroutines

	public static CoroutineWrapper Delay(float time, Action callback, bool unscaledTime = false) {
		return StartCoroutine(IE_Delay(time, callback, unscaledTime));
	}
	public static CoroutineWrapper Wait(Func<bool> rule, Action callback) {
		return StartCoroutine(IE_Wait(rule, callback));
	}
	public static CoroutineWrapper While(Action func, YieldInstruction whileSpeed) {
		return StartCoroutine(IE_While(func, whileSpeed));
	}

	public static IEnumerator IE_While(Action func, YieldInstruction whileSpeed) {
		while (true) {
			yield return whileSpeed;
			func?.Invoke();
		}
	}

	public static IEnumerator IE_Delay(float time, Action callback, bool unscaledTime) {
		if (unscaledTime) {
			yield return new WaitForSecondsRealtime(time);
		} else {
			yield return new WaitForSeconds(time);
		}
		callback?.Invoke();
		yield break;
	}
	public static IEnumerator IE_Wait(Func<bool> rule, Action callback) {
		yield return new WaitWhile(rule);
		callback?.Invoke();
		yield break;
	}

	#endregion


}

public class CoroutineWrapper {

	private CoroutineWrapper() { }
	public CoroutineWrapper(IEnumerator coroutine, MonoBehaviour mono, Action endCallback = null) {
		_mono = mono;
		_coroutine = coroutine;
		_endCallback = endCallback;
	}

	private Action _endCallback;
	private MonoBehaviour _mono;
	private IEnumerator _coroutine;

	public bool IsPaused { get; private set; }
	public bool IsRunning { get; private set; }

	public void Start() {
		if (IsRunning) {
			return;
		}
		IsRunning = true;
		_mono.StartCoroutine(Wrapper());
	}
	public void Pause() {
		IsPaused = true;
	}
	public void Resume() {
		IsPaused = false;
	}
	public void Stop(bool disposeCallback = false) {
		if (disposeCallback) {
			_endCallback = null;
		}
		IsRunning = false;
	}
	private void End() {
		_endCallback?.Invoke();
	}

	private IEnumerator Wrapper() {
		IEnumerator i = _coroutine;
		yield return null;
		while (IsRunning) {
			if (IsPaused) {
				yield return null;
			} else {
				if (i != null && IsRunning && i.MoveNext()) {
					yield return i.Current;
				} else {
					IsRunning = false;
				}
			}
		}
		End();
	}
}






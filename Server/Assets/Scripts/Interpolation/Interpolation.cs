
using UnityEngine;
using Transport.Universal;
using System.Collections.Generic;

public interface IInterpolationPattern<T> where T : struct {
	T Interpolate(T current, float lerpAmount, Interpolation<T>.FrameUpdate to, Interpolation<T>.FrameUpdate from, Interpolation<T>.FrameUpdate previous);
}

public class Interpolation<T> where T : struct {

	public Interpolation(TickManager tickManager, IInterpolationPattern<T> interpolationPattern) {

		_tickManager = tickManager;
		_interpolationPattern = interpolationPattern;

		_to = new FrameUpdate(_tickManager.ServerTick, default);
		_from = new FrameUpdate(_tickManager.ServerTick, default);
		_previous = new FrameUpdate(_tickManager.ServerTick, default);

		_squareMovementThreshold = _movementThreshold * _movementThreshold;

	}

	public T CurrentValue { get; private set; } = default;
	private IInterpolationPattern<T> _interpolationPattern;

	private float _timeElapsed = 0f;
	private float _timeToReachTarget = 0.05f;
	private float _movementThreshold = 0.1f;

	private TickManager _tickManager;
	private readonly List<FrameUpdate> _futureUpdates = new List<FrameUpdate>();
	private float _squareMovementThreshold;

	private FrameUpdate _to;
	private FrameUpdate _from;
	private FrameUpdate _previous;

	public void AddTick(uint tick, T value) {

		if (tick <= _tickManager.InterpolationTick) {
			return;
		}

		for (int i = 0; i < _futureUpdates.Count; i++) {

			if (tick < _futureUpdates[i].Tick) {
				_futureUpdates.Insert(i, new FrameUpdate(tick, value));
				return;
			}

		}

		_futureUpdates.Add(new FrameUpdate(tick, value));

	}

	public Interpolation<T> CallUpdate() {

		for (int i = 0; i < _futureUpdates.Count; i++) {

			if (_tickManager.ServerTick >= _futureUpdates[i].Tick) {

				_previous = _to;
				_to = _futureUpdates[i];
				_from = new FrameUpdate(_tickManager.ServerTick, CurrentValue);

				_futureUpdates.RemoveAt(i);
				i--;

				_timeElapsed = 0f;
				_timeToReachTarget = (_to.Tick - _from.Tick) * Time.fixedDeltaTime;

			}

		}

		_timeElapsed += Time.deltaTime;
		CurrentValue = _interpolationPattern.Interpolate(CurrentValue, _timeElapsed / Mathf.Clamp(_timeToReachTarget, 0f, float.MaxValue), _to, _from, _previous);

		return this;

	}

	public struct FrameUpdate {

		public readonly uint Tick;
		public readonly T Value;

		public FrameUpdate(uint tick, T value) {

			Tick = tick; Value = value;

		}

	}

}
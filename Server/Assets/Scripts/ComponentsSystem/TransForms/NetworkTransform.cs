

using System.Collections;
using System.Collections.Generic;
using Transport.Messaging;
using UnityEngine;
using Qos = UnityEngine.Networking.QosType;

public class NetworkTransform : NetworkStaticTransform, IRuntimeSync {

	[SerializeField] private Transform _transForm;
	[SerializeField] private Space _space = Space.World;
	[SerializeField, Range(0f, 0.3f)] private float SyncDistance = 0.05f;
	[SerializeField, Range(0f, 5.0f)] private float SyncRotation = 1f;

	#region TransForm
	private void Awake() {
		_transForm = _transForm ?? transform;
	}
	private Vector3 position {
		get => _space == Space.World ? _transForm.position : _transForm.localPosition;
		set { if (_space == Space.World) { _transForm.position = value; } else { _transForm.localPosition = value; } }
	}
	private Vector3 eulerAngles {
		get => _space == Space.World ? _transForm.eulerAngles : _transForm.localEulerAngles;
		set { if (_space == Space.World) { _transForm.eulerAngles = value; } else { _transForm.localEulerAngles = value; } }
	}
	private Quaternion rotation {
		get => _space == Space.World ? _transForm.rotation : _transForm.localRotation;
		set { if (_space == Space.World) { _transForm.rotation = value; } else { _transForm.localRotation = value; } }
	}
	#endregion

	protected Vector3 _lastPosition;
	protected Quaternion _lastRotation;
	protected ushort _lastTick;

	protected byte _sTime = 0;
	protected SyncPart _syncType;

	public enum SyncPart {
		None = 0, Position = 1, Rotation = 2, Full = 3, Stabilization = 4
	}

	public bool HasSyncData {
		protected set { }
		get {
			if (_syncType == SyncPart.Stabilization && (_sTime += 1) >= 20) {
				return true;
			}
			return _syncType != SyncPart.None;
		}
	}
	
	public ushort DataLenght { get {
			return GetMsgDataLenght(_syncType);
		}
	}

	public Qos SyncChannel { get => Qos.Unreliable; }

	private ushort GetMsgDataLenght(SyncPart sync) {
		switch (sync) {
		case SyncPart.None: return 0; // Skip
		case SyncPart.Full: return 29; // Full sync
		case SyncPart.Position: return 17; // Half sync
		case SyncPart.Rotation: return 17; // Half sync
		case SyncPart.Stabilization: return 5; // Extrapolation tick
		}
		return 0;
	}

	private Vector3 _targetPosition = Vector3.zero;
	private Quaternion _targetRotation = Quaternion.identity;

	private float _time;
	private float _timeElapsed = 0f;
	private float _speed;
	private Vector3 _lstPos;
	private float _preferenceSpeed = 0f;

	protected virtual void FixedUpdate() {

		_speed = Mathf.Lerp(_speed, _preferenceSpeed, 0.7f);
		position = Vector3.MoveTowards(position, _targetPosition, _speed);
		rotation = Quaternion.Lerp(rotation, _targetRotation, 0.5f);

		if (Vector3.Distance(position, _lastPosition) >= SyncDistance) {
			_lastPosition = position;
			_syncType = _syncType.IsOneOf(SyncPart.None, SyncPart.Position, SyncPart.Stabilization) ? SyncPart.Position : SyncPart.Full;
		}
		if (Quaternion.Angle(rotation, _lastRotation) >= SyncRotation) {
			_lastRotation = rotation;
			_syncType = _syncType.IsOneOf(SyncPart.None, SyncPart.Rotation, SyncPart.Stabilization) ? SyncPart.Rotation : SyncPart.Full;
		}

	}

	public override void ApplySpawnData(MsgStreamReader reader, bool skipSunc = false) {
		if (skipSunc) { reader.Skip(24); return; }
		_lastPosition = _targetPosition = position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		_lastRotation = _targetRotation = rotation = Quaternion.Euler(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

	public void ApplySyncData(MsgStreamReader reader, bool skipSync = false) {

		if (skipSync) {
			// Where 3 - sync byte + readed component id
			reader.Skip(GetMsgDataLenght((SyncPart)reader.ReadByte()) - 3);
			return;
		}

		SyncPart sType = (SyncPart)reader.ReadByte();
		ushort tick = reader.ReadUInt16();

		switch (sType) {
		case SyncPart.Position:
			ReadPosition();
			break;
		case SyncPart.Rotation:
			ReadRotation();
			break;
		case SyncPart.Full:
			ReadPosition();
			ReadRotation();
			break;
		case SyncPart.Stabilization:
			Stabilizate();
			break;
		}

		_lastTick = tick;

		void Stabilizate() {
			
			Vector3 pos = _lstPos;
			_lastTick = tick;
			_targetPosition = pos;

		}

		void ReadPosition() {
			
			// Косяк - мы можем не успеть пройти эту дистанцию (TODO - старый коммент ПРОВЕРИТЬ!)
			Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

			if (_lastTick >= tick) { return; }

			_time = Time.fixedDeltaTime * (tick - _lastTick);
			_timeElapsed = 0f;
			_preferenceSpeed = (Vector3.Distance(_lstPos, pos) / _time) * Time.fixedDeltaTime;

			Vector3 v = ((pos - _lstPos).normalized * (Vector3.Distance(_lstPos, pos) * 5f)) * _time;
			//Debug.Log(v.ToString());
			_lstPos = pos;
			pos += v;
			
			_lastTick = tick;
			_targetPosition = pos;

		}

		void ReadRotation() {
			_targetRotation = Quaternion.Euler(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

	}

	public override void CreateSpawnData(ref MsgStreamWriter writer) {

		writer.WriteUInt16(ComponentId);
		writer.WriteSingle(position.x).WriteSingle(position.y).WriteSingle(position.z);
		writer.WriteSingle(eulerAngles.x).WriteSingle(eulerAngles.y).WriteSingle(eulerAngles.z);

	}
	public void CreateSyncData(ref MsgStreamWriter writer, bool writeAnyway = false) {

		if (!writeAnyway && !HasSyncData) { return; }

		writer.WriteUInt16(ComponentId);
		writer.WriteByte((byte)_syncType);

		writer.WriteUInt16(ServerTick.Tick); // TODO - normal ticks

		if (_syncType.IsOneOf(SyncPart.Full, SyncPart.Position)) {
			writer.WriteSingle(position.x).WriteSingle(position.y).WriteSingle(position.z);
		}
		if (_syncType.IsOneOf(SyncPart.Full, SyncPart.Rotation)) {
			writer.WriteSingle(eulerAngles.x).WriteSingle(eulerAngles.y).WriteSingle(eulerAngles.z);
		}
		_sTime = 0;
		_syncType = SyncPart.Stabilization;

	}

}



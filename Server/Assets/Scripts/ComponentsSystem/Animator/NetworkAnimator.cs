
using UnityEngine;
using Transport.Messaging;
using UnityEngine.Networking;
using System.Collections.Generic;
using PType = UnityEngine.AnimatorControllerParameterType;

[System.Serializable]
public class Parameter {
	[ReadOnly] public byte Id;
	[ReadOnly] public string Name;
	[ReadOnly] public PType Type;
}

public class NetworkAnimator : NetworkComponent, IRuntimeSync {

	[SerializeField] private Animator _animator;
	[SerializeField] private List<Parameter> _params;
	private byte _w = 0;
	private MsgStreamWriter _writer = new MsgStreamWriter();

#if UNITY_EDITOR
	[ContextMenu("CREATE STATE LIST")]
	private void CreateStates() {
		_params.Clear();
		for (byte i = 0; i < _animator.parameterCount; i++) {
			var p = new Parameter {
				Id = i,
				Name = _animator.parameters[i].name,
				Type = _animator.parameters[i].type
			};
			_params.Add(p);
		}
	}
#endif

	public QosType SyncChannel => QosType.Reliable;
	public bool HasSyncData { get; private set; } = false;

	public ushort DataLenght => HasSyncData ? (ushort)(3 + _writer.Lenght) : (ushort)0;

	// TODO - beheaviours
	public void ApplySyncData(MsgStreamReader reader, bool skipSync = false) {

		byte whiles = reader.ReadByte();
		for (byte w = 0; w < whiles; w++) {

			byte pId = reader.ReadByte();
			var info = _params.Find(x => x.Id == pId);

			switch (info.Type) {
			case PType.Trigger:
				if (skipSync) { reader.ReadByte(); break; }
				if (reader.ReadByte() == 1) {
					SetTrigger(info.Name);
				} else {
					ResetTrigger(info.Name);
				}
				break;
			case PType.Bool:
				if (skipSync) { reader.ReadByte(); break; }
				SetBool(info.Name, reader.ReadByte() == 1);
				break;
			case PType.Float:
				if (skipSync) { reader.ReadSingle(); break; }
				_animator?.SetFloat(info.Name, reader.ReadSingle());
				break;
			case PType.Int:
				if (skipSync) { reader.ReadInt32(); break; }
				_animator?.SetInteger(info.Name, reader.ReadInt32());
				break;
			}

		}

	}

	public void CreateSyncData(ref MsgStreamWriter writer, bool writeAnyway = false) {
		if (HasSyncData) {
			writer.WriteUInt16(ComponentId);
			writer.WriteByte(_w);
			writer.WriteSolidByteArray(_writer.Buffer);
			HasSyncData = false;
			_w = 0;
			_writer.Dispose();
		}
	}

#region Animation events

	// TODO - optimization
	public void SetTrigger(string name) {
		_animator?.SetTrigger(name);
		_w += 1;
		_writer.WriteByte(NameToId(name)).WriteByte((byte)1);
		HasSyncData = true;
	}

	public void ResetTrigger(string name) {
		_animator?.ResetTrigger(name);
		_w += 1;
		_writer.WriteByte(NameToId(name)).WriteByte((byte)0);
		HasSyncData = true;
	}

	public void SetBool(string name, bool value) {
		_animator?.SetBool(name, value);
		_w += 1;
		_writer.WriteByte(NameToId(name)).WriteByte((byte)(value ? 1 : 0));
		HasSyncData = true;
	}

	private byte NameToId(string name) {
		return _params.Find(x => x.Name == name).Id;
	}

#endregion

}
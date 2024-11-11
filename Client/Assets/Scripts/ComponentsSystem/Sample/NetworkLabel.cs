using UnityEngine;
using Transport.Messaging;
using Qos = UnityEngine.Networking.QosType;
using Writer = Transport.Messaging.MsgStreamWriter;

public class NetworkLabel : NetworkComponent, IRuntimeSync {

	[SerializeField] private TMPro.TMP_Text _label;
	public string Text => _label.text;

	public bool HasSyncData { get; protected set; } = false;
	public Qos SyncChannel { get => Qos.Reliable; }
	public override ushort SpawnDataLenght { get => (ushort)(2 + Writer.WriteStringLenght(_label.text)); }
	public ushort DataLenght { get => (ushort)(HasSyncData ? 2 + Writer.WriteStringLenght(_label.text) : 0); }

	public void ChangeLabel(string newValue) 
	{
		if (_label.text != newValue) {
			_label.text = newValue;
			HasSyncData = true;
		}
	}

	public override void ApplySpawnData(MsgStreamReader reader, bool skipSync = false) => ApplySyncData(reader, false);

	public override void CreateSpawnData(ref Writer writer)
	{
		writer.WriteUInt16(ComponentId);
		writer.WriteString(_label.text);
	}

	public void ApplySyncData(MsgStreamReader reader, bool skipSync = false) {
		var text = reader.ReadString();
		if (!skipSync) {
			_label.text = text;
		}
		HasSyncData = EnableSyncing;
#if SERVER
		HasSyncData = true;
#endif
	}

	public void CreateSyncData(ref Writer writer, bool writeAnyway = false) {
		if (writeAnyway || HasSyncData) {
			writer.WriteUInt16(ComponentId);
			writer.WriteString(_label.text);
			HasSyncData = false;
		}
	}

}
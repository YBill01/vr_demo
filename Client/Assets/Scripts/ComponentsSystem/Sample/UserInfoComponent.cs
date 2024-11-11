
using Transport.Messaging;
using Qos = UnityEngine.Networking.QosType;
using Writer = Transport.Messaging.MsgStreamWriter;

public class UserInfoComponent : NetworkComponent, IRuntimeSync {

	[ReadOnly] public uint MasterNetworkId;
	[ReadOnly] public string UserId;
	[ReadOnly] public string Username;

	public bool HasSyncData { get; protected set; } = false;
	public Qos SyncChannel { get => Qos.Reliable; }
	public override ushort SpawnDataLenght { get => (ushort)(6 + Writer.StringLenght(UserId) + Writer.StringLenght(Username)); }
	public ushort DataLenght { get => (ushort)(HasSyncData ? 6 + Writer.StringLenght(UserId) + Writer.StringLenght(Username) : 0); }

	public override void ApplySpawnData(MsgStreamReader reader, bool sipSync = false) => ApplySyncData(reader, sipSync);
	public void ApplySyncData(MsgStreamReader reader, bool skipSync = false) {
		// Do not skip this!
		MasterNetworkId = reader.ReadUInt32();
		UserId = reader.ReadString();
		Username = reader.ReadString();
#if SERVER
		HasSyncData = true;
#endif
	}
	public override void CreateSpawnData(ref Writer writer) {
		writer.WriteUInt16(ComponentId);
		writer.WriteUInt32(MasterNetworkId);
		writer.WriteString(UserId);
		writer.WriteString(Username);
	}
	public void CreateSyncData(ref Writer writer, bool writeAnyway = false) {
		if (writeAnyway || HasSyncData) {
			writer.WriteUInt16(ComponentId);
			writer.WriteUInt32(MasterNetworkId);
			writer.WriteString(UserId);
			writer.WriteString(Username);
			HasSyncData = false;
		}
		
	}

}
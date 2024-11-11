
using Transport.Messaging;
using UnityEngine;

public class NetworkStaticTransform : NetworkComponent {

	public override ushort SpawnDataLenght { get => 26; }

	public override void CreateSpawnData(ref MsgStreamWriter writer) {
		writer.WriteUInt16(ComponentId);
		writer.WriteSingle(transform.position.x).WriteSingle(transform.position.y).WriteSingle(transform.position.z);
		writer.WriteSingle(transform.eulerAngles.x).WriteSingle(transform.eulerAngles.y).WriteSingle(transform.eulerAngles.z);
	}

	public override void ApplySpawnData(MsgStreamReader reader, bool skipSunc = false) {
		if (skipSunc) { reader.Skip(24); return; }
		transform.position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		transform.rotation = Quaternion.Euler(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

}


using UnityEngine;
using Transport.Messaging;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkObject))]
public class VolPlayer : NetworkComponent, ISync {

	public QosType SyncChannel => QosType.Reliable;
	public ushort DataLenght { get; private set; } = 0;

	public override void CreateSpawnData(ref MsgStreamWriter writer) => CreateSyncData(ref writer);
	public override void ApplySpawnData(MsgStreamReader reader, bool skipSync = false) => ApplySyncData(reader, false);

	public void ApplySyncData(MsgStreamReader reader, bool skipSync = false) {
		if (skipSync) {
			reader.SkipString();
			return;
		}
		SetSequenceInfo(reader.ReadString());
	}

	public void CreateSyncData(ref MsgStreamWriter writer, bool writeAnyway = false) {
		writer.WriteUInt16(ComponentId);
		writer.WriteString(_playbackInfo);
	}

	[SerializeField]
	private AEstAR.VolumetricVideo.VVPlayer _4DsPlayer;
	private string _playbackInfo = null;

	public void SetSequenceInfo(string info) {

		if (string.IsNullOrEmpty(info)) {

			_playbackInfo = null;
			DataLenght = 0;
			_4DsPlayer.Stop();

		} else {

			_playbackInfo = info;
			DataLenght = (ushort)(2 + MsgStreamWriter.WriteStringLenght(info));

			_4DsPlayer.sequenceJSONInfo = _playbackInfo;
			_4DsPlayer.Init(_playbackInfo);
			_4DsPlayer.Play();
			Debug.Log("Start play vol video");

		}

	}

}


using UnityEngine;
using Transport.Messaging;
using Transport.Universal;

public class TextxChat {

	private HostInfo _host;

	public void Initialize() {
		Multihost.GetHost("BattleServer", out _host);
		Multihost.MsgReader.AddHandler(MsgType.ChatMessage, OnTextMsgReceived);
	}

	private void OnTextMsgReceived(NetworkMsg msg) {
		var line = msg.reader.ReadString();
		Debug.Log(string.Format("New chat msg: {0}", line));
	}

	public void SendTextMsg(string line) {
		using (var msg = new MsgStreamWriter(MsgType.ChatMessage).WriteString(line)) {
			Multihost.MsgSender.Send(msg.Buffer, _host.ConnId, _host.HostId, Multihost.ReliableChanel);
		}
	}

}

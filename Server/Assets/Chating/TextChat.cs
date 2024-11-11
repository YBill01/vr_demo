
using UnityEngine;
using Transport.Messaging;
using Transport.Universal;

public class TextChat {

	public void Initialize() {
        Multihost.MsgReader.AddHandler(MsgType.ChatMessage, OnTextMsgReceived);
    }

    private void OnTextMsgReceived(NetworkMsg msg) {
        var line = msg.reader.ReadString();
        SendTextMsg(line);
    }

	public void SendTextMsg(string line) {
        using (var msg = new MsgStreamWriter(MsgType.ChatMessage).WriteString(line)) {
            foreach (var conn in Multihost.ConnectionsManager.GetAllConnections.Values) {
                Multihost.MsgSender.Send(msg.Buffer, conn.ConnectId, conn.HostId, Multihost.ReliableChanel);
            }
        }
    }

}

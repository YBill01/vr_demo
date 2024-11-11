
using UnityEngine;
using Transport.Messaging;
using Transport.Universal;
using System.Collections.Generic;

public class ChatManager : Manager, IFixedUpdate {

	private short _messagesLimit = 5000;
	private Queue<ChatMessage> _chatMessages = new Queue<ChatMessage>();

	public override void Final() {
		base.Final();
		SubscribeListeners();
		Game.Updating_Subscribe(this);
	}

	private void SubscribeListeners() {
		Multihost.MsgReader.AddHandler(MsgType.SetClientId, SaveUserInfo);
		Multihost.MsgReader.AddHandler(MsgType.ChatMessage, OnChatMsgReceived);
	}
	// TODO - dispose
	private void UnsubscribeListeners() {
		Multihost.MsgReader.RemoveHandler(MsgType.SetClientId, SaveUserInfo);
		Multihost.MsgReader.RemoveHandler(MsgType.ChatMessage, OnChatMsgReceived);
	}
	private void SaveUserInfo(NetworkMsg msg) {
		try {
			uint masterNetId = msg.reader.ReadUInt32();
			string userId = msg.reader.ReadString();
			string username = msg.reader.ReadString();
			Multihost.UsersManager.SetUserInfo(masterNetId, userId, username);
			Debug.Log(string.Format("UserInfo has been changed: ID:{0} UID:{1} UsName:{2}", masterNetId, userId, username));
		} catch {
			Debug.Log("Receive wrong id format. [Master]");
		}
	}

	private void OnChatMsgReceived(NetworkMsg msg) {

		Debug.Log("New message: " + msg);
		var userNetId = msg.reader.ReadUInt32();

		if (Multihost.UsersManager.GetUser(userNetId, out var user)) {
			_chatMessages.Enqueue(new ChatMessage() { User = user, Message = msg.reader.ReadString() });
		} else {
			Debug.Log(string.Format("User: {0} Do not exist", userNetId));
		}

	}

	public void FixedUpdate(float timeDelta) {

		if (_chatMessages.Count > 100) {
			Debug.Log(string.Format("In chat Q more than 100 messages! Total messages in Q: {0}", _chatMessages.Count));
		} else if (_chatMessages.Count <= 0) {
			return;
		}

		short w = 0;
		_messagesLimit = (short)(12520 / Multihost.ConnectionsManager.ConnectionsCount);

		while (w++ < _messagesLimit) {

			if (_chatMessages.Count <= 0) {
				break;
			}

			var chatMsg = _chatMessages.Dequeue();

			using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.ChatMessage)) {

				writer.WriteString(chatMsg.User.Username);
				writer.WriteString(chatMsg.Message);
				byte[] msg = writer.Buffer;

				var channel = Multihost.ReliableChanel;

				foreach (var conn in Multihost.ConnectionsManager.GetAllConnections.Values) {
					Multihost.MsgSender.Send(msg, conn.ConnectId, conn.HostId, channel);
				}

			}

		}

	}

	private struct ChatMessage {
		public User User;
		public string Message;
	}

}
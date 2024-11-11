
using System;
using UnityEngine;
using Transport.Messaging;
using Transport.Universal;

public delegate void ChatMessage(string username, string message);

public class Chatting : Manager {

	private uint _masterNetworkId;
	private HostInfo _chattingHost;

	public bool IsConnected { get; private set; }

	public ChatMessage OnChatMsgReceived;

	public void ConnectToChatServer(Action onConnected, Action<string> onFailed) {

		if (IsConnected) {
			onConnected?.Invoke();
		}

		var client = Game.GetManager<BattleClientCore>();
		_masterNetworkId = client.MasterNetworkId;
		client.OpenConnection(OnConnectedToChatServer, onFailed, "ChattingServer");

		void OnConnectedToChatServer(HostInfo hostInfo) {

			IsConnected = true;

			_chattingHost = hostInfo;
			Multihost.OnHostConnectionClosed += OnHostDisconnected;

			using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.SetClientId)) {

				writer.WriteUInt32(client.MasterNetworkId);
				writer.WriteString(client.UserId);
				writer.WriteString(client.Username);

				Multihost.MsgSender.Send(writer.Buffer, _chattingHost.ConnId, _chattingHost.HostId, Multihost.ReliableChanel);

			}

			Multihost.MsgReader.AddHandler(MsgType.ChatMessage, OnNewChatMsgReceiver);
			onConnected?.Invoke();

		}

	}

	public void SendChatMsg(string msg) {
		using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.ChatMessage)) {
			writer.WriteUInt32(_masterNetworkId);
			writer.WriteString(msg);
			Multihost.MsgSender.Send(writer.Buffer, _chattingHost.ConnId, _chattingHost.HostId, Multihost.ReliableChanel);
		}
	}

	private void OnNewChatMsgReceiver(NetworkMsg msg) {
		Debug.Log("New chat msg received");
		OnChatMsgReceived?.Invoke(msg.reader.ReadString(), msg.reader.ReadString());
	}

	private void OnHostDisconnected(int connectId, int hostId, HostInfo host) {

		// TODO - restore connection
		if (host.HostName == "ChattingServer") {
			IsConnected = false;
			Multihost.MsgReader.RemoveHandler(MsgType.ChatMessage, OnNewChatMsgReceiver);
			Multihost.OnHostConnectionClosed -= OnHostDisconnected;
		}

	}
	
}

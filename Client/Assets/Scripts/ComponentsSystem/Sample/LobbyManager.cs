
using System.Collections;
using System.Collections.Generic;
using Transport.Messaging;
using Transport.Server;
using Transport.Universal;
using UnityEngine;

namespace Transport.Client {

	public class LobbyManager : Manager {

		private readonly BattleClientCore Client;

		public LobbyManager(BattleClientCore client) {
			Client = client;
			//DebugText.Instance.SetText("Join Start");
			JoinToLobby();
		}

		public void JoinToLobby() {

			if (Multihost.GetHost("BattleServer", out var hostInfo)) {

				using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.JoinToLobby)) {

					writer.WriteUInt32(Client.MasterNetworkId);
					writer.WriteString(Client.UserId);
					writer.WriteString(Client.Username);

					Multihost.MsgReader.AddOneShotHandler(MsgType.JoinToLobby, JoinToLobbyCallback);
					Multihost.MsgSender.Send(writer.Buffer, hostInfo.ConnId, hostInfo.HostId, Multihost.ReliableChanel);

				}

			} else {
				Debug.LogError("FIrst open connection with BattleServer");
			}

		}

		private void JoinToLobbyCallback(NetworkMsg msg) {

			if (msg.reader.ReadByte() == 0) {

				uint playerObjectNetworkId = msg.reader.ReadUInt32();
				// TODO - catch controll
				Debug.Log("Succses create player object on server side!");

				if (Game.GetManager<Client_NetworkObjects>().GetObject<PlayerNetworkObject>(playerObjectNetworkId, out var playerObj))
				{
					playerObj.ThisLocalPlayer = true;
					// TODO - sync update

				} else {
					Debug.LogError("Cant get player network Object!");
				}

			} else {
				Debug.LogError(string.Format("Join to lobby Fail: {0}", msg.reader.ReadString()));
			}

			msg.reader.Dispose();

		}

	}

}
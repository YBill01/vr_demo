
namespace Transport.Universal {

	using System.Collections;
	using System.Collections.Generic;
	using Transport.Messaging;
	using Transport.Server;
	using UnityEngine;

	public class LobbyManager : Manager {

		public LobbyManager() {

			Multihost.UsersManager.OnUserDestroyed += LeftFromLobby;
			Multihost.MsgReader.AddHandler(MsgType.JoinToLobby, JoinToLobby);

		}

		private Dictionary<User, NetworkObject> _players = new Dictionary<User, NetworkObject>();

		// TODO - save player in lobby list
		// TODO - track dissconnects
		private void JoinToLobby(NetworkMsg msg) {

			Debug.Log("Try join to lobby");

			if (Multihost.ConnectionsManager.GetConnection(msg.connectID, msg.hostId, out var connection) && Multihost.UsersManager.GetUser(connection.NetworkId, out var user) && !_players.ContainsKey(user)) 
			{
				var playerObj = Game.GetManager<Server_NetworkObjects>().InstantiateObject<PlayerNetworkObject>(0, out var onDoneSetup);
				playerObj.GetComponent<NetworkListener>().Connection = connection;
				var userInfo = playerObj.GetComponent<UserInfoComponent>();
				userInfo.MasterNetworkId = msg.reader.ReadUInt32();
				userInfo.UserId = msg.reader.ReadString();
				userInfo.Username = msg.reader.ReadString();
				playerObj.GetComponent<NetworkLabel>().ChangeLabel(userInfo.Username);
				// TODO - spawn points
				_players.Add(user, playerObj);

				onDoneSetup?.Invoke(playerObj);
				var writer = new MsgStreamWriter(MsgType.JoinToLobby).WriteByte(0).WriteUInt32(playerObj.NetworkId);
				Multihost.MsgSender.Send(writer.Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);


			} else {
				Multihost.MsgSender.Send(new MsgStreamWriter(MsgType.JoinToLobby).WriteByte(1).WriteString("Cant find connection on connections or player already exist").Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);
			}

		}

		private void LeftFromLobby(User user) {

			if (_players.ContainsKey(user)) {
				
				Game.GetManager<Server_NetworkObjects>().DestroyObject(_players[user].NetworkId);
				_players.Remove(user);

			}

		}

	}

}
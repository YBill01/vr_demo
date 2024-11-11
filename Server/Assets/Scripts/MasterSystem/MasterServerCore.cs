

using System.Collections;
using System.Collections.Generic;
using Transport.Messaging;
using Transport.Universal;
using UnityEngine;
using System.IO;

// TODO
// Dissconnect one of battle servers (clients)
// Reflected messages
// Client links

public class MasterServerCore : Manager {

	public IdManager IdManager { get; private set; } = null;

	private ServerConfigs m_Configs;
	private List<ServerStatus> _battleServers = new List<ServerStatus>();

	public MasterServerCore() {

		m_Configs = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "ServerConfig.txt")));
		m_Configs.IpAdress = m_Configs.UseLocalIpAdress ? "127.0.0.1" : ExternalIp.GetIpAdress();

		Multihost.TurnOn();
		Multihost.AddHost("Master_Main", HostType.Host, m_Configs.MainPort);
		Multihost.AddHost("Master_Web", HostType.WebHost, m_Configs.WebPort);
		CoroutineBehavior.StartCoroutine(SendMasterInfoToRestApi());
		SubscribeListeners();

		Debug.Log(string.Format("IP: {0} WebPort: {1} MainPort: {2}", m_Configs.IpAdress, m_Configs.WebPort, m_Configs.MainPort));

	}
	// TODO - RestAPI point
	private IEnumerator SendMasterInfoToRestApi() {
		while (true) {

			yield return new WaitForSeconds(5f);

		}
	}

	private void SubscribeListeners() {

		Multihost.MsgReader.AddHandler(MsgType.GetConnectInfo, GetConnectInfo);
		Multihost.MsgReader.AddHandler(MsgType.MasterIdRequest, MasterIdRequestCallback);
		Multihost.MsgReader.AddHandler(MsgType.SetConnectInfo, SetConnectInfo);

		Multihost.OnSomeoneDisconnected += OnConnectionLost;
		IdManager = new IdManager();
	}

	private void MasterIdRequestCallback(NetworkMsg msg)
	{
		Debug.Log("Message: " + msg);
	}



	private void OnConnectionLost(int connId, int hostId) {
		if (_battleServers.ContaitsWhere(x => x.Connect.ConnectId == connId && x.Connect.HostId == hostId, out var sStatus)) {

			_battleServers.Remove(sStatus);
			// TODO - server has clients?
		}
	}

	// Try get connection info to battle servers
	private void GetConnectInfo(NetworkMsg msg) {

		if (msg.reader.ReadString() == "Any") {

			var serverType = msg.reader.ReadString();
			var servers = _battleServers.FindAll(x => x.ServerType == serverType);

			if (servers.Count > 0) {

				float fill = 1.2f;
				ServerStatus targetConnection = null;

				for (short i = 0; i < servers.Count; i++) {

					float f = servers[i].CurrentConnections / servers[i].MaxConnections;

					if (f < fill) {
						fill = f;
						targetConnection = servers[i];
					}

					if (fill < 0.70f) {
						SendConnectInfo(targetConnection);
						break;
					}

				}

				if (targetConnection == null) {
					SendError(string.Format("All servers with this type: {0} is overloaded now", serverType));
				} else {
					SendConnectInfo(targetConnection);
				}

			} else {

				string error = string.Format("Cant find servers with this type: {0}", serverType);
				foreach (var serv in _battleServers) {
					error += " " + serv.ServerType;
				}

				//SendError(string.Format("Cant find servers with this type: {0}", serverType));
				SendError(error);
			}

		} else {

			var id = msg.reader.ReadUInt32();

			if (_battleServers.ContaitsWhere(x => x.ServerId == id, out var dirServ)) {

				if (dirServ.CurrentConnections < (dirServ.MaxConnections + dirServ.MaxOverload)) {
					SendConnectInfo(dirServ);
				} else {
					SendError(string.Format("Server with ID: {0} Is overload!", id));
				}

			} else {
				SendError(string.Format("Cant find server with ID: {0}", id));
			}

		}

		void SendConnectInfo(ServerStatus server) {

			MsgStreamWriter writer = new MsgStreamWriter(MsgType.GetConnectInfo);
			writer.WriteByte(0);

			ServerInfo servInf = new ServerInfo() {
				Ip = server.Ip, ServerType = server.ServerType,
				ServerId = server.ServerId, WebPort = server.WebPort,
				MainPort = server.MainPort
			};

			writer.WriteString(JsonUtility.ToJson(servInf));
			Multihost.MsgSender.Send(writer.Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);

		}

		void SendError(string reason) {

			MsgStreamWriter writer = new MsgStreamWriter(MsgType.GetConnectInfo);
			writer.WriteByte(1).WriteString(reason);
			Multihost.MsgSender.Send(writer.Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);

		}

	}
	// Servers set info about self
	private void SetConnectInfo(NetworkMsg msg) {

		var id = msg.reader.ReadUInt32();

		if (_battleServers.ContaitsWhere(x => x.ServerId == id, out var server)) {

			server.ServerType = msg.reader.ReadString();
			msg.reader.ReadString();
			msg.reader.ReadInt32();
			msg.reader.ReadInt32();
			server.MaxConnections = msg.reader.ReadUInt16();
			server.CurrentConnections = msg.reader.ReadUInt16();
			server.MaxOverload = msg.reader.ReadUInt16();

		} else {
			
			Multihost.ConnectionsManager.GetConnection(msg.connectID, msg.hostId, out var connection);
			var serverStatus = new ServerStatus() {
				ServerId = id,
				ServerType = msg.reader.ReadString(),
				Ip = msg.reader.ReadString(),
				WebPort = msg.reader.ReadInt32(),
				MainPort = msg.reader.ReadInt32(),
				MaxConnections = msg.reader.ReadUInt16(),
				CurrentConnections = msg.reader.ReadUInt16(),
				MaxOverload = msg.reader.ReadUInt16(),
				Connect = connection,
				Clients = new List<ClientInfo>(),
			};

			_battleServers.Add(serverStatus);

			Debug.Log("Find new server! ServerType: " + serverStatus.ServerType);
			Debug.Log(string.Format("IP: {0} WebPort: {1} MainPort: {2} Type: {3}", serverStatus.Ip, serverStatus.WebPort, serverStatus.MainPort, serverStatus.ServerType));
			Debug.Log(string.Format("MaxConnections: {0} MaxOverload: {1}", serverStatus.MaxConnections, serverStatus.MaxOverload));

		}

	}

	// Create users online list
	private void OnClientJoinToOtherServer(NetworkMsg msg) {

		Debug.Log("Client join to other server");

		ClientInfo client = new ClientInfo() {
			NetworkId = msg.reader.ReadUInt32(),
			UserId = msg.reader.ReadString(),
			Username = msg.reader.ReadString(),
			ServerId = msg.reader.ReadUInt32()
		};

		if (_battleServers.ContaitsWhere(x => x.ServerId == client.ServerId, out var server)) {
			if (!server.Clients.ContaitsWhere(x => x.NetworkId == client.NetworkId)) {
				server.Clients.Add(client);
			} else {
				Debug.Log(string.Format("Server allready contains User with id: {0}", client.NetworkId));
			}
		} else {
			Debug.Log(string.Format("Cant find server with id: {0}", client.ServerId));
		}

	}

	private void OnClientLeftFromOtherServer(NetworkMsg msg) {

		Debug.Log("Client left from other server");
		uint serverId = msg.reader.ReadUInt32();
		uint clientNetId = msg.reader.ReadUInt32();

		if (_battleServers.ContaitsWhere(x => x.ServerId == serverId, out var server)) {
			if (server.Clients.ContaitsWhere(x => x.NetworkId == clientNetId, out var client)) {
				server.Clients.Remove(client);
				Debug.Log(string.Format("Client remove from server list. Client: {0} Server: {1}", clientNetId, serverId));
			}
		}

	}

	private class ServerStatus : ServerInfo {

		public ushort MaxConnections;
		public ushort CurrentConnections;
		public ushort MaxOverload;

		public Connection Connect;
		public List<ClientInfo> Clients = new List<ClientInfo>();

	}

}

// TODO - track users dissconnect and return id to pull!
public class IdManager {

	private Dictionary<string, uint> _usernameToNetId = new Dictionary<string, uint>();
	private Dictionary<uint, string> _netIdToUsername = new Dictionary<uint, string>();

	public IdManager() {
		Debug.Log("IDManager");
		_usernameToNetId = new Dictionary<string, uint>();
		_netIdToUsername = new Dictionary<uint, string>();

		Multihost.MsgReader.AddHandler(MsgType.MasterIdRequest, UserMasterIdRequest);

	}

	public bool TryGetUserId(uint networkId, out string username) {
		return _netIdToUsername.TryGetValue(networkId, out username);
	}
	
	public bool TryGetNetworkId(string username, out uint networkId) {
		return _usernameToNetId.TryGetValue(username, out networkId);
	}

	private void UserMasterIdRequest(NetworkMsg msg) {
		using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.MasterIdRequest)) {
			Debug.Log("Message" + msg);
			writer.WriteUInt32(GetUserNetworkId(msg.reader.ReadString()));
			Multihost.MsgSender.Send(writer.Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);
		}
	}

	public uint GetUserNetworkId(string username) {
		if (_usernameToNetId.TryGetValue(username, out var networkId)) {
			return networkId;
		} else {
			uint netId = Increment.Value;
			_usernameToNetId.Add(username, netId);
			_netIdToUsername.Add(netId, username);
			return netId;
		}
	}

}


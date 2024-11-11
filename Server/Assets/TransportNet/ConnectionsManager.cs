

namespace Transport.Universal {

	using System;
	using UnityEngine;
	using System.Linq;
	using System.Collections.Generic;
	using Transport.Messaging;

	public delegate void OnConnectionChanged(Connection connection);

	public class ConnectionsManager {

		public ConnectionsManager() {
			Multihost.OnHostCenterTurnOn += TurnOn;
		}

		public int ConnectionsCount => _linkedConnections.Count;

		public OnConnectionChanged OnConnectionCreated;
		public OnConnectionChanged OnConnectionDestroyed;

		//private readonly Server _server;
		private Dictionary<int, Dictionary<int, Connection>> _connections = new Dictionary<int, Dictionary<int, Connection>>();
		private Dictionary<uint, Connection> _linkedConnections = new Dictionary<uint, Connection>();

		public Dictionary<uint, Connection> GetAllConnections => _linkedConnections;

		public bool GetConnection(int connectID, int hostId, out Connection connection) {
			if (!_connections.ContainsKey(hostId)) {
				_connections.Add(hostId, new Dictionary<int, Connection>());
			}
			return _connections[hostId].TryGetValue(connectID, out connection);
		}

		public bool GetConnection(uint networkId, out Connection connection) {
			return _linkedConnections.TryGetValue(networkId, out connection);
		}

		public bool GetConnectionByMasterId(uint networkId, out Connection connection) {
			var conn = _linkedConnections.Values.FirstOrDefault(x => x.MasterNetworkId == networkId);
			connection = conn;
			return conn != default;
		}

		private void ChangeUserInfo(NetworkMsg msg) {
			uint masterNetId = msg.reader.ReadUInt32();
			Multihost.UsersManager.SetUserInfo(masterNetId, msg.reader.ReadString(), msg.reader.ReadString());
			_connections[msg.hostId][msg.connectID].ChangeMasterNetId(masterNetId);
			Debug.Log("User info is changed");
		}

		private void OnNewUserConnected(int connectID, int hostId) {

			var newConnection = new Connection(connectID, hostId, Increment.Value);

			if (!_connections.ContainsKey(hostId)) {
				_connections.Add(hostId, new Dictionary<int, Connection>());
			}

			if (!_connections[hostId].ContainsKey(connectID) && !_linkedConnections.ContainsKey(newConnection.NetworkId)) {
				_connections[hostId].TryAdd(connectID, newConnection); _linkedConnections.TryAdd(newConnection.NetworkId, newConnection);
				Multihost.MsgSender.Send(MsgType.SetClientId, BitConverter.GetBytes(newConnection.NetworkId), connectID, hostId, Multihost.ReliableChanel);
				OnConnectionCreated?.Invoke(newConnection);
			}

			DebugConnectionsCount();

		}

		private void OnUserDisconnected(int connectID, int hostId) {

			if (!_connections.ContainsKey(hostId)) {
				_connections.Add(hostId, new Dictionary<int, Connection>());
			}
			if (_connections[hostId].TryGetValue(connectID, out var client)) {
				_connections[hostId].Remove(connectID);
				_linkedConnections.Remove(client.NetworkId);
				Increment.Value = client.NetworkId;
				OnConnectionDestroyed?.Invoke(client);
			}
			
			DebugConnectionsCount();

		}

		private void DebugConnectionsCount() {
			Debug.Log(string.Format("Users: {0}", ConnectionsCount));
		}

		private void TurnOn() {

			Multihost.OnHostCenterTurnOn -= TurnOn;
			Multihost.OnHostCenterTurnOff += ShutDown;

			_connections = new Dictionary<int, Dictionary<int, Connection>>() { };
			_linkedConnections = new Dictionary<uint, Connection>(Multihost.MaxConnections);

			Multihost.OnSomeoneConnected += OnNewUserConnected;
			Multihost.OnSomeoneDisconnected += OnUserDisconnected;
			Multihost.MsgReader.AddHandler(MsgType.SetClientInfo, ChangeUserInfo);

		}

		private void ShutDown() {

			Multihost.OnHostCenterTurnOn += TurnOn;
			Multihost.OnHostCenterTurnOff -= ShutDown;
			Multihost.OnSomeoneConnected -= OnNewUserConnected;
			Multihost.OnSomeoneDisconnected -= OnUserDisconnected;
			Multihost.MsgReader.RemoveHandler(MsgType.SetClientInfo, ChangeUserInfo);

		}

		public void SendToAll(short msgType, byte[] data, int chanelId) {

			foreach (var conn in _linkedConnections.Values) {
				if (conn != null) {
					Multihost.MsgSender.Send(msgType, data, conn.ConnectId, conn.HostId, chanelId);
				}
			}

		}

	}

	public class Connection {

		public readonly int HostId;
		public readonly int ConnectId;
		public readonly uint NetworkId;
		public uint MasterNetworkId { get; private set; }

		public Connection(int connectId, int hostId, uint networkId) {
			ConnectId = connectId; HostId = hostId; NetworkId = networkId;
		}
		public void ChangeMasterNetId(uint netId) {
			MasterNetworkId = netId;
		}

	}

}

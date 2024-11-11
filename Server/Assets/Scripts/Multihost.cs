
#pragma warning disable
namespace Transport.Universal {

	using UnityEngine;
	using UnityEngine.Networking;
	using System.Collections.Generic;
	using Transport.Messaging;
	using Transport.Messaging;
	using System.Threading;
	using Unity.Collections;

	public delegate void ServerAction();
	public delegate void ConnectionAction(int connectId, int hostId);
	public delegate void HostConnectionAction(int connectId, int hostId, HostInfo host);

	public enum HostType {
		Host = 0, WebHost = 1 
	}

	public struct HostInfo {
		public int HostId;
		public string HostName;
		public bool IsServerHost;
		public int ConnId;
		public HostInfo(int id, bool isServer, string name, int connId = int.MaxValue) {
			HostId = id; IsServerHost = isServer; HostName = name; ConnId = connId;
		}
	}

	public class MultihostUpdate : MonoBehaviour {
		private void FixedUpdate() {
			Multihost.FixedUpdate(Time.fixedDeltaTime);
		}
	}

	[DefaultExecutionOrder(300)]
	public static class Multihost {

		static Multihost() {

			ConnectionsManager = new ConnectionsManager();
			UsersManager = new UsersManager(ConnectionsManager);
			MsgReader = new MsgReader();
			MsgSender = new MsgSender();

			var updater = new GameObject("MultihostUpdate", typeof(MultihostUpdate));
			UnityEngine.Object.DontDestroyOnLoad(updater);

		}

		public static readonly int MaxConnections = 350;

		public static int ReliableChanel { get; private set; }
		public static int UnreliableChanel { get; private set; }
		public static int RelaibleFragmentedChanel { get; private set; }
		public static int UnrelaibleFragmentedChanel { get; private set; }

		public static int GetChanel(QosType qos) {

			switch (qos) {
			case QosType.Reliable: return ReliableChanel;
			case QosType.Unreliable: return UnreliableChanel;
			case QosType.ReliableFragmented: return RelaibleFragmentedChanel;
			case QosType.UnreliableFragmented: return UnrelaibleFragmentedChanel;
			}

			return 0;
		}

		public static bool IsInitialized { get; private set; }

		public static readonly ConnectionsManager ConnectionsManager;
		public static readonly UsersManager UsersManager;
		public static readonly MsgReader MsgReader;
		public static readonly MsgSender MsgSender;

		private static byte _error;

		#region Handlers

		public static ServerAction OnHostCenterTurnOn;
		public static ServerAction OnHostCenterTurnOff;

		public static ConnectionAction OnConnectionOpen;
		public static ConnectionAction OnConnectionClosed;

		public static HostConnectionAction OnHostConnectionOpen;
		public static HostConnectionAction OnHostConnectionClosed;

		public static ConnectionAction OnSomeoneConnected;
		public static ConnectionAction OnSomeoneDisconnected;

		#endregion

		#region Server Off\On

		private static GlobalConfig _globalConfig;
		private static ConnectionConfig _connConfig;
		private static HostTopology _hostTopology;
		private static Dictionary<string, HostInfo> _hosts = new Dictionary<string, HostInfo>();
		private static Dictionary<int, HostInfo> _linkedHosts = new Dictionary<int, HostInfo>();

		public static void TurnOn() {

			if (IsInitialized) {
				return;
			}

			_globalConfig = new GlobalConfig();
			_globalConfig.ReactorMaximumReceivedMessages = 2048;
			_globalConfig.ReactorMaximumSentMessages = 4096;
			_globalConfig.ThreadPoolSize = 6;

			NetworkTransport.Init(_globalConfig);
			_connConfig = new ConnectionConfig();

			//_connConfig.InitialBandwidth = 720000000;
			_connConfig.AcksType = ConnectionAcksType.Acks128;

			_connConfig.MaxCombinedReliableMessageSize = 200;
			_connConfig.MaxSentMessageQueueSize = 2048;
			//_connConfig.PacketSize = 1470;

			ReliableChanel = _connConfig.AddChannel(QosType.Reliable);
			UnreliableChanel = _connConfig.AddChannel(QosType.Unreliable);
			RelaibleFragmentedChanel = _connConfig.AddChannel(QosType.ReliableFragmented);
			UnrelaibleFragmentedChanel = _connConfig.AddChannel(QosType.UnreliableFragmented);


			_hostTopology = new HostTopology(_connConfig, MaxConnections);
			IsInitialized = true;
			OnHostCenterTurnOn?.Invoke();

		}

		public static void TurnOff(bool restart = false) {

			if (!IsInitialized) {
				return;
			}

			IsInitialized = false;
			OnHostCenterTurnOff?.Invoke();
			_hosts.Clear();
			_linkedHosts.Clear();
			NetworkTransport.Shutdown();
			if (restart) { TurnOn(); }

		}

		public static bool GetHost(string hostName, out HostInfo host) {
			return _hosts.TryGetValue(hostName, out host);
		}

		public static void RemoveHost(string hostName) {
			if (_hosts.ContainsKey(hostName)) {
				NetworkTransport.RemoveHost(_hosts[hostName].HostId);
				_linkedHosts.Remove(_hosts[hostName].HostId);
				_hosts.Remove(hostName);
			}
		}

		public static void AddHost(string hostName, HostType host, int port) {

			if (!IsInitialized) {
				TurnOn();
			}
			if (_hosts.ContainsKey(hostName)) {
				throw new System.NotImplementedException(string.Format("Host: {0} allready exist", hostName));
			}
				
			_hosts.Add(hostName, new HostInfo(host == HostType.Host ?
				NetworkTransport.AddHost(_hostTopology, port, null) :
				NetworkTransport.AddWebsocketHost(_hostTopology, port, null), true, hostName));
			_linkedHosts.Add(_hosts[hostName].HostId, _hosts[hostName]);

		}

		public static void AddHost(string hostName, string ip, int port) {

			if (!IsInitialized) {
				TurnOn();
			}
			if (_hosts.ContainsKey(hostName)) {
				throw new System.NotImplementedException(string.Format("Host: {0} allready exist", hostName));
			}

			int hostId = NetworkTransport.AddHost(_hostTopology, 0);
			_hosts.Add(hostName, new HostInfo(hostId, false, hostName, NetworkTransport.Connect(hostId, ip, port, 0, out _error)));
			_linkedHosts.Add(_hosts[hostName].HostId, _hosts[hostName]);

		}

		#endregion

		#region HostListen region

		private static NetworkEventType _networkEvent;
		private static byte[] bufer = new byte[32000]; // 500*64

		public static void FixedUpdate(float timeDelta) {

			if (IsInitialized && _hosts.Count > 0) {
				
				int messagesProcessed = 0;
				do {

					ResivedMsg res = new ResivedMsg();
					//_networkEvent = NetworkTransport.Receive(out res.recHostId, out res.connectionId, out res.channelId, res.recBuffer, res.bufferSize, out res.dataSize, out res.error);
					_networkEvent = NetworkTransport.Receive(out res.recHostId, out res.connectionId, out res.channelId, bufer, 32000, out res.dataSize, out res.error);

					switch (_networkEvent) {
					case NetworkEventType.ConnectEvent:

						if (_linkedHosts[res.recHostId].IsServerHost) {
							Debug.Log(string.Format("<color=green>New_Connect Host: {0} ID: {1}</color>", res.recHostId, res.connectionId));
							OnSomeoneConnected?.Invoke(res.connectionId, res.recHostId);
						} else {
							Debug.Log(string.Format("<color=green>New_Connect TO other Host: {0} ID: {1}</color>", res.recHostId, res.connectionId));
							OnConnectionOpen?.Invoke(res.connectionId, res.recHostId);
							OnHostConnectionOpen?.Invoke(res.connectionId, res.recHostId, _linkedHosts[res.recHostId]);
						}

						break;
					case NetworkEventType.DataEvent:
						
						//MsgReader.Read(res);
						MsgReader.Read(res, bufer);

						break;
					case NetworkEventType.DisconnectEvent:

						if (_linkedHosts[res.recHostId].IsServerHost) {
							Debug.Log(string.Format("<color=yellow>Dis_Connect Host: {0} ID: {1}</color>", res.recHostId, res.connectionId));
							OnSomeoneDisconnected?.Invoke(res.connectionId, res.recHostId);
						} else {
							Debug.Log(string.Format("<color=yellow>Dis_Connect FROM ther Host: {0} ID: {1}</color>", res.recHostId, res.connectionId));
							var host = _linkedHosts[res.recHostId];
							RemoveHost(_linkedHosts[res.recHostId].HostName);
							OnConnectionClosed?.Invoke(res.connectionId, res.recHostId);
							OnHostConnectionClosed?.Invoke(res.connectionId, res.recHostId, host);
						}

						break;

					}

					messagesProcessed++;

				} while (_networkEvent != NetworkEventType.Nothing && messagesProcessed < 2048);

			}

		}

#endregion

	}

}
#pragma warning restore
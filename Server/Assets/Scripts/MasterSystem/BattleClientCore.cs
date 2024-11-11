using System;
using System.IO;
using Transport.Messaging;
using Transport.Universal;
using UnityEngine;

public class BattleClientCore : Manager {

	public uint NetworkId { get; private set; }
	public string UserId { get; private set; }
	public string Username { get; private set; }
	public uint ServerId { get; private set; }
	public ClientInfo UserInfo => new ClientInfo() {
		NetworkId = NetworkId, UserId = UserId,
		Username = Username, ServerId = ServerId
	};

	private ServerInfo _masterServerInfo = null;
	private ServerInfo _battleServerInfo = null;
	public HostInfo BattleServer { get; private set; }
	public int ConnId => BattleServer.ConnId;
	public int HostId => BattleServer.HostId;

	public void OnUserLogIn(string username, string userId = "Guest") {
		Username = username; UserId = userId;
	}

	#region Server connection
	// TODO - RestAPI point
	private void GetMasterConnection(Action<ServerInfo> onGetMasterConnInfo, Action<string> onConnCreateFailed) {

		var master = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "MasterConfig.txt")));

		var MasterServerInfo = new ServerInfo() {
			Ip = master.IpAdress, MainPort = master.MainPort, WebPort = master.WebPort,
			ServerId = 0, ServerType = master.ServerType
		};

		onGetMasterConnInfo?.Invoke(MasterServerInfo);

	}

	public void ConnectToServer(Action onConneced, Action<string> error) {

		GetMasterConnection(OnReceiveMasterConnConfig, Error);

		void OnReceiveMasterConnConfig(ServerInfo masterInfo) {
			_masterServerInfo = masterInfo;
			var connectionProcess = new GetBattleConnection("BattleServer", _masterServerInfo, OnReceiveBattleConnConfig, Error);
		}

		void OnReceiveBattleConnConfig(ServerInfo battleServerInfo, uint masterNetworkId) {
			Debug.Log("");
			NetworkId = masterNetworkId;
			ServerId = battleServerInfo.ServerId;
			_battleServerInfo = battleServerInfo;

			Multihost.OnHostConnectionOpen += OnConnectToHost;
			Multihost.OnHostConnectionClosed += OnDissConnectFromHost;
			Multihost.AddHost("BattleServer", _battleServerInfo.Ip, _battleServerInfo.MainPort);

			void OnConnectToHost(int connectId, int hostId, HostInfo host) {

				if (host.HostName == "BattleServer") {

					ClearHandlers();
					Debug.Log("Connected to BattleServer");

					BattleServer = host;

					var writer = new MsgStreamWriter(MsgType.SetClientInfo)
					.WriteUInt32(NetworkId)
					.WriteString(UserId)
					.WriteString(Username);
					Multihost.MsgSender.Send(writer.Buffer, host.ConnId, host.HostId, Multihost.ReliableChanel);

					onConneced?.Invoke();

				}

			}

			void OnDissConnectFromHost(int connectId, int hostId, HostInfo host) {

				if (host.HostName == "BattleServer") {
					ClearHandlers();
					Debug.LogWarning("Dis_Connected from BattleServer");
					Debug.LogWarning(JsonUtility.ToJson(battleServerInfo));
					error?.Invoke("Cant connect to Battle host");
				}

			}

			void ClearHandlers() {
				Multihost.OnHostConnectionOpen -= OnConnectToHost;
				Multihost.OnHostConnectionClosed -= OnDissConnectFromHost;
			}

		}

		void Error(string errorMsg) {
			error?.Invoke(errorMsg);
		}

	}

	public class GetBattleConnection {

		public GetBattleConnection(string battleServerType, ServerInfo masterHost, Action<ServerInfo, uint> onSuccsess, Action<string> onFailed) {

			_fail = onFailed;
			_succsess = onSuccsess;

			Multihost.OnHostConnectionOpen += OnConnectToHost;
			Multihost.OnHostConnectionClosed += OnDissConnectFromHost;

			Multihost.AddHost("Master", masterHost.Ip, masterHost.MainPort);

		}

		private readonly Action<string> _fail;
		// Server info + MasterNetworkId
		private readonly Action<ServerInfo, uint> _succsess;

		private HostInfo _masterHost;
		private uint _masterNetworkId;
		private ServerInfo _battleServerInfo;

		private void OnConnectToHost(int connectId, int hostId, HostInfo host) {

			if (host.HostName == "Master") {
				_masterHost = host;
				Multihost.MsgReader.AddOneShotHandler(MsgType.SetClientId, OnReceiveNetworkId);
				Debug.Log("Connected to master server");
			}

		}

		private void OnDissConnectFromHost(int connectId, int hostId, HostInfo host) {

			if (host.HostName == "Master") {

				Debug.LogWarning("Dis_Connected to master server");
				Multihost.MsgReader.RemoveOneShotHandler(MsgType.SetClientId, OnReceiveNetworkId);
				_fail?.Invoke("Cant connect to Master host");

			}

		}

		private void OnReceiveNetworkId(NetworkMsg msg) {

			Debug.Log("Receive network id");
			_masterNetworkId = msg.reader.ReadUInt32();

			var writer = new MsgStreamWriter(MsgType.GetConnectInfo).WriteString("Any").WriteString("BattleServer");

			Multihost.MsgReader.AddOneShotHandler(MsgType.GetConnectInfo, OnReceiveBattleHostInfo);
			Multihost.MsgSender.Send(writer.Buffer, _masterHost.ConnId, _masterHost.HostId, Multihost.ReliableChanel);

		}

		private void OnReceiveBattleHostInfo(NetworkMsg msg) {

			Multihost.OnHostConnectionOpen -= OnConnectToHost;
			Multihost.OnHostConnectionClosed -= OnDissConnectFromHost;

			Multihost.RemoveHost("Master");

			if (msg.reader.ReadByte() == 0) {

				_battleServerInfo = JsonUtility.FromJson<ServerInfo>(msg.reader.ReadString());
				Debug.Log("Receive battle connection");

				_succsess?.Invoke(_battleServerInfo, _masterNetworkId);

			} else {

				Debug.LogError(msg.reader.ReadString());

			}

		}

	}

	#endregion

}






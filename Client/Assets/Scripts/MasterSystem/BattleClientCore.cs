
using System;
using System.IO;
using UnityEngine;
using Transport.Messaging;
using Transport.Universal;

public class BattleClientCore : Manager {

	public BattleClientCore() => Multihost.TurnOn();

	public string UserId { get; private set; } = string.Empty;
	public string Username { get; private set; } = string.Empty;
	public uint MasterNetworkId { get; private set; } = uint.MaxValue;

	public string SkinsName;
	private ServerInfo _masterServerInfo = null;

	public void ChangeUserInfo(string username, string userId = "Guest") {
		Username = username; UserId = userId;
	}

	public void ClearUserInfo() {
		Username = UserId = string.Empty; MasterNetworkId = uint.MaxValue;
	}

	#region MasterMain

	public void GetMasterNetworkId(Action<uint> onSuccsess, Action<string> onFailed) {

		if (MasterNetworkId != uint.MaxValue) {
			onSuccsess?.Invoke(MasterNetworkId);
			return;
		}

		if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Username)) {
			onFailed?.Invoke("Unauthorized user");
			return;
		}
		Debug.Log("BeforeOpenMaster");
		OpenMasterConnection(OnConnectedToMaster, onFailed);

		void OnConnectedToMaster(HostInfo host) {
			Multihost.MsgReader.AddOneShotHandler(MsgType.MasterIdRequest, OnReceiveMasterNetworkId);
			using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.MasterIdRequest)) {
				writer.WriteString(Username);
				Multihost.MsgSender.Send(writer.Buffer, host.ConnId, host.HostId, Multihost.ReliableChanel);
			}

			void OnReceiveMasterNetworkId(NetworkMsg msg) {
				Debug.Log("ReceiveMaster");
				MasterNetworkId = msg.reader.ReadUInt32();
				onSuccsess?.Invoke(MasterNetworkId);
				CloseMasterConnection();
			}

		}

	}
	// TODO - create server type enums ???
	public void OpenConnection(Action<HostInfo> onConnected, Action<string> onFailed, string serverType = "BattleServer") {
		
		// We must have master network id before try connecting to one of other servers
		if (MasterNetworkId == uint.MaxValue) {
			GetMasterNetworkId((x) => OpenConnection(onConnected, onFailed, serverType), onFailed);
			return;
		}
		// Mb we already have this connection? -> if we must reconnected - broke connection first
		if (Multihost.GetHost(serverType, out var hostInfo)) {
			onConnected(hostInfo);
			return;
		}

		OpenMasterConnection(OnConnectedToMaster, onFailed);

		void OnConnectedToMaster(HostInfo host) {
			
			
			Multihost.MsgReader.AddOneShotHandler(MsgType.GetConnectInfo, OnServerInfoReceived);

			using (MsgStreamWriter writer = new MsgStreamWriter(MsgType.GetConnectInfo)) {
				writer.WriteString("Any");
				writer.WriteString(serverType);
				Multihost.MsgSender.Send(writer.Buffer, host.ConnId, host.HostId, Multihost.ReliableChanel);
			}

			void OnServerInfoReceived(NetworkMsg msg) {
				
				if (msg.reader.ReadByte() == 0) { // Error byte

					ServerInfo serverConnInfo = JsonUtility.FromJson<ServerInfo>(msg.reader.ReadString());
					// Final connect to target server
					TryConnectToHost(serverConnInfo, onConnected, onFailed);
					CloseMasterConnection();

				} else {

					onFailed?.Invoke(msg.reader.ReadString());
					CloseMasterConnection();

				}

			}

		}

	}

	#endregion

	#region MasterConnection

	private int _currentMasterConnectionUsers = 0;

	public void OpenMasterConnection(Action<HostInfo> onCreated, Action<string> onConnCreateFailed) {

		// If we already open master connection
		if (Multihost.GetHost("Master", out var hostInfo)) {
			SuccessConnected(hostInfo);
			return;
		}

		#region RestAPi

		if (_masterServerInfo == null) {

			// TODO - RestAPI point
			var master = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "MasterConfig.txt")));

			_masterServerInfo = new ServerInfo() {
				Ip = master.IpAdress, MainPort = master.MainPort, WebPort = master.WebPort,
				ServerId = 0, ServerType = master.ServerType
			};

			TryConnectToHost(_masterServerInfo, SuccessConnected, onConnCreateFailed);

		} else {

			TryConnectToHost(_masterServerInfo, SuccessConnected, onConnCreateFailed);

		}
		
		#endregion

		void SuccessConnected(HostInfo masterHost) {
			if (onCreated != null) {
				_currentMasterConnectionUsers += 1;
				onCreated.Invoke(masterHost);
			}
		}

	}

	public void CloseMasterConnection() {
		if ((_currentMasterConnectionUsers -= 1) <= 0) {
			Multihost.RemoveHost("Master");
			_currentMasterConnectionUsers = 0;
		}
	}

	#endregion

	#region Universal connection

	private void TryConnectToHost(ServerInfo connInfo, Action<HostInfo> onSuccsess, Action<string> onFailed) {

		Listeners(true);
		Multihost.AddHost(connInfo.ServerType, connInfo.Ip, connInfo.MainPort);

		void OnConnectToHost(int connectId, int hostId, HostInfo host) {
			if (host.HostName == connInfo.ServerType) {
				Debug.Log(string.Format("Connected to [{0}] server", connInfo.ServerType));
				Multihost.GetHost(connInfo.ServerType, out var hostInfo);
				Listeners(false);
				onSuccsess?.Invoke(hostInfo);
			}
		}

		void OnDissConnectFromHost(int connectId, int hostId, HostInfo host) {
			if (host.HostName == connInfo.ServerType) {
				Debug.LogWarning(string.Format("Connect to [{0}] server failed", connInfo.ServerType));
				Listeners(false);
				onFailed?.Invoke("Connection time out");
			}
		}

		void Listeners(bool subscribe = false) {
			if (subscribe) {
				Multihost.OnHostConnectionOpen += OnConnectToHost;
				Multihost.OnHostConnectionClosed += OnDissConnectFromHost;
			} else {
				Multihost.OnHostConnectionOpen -= OnConnectToHost;
				Multihost.OnHostConnectionClosed -= OnDissConnectFromHost;
			}
		}

	}

	#endregion

}






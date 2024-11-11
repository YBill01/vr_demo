
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Transport.Messaging;
using Transport.Server;
using Transport.Universal;
using UnityEngine;

public class BattleServerCore : Manager {

	public BattleServerCore(Action onCreateConnections = null, Action<string> error = null, string serverType = "BattleServer") {

		m_Configs = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "ServerConfig.txt")));
		m_Configs.IpAdress = m_Configs.UseLocalIpAdress ? "127.0.0.1" : ExternalIp.GetIpAdress();
		ServerType = serverType;
		CreateConnections(onCreateConnections, error);

	}

	public BattleServerCore()
	{
		
	}
	
	protected ServerConfigs m_Configs;
	protected ServerInfo _masterServerInfo = null;
	public string ServerType { get; protected set; }

	public uint ServerId { get; protected set; }
	public HostInfo BattleHost { get; protected set; }
	public HostInfo MasterHost { get; protected set; }

	// TODO - RestAPI point
	protected virtual void GetMasterConnection(Action<ServerInfo> onGetMasterConnInfo, Action<string> onConnCreateFailed) {

		var master = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "MasterConfig.txt")));

		var MasterServerInfo = new ServerInfo() {
			Ip = master.IpAdress, MainPort = master.MainPort, WebPort = master.WebPort,
			ServerId = 0, ServerType = master.ServerType
		};

		_masterServerInfo = MasterServerInfo;
		onGetMasterConnInfo?.Invoke(_masterServerInfo);

	}

	protected virtual void CreateConnections(Action onCreateConnections, Action<string> error) {

		GetMasterConnection(OnReceiveMasterConnConfig, Error);

		void OnReceiveMasterConnConfig(ServerInfo masterInfo) {
			RunMultihost(masterInfo, RunBattleServer, Error);
		}
		void RunBattleServer() {
			Multihost.AddHost("BattleHost", HostType.Host, m_Configs.MainPort);
			Multihost.GetHost("BattleHost", out var host);
			BattleHost = host;
			CoroutineBehavior.StartCoroutine(SendInfoToMaster());
			onCreateConnections?.Invoke();
		}
		void Error(string errorMsg) {
			error?.Invoke(errorMsg);
		}

	}

	protected IEnumerator SendInfoToMaster() {

		while (true) {
			SendSelfInfoToMaster();
			yield return new WaitForSeconds(2f);
		}

	}

	// When user join to server
	private void OnReceiveUserInfo(User user) {

		var writer = new MsgStreamWriter(MsgType.OnClientJoinToServer);
		writer.WriteUInt32(user.Connection.NetworkId);
		writer.WriteString(user.UserId);
		writer.WriteString(user.Username);
		writer.WriteUInt32(ServerId);
		Multihost.MsgSender.Send(writer.Buffer, MasterHost.ConnId, MasterHost.HostId, Multihost.ReliableChanel);

	}
	// When user left from server
	protected void OnUserDestroyed(User user) {

		var writer = new MsgStreamWriter(MsgType.OnClientLeftFromServer);
		writer.WriteUInt32(ServerId);
		writer.WriteUInt32(user.Connection.NetworkId);
		Multihost.MsgSender.Send(writer.Buffer, MasterHost.ConnId, MasterHost.HostId, Multihost.ReliableChanel);

	}

	protected void SendSelfInfoToMaster() {

		MsgStreamWriter writer = new MsgStreamWriter(MsgType.SetConnectInfo);

		writer.WriteUInt32(ServerId);
		writer.WriteString(m_Configs.ServerType);
		writer.WriteString(m_Configs.IpAdress);
		writer.WriteInt32(m_Configs.WebPort); // web
		writer.WriteInt32(m_Configs.MainPort); // main
		writer.WriteUInt16((ushort)Multihost.MaxConnections);
		writer.WriteUInt16((ushort)Multihost.ConnectionsManager.ConnectionsCount);
		writer.WriteUInt16(16);
		
		Multihost.GetHost("Master", out var masterHost);
		Multihost.MsgSender.Send(writer.Buffer, masterHost.ConnId, masterHost.HostId, Multihost.ReliableChanel);

	}

	protected void RunMultihost(ServerInfo masterHostInfo, Action onSuccsess, Action<string> onFailed) {

		Multihost.TurnOn();
		
		if (Multihost.GetHost("Master", out var hostInfo)) {
			onSuccsess?.Invoke();
		} else {
			Multihost.OnHostConnectionOpen += OnConnectToHost;
			Multihost.OnHostConnectionClosed += OnDissConnectFromHost;
			Multihost.AddHost("Master", masterHostInfo.Ip, masterHostInfo.MainPort);
		}

		void OnConnectToHost(int connectId, int hostId, HostInfo host) {
			if (host.HostName == "Master") {
				Debug.Log("Connected to master server");
				Multihost.MsgReader.AddOneShotHandler(MsgType.SetClientId, SetConnectionId);
			}
		}
		void OnDissConnectFromHost(int connectId, int hostId, HostInfo host) {
			if (host.HostName == "Master") {
				Debug.LogWarning("Dis_Connected to master server");
				ClearHandlers();
				onFailed?.Invoke("Cant connect to master server");
			}
		}
		void SetConnectionId(NetworkMsg msg) {
			ServerId = msg.reader.ReadUInt32();
			SendSelfInfoToMaster();
			ClearHandlers();
			Multihost.GetHost("Master", out var host);
			MasterHost = host;
			onSuccsess?.Invoke();
		}
		void ClearHandlers() {
			Multihost.OnHostConnectionOpen -= OnConnectToHost;
			Multihost.OnHostConnectionClosed -= OnDissConnectFromHost;
		}
	}
}



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Transport.Universal;
using UnityEngine;

public class ChattingServerCore : BattleServerCore {

	public ChattingServerCore(Action onCreateConnections = null, Action<string> error = null) {

		m_Configs = JsonUtility.FromJson<ServerConfigs>(File.ReadAllText(Path.Combine(Application.dataPath, "ServerConfig.txt")));
		m_Configs.IpAdress = m_Configs.UseLocalIpAdress ? "127.0.0.1" : ExternalIp.GetIpAdress();
		ServerType = "ChattingServer";
		CreateConnections(onCreateConnections, error);
	}

	protected sealed override void CreateConnections(Action onCreateConnections, Action<string> error) {

		GetMasterConnection(OnReceiveMasterConnConfig, Error);

		void OnReceiveMasterConnConfig(ServerInfo masterInfo) {
			RunMultihost(masterInfo, RunBattleServer, Error);
		}
		void RunBattleServer() {
			Multihost.AddHost("ChattingHost", HostType.Host, m_Configs.MainPort);
			Multihost.GetHost("ChattingHost", out var host);
			BattleHost = host;
			CoroutineBehavior.StartCoroutine(SendInfoToMaster());
			onCreateConnections?.Invoke();
		}
		void Error(string errorMsg) {
			error?.Invoke(errorMsg);
		}

	}
	
}

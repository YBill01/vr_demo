
using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Client;
using UnityEngine;
using Transport.Universal;
using Transport.Messaging;

using LobbyManager = Transport.Client.LobbyManager;

public class ClientStartInstruction : CustomStartInstuction {

	public override void Initialize() {
		
	}

	public override void OnGameInitialized() {
		
		// TODO - Testing
		var client = Game.AddManager(new BattleClientCore());
		Game.AddManager(new Chatting());

		return;

		//client.OnUserLogIn("Guest", "RandomUser");
		//client.ConnectToServer(OnConnected, OnConnectionFailed);

		void OnConnected() {

			Debug.Log("Success connected to server!");
			Game.AddManager(new Client_NetworkObjects()).Final();
			Game.AddManager(new LobbyManager(client));

		}

		void OnConnectionFailed(string obj) {
			Debug.Log(string.Format("Connection failed! Reason: {0}", obj));
		}

	}

}

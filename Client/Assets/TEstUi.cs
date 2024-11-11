

using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Client;
using UnityEngine;
using Transport.Universal;
using Transport.Messaging;
using TMPro;

using LobbyManager = Transport.Client.LobbyManager;

public class TEstUi : MonoBehaviour {

	public bool isBot = false;

	public GameObject JoinScreen;
	public TMP_InputField Username;

	public int TotalUsers;
	public TextMeshProUGUI UsersCountLabel;

	private void Awake() {
		if (isBot) {
			CoroutineBehavior.Delay(2f, () =>{
			 Username.text = "Guest" + UnityEngine.Random.Range(0, int.MaxValue);
			SetUsernameAndJoin();  Debug.Log("Try join");
			});
		}
	}

	public void SetUsernameAndJoin() {

		var client = Game.GetManager<BattleClientCore>();
		client.ChangeUserInfo(Username.text, "UserID");

		client.OpenConnection(OnConnected, OnConnectionFailed, "BattleServer");

		void OnConnected(HostInfo host) {

			Debug.Log("Success connected to [BattleServer] !");

			var netObjects = Game.AddManager(new Client_NetworkObjects());
			netObjects.Final();
			netObjects.OnNetObj_Created += OnNetObjectCreated;
			netObjects.OnNetObj_Destroyed += OnNetObjectDestroyed;

			Game.AddManager(new LobbyManager(client));

			JoinScreen.SetActive(false);

		}

		void OnConnectionFailed(string obj) {
			Debug.Log(string.Format("Connection failed! Reason: {0}", obj));
		}

	}

	// Test label | TODO - remove
	private void OnNetObjectCreated(NetworkObject obj) {
		if (obj is PlayerNetworkObject) {
			TotalUsers++;
			UsersCountLabel.text = string.Format("Users: {0}", TotalUsers);
		}
	}
	private void OnNetObjectDestroyed(NetworkObject obj) {
		if (obj is PlayerNetworkObject) {
			TotalUsers--;
			UsersCountLabel.text = string.Format("Users: {0}", TotalUsers);
		}
	}

	private void Update() 
	{
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Log("Try get IpAdress");
			Debug.Log(ExternalIp.GetIpAdress());
		}


	}



}


using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Messaging;
using Transport.Server;
using Transport.Universal;
using UnityEngine;
using TickManager = Transport.Server.TickManager;

public class BattleStartInstruction : CustomStartInstuction {

	public override void Initialize() {
		
	}

	public override void OnGameInitialized() {

		Game.AddManager(new BattleServerCore(OnBattleServerIsCreated));
		
		void OnBattleServerIsCreated() {

			Game.AddManager(new Server_NetworkObjects()).Final();
			Game.AddManager(new LobbyManager());

		}

	}
	
}

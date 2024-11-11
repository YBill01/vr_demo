
using System.Collections;
using System.Collections.Generic;
using Transport.Server;
using UnityEngine;

public class MasterStartInstruction : CustomStartInstuction {

	public override void Initialize() {
		//Game.AddManager(new HostCenter());
	}

	public override void OnGameInitialized() {

		Game.AddManager(new MasterServerCore());
		//Game.AddManager(new TickManager()).Final();

	}

}

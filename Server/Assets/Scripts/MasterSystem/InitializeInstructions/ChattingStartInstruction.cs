
using UnityEngine;

public class ChattingStartInstruction : CustomStartInstuction {

	public override void Initialize() {

	}

	public override void OnGameInitialized() {

		Game.AddManager(new ChattingServerCore(OnChattingServerIsCreated));

		void OnChattingServerIsCreated() {

			Game.AddManager(new ChatManager()).Final();
			Debug.Log("Chattig server started");
		}

	}

}
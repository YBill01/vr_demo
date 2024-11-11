
namespace Chofu.RestApi {

	using System;

	public class UserInfo : Request<UserInventory> {

		public UserInfo(Action<Responce<UserInventory>> callback = null) {

			MsgType = MsgType.Get;
			Uri = Url.Get.UserInfo;
			Token = Messaging.Access_token;

			_callback = callback;
			Messaging.SendMessage(this);

		}

	}

	[Serializable]
	public struct UserInventory {
		public int id;
		public int balance;
		public InvItem[] inventory;

	}
	[Serializable]
	public struct InvItem {
		public int id;
		public string name;
		public bool isTradeble;
		public int maxStack;
		public int count;
	}

}

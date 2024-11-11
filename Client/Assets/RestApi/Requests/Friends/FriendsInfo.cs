
namespace Chofu.RestApi {

	using System;

	public class GetFriends : Request<FriendList>, ICustomErrorHandler {


		public GetFriends(FrienStatus status, Action<Responce<FriendList>> callback = null) {

			MsgType = MsgType.Get;

			switch (status) {

			case FrienStatus.Current:
				Uri = Url.Get.FriendsCurrent;
				break;

			case FrienStatus.Incoming:
				Uri = Url.Get.FriendsIncoming;
				break;

			case FrienStatus.Outgoing:
				Uri = Url.Get.FriendsOutgoing;
				break;

			}

			Token = Messaging.Access_token;

			_callback = callback;
			Messaging.SendMessage(this);

		}

		public bool MessageHasErrors(string message, out ClearResponce error) {
			error = default; return false;
		}

		public override void ReadMessage(string message) {

			try {

				var line = string.Format("{0}{1}Items{1}:{2}{3}", '{', '"', message, '}');
				FriendList list = new FriendList() { message = "OK", friends = JsonHelper.FromJson<FriendInfo>(line) };
				Responce = new Responce<FriendList>() { data = list, status = true };

			} catch {

				var msg = UnityEngine.JsonUtility.FromJson<ClearResponce>(message);
				Responce = new Responce<FriendList>() { status = true, data = new FriendList() { message = msg.messages } };

			}

			_callback?.Invoke(Responce);

		}

	}

	public enum FrienStatus {
		Current, Incoming, Outgoing
	}
	[Serializable]
	public struct FriendList {
		public string message;
		public FriendInfo[] friends;
	}
	[Serializable]
	public struct FriendInfo {
		public int id;
		public int friend;
		public long updated;
	}

}
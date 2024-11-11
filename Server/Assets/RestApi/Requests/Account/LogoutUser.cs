
namespace Chofu.RestApi {

	using System;

	public class LogoutUser : Request<LogoutStatus> {

		public LogoutUser(Action<Responce<LogoutStatus>> callback = null) {

			MsgType = MsgType.Post;
			Uri = Url.Post.Logout;
			Token = Messaging.Refresh_token;

			_callback = callback;
			Messaging.SendMessage(this);

		}

	}
	
	[Serializable]
	public struct LogoutStatus {
		public int fieldCount;
		public int affectedRows;
		public int insertId;
		public string info;
		public int serverStatus;
		public int warningStatus;
	}

}
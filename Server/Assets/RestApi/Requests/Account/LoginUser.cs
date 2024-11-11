
namespace Chofu.RestApi {

	using System;

	[Serializable]
	public class LoginUser : Request<LoginResult>, IJsonForm {

		public string username;
		public string password;

		public LoginUser(string username, string password, Action<Responce<LoginResult>> callback = null) {

			this.username = username;
			this.password = password;

			MsgType = MsgType.Post;
			Uri = Url.Post.Login;

			_callback = callback;
			Messaging.SendMessage(this);

		}

		public override void ReadMessage(string message) {

			Responce = Responce<LoginResult>.Read(message);
			Messaging.Access_token = Responce.data.accessToken;
			Messaging.Refresh_token = Responce.data.refreshToken;

			_callback?.Invoke(Responce);

		}

	}

	[Serializable]
	public struct LoginResult {
		public string accessToken;
		public string refreshToken;
		public User user;
	}
	[Serializable]
	public struct User {
		public int id;
		public string username;
	}

}


namespace Chofu.RestApi {

	using System;

	[Serializable]
	public class RegisterUser : Request<RegistrationResult>, IJsonForm {

		public string username;
		public string password;

		public RegisterUser(string username, string password, Action<Responce<RegistrationResult>> callback = null) {

			this.username = username;
			this.password = password;

			MsgType = MsgType.Post;
			Uri = Url.Post.Register;

			_callback = callback;
			Messaging.SendMessage(this);

		}

		public override void ReadMessage(string message) {

			Responce = Responce<RegistrationResult>.Read(message);
			Messaging.Access_token = Responce.data.accessToken;
			Messaging.Refresh_token = Responce.data.refreshToken;

			_callback?.Invoke(Responce);

		}

	}

	[Serializable]
	public struct RegistrationResult {
		public string accessToken;
		public string refreshToken;
		public RegisteredUser user;
	}
	[Serializable]
	public struct RegisteredUser {
		public int id;
		public string username;
		public string secretkey;
	}

}


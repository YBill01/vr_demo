
namespace Chofu.RestApi {

	using System;

	[Serializable]
	public class ChangePassword : Request<ClearResponce>, IJsonForm {

		public string currentPassword;
		public string newPassword;

		public ChangePassword(string currentPassword, string newPassword, Action<Responce<ClearResponce>> callback = null) {

			this.currentPassword = currentPassword;
			this.newPassword = newPassword;

			MsgType = MsgType.Put;
			Token = Messaging.Access_token;
			Uri = Url.Put.CahangePassword;

			_callback = callback;
			Messaging.SendMessage(this);

		}

		public override void ReadMessage(string message) {
			bool status = message == "User password was updated!";
			Responce = new Responce<ClearResponce>() { data = new ClearResponce() { messages = message }, status = status };
			_callback?.Invoke(Responce);
		}

	}

	[Serializable]
	public class ChangePasswordWithKey : Request<SecretKey>, IJsonForm {

		public string key;
		public string newPassword;

		public ChangePasswordWithKey(string key, string newPassword, Action<Responce<SecretKey>> callback = null) {

			this.key = key;
			this.newPassword = newPassword;

			MsgType = MsgType.Put;
			Uri = Url.Put.CahangePasswordWithKey;

			_callback = callback;
			Messaging.SendMessage(this);

		}

	}

	[Serializable]
	public struct SecretKey {
		public string newSecretKey;
	}

}

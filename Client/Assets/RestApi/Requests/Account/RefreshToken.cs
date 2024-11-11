
namespace Chofu.RestApi {

	using System;

	public class RefreshToken : Request<LoginResult> {

		public RefreshToken(Action<Responce<LoginResult>> callback = null) {

			MsgType = MsgType.Get;
			Uri = Url.Get.TokenRefresh;
			Token = Messaging.Refresh_token;

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

}

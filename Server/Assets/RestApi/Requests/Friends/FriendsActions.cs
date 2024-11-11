

namespace Chofu.RestApi {

	using System;

	public class FriendRequest : Request<ClearResponce>, ICustomErrorHandler {

		[NonSerialized]
		private readonly FriendRequestType _requestType;
		[NonSerialized]
		private readonly string[] _regularGoodResponses = new string[] { "Successfully send!", "Approved", "Rejected", "Removed", "Declined" };

		public FriendRequest(FriendRequestType requestType, int userId, Action<Responce<ClearResponce>> callback = null) {

			_requestType = requestType;

			MsgType = requestType.IsOneOf(FriendRequestType.Remove, FriendRequestType.Decline) ? MsgType.Delete : MsgType.Post;

			switch (requestType) {
			case FriendRequestType.Send:
				Uri = string.Format(Url.Post.FriendRequest_Send, userId);
				break;
			case FriendRequestType.Approve:
				Uri = string.Format(Url.Post.FriendRequest_Approve, userId);
				break;
			case FriendRequestType.Reject:
				Uri = string.Format(Url.Post.FriendRequest_Reject, userId);
				break;
			case FriendRequestType.Remove:
				Uri = string.Format(Url.Delete.FriendRemove, userId);
				break;
			case FriendRequestType.Decline:
				Uri = string.Format(Url.Delete.FriendRequest_Decline, userId);
				break;
			}

			Token = Messaging.Access_token;
			_callback = callback;
			Messaging.SendMessage(this);

		}

		public byte[] GetRequestBody() {
			return new byte[1];
		}

		public bool MessageHasErrors(string message, out ClearResponce error) {
			error = default; return false;
		}

		public override void ReadMessage(string message) {

			Responce = new Responce<ClearResponce>() {
				status = message.Replace("\"", "").IsOneOf(_regularGoodResponses),
				data = new ClearResponce() { messages = message } };

			_callback?.Invoke(Responce);

		}

	}

	public enum FriendRequestType {
		Send, Approve, Reject, Remove, Decline
	}

}
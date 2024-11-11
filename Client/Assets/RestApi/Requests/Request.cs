
namespace Chofu.RestApi {

	using System;
	using UnityEngine;

	public interface IRequest {
		bool IsDone { get; }
		string Uri { get; }
		string Token { get; }
		MsgType MsgType { get; }
		CoroutineWrapper Send(Action callback = null, Action<Error> onFailed = null);
	}

	public interface IResponce<T> {
		Responce<T> Responce { get; }
		void ReadMessage(string message);
	}

	public interface ICustomPostForm {
		WWWForm GetRequestForm();
	}
	public interface ICustomPutForm {
		byte[] GetRequestForm();
	}
	public interface IJsonForm {
		
	}
	public interface ICustomJsonForm : IJsonForm {
		byte[] GetRequestBody();
	}
	public interface ICustomErrorHandler {
		bool MessageHasErrors(string message, out ClearResponce error);
	}


	[Serializable]
	public abstract class Request<T> : IRequest, IResponce<T> {

		public bool IsDone { get; protected set; } = false;
		public string Uri { get; protected set; }
		public string Token { get; protected set; }
		public MsgType MsgType { get; protected set; } = MsgType.Post;
		public Responce<T> Responce { get; protected set; }

		protected Action<Responce<T>> _callback = null;

		public virtual void ReadMessage(string message) {
			Responce = Responce<T>.Read(message);
			_callback?.Invoke(Responce);
		}

		public virtual CoroutineWrapper Send(Action callback = null, Action<Error> onFailed = null) {
			return MessagesSender<T>.Send(this, this, callback += () => IsDone = true, onFailed);
		}

	}

}

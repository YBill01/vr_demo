
namespace Chofu.RestApi {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Networking;

	public delegate void MsgSendError(Error error);
	public enum Error { Connection, BadResponce }

	public enum MsgType {
		Post, Put, Get, Delete
	}

	public static class Messaging {

		public static MsgSendError OnMessageSendingIsFailed;
		// TODO - make this safy
		public static string Access_token = string.Empty;
		public static string Refresh_token = string.Empty;

		private static bool s_isStoped = false;
		private static CoroutineWrapper s_webRequestSenging;
		private static IRequest s_currentRequest;
		private static Queue<IRequest> s_requests = new Queue<IRequest>();

		public static void SendMessage(IRequest request) {
			s_requests.Enqueue(request);
			NextMessage();
		}

		public static void StopSending() {
			if (s_isStoped) {
				return;
			}
			s_webRequestSenging?.Stop(true);
			s_webRequestSenging = null;
			s_isStoped = true;
		}

		public static void ContinueSending() {
			if (!s_isStoped) {
				return;
			}
			s_isStoped = false;
			NextMessage();
		}

		private static void NextMessage() {
			if (s_isStoped || s_webRequestSenging != null) {
				return;
			}
			IRequest msg = s_currentRequest;
			if (msg == null) {
				s_requests.TryDequeue(out msg);
			}
			if (msg != null) {
				s_currentRequest = msg;
				s_webRequestSenging = msg.Send
					(() => { s_webRequestSenging = null; s_currentRequest = null; NextMessage(); },
					(x) => OnMessageSendingIsFailed?.Invoke(x));
			}
		}

	}

	public class MessagesSender<T> {

		public static CoroutineWrapper Send(IRequest request, IResponce<T> responce, Action callback, Action<Error> onFaiiled = null) {
			return CoroutineBehavior.StartCoroutine(SendMessage(request, responce, callback, onFaiiled));
		}

		private static IEnumerator SendMessage(IRequest request, IResponce<T> responce, Action callback, Action<Error> onFaiiled = null) {

			using (UnityWebRequest www = CreateRequest()) {

				www.SetRequestHeader("Authorization", "Bearer " + request.Token);

				yield return www.SendWebRequest();

				Debug.Log(String.Format("Msg: {0} \n {1}", request.GetType().FullName, www.downloadHandler.text));

				if (!www.result.IsOneOf(UnityWebRequest.Result.ConnectionError, UnityWebRequest.Result.DataProcessingError)) {

					if (request is ICustomErrorHandler errorHandler) {
						if (errorHandler.MessageHasErrors(www.downloadHandler.text, out var error)) {
							ThrowError(error);
						} else {
							CallCallback();
						}
					} else {
						if (Responce.ReadErrors(www.downloadHandler.text, out var errors)) {
							ThrowError(errors);
						} else {
							CallCallback();
						}
					}

					void CallCallback() {
						responce.ReadMessage(www.downloadHandler.text); // actualy msg callback
						callback?.Invoke();
					}

					void ThrowError(ClearResponce error) {
						onFaiiled?.Invoke(Error.BadResponce);
						Debug.LogWarning(string.Format("Msg code error! Msg_FullName: {0}", request.GetType().FullName));
						callback?.Invoke(); // restore msg pump
					}
					
				} else {

					onFaiiled?.Invoke(Error.Connection);
					Debug.LogWarning("Connection error!");
					callback?.Invoke(); // restore msg pump

				}

			}

			UnityWebRequest CreateRequest() {

				switch (request.MsgType) {

				case MsgType.Post:

					if (request is IJsonForm jsPostForm) {
						UnityWebRequest webRequest = UnityWebRequest.Post(request.Uri, "POST");
						if (jsPostForm is ICustomJsonForm customJsPostForm) {
							webRequest.uploadHandler = new UploadHandlerRaw(customJsPostForm.GetRequestBody());
						} else {
							webRequest.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(request)));
						}
						webRequest.SetRequestHeader("Content-Type", "application/json");
						return webRequest;
					}

					WWWForm form = new WWWForm();

					if (request is ICustomPostForm customPostForm) {
						form = customPostForm.GetRequestForm();
					} else {
						foreach (var field in request.GetType().GetFields()) {
							form.AddField(field.Name.ToLower(), field.GetValue(request).ToString());
						}
					}
					
					return UnityWebRequest.Post(request.Uri, form);

				case MsgType.Put:

					if (request is IJsonForm jsPutForm) {
						UnityWebRequest webRequest = UnityWebRequest.Put(request.Uri, "PUT");
						if (jsPutForm is ICustomJsonForm customJsPutForm) {
							webRequest.uploadHandler = new UploadHandlerRaw(customJsPutForm.GetRequestBody());
						} else {
							webRequest.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(request)));
						}
						webRequest.SetRequestHeader("Content-Type", "application/json");
						return webRequest;
					}

					if (request is ICustomPutForm customPutForm) {
						return UnityWebRequest.Put(request.Uri, customPutForm.GetRequestForm());
					} else {
						return UnityWebRequest.Put(request.Uri, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(request).ToLower()));
					}

				case MsgType.Get:

					return UnityWebRequest.Get(request.Uri);

				case MsgType.Delete:

					if (request is IJsonForm jsDelForm) {
						UnityWebRequest webRequest = UnityWebRequest.Delete(request.Uri);
						if (jsDelForm is ICustomJsonForm customJsDelForm) {
							webRequest.uploadHandler = new UploadHandlerRaw(customJsDelForm.GetRequestBody());
						} else {
							webRequest.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(request)));
						}
						webRequest.downloadHandler = new DownloadHandlerBuffer();
						webRequest.SetRequestHeader("Content-Type", "application/json");
						return webRequest;
					}
					
					var delRequest = UnityWebRequest.Delete(request.Uri);
					delRequest.downloadHandler = new DownloadHandlerBuffer();
					return delRequest;

				}

				return null;

			}

		}

	}

}
 
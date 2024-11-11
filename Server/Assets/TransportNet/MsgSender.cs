
#pragma warning disable
namespace Transport.Messaging {

	using System;
	using System.Text;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Networking;
	using Transport.Messaging;

	public class MsgSender {
		
		private byte _errorByte = 0;

		/// Отправить сообщение на клиент
		public void Send<T>(short _msgType, T _object, int _connectID, int _hostId, int _chanelID) {

			string jsonData = JsonUtility.ToJson(_object);
			Send(_msgType, jsonData, _connectID, _hostId, _chanelID);

		}
		public void Send(byte[] msgContent, int connectId, int hostId, int chanelId) {

			NetworkTransport.Send(hostId, connectId, chanelId, msgContent, msgContent.Length, out _errorByte);

		}
		public void Send(short msgType, byte[] msgContent, int connectId, int hostId, int chanelId) {

			byte[] msgBody = new byte[2 + msgContent.Length];
			Array.Copy(BitConverter.GetBytes(msgType), msgBody, 2);
			Array.Copy(msgContent, 0, msgBody, 2, msgContent.Length);
			byte errorByte = 0;
			
			NetworkTransport.Send(hostId, connectId, chanelId, msgBody, msgBody.Length, out errorByte);

		}
		public void Send(short msgType, string jsonData, int connectID, int hostId, int chanelID) {

			byte[] msgContent = Encoding.UTF8.GetBytes(jsonData);
			byte[] finalBytes = new byte[msgContent.Length + 2];
			Array.Copy(BitConverter.GetBytes(msgType), 0, finalBytes, 0, 2);
			Array.Copy(msgContent, 0, finalBytes, 2, msgContent.Length);
			
			NetworkTransport.Send(hostId, connectID, chanelID, finalBytes, finalBytes.Length, out var errorByte);

			if (errorByte == 2) {
				// Wrong Connect ID
			}

		}
	}
}
#pragma warning restore
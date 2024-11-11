
namespace Transport.Client 
{
	using System.Collections.Generic;
	using Transport.Messaging;
	using Transport.Universal;
	using UnityEngine;

	using Qos = UnityEngine.Networking.QosType;

	// Destroy - uint (networkId) - write in row
	// Create - uint (networkId) + uint (objectId) + data - write in row
	// Refresh - uint (networkId) + data - write in row
	// Sync - uint (networkId) + data - write in row

	public delegate void OnNetworkObject(NetworkObject obj);

	public class Client_NetworkObjects : NetworkObjects, IFixedUpdate {

		public OnNetworkObject OnNetObj_Created;
		public OnNetworkObject OnNetObj_Destroyed;

		private HostInfo _host;
		private MsgsSyncBuffer _buffersManager = new MsgsSyncBuffer();
		// This objects must update from client side
		private List<NetworkObject> _syncSubscribers = new List<NetworkObject>();

		public override void Final()
		{
			base.Final();
			Initialize<Client_NetworkObjects>(Multihost.MsgReader);
			Multihost.GetHost("BattleServer", out _host);
			Game.Updating_Subscribe(this);
			Debug.Log("Client NetworkObjects is Initialized");

		}

		public void SubscribeForUpdate(NetworkObject obj) {
			_syncSubscribers.AddWithoutDoubles(obj);
		}
		public void UnsubscripeFromUpdate(NetworkObject obj) {
			_syncSubscribers.Remove(obj);
		}

		public bool GetObject<T>(uint networkId, out T networkObject) where T : NetworkObject {
			bool result = _networkObjects.TryGetValue(networkId, out var obj);
			networkObject = obj as T;
			return result;
		}

		protected override void Network_ObjectCreate(NetworkMsg msg) {

			// For couple objects
			while (msg.reader.DataLeft > 4) {

				uint networkId = msg.reader.ReadUInt32();
				if (!_networkObjects.ContainsKey(networkId)) {

					// TODO - Resources loader
					uint objId = msg.reader.ReadUInt32();
					var obj = (Object.Instantiate(Resources.Load(string.Format("NetworkObjects/{0}",0.ToString()))) as GameObject)?.GetComponent<NetworkObject>();
					obj.ReadSpawnData(msg.reader);
					obj.Initialize(networkId);
					OnNetObj_Created?.Invoke(obj);
					Debug.Log("Network object created");

				}
				else 
				{
					msg.reader.Skip(4); // Just skip ObjectId
					_networkObjects[networkId].ReadSpawnData(msg.reader);
					//throw new System.NotImplementedException("Object with same UID already exist");
				}

			}
			
			msg.reader.Dispose();

		}

		protected override void Network_ObjectDestroy(NetworkMsg msg) {

			// For couple objects
			while (msg.reader.DataLeft > 1) {

				uint networkId = msg.reader.ReadUInt32();
				if (_networkObjects.TryGetValue(networkId, out var obj)) {

					OnNetObj_Destroyed?.Invoke(obj);
					Object.Destroy(obj.gameObject);

				}

			}
			
			msg.reader.Dispose();

		}

		public override void RequestComponentUpdate(NetworkObject obj) {
			using (var writer = new MsgStreamWriter(MsgType.Refresh_NetworkObjs)) {
				writer.WriteByte(0).WriteUInt32(obj.NetworkId);
				Multihost.MsgSender.Send(writer.Buffer, _host.ConnId, _host.HostId, Multihost.ReliableChanel);
			}
		}

		public override void OnCreate(NetworkObject obj, uint networkId = uint.MaxValue) {
			base.OnCreate(obj, networkId);
			// Just in case
			if (!_networkObjects.ContainsKey(obj.NetworkId)) {
				Debug.Log("Create new NetworkObject");
				_networkObjects.Add(obj.NetworkId, obj);
			} else {
				throw new System.NotImplementedException(string.Format("NetworkObjects already contains this key: {0}", obj.NetworkId));
			}
		}
		public override void OnDestroy(NetworkObject obj) {
			base.OnDestroy(obj);
			// Just in case
			if (!_networkObjects.Remove(obj.NetworkId)) {
				throw new System.NotImplementedException(string.Format("NetworkObjects do not contains this key: {0} But you try Destroy it", obj.NetworkId));
			} else {
				Debug.Log("Network object Destroyed");
			}
		}

		public void FixedUpdate(float timeDelta) {

			if (_syncSubscribers.Count <= 0) {
				return;
			}

			ServerTick.MakeTick();

			CreateMessages(MsgType.Synchronize_NetworkObjs, Qos.Reliable, Qos.Unreliable);
			SendMessages(Qos.Reliable,Qos.ReliableFragmented, Qos.Unreliable, Qos.UnreliableFragmented);

			void CreateMessages(short msgType, params Qos[] chanels) {

				ushort partsCount = 0;
				ushort dataLenght = 0;
				MsgStreamWriter writer = null;

				for (int i = 0; i < chanels.Length; i++) {

					var b = _buffersManager.GetBuffer(chanels[i], msgType);

					foreach (var obj in _syncSubscribers) {

						dataLenght = obj.SyncDataLenght(chanels[i], out partsCount);

						if (dataLenght > 0) {
							writer = b.GetWriter(dataLenght);
							obj.WriteSyncData(chanels[i], ref writer, partsCount);
						} else {
							continue;
						}

					}

				}

			}

			void SendMessages(params Qos[] chanels) {

				
				var buffers = _buffersManager.GetBuffers(chanels);

				if (buffers == null || buffers.Count <= 0) {
					return;
				}

				foreach (var b in buffers) {
					
					int chanel = Multihost.GetChanel(b.Qos);
					foreach (var msg in b.GetMessages()) {
						Multihost.MsgSender.Send(msg.Buffer, _host.ConnId, _host.HostId, chanel);
					}

				}

				_buffersManager.RemoveBuffer(buffers.ToArray());

			}

		}

		
	}

}

namespace Transport.Universal {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Transport.Messaging;
	using UnityEngine;
	using System.Linq;
	using Transport.Universal;

	using QosType = UnityEngine.Networking.QosType;
	using Object = UnityEngine.Object;

	public class MsgsSyncBuffer {

		private List<MsgBuffer> _buffers = new List<MsgBuffer>();

		public MsgBuffer GetBuffer(QosType qos, short msgType) {
			if (!qos.IsOneOf(QosType.Reliable, QosType.Unreliable)) {
				throw new NotImplementedException("Only Reliable & Unreliable types supported!");
			}
			QosType[] types = new QosType[] { qos, qos == QosType.Reliable ? QosType.ReliableFragmented : QosType.UnreliableFragmented };
			if (_buffers.ContaitsWhere(x => x.Qos.IsOneOf(types) && x.MsgType == msgType, out var b)) {
				return b;
			} else {
				var buffer = new MsgBuffer(msgType, qos);
				_buffers.Add(buffer);
				return buffer;
			}
		}

		public List<MsgBuffer> GetBuffers(params QosType[] qos) {
			return (from b in _buffers where b.Qos.IsOneOf(qos) select b).ToList();
		}

		public void RemoveBuffer(params MsgBuffer[] buffers) {
			for (int i = 0; i < buffers.Length; i++) {
				buffers[i]?.Dispose();
				_buffers.Remove(buffers[i]);
			}
		}

		public void Clear() {
			_buffers.ForEach(x => x?.Dispose());
			_buffers.Clear();
		}
		
	}

	public class MsgBuffer : IDisposable {

		public readonly short MsgType;
		public QosType Qos { get; private set; }
		public ushort MaxBufferLenght { get; private set; } // 1018, 32000

		private const ushort _singleBufferSize = 1000;
		private const ushort _fragmentedBufferSize = 32000;

		private MsgStreamWriter c_Message;
		private List<MsgStreamWriter> m_Messages;

		public MsgBuffer(short msgType, QosType qos) {

			if (!qos.IsOneOf(QosType.Reliable, QosType.Unreliable)) {
				throw new NotImplementedException("Only Reliable & Unreliable types supported!");
			}

			MsgType = msgType;
			Qos = qos;
			MaxBufferLenght = _singleBufferSize;

			c_Message = new MsgStreamWriter(MsgType);
			m_Messages = new List<MsgStreamWriter>();

		}

		public MsgStreamWriter GetWriter(ushort dataLenght) {
			if (c_Message.Lenght + dataLenght < MaxBufferLenght) {
				return c_Message;
			} else {
				// This mean - msg allready fragmented
				if (MaxBufferLenght > _singleBufferSize) {
					Qos = Qos == QosType.Reliable ? QosType.ReliableFragmented : QosType.UnreliableFragmented;
					MaxBufferLenght = _fragmentedBufferSize;
					return c_Message;
				} else {
					m_Messages.Add(c_Message);
					c_Message = new MsgStreamWriter(MsgType);
					return c_Message;
				}
			}
		}
		/// Use this in last moment!
		public List<MsgStreamWriter> GetMessages() {
			if (c_Message.Lenght >= 4) {
				m_Messages.Add(c_Message);
			}
			if (m_Messages.Count > 0) {
				return m_Messages;
			} else {
				return new List<MsgStreamWriter>();
			}
		}

		public void Dispose() {
			c_Message?.Dispose();
			m_Messages.ForEach(x => x.Dispose());
			c_Message = null;
			m_Messages.Clear();
		}

	}

}

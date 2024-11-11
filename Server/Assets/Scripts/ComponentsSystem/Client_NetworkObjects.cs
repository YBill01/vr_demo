
namespace Transport.Server {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Transport.Messaging;
	using UnityEngine;
	using System.Linq;
	using Transport.Universal;
	using Qos = UnityEngine.Networking.QosType;
	using Object = UnityEngine.Object;

	public delegate void Tick();
	public enum TickState {
		StartGlobal = 0, EndGlobal = 1,
		StartReliable = 2, AfterMsgsReliable = 3, EndReliable = 4,
		StartUnreliable = 5, AfterMsgsUnreliable = 6, EndUnreliable = 7
	}

	public class Server_NetworkObjects : NetworkObjects, IFixedUpdate {
		
		#region Ticks

		public Tick OnStart_ReliableTick;
		public Tick OnMessagesCreated_ReliableTick;
		public Tick OnEnd_ReliableTick;

		public Tick OnStart_UnreliableTick;
		public Tick OnMessagesCreated_UnreliableTick;
		public Tick OnEnd_UnreliableTick;

		public Tick OnStart_SyncTick;
		public Tick OnEnd_SyncTick;

		#endregion

		#region Ignore on server
		protected override void Network_ObjectCreate(NetworkMsg msg) { }
		protected override void Network_ObjectDestroy(NetworkMsg msg) { }
		#endregion

		public override void Final() {
			base.Final();
			Initialize<Server_NetworkObjects>(Multihost.MsgReader);
			Game.Updating_Subscribe(this);
			Debug.Log("Server NetworkObjects is Initialized");
		}

		public override void Dispose() {
			base.Dispose();
			Game.Updating_Unsubscribe(this);
		}

		private MsgsSyncBuffer _buffersManager = new MsgsSyncBuffer();
		private List<uint> _syncIgnore = new List<uint>(128);

		private List<NetworkListener> _networkListeners = new List<NetworkListener>();
		private List<uint> _runtimeSynchronizable = new List<uint>();

		#region Instantiate & Destroy

		public T InstantiateObject<T>(uint objectId, out Action<T> endSpawnSync) where T : NetworkObject {

			// TODO - Resources loader
			var newObj = (Object.Instantiate(Resources.Load(string.Format("NetworkObjects/{0}", objectId))) as GameObject).GetComponent<T>();
			newObj.Initialize(uint.MaxValue);
			endSpawnSync = EndSpawnSync;
			return newObj;

			void EndSpawnSync(T setupedNetObj) {

				_syncIgnore.Add(setupedNetObj.NetworkId);

				if (setupedNetObj.gameObject.TryGetComponent<NetworkListener>(out var netListener)) {
					// This is a new player or someone who must see other networkObjects
					if (_networkListeners.AddWithoutDoubles(netListener)) {
						SyncWithInstance(netListener, newObj);
					} else {
						throw new NotImplementedException("This look like double initialization to this object!");
					}
				}

				SendSpawnMessage(newObj);

			}

		}

		public void InstantiateObject<T>(T obj) where T : NetworkObject {

			if (!_networkObjects.ContainsValue(obj)) {

				obj.Initialize(obj.NetworkId);
				_syncIgnore.Add(obj.NetworkId);

				if (obj.gameObject.TryGetComponent<NetworkListener>(out var netListener)) {
					// This is a new player or someone who must see other networkObjects
					if (_networkListeners.AddWithoutDoubles(netListener)) {
						SyncWithInstance(netListener, obj);
					} else {
						throw new NotImplementedException("This look like double initialization to this object!");
					}
				}

				SendSpawnMessage(obj);

			} else {
				throw new NotImplementedException("This look like double initialization to this object!");
			}

		}

		public bool GetObject<T>(uint networkId, out T networkObject) where T : NetworkObject {
			bool result = _networkObjects.TryGetValue(networkId, out var obj);
			networkObject = obj as T;
			return result;
		}

		// Send immediately, we are do not want wait for next update
		private void SyncWithInstance<T>(NetworkListener listener, params T[] except) where T : NetworkObject {

			using (MsgBuffer buffer = new MsgBuffer(MsgType.Create_NetworkObjs, Qos.Reliable)) {

				ushort dataLenght = 0;
				ushort partsCount = 0;
				MsgStreamWriter writer = null;

				foreach (var obj in _networkObjects.Values) {
					if (!obj.IsOneOf(except)) {
						dataLenght = obj.SpawnDataLenght(out partsCount);
						if (dataLenght > 0) {
							writer = buffer.GetWriter(dataLenght);
							obj.WriteSpawnData(ref writer, partsCount);
						} else {
							continue;
						}
					}
				}

				var messages = buffer.GetMessages();

				if (messages.Count > 0) {
					for (int m = 0; m < messages.Count; m++) {
						Multihost.MsgSender.Send(messages[m].Buffer, listener.Connection.ConnectId, listener.Connection.HostId, Multihost.GetChanel(buffer.Qos));
					}
				}

			}

		}

		// Look on Sync
		private void SendSpawnMessage<T>(T target) where T : NetworkObject {

			// 90% - player. We can send spawn msg couple times without any problems.
			if (target.TryGetComponent<NetworkListener>(out var listener)) {

				var spawnMsg = new MsgStreamWriter(MsgType.Create_NetworkObjs);
				var lenght = target.SpawnDataLenght(out var parts);
				target.WriteSpawnData(ref spawnMsg, parts);
				Multihost.MsgSender.Send(spawnMsg.Buffer, listener.Connection.ConnectId, listener.Connection.HostId, Multihost.ReliableChanel);

			}

			var buffer = _buffersManager.GetBuffer(Qos.Reliable, MsgType.Create_NetworkObjs);
			var dataLenght = target.SpawnDataLenght(out var partsCount);
			var writer = buffer.GetWriter(dataLenght);
			target.WriteSpawnData(ref writer, partsCount);

		}

		public void DestroyObject(uint networkId) {
			if (_networkObjects.TryGetValue(networkId, out var networkObject)){
				DestroyObject(networkObject);
			}
		}

		public void DestroyObject<T>(T networkObject) where T : NetworkObject {

			if (_networkObjects.ContainsKey(networkObject.NetworkId)) {

				_syncIgnore.Add(networkObject.NetworkId);
				var buffer = _buffersManager.GetBuffer(Qos.Reliable, MsgType.Destroy_NetworkObjs);
				buffer.GetWriter(4).WriteUInt32(networkObject.NetworkId);

				// If this player or someone who must see other networkObjects - SEND THIS MESSAGE IMMEDIATELY
				if (networkObject.gameObject.TryGetComponent<NetworkListener>(out var listener) && _networkListeners.Contains(listener)) {
					_networkListeners.Remove(listener);
					// TODO - Attempt to send to not connected connection
					var destroyMsg = new MsgStreamWriter(MsgType.Destroy_NetworkObjs).WriteUInt32(networkObject.NetworkId).Buffer;
					Multihost.MsgSender.Send(destroyMsg, listener.Connection.ConnectId, listener.Connection.HostId, Multihost.ReliableChanel);
				}
				
				Object.Destroy(networkObject.gameObject);

			}

		}

		#endregion

		#region Public

		public void UpdateObjectNow(NetworkObject obj) {
			// TODO - проверка с кем нужно синхронизировать а с кем нет
			var buffer = _buffersManager.GetBuffer(Qos.Reliable, MsgType.Synchronize_NetworkObjs);
			var dataLenght = obj.SyncDataLenght_Full(out var partsCount);
			var writer = buffer.GetWriter(dataLenght);
			obj.WriteSyncData(ref writer, partsCount);
		}

		public override void RequestComponentUpdate(NetworkObject obj) {

			if (obj.gameObject.TryGetComponent<NetworkListener>(out var listener)) {
				using (var writer = new MsgStreamWriter(MsgType.Refresh_NetworkObjs)) {
					writer.WriteByte(0).WriteUInt32(obj.NetworkId);
					Multihost.MsgSender.Send(writer.Buffer, listener.Connection.ConnectId, listener.Connection.HostId, Multihost.ReliableChanel);
				}
			} else {
				obj.ObjectTargetlyUpdated();
			}
			
		}

		public override void OnCreate(NetworkObject obj, uint networkId = uint.MaxValue) {
			base.OnCreate(obj, networkId);
			// Just in case
			if (!_networkObjects.ContainsKey(obj.NetworkId)) {
				_networkObjects.Add(obj.NetworkId, obj);
				if (obj.HasSyncComponent<IRuntimeSync>()) {
					_runtimeSynchronizable.Add(obj.NetworkId);
				}
			} else {
				throw new NotImplementedException(string.Format("NetworkObjects already contains this key: {0}", obj.NetworkId));
			}
		}

		public override void OnDestroy(NetworkObject obj) {
			base.OnDestroy(obj);
			// Just in case
			_runtimeSynchronizable.Remove(obj.NetworkId);
			if (!_networkObjects.Remove(obj.NetworkId)) {
				throw new NotImplementedException(string.Format("NetworkObjects do not contains this key: {0} But you try Destroy it", obj.NetworkId));
			}
		}

		#endregion

		#region Syncing

		// TODO - Chunk system
		// TODO - Chunk bit three
		// TODO - Chunk leavers
		// TODO - Complex Sync Message type
		// TODO - Partial messages sending
		// TODO - delayed messages sending
		// TODO - specific message (msg must select QosType to send self)
		private bool _syncRelaible = true;

		public void FixedUpdate(float timeDelta) {

			ServerTick.MakeTick();

			if (_networkListeners.Count <= 0) {
				return;
			}

			OnStart_SyncTick?.Invoke();

			if (_syncRelaible) {

				OnStart_ReliableTick?.Invoke();
				CreateMessages(MsgType.Synchronize_NetworkObjs, Qos.Reliable);

				_syncIgnore.Clear();
				OnMessagesCreated_ReliableTick?.Invoke();

				SendMessages(Qos.Reliable, Qos.ReliableFragmented);
				OnEnd_ReliableTick?.Invoke();

			} else {

				OnStart_UnreliableTick?.Invoke();
				CreateMessages(MsgType.Synchronize_NetworkObjs, Qos.Unreliable);

				OnMessagesCreated_UnreliableTick?.Invoke();

				SendMessages(Qos.Unreliable, Qos.UnreliableFragmented);
				OnEnd_UnreliableTick?.Invoke();

			}

			OnEnd_SyncTick?.Invoke();

			_syncRelaible = !_syncRelaible;

			// Пока синхронизируются все со всеми
			// ОЧЕНЬ АККУРАТНО С ЭТИМИ 2мя МЕТОДАМИ!!!!!!!!!!!

			void CreateMessages(short msgType, params Qos[] chanels) {

				ushort partsCount = 0;
				ushort dataLenght = 0;
				MsgStreamWriter writer = null;

				for (int i = 0; i < chanels.Length; i++) {

					var b = _buffersManager.GetBuffer(chanels[i], msgType);

					foreach (var obj in (from id in _runtimeSynchronizable where !id.IsOneOf(_syncIgnore) select _networkObjects[id])) {

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

					// TODO - combines
					// Now all messages send to all users!!!!
					int chanel = Multihost.GetChanel(b.Qos);

					foreach (var msg in b.GetMessages()) {

						for (int l = _networkListeners.Count - 1; l >= 0; l--) {
							Multihost.MsgSender.Send(msg.Buffer, _networkListeners[l].Connection.ConnectId, _networkListeners[l].Connection.HostId, chanel);
						}

					}

				}

				_buffersManager.RemoveBuffer(buffers.ToArray());

			}

		}

		#endregion

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


namespace Transport.Messaging {

	using System;
	using System.Linq;
	using System.Text;
	using System.Collections.Generic;

	public delegate void OnReciveNetMsg(NetworkMsg msg);

	public class MsgReader {
		
		#region Listeners
		public bool WeHaweSumHandlers { get { return _handlers != null && _handlers.Count > 0; } }
		private Dictionary<short, OnReciveNetMsg> _handlers = new Dictionary<short, OnReciveNetMsg>();
		private List<OneShotHandler> _oneShotHandlers = new List<OneShotHandler>();
		#endregion

		/// Read data event
		public void Read(ResivedMsg msg) {
			Read(msg, msg.recBuffer);
		}

		public void Read(ResivedMsg msg, byte[] sideBufer) {
			NetworkMsg recievedMsg = new NetworkMsg(msg.connectionId, msg.recHostId, new MsgStreamReader(sideBufer, 0, msg.dataSize));
			short msgId = recievedMsg.reader.ReadInt16();
			CallHandler(msgId, recievedMsg);
		}

		/// Add listener
		public void AddHandler(short msgId, OnReciveNetMsg listener) {
			try {
				_handlers[msgId] += listener;
			} catch {
				_handlers.Add(msgId, null);
				_handlers[msgId] += listener;
			}
		}

		public void AddOneShotHandler(short msgId, OnReciveNetMsg listener) {
			_oneShotHandlers.Add(new OneShotHandler(msgId, listener));
		}

		/// Remove listener
		public void RemoveHandler(short msgID, OnReciveNetMsg listener = null) {
			if (listener == null) {
				_handlers.Remove(msgID);
			} else if (_handlers.TryGetValue(msgID, out var handler)){
				handler -= listener;
			}
		}

		public void RemoveOneShotHandler(short msgID, OnReciveNetMsg listener) {
			if (_oneShotHandlers.ContaitsWhere(x => x.MsgId == msgID && x.Listener == listener, out var handler)) {
				_oneShotHandlers.Remove(handler);
			}
		}

		/// Remove all listeners
		public void ClearHandlers() {
			_handlers.Clear();
			_oneShotHandlers.Clear();
		}

		/// Call listener - local (testing)
		public void InvokeHandler(short msgId, int connectId, int hostId, byte[] msgBytes) {
			CallHandler(msgId, connectId, hostId, msgBytes);
		}

		/// Call registered listener
		private void CallHandler(short msgId, int connectId, int hostId, byte[] msgBytes) {
			CallHandler(msgId, new NetworkMsg(connectId, hostId, msgBytes));
		}

		private void CallHandler(short msgId, NetworkMsg msg) {

			if (_handlers.TryGetValue(msgId, out var handler)) {
				handler?.Invoke(msg);
			}

			var oneShot = (from h in _oneShotHandlers where h.MsgId == msgId select h).ToList();
			oneShot.ForEach(h => { h.Listener?.Invoke(msg); _oneShotHandlers.Remove(h); });

		}

		private class OneShotHandler {

			public readonly short MsgId;
			public readonly OnReciveNetMsg Listener;

			public OneShotHandler(short msgId, OnReciveNetMsg listener) {
				MsgId = msgId; Listener = listener;
			}

		}

	}

	#region MsgBase

	// Сообщение которое считывает сервер или клиент
	public class ResivedMsg {
		public int recHostId;
		public int connectionId;
		public int channelId;
		public byte[] recBuffer = new byte[1024];
		public int bufferSize = 1024;
		public int dataSize;
		public byte error;
	}

	// Сообщение в которое конвертируется сообщение выше
	public class NetworkMsg {

		public int hostId;
		public int connectID;
		public MsgStreamReader reader;

		public NetworkMsg(int connectID, int hostId, byte[] msgBuffer) {
			this.hostId = hostId;
			this.connectID = connectID;
			reader = new MsgStreamReader(msgBuffer);
		}

		public NetworkMsg(int connectID, int hostId, MsgStreamReader reader) {
			this.hostId = hostId;
			this.connectID = connectID;
			this.reader = reader;
		}

	}

	public class MsgStreamReader : IDisposable {

		public byte[] Buffer;
		private int _readIndex = 0;

		public int DataLeft => (Buffer.Length-1) - _readIndex;

		public void Skip(int lenght) {
			_readIndex += lenght;
		}

		public void SkipString() => SkipArray();

		public void SkipArray() {
			var lenght = ReadInt32();
			_readIndex += lenght;
		}

		public void MoveBack(int lenght) {
			_readIndex -= lenght;
			_readIndex = _readIndex < 0 ? 0 : _readIndex;
		}

		public MsgStreamReader(byte[] buffer, int startIndex = 0, int lenght = 0) {
			if (lenght == 0) {
				Buffer = buffer;
			} else {
				Buffer = new byte[lenght];
				Array.Copy(buffer, startIndex, Buffer, 0, lenght);
			}
		}

		public short ReadInt16() {
			_readIndex += 2;
			return BitConverter.ToInt16(Buffer, _readIndex - 2);
		}
		public int ReadInt32() {
			_readIndex += 4;
			return BitConverter.ToInt32(Buffer, _readIndex - 4);
		}
		public long ReadInt64() {
			_readIndex += 4;
			return BitConverter.ToInt64(Buffer, _readIndex - 4);
		}
		public ushort ReadUInt16() {
			_readIndex += 2;
			return BitConverter.ToUInt16(Buffer, _readIndex - 2);
		}
		public uint ReadUInt32() {
			_readIndex += 4;
			return BitConverter.ToUInt32(Buffer, _readIndex - 4);
		}
		public ulong ReadUInt64() {
			_readIndex += 4;
			return BitConverter.ToUInt64(Buffer, _readIndex - 4);
		}
		public float ReadSingle() {
			_readIndex += 4;
			return BitConverter.ToSingle(Buffer, _readIndex - 4);
		}
		public double ReadDouble() {
			_readIndex += 8;
			return BitConverter.ToDouble(Buffer, _readIndex - 8);
		}
		public byte ReadByte() {
			_readIndex += 1;
			return Buffer[_readIndex - 1];
		}
		public byte[] ReadByteArray() {
			var lenght = ReadInt32();
			_readIndex += lenght;
			return Buffer.Skip(_readIndex - lenght).Take(lenght).ToArray();
		}
		public string ReadString() {
			var lenght = ReadInt32();
			_readIndex += lenght;
			return Encoding.UTF8.GetString(Buffer, _readIndex - lenght, lenght);
		}
		public void Dispose() {
			Buffer = new byte[0]; _readIndex = 0;
		}

	}

	public class MsgStreamWriter : IDisposable {

		public byte[] Buffer { get => _buffer.ToArray(); }
		public ushort Lenght => (ushort)_buffer.Count;
		private List<byte> _buffer = new List<byte>();

		public MsgStreamWriter() {
			_buffer = new List<byte>();
		}
		public MsgStreamWriter(short msgId) {
			_buffer = new List<byte>();
			WriteInt16(msgId);
		}

		public MsgStreamWriter WriteInt16(short value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteInt32(int value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteInt64(long value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteUInt16(ushort value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteUInt32(uint value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteUInt64(ulong value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteSingle(float value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteDouble(double value) {
			_buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		public MsgStreamWriter WriteByte(byte value) {
			_buffer.Add(value);
			return this;
		}
		public MsgStreamWriter WriteByteArray(byte[] value) {
			WriteInt32(value.Length);
			_buffer.AddRange(value);
			return this;
		}
		public MsgStreamWriter WriteSolidByteArray(byte[] value) {
			_buffer.AddRange(value);
			return this;
		}
		public MsgStreamWriter WriteString(string value) {
			value = string.IsNullOrEmpty(value) ? " " : value;
			var stringBuffer = Encoding.UTF8.GetBytes(value);
			WriteInt32(stringBuffer.Length);
			_buffer.AddRange(stringBuffer);
			return this;
		}

		public static ushort StringLenght(string value) {
			return (ushort)(string.IsNullOrEmpty(value) ? Encoding.UTF8.GetBytes(" ").Length : Encoding.UTF8.GetBytes(value).Length);
		}
		public static ushort WriteStringLenght(string value) {
			return (ushort)((string.IsNullOrEmpty(value) ? Encoding.UTF8.GetBytes(" ").Length : Encoding.UTF8.GetBytes(value).Length) + 4);
		}

		public void Dispose() {
			_buffer.Clear();
			_buffer = new List<byte>(0);
		}
	}

	#endregion

}


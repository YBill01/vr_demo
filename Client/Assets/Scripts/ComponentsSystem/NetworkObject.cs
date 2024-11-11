
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Transport.Client;
using Transport.Universal;
using Transport.Messaging;

using Qos = UnityEngine.Networking.QosType;

[DefaultExecutionOrder(311)]
public abstract class NetworkObjects : Manager, IDisposable {

	protected MsgReader _msgReader = null;
	protected Dictionary<uint, NetworkObject> _networkObjects = new Dictionary<uint, NetworkObject>();

	#region Initializing

	public virtual T Initialize<T>(MsgReader reader) where T : NetworkObjects {

		_msgReader = reader ?? throw new NullReferenceException("Reader cant be Null type");

		_msgReader.AddHandler(MsgType.Create_NetworkObjs, Network_ObjectCreate);
		_msgReader.AddHandler(MsgType.Destroy_NetworkObjs, Network_ObjectDestroy);
		_msgReader.AddHandler(MsgType.Refresh_NetworkObjs, Network_ObjectRefresh);
		_msgReader.AddHandler(MsgType.Synchronize_NetworkObjs, Network_ObjectSync);

		return this as T;

	}
	public virtual void Dispose() {

		if (_msgReader != null) {
			_msgReader.RemoveHandler(MsgType.Create_NetworkObjs, Network_ObjectCreate);
			_msgReader.RemoveHandler(MsgType.Destroy_NetworkObjs, Network_ObjectDestroy);
			_msgReader.RemoveHandler(MsgType.Refresh_NetworkObjs, Network_ObjectRefresh);
			_msgReader.RemoveHandler(MsgType.Synchronize_NetworkObjs, Network_ObjectSync);
		}

	}

	#endregion

	#region NetworkHandlers

	protected abstract void Network_ObjectCreate(NetworkMsg msg);
	protected abstract void Network_ObjectDestroy(NetworkMsg msg);

	protected virtual void Network_ObjectRequest(NetworkMsg msg) {
		if (_networkObjects.TryGetValue(msg.reader.ReadUInt32(), out var obj) && obj is IRequested requestListener) {
			requestListener.ReadRequest(msg.reader);
		}
	}

	protected virtual void Network_ObjectSync(NetworkMsg msg) {
		// For couple objects
		while (msg.reader.DataLeft > 2) {
			if (_networkObjects.TryGetValue(msg.reader.ReadUInt32(), out var obj)) {
				obj.ReadSyncData(msg.reader);
			} 
			else {
				throw new System.NotImplementedException("Wrong sync data on object!");
			}
		}
		msg.reader.Dispose();
	}
	protected virtual void Network_ObjectRefresh(NetworkMsg msg) {
		// 0 - request of update | 1 - update result
		if (msg.reader.ReadByte() == 0) { // Create update data
			uint networkId = msg.reader.ReadUInt32();
			if (_networkObjects.TryGetValue(networkId, out var obj)) {
				var writer = new MsgStreamWriter(MsgType.Refresh_NetworkObjs);
				writer.WriteByte(1); // write as result
				obj.SyncDataLenght_Full(out var dataPartsCount);
				obj.WriteSyncData(ref writer, dataPartsCount, true);
				Multihost.MsgSender.Send(writer.Buffer, msg.connectID, msg.hostId, Multihost.ReliableChanel);
			} else {
				throw new NotImplementedException("Wrong sync request!");
			}
		} else { // read update data
			uint networkId = msg.reader.ReadUInt32();
			if (_networkObjects.TryGetValue(networkId, out var obj)) {
				obj.ReadSyncData(msg.reader);
				obj.ObjectTargetlyUpdated();
			} else {
				throw new NotImplementedException("Wrong sync result on target update! Object not found.");
			}
		}
		msg.reader.Dispose();
	}

	#endregion

	#region Public

	public virtual void OnCreate(NetworkObject obj, uint networkId = uint.MaxValue) {
		if (networkId == uint.MaxValue) {
			obj.NetworkId = Increment.Value;
		} else {
			obj.NetworkId = networkId;
			Increment.AddBan(networkId);
		}
	}
	public virtual void OnDestroy(NetworkObject obj) {
		Increment.Value = obj.NetworkId;
	}
	public abstract void RequestComponentUpdate(NetworkObject obj);

	#endregion

}

// Instantiate_on_Server > Initialize_on_Server > Instantiate_on_Client > Initialize_on_Client > ApplySyncData
[DefaultExecutionOrder(312)]
public class NetworkObject : MonoBehaviour {

	public uint ObjectId;
	[ReadOnly] public uint NetworkId;
	public List<NetworkComponent> Components = new List<NetworkComponent>();
	private List<ISync> _synchronizableComponents = new List<ISync>();

	#region Editor
#if UNITY_EDITOR
	[SerializeField, ReadOnly]
	private int _componentsCount = 0;
	[ContextMenu("Refresh ComponentsID")]
	private void RefreshComponentsID() {
		if (_componentsCount == 0) {
			if (Components.Count > 1) {
				_componentsCount = Components.Count;
				return;
			}
		}
		for (ushort i = 1; i < Components.Count+1; i++) {
			Components[i-1].ComponentId = i;
		}
		_componentsCount = Components.Count;
	}
#endif
	#endregion

	#region Instancing

	// ??????? ?? ?????????? ????? ??????????
	// ??????? ?? ??????? ????-??????????
	// ????????? ?? ??????? ?? ???????

	public virtual void Initialize(uint networkId = uint.MaxValue) {
		foreach (var c in Components) {
			if (c != null) {
				c.InitComponent();
				if (c is ISync s) {
					_synchronizableComponents.Add(s);
				}
			}
		}
		Game.GetManager<Client_NetworkObjects>().OnCreate(this, networkId);
	}
	public virtual void DestroyObject() {
		Components.ForEach(x => x?.DestroyComponent());
		Game.GetManager<Client_NetworkObjects>().OnDestroy(this);
	}
	protected virtual void OnDestroy() {
		DestroyObject();
	}

	#endregion

	#region Components

	public bool HasComponent<T>() where T : NetworkComponent {
		return Components.ContaitsWhere(x => x is T);
	}
	public T GetNetworkComponent<T>() where T : NetworkComponent {
		return Components.Find(x => x is T) as T;
	}
	public bool HasSyncComponent<T>() where T : ISync {
		return _synchronizableComponents.ContaitsWhere(x => x is T);
	}

	#region UpdatingRequest

	private Action _updatingCallback;

	public void RequestAnUpdate(Action onDone) {

		if (_updatingCallback == null) {
			_updatingCallback = onDone;
		} else {
			_updatingCallback += onDone;
		}

#if CLIENT
		Game.GetManager<Client_NetworkObjects>().RequestComponentUpdate(this);
#elif SERVER
		Game.GetManager<Server_NetworkObjects>().RequestComponentUpdate(this);
#endif

	}

	public void ObjectTargetlyUpdated() {
		_updatingCallback?.Invoke(); _updatingCallback = null;
	}

#endregion



#endregion

#region Syncing

	protected ushort _count = 0;
	protected ushort _lenght = 0;
	protected ushort _lstLenght = 0;

	/// First you need to check the existence of this date => SpawnDataLenght
	public virtual ushort SpawnDataLenght(out ushort componentsToSync) {

		_lenght = _lstLenght = _count = 0;

		foreach (var c in Components) {
			_lstLenght = c.SpawnDataLenght;
			_count += _lstLenght > 0 ? (ushort)1 : (ushort)0;
			_lenght += _lstLenght;
		}

		componentsToSync = _count;
		return _lenght;

	}
	public virtual void WriteSpawnData(ref MsgStreamWriter writer, ushort dataPartsCount) {
		// NetworkId, ComponentId, ComponentsCount <= we get it previo
		writer.WriteUInt32(NetworkId).WriteUInt32(ObjectId).WriteUInt16(dataPartsCount);
		// Write data to object
		foreach (var x in Components) { x.CreateSpawnData(ref writer); }
	}
	public virtual void ReadSpawnData(MsgStreamReader reader, bool skipSync = false) {
		// Reader read NetworkId & ObjectId before this moment
		ushort dataPartsCount = reader.ReadUInt16();
		// Apply Spawn data to components
		for (ushort i = 0; i < dataPartsCount; i++)
		{
			ushort componentId = reader.ReadUInt16();
			if (Components.ContaitsWhere(x => x.ComponentId == componentId, out var component)) {
				DebugText.Instance.AddText("Init spawn data on component id: " + componentId);
				component.ApplySpawnData(reader);
			} else {
				Debug.LogWarning(string.Format("Cant find component in this object: {0}", gameObject.name));
				break;
			}
		}
	}
	
	/// First you need to check the existence of this date => SyncDataLenght
	public virtual ushort SyncDataLenght(Qos qos, out ushort componentsToSync) {

		_lenght = _lstLenght = _count = 0;

		foreach (var x in (from c in _synchronizableComponents where c.SyncChannel == qos select c)) {
			_lstLenght = x.DataLenght;
			_count += _lstLenght > 0 ? (ushort)1 : (ushort)0;
			_lenght += _lstLenght;
		}

		componentsToSync = _count;
		return _lenght;

	}
	public virtual ushort SyncDataLenght_Full(out ushort componentsToSync) {

		_lenght = _lstLenght = _count = 0;

		foreach (var x in _synchronizableComponents) {
			_lstLenght = x.DataLenght;
			_count += _lstLenght > 0 ? (ushort)1 : (ushort)0;
			_lenght += _lstLenght;
		}

		componentsToSync = _count;
		return _lenght;

	}

	// Write all possible data
	public virtual void WriteSyncData(ref MsgStreamWriter writer, ushort dataPartsCount, bool writeAnyway = false) {
		// NetworkId, ComponentsCount <= we get it previo
		writer.WriteUInt32(NetworkId).WriteUInt16(dataPartsCount);
		foreach (var x in _synchronizableComponents) {
			x.CreateSyncData(ref writer, writeAnyway);
		}
	}
	// Write all possible data for channel
	public virtual void WriteSyncData(Qos qos, ref MsgStreamWriter writer, ushort dataPartsCount, bool writeAnyway = false) {
		// NetworkId, ComponentsCount <= we get it previo
		writer.WriteUInt32(NetworkId).WriteUInt16(dataPartsCount);
		// Write data to object
		foreach (var x in (from c in _synchronizableComponents where c.SyncChannel == qos select c)) {
			x.CreateSyncData(ref writer, writeAnyway);
		}
	}
	// Write data from target components
	public virtual void WriteSyncData<T>(Qos qos, ref MsgStreamWriter writer, ushort dataPartsCount, bool writeAnyway = false) where T : class, ISync {
		// NetworkId, ComponentsCount <= we get it previo
		writer.WriteUInt32(NetworkId).WriteUInt16(dataPartsCount);
		// Write data to object
		foreach (var x in (from c in _synchronizableComponents where c is T && c.SyncChannel == qos select c as T)) {
			x.CreateSyncData(ref writer, writeAnyway);
		}
	}

	public virtual void ReadSyncData(MsgStreamReader reader, bool skipSync = false) {
		// Reader read NetworkId before this moment
		ushort dataPartsCount = reader.ReadUInt16();
		// Apply Sync data to components
		for (ushort i = 0; i < dataPartsCount; i++) {
			ushort componentId = reader.ReadUInt16();
			if (Components.ContaitsWhere(x => x.ComponentId == componentId, out var c) && c is ISync s) {
				s.ApplySyncData(reader, skipSync);
			} else {
				if (!Components.ContaitsWhere(x => x.ComponentId == componentId)) {
					Debug.LogWarning(string.Format("Cant find component in this object: {0}", gameObject.name));
				} else {
					Debug.LogWarning(string.Format("Component not ISync type! ObjName: {0}", gameObject.name));
				}
				break;
			}
		}
	}

#endregion

}




[DefaultExecutionOrder(313)]
public abstract class NetworkComponent : MonoBehaviour {

	[ReadOnly] public ushort ComponentId;
	[ReadOnly] public bool EnableSyncing;

	public virtual void InitComponent() { }
	public virtual void DestroyComponent() { }
	
	public virtual ushort SpawnDataLenght { get => 2; }
	public virtual void ApplySpawnData(MsgStreamReader reader, bool skipSync = false) {
		// Reader read ComponentId before this moment
	}
	public virtual void CreateSpawnData(ref MsgStreamWriter writer) {
		writer.WriteUInt16(ComponentId);
	}
	
}

public interface ISync {
	// Reliable or Unreliable
	Qos SyncChannel { get; } 
	ushort DataLenght { get; }
	void ApplySyncData(MsgStreamReader reader, bool skipSync = false);
	void CreateSyncData(ref MsgStreamWriter writer, bool writeAnyway = false);
}

public interface IRuntimeSync : ISync {
	bool HasSyncData { get; }
}

public interface ISelfRequestedSync : ISync {
	void GetInLineToUpdate();
}

public interface ISideRequestedSync : ISync {
	void UpdateObject(Action onDone = null);
}

public interface IRequested {
	void ReadRequest(MsgStreamReader reader);
}

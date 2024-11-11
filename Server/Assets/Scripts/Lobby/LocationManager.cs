


namespace Transport.Universal {


	using System.Collections;
	using System.Collections.Generic;
	using Transport.Messaging;
	using Transport.Server;
	using UnityEngine;
	using System.Linq;

	public class LocationManager : Manager {

		private MsgReader _msgReader;

		private ByteIncrement _increment = new ByteIncrement();
		private Dictionary<byte, Location> _locations = new Dictionary<byte, Location>();

		public override void Final() {
			base.Final();
			SubscribeHandlers();
		}

		public void TryJoinToLocation(NetworkMsg msg) {

			// PlayerNetworkId
			// Join type - 0 (with location id) 1 (with instance uid)
			// location or instance id

			// Create or find location
			
			uint payerId = msg.reader.ReadUInt32();
			Location location = null;

			if (msg.reader.ReadByte() == 0) {
				byte instanceId = msg.reader.ReadByte();
				if (_locations.TryGetValue(instanceId, out location)) {
					// Go next
				} else {
					Debug.LogError(string.Format("Instance location do not exist! {0}", instanceId));
					return;
				}
			} else {
				ushort locationId = msg.reader.ReadUInt16();
				location = _locations.Values.FirstOrDefault(x => x.LocationId == locationId);
				if (location == default) {
					location = CreateLocation(locationId);
				}
			}

			// Join

			// Create player networck object
			// Send location info back to player -> in client sizde load location
			
			// Player request sync with location -> prepare location and start listening network updates

		}

		private Location CreateLocation(ushort locationId) {
			var locationObject = Resources.Load(string.Format("Locations/Location_{0}", locationId), typeof(Location)) as Location;
			var location = Object.Instantiate(locationObject, Vector3.up * (256 * 100), Quaternion.identity);
			location.Initialize(_increment.Value);
			_locations.Add(location.InstanceId, location);
			return location;
		}

		#region NetworkHnadlers

		private void SubscribeHandlers() {
			_msgReader ??= Multihost.MsgReader;

			_msgReader.AddHandler(MsgType.JoinToLobby, TryJoinToLocation);

			_msgReader.AddHandler(MsgType.Create_NetworkObjs, Network_ObjectCreate);
			_msgReader.AddHandler(MsgType.Destroy_NetworkObjs, Network_ObjectDestroy);
			_msgReader.AddHandler(MsgType.Refresh_NetworkObjs, Network_ObjectRefresh);
			_msgReader.AddHandler(MsgType.Synchronize_NetworkObjs, Network_ObjectSync);
		}

		private void UnsubscribeHandlers() {
			_msgReader ??= Multihost.MsgReader;

			_msgReader.RemoveHandler(MsgType.JoinToLobby, TryJoinToLocation);

			_msgReader.RemoveHandler(MsgType.Create_NetworkObjs, Network_ObjectCreate);
			_msgReader.RemoveHandler(MsgType.Destroy_NetworkObjs, Network_ObjectDestroy);
			_msgReader.RemoveHandler(MsgType.Refresh_NetworkObjs, Network_ObjectRefresh);
			_msgReader.RemoveHandler(MsgType.Synchronize_NetworkObjs, Network_ObjectSync);
		}

		private void Network_ObjectCreate(NetworkMsg msg) {
			byte locationId = msg.reader.ReadByte();
			if (_locations.TryGetValue(locationId, out var location)) {
				location.NetworkObjects.Network_ObjectCreate(msg);
			}
		}
		private void Network_ObjectDestroy(NetworkMsg msg) {
			byte locationId = msg.reader.ReadByte();
			if (_locations.TryGetValue(locationId, out var location)) {
				location.NetworkObjects.Network_ObjectDestroy(msg);
			}
		}
		private void Network_ObjectRefresh(NetworkMsg msg) {
			byte locationId = msg.reader.ReadByte();
			if (_locations.TryGetValue(locationId, out var location)) {
				location.NetworkObjects.Network_ObjectRefresh(msg);
			}
		}
		private void Network_ObjectSync(NetworkMsg msg) {
			byte locationId = msg.reader.ReadByte();
			if (_locations.TryGetValue(locationId, out var location)) {
				location.NetworkObjects.Network_ObjectSync(msg);
			}
		}

		#endregion

	}

	// TODO - normal increments
	public class ByteIncrement {

		private byte _lst = 0;
		private List<byte> _openList = new List<byte>();

		public byte Value {
			get {
				if (_openList.Count > 0) {
					var v = _openList[_openList.Count - 1];
					_openList.RemoveAt(_openList.Count - 1);
					return v;
				} else {
					return (_lst += 1);
				}
			}
			set {
				_openList.Add(value);
			}
		}
	}

	public class Location : MonoBehaviour {

		[SerializeField]
		private ushort _locationId;
		public ushort LocationId { get => _locationId; }

		[SerializeField]
		private NetworkObject _playerPrefab;
		public NetworkObject PlayerPrefab { get => _playerPrefab; }

		public byte InstanceId { get; private set; }
		public Vector3 Ancor { get; private set; }
		public Location_NetworkObjects NetworkObjects { get; private set; }

		public void Initialize(byte instanceId) {

			InstanceId = instanceId;
			Ancor = Vector3.up * (instanceId * 100);
			transform.position = Ancor;

			NetworkObjects = new Location_NetworkObjects();
			NetworkObjects.Final();

		}

#if UNITY_EDITOR

		[ContextMenu("MarkLocationObjects")]
		private void MarkLocation() {
			Debug.Log("Mark");
		}

#endif

	}

}


namespace Transport.Universal {
	
	using UnityEngine;

	[RequireComponent(typeof(NetworkObject))]
	public class NetworkListener : MonoBehaviour {
		[HideInInspector] public ushort ChunkId;
		[HideInInspector] public Connection Connection;
	}

}
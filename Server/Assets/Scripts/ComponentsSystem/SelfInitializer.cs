
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class SelfInitializer : MonoBehaviour {

	// TODO - make this correct!
#if SERVER
	private void Awake() {
		Invoke("Initialize", 5f);
		
	}
	private void Initialize() {
		var netObj = GetComponent<NetworkObject>();
		netObj.NetworkId = Increment.Value;
		Game.GetManager<Transport.Server.Server_NetworkObjects>().InstantiateObject(netObj);
	}

#endif

}

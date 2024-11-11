
using UnityEngine;

public class ServerTick : MonoBehaviour {

	public static ushort Tick = 0;

	public static void MakeTick() {
		if ((Tick += 1) == ushort.MaxValue) {
			Tick = 0;
		}
	}

}

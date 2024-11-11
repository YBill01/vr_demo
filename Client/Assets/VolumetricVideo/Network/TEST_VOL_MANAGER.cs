
using Newtonsoft.Json;
using System;
using UnityEngine;
using static VVPlaybackInfo;

public class TEST_VOL_MANAGER : MonoBehaviour {

	public TextAsset Sequence;
	public VolPlayer PlayerNetComponent;

	private bool _isSet = false;

#if SERVER

	private void Update() {
		if (DateTime.UtcNow.Minute % 3 == 0 && !_isSet) {

			var info = new VVPlaybackInfo(Sequence.text);
			info.sequences.startDate = DateTime.UtcNow;

			JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ" };
			//sequences = JsonConvert.DeserializeObject<Sequences>(info, jsonSettings);
			string str = JsonConvert.SerializeObject(info.sequences, jsonSettings);
			PlayerNetComponent.SetSequenceInfo(str);
			Debug.Log("Play video - Init");
			_isSet = true;

		} else if(DateTime.UtcNow.Minute % 3 != 0) {
			_isSet = false;
		}
	}

#endif

}

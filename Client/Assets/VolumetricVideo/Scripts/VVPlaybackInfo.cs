using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class VVPlaybackInfo
{
	public Sequences sequences;

	public VVPlaybackInfo(string info)
	{

		if (string.IsNullOrEmpty(info) || string.IsNullOrWhiteSpace(info)) {
			return;
		}

		Debug.Log(info);
		JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ" };
		sequences = JsonConvert.DeserializeObject<Sequences>(info, jsonSettings);

		Debug.Log(string.Format("S IS NULL: {0}", sequences == null));
		Debug.Log(string.Format("FRAMES IS NULL: {0}", sequences.sequences == null));
		Debug.Log(string.Format("Count: {0}", sequences.sequences.Count));

		Init();

	}

	public void Init()
	{
		double t = 0;
		for (int i = 0; i < sequences.sequences.Count; i++)
		{
			sequences.sequences[i].startTime = sequences.startDate.AddSeconds(t);
			t += sequences.sequences[i].duration;
			sequences.sequences[i].endTime = sequences.startDate.AddSeconds(t);
		}
	}

	[Serializable]
	public class Sequences
	{
		public DateTime startDate;
		public List<Sequence> sequences;

	}

	[Serializable]
	public class Sequence
	{
		public string name;
		public float duration;

		[NonSerialized]
		public DateTime startTime;
		[NonSerialized]
		public DateTime endTime;
	}
}
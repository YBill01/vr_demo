using System;
using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	[Serializable]
	public struct VVHostInfo
	{
		public string url;
		[Space(10)]
		public string name;
		[TextArea(2, 4)]
		public string description;
	}
}
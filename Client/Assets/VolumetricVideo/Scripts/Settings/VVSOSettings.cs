using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	[CreateAssetMenu(menuName = "AEstAR/VolumetricVideo/VolumetricVideo Settings", fileName = "VVSettings")]
	public class VVSOSettings : ScriptableObject
	{

		public VVHostInfo[] hosts;

		public LoadingMethodType loadingMethod;
		public enum LoadingMethodType
		{
			Streaming = 0,
			Sequencing = 1
		}





	}
}
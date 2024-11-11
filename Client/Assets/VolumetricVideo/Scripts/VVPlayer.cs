using System;
using System.Collections;
using unity4dv;
using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	[RequireComponent(typeof(VVView))]
	public class VVPlayer : MonoBehaviour, IVVPlayer
	{

		[SerializeField]
		public VVSOSettings settings;

		//[NonSerialized]
		public VVHostInfo hostInfo;

		private VVView _view;
		public IVVView view => _view;

		public VVPlaybackState state => _playback.state;

		public Plugin4DS plugin4DS;
		public Plugin4DS plugin4DS2;

		//public DateTime serverTime;
		[SerializeField]
		public string sequenceJSONInfo;

		public Plugin4DS activePlagin4DS;
		

		private VVPlaybackBase _playback;

		private TimeSpan _correctionTimeSpan;

		public DateTime time => DateTime.UtcNow - _correctionTimeSpan;
		public VVPlaybackInfo playbackInfo;

		private string _prevInfoText = string.Empty;

		private void Start()
		{
			//plugin4DS.gameObject.SetActive(false);

			_view = GetComponent<VVView>();

			//yield return new WaitForEndOfFrame();



			Init(sequenceJSONInfo);


			VVPlayerEvents.ReadyToInit?.Invoke(this);
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;

			if (_playback != null)
			{
				_playback.Update(ref deltaTime);
			}

		}

		public void Init(string info)
		{
			//_correctionTimeSpan = DateTime.UtcNow - serverTime;

			if (IsValid(info))
			{
				Debug.LogWarning("Not walid info!");
				return;
			}

			_prevInfoText = info;

			playbackInfo = new VVPlaybackInfo(info);

			switch (settings.loadingMethod)
			{
				case VVSOSettings.LoadingMethodType.Streaming:

					SetPlayback(new VVPlaybackStreaming(this));

					break;

				case VVSOSettings.LoadingMethodType.Sequencing:

					SetPlayback(new VVPlaybackSequencing(this));

					break;
			}

			_playback.Init();
		}



		/*public void Init(DateTime serverTime, VVPlaybackInfo aplaybackInfo)
		{
			_correctionTimeSpan = DateTime.UtcNow - serverTime;

			playbackInfo = aplaybackInfo;

			switch (settings.loadingMethod)
			{
				case VVSOSettings.LoadingMethodType.Streaming:

					SetPlayback(new VVPlaybackStreaming(this));

					break;

				case VVSOSettings.LoadingMethodType.Sequencing:

					SetPlayback(new VVPlaybackSequencing(this));

					break;
			}

			_playback.Init();
		}*/

		public bool IsValid(string info)
		{
			return false;
			if (info == _prevInfoText)
			{
				return true;
			}

			return false;
		}

		public IVVView GetView()
		{
			return _view;
		}

		public void Play()
		{
			_playback.TryPlay();
		}

		public void Stop()
		{
			_playback.Stop();
		}

		public void Reset()
		{
			if (_playback != null)
			{
				//_playback.Reset();
				_playback = null;
			}


		}



		private void SetPlayback(VVPlaybackBase playback)
		{
			_playback = playback;
		}

		
	}
}
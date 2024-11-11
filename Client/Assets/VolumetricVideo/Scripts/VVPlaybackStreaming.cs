using unity4dv;
using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	public class VVPlaybackStreaming : VVPlaybackBase
	{


		public VVPlaybackStreaming(VVPlayer vvPlayer) : base(vvPlayer)
		{

			Debug.Log("PlaybackStreaming");
		}

		public override void Init()
		{
			base.Init();

			//_vvPlayer.plugin4DS.


		}

		protected override void InitPlugin4DS()
		{
			base.InitPlugin4DS();

			return;

			/*_vvPlayer.plugin4DS.SourceType = unity4dv.SOURCE_TYPE.Network;

			switch (state)
			{
				case VVPlaybackState.Waiting:



					break;

				case VVPlaybackState.Playing:

					_vvPlayer.plugin4DS.gameObject.SetActive(true);
					_vvPlayer.plugin4DS.SequenceDataPath = $"{_vvPlayer.hostInfo.url}/{_vvPlayer.playbackInfo.sequences.sequences[currentSequence].name}";

					_vvPlayer.plugin4DS.Initialize();
					_vvPlayer.plugin4DS.Play(true);

					break;

				case VVPlaybackState.Finished:

					_vvPlayer.plugin4DS.Stop();
					_vvPlayer.plugin4DS.gameObject.SetActive(false);

					break;
			}*/

		}


		protected override void PrePlay(Plugin4DS plugin4DS, bool next = false)
		{
			plugin4DS.SourceType = SOURCE_TYPE.Network;

			plugin4DS.SequenceName = $"{_vvPlayer.hostInfo.url}/{_vvPlayer.playbackInfo.sequences.sequences[currentSequence + (next ? 1 : 0)].name}";

			plugin4DS.Initialize(true);

			Debug.Log($"PrePlay::{currentSequence}");
		}
		protected override void Play(Plugin4DS plugin4DS)
		{
			int frame = (int)((((_vvPlayer.time - _vvPlayer.playbackInfo.sequences.sequences[currentSequence].startTime).TotalSeconds) / _vvPlayer.playbackInfo.sequences.sequences[currentSequence].duration) * _vvPlayer.plugin4DS.LastActiveFrame);

			plugin4DS.GotoFrame(frame);
			plugin4DS.Play(true);

			plugin4DS.gameObject.SetActive(true);

			Debug.Log($"Play::{currentSequence}");
		}

	}
}
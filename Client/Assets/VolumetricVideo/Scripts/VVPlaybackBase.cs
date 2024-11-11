using System;
using System.Collections;
using System.Collections.Generic;
using unity4dv;
using UnityEngine;
using System.Linq;

namespace AEstAR.VolumetricVideo
{
	public class VVPlaybackBase
	{
		protected VVPlayer _vvPlayer;

		public VVPlaybackState state;

		protected int currentSequence;

		protected DateTime _timeMark;

		protected Plugin4DS[] _plugins4DS;

		protected Plugin4DS _prePlayPlugin4DS;
		protected Plugin4DS _playPlugin4DS;

		protected bool _isPlaying;

		public VVPlaybackBase(VVPlayer vvPlayer)
		{
			_vvPlayer = vvPlayer;

		}

		public virtual void Init()
		{
			_plugins4DS = new Plugin4DS[2];
			_plugins4DS[0] = _vvPlayer.plugin4DS;
			_plugins4DS[1] = _vvPlayer.plugin4DS2;

			_playPlugin4DS = _plugins4DS[0];
			_prePlayPlugin4DS = _plugins4DS[1];

			_vvPlayer.activePlagin4DS = _vvPlayer.plugin4DS;




			InitPlugin4DS();


			UpdateCurrentSequence();
			//CheckState();

			//currentSequence = GetCurrentSequence();
		}

		protected virtual void InitPlugin4DS()
		{
			foreach (var item in _plugins4DS)
			{
				ResetPlugin4DS(item);
			}


			

			//_vvPlayer.plugin4DS.Initialize(true);

			//return;

			/*Debug.Log("----------------");
			Debug.Log(_vvPlayer.plugin4DS.ActiveNbOfFrames);
			Debug.Log(_vvPlayer.plugin4DS.FirstActiveFrame);
			Debug.Log(_vvPlayer.plugin4DS.LastActiveFrame);
			Debug.Log(_vvPlayer.plugin4DS.CurrentFrame);
			Debug.Log(_vvPlayer.plugin4DS.SequenceNbOfFrames);
			Debug.Log(_vvPlayer.plugin4DS.ChunkBufferSize);
			Debug.Log(_vvPlayer.plugin4DS.Framerate);*/

		}

		//private DateTime _timer;

		private void Plugin4DSOnFirstFrame(int eventFrame)
		{
			//_timer = DateTime.UtcNow;

			/*Debug.Log("----------------");
			Debug.Log(_vvPlayer.plugin4DS.ActiveNbOfFrames);
			Debug.Log(_vvPlayer.plugin4DS.FirstActiveFrame);
			Debug.Log(_vvPlayer.plugin4DS.LastActiveFrame);
			Debug.Log(_vvPlayer.plugin4DS.CurrentFrame);
			Debug.Log(_vvPlayer.plugin4DS.SequenceNbOfFrames);
			Debug.Log(_vvPlayer.plugin4DS.ChunkBufferSize);
			Debug.Log(_vvPlayer.plugin4DS.Framerate);*/

			//_vvPlayer.plugin4DS.GotoFrame(280);
			//_vvPlayer.plugin4DS.CurrentFrame = 280;

			//throw new NotImplementedException();
		}

		private void Plugin4DSOnLastFrame(int eventFrame)
		{
			//_vvPlayer.plugin4DS.Close();

			//TimeSpan timeSpan = DateTime.UtcNow - _timer;
			//Debug.Log(timeSpan.TotalMilliseconds);

			//throw new NotImplementedException();
		}

		

		public virtual void ResetPlugin4DS(Plugin4DS plugin4DS)
		{
			//_vvPlayer.plugin4DS.OnFirstFrame.RemoveListener(Plugin4DSOnFirstFrame);
			//_vvPlayer.plugin4DS.OnLastFrame.RemoveListener(Plugin4DSOnLastFrame);

			//_vvPlayer.plugin4DS.Stop();
			plugin4DS.gameObject.SetActive(false);

			plugin4DS.Loop = false;
			plugin4DS.AutoPlay = false;
			plugin4DS.SequenceName = string.Empty;
			//_vvPlayer.plugin4DS.OnFirstFrame.AddListener(Plugin4DSOnFirstFrame);
			//_vvPlayer.plugin4DS.OnLastFrame.AddListener(Plugin4DSOnLastFrame);
			plugin4DS._dataInStreamingAssets = false;
			plugin4DS.Close();
			plugin4DS.PreviewFrame = -1;
			plugin4DS.LastActiveFrame = -1;
		}

		public void Update(ref float deltaTime)
		{
			if (!_isPlaying)
			{
				return;
			}

			CheckState();


			// to streaming...
			if (state == VVPlaybackState.Playing)
			{
				/*if (_prePlayPlugin4DS == null)
				{
					foreach (var item in _plugins4DS)
					{
						if (!item.IsInitialized)
						{
							_prePlayPlugin4DS = item;

							break;
						}
					}
				}*/

				if (_playPlugin4DS.IsInitialized && _playPlugin4DS.CurrentFrame >= _playPlugin4DS.SequenceNbOfFrames - 1)
				{
					ResetPlugin4DS(_playPlugin4DS);

					if (currentSequence >= _vvPlayer.playbackInfo.sequences.sequences.Count - 1)
					{
						return;
					}

					Play(_prePlayPlugin4DS);

					Plugin4DS playPlugin4DST = _playPlugin4DS;
					_playPlugin4DS = _prePlayPlugin4DS;
					_prePlayPlugin4DS = playPlugin4DST;

					Debug.Log("Play done...");
				}

				if (!_prePlayPlugin4DS.IsInitialized && _playPlugin4DS.IsPlaying && _playPlugin4DS.CurrentFrame + _playPlugin4DS.ChunkBufferSize >= _playPlugin4DS.SequenceNbOfFrames)
				{
					currentSequence++;
					PrePlay(_prePlayPlugin4DS, true);

					Debug.Log("Next load...");
				}


				//Debug.Log(_vvPlayer.activePlagin4DS.ActiveNbOfFrames);
				//Debug.Log(_vvPlayer.activePlagin4DS.SequenceNbOfFrames);
				//Debug.Log(_vvPlayer.activePlagin4DS.ChunkBufferSize);


			}

		}

		protected void CheckState()
		{
			//UpdateCurrentSequence();

			VVPlaybackState sState;

			if (_vvPlayer.time < _vvPlayer.playbackInfo.sequences.startDate)
			{
				sState = VVPlaybackState.Waiting;
			}
			else if (_vvPlayer.time >= _vvPlayer.playbackInfo.sequences.startDate && _vvPlayer.time < _vvPlayer.playbackInfo.sequences.sequences.Last().endTime)
			{
				sState = VVPlaybackState.Playing;
			}
			else
			{
				sState = VVPlaybackState.Finished;
			}

			if (state != sState)
			{
				state = sState;

				ChangeState();
			}
		}
		protected void ChangeState()
		{


			VVPlayerEvents.ChangeState?.Invoke(_vvPlayer);

			if (state == VVPlaybackState.Playing)
			{
				PrePlay(_playPlugin4DS);
				Play(_playPlugin4DS);
			}
			else if (state == VVPlaybackState.Finished)
			{
				foreach (var item in _plugins4DS)
				{
					ResetPlugin4DS(item);
				}
			}
		}


		protected virtual void PrePlay(Plugin4DS plugin4DS, bool next = false)
		{

		}
		protected virtual void Play(Plugin4DS plugin4DS)
		{
			
		}
		public virtual bool TryPlay()
		{
			if (_isPlaying)
			{
				return true;
			}

			_isPlaying = true;

			UpdateCurrentSequence();

			return true;
		}
		public virtual void Stop()
		{
			if (_isPlaying)
			{
				_isPlaying = false;
			}

			state = VVPlaybackState.Paused;

			if (_playPlugin4DS != null && _playPlugin4DS.IsInitialized)
			{
				_playPlugin4DS.Close();
			}
		}

		protected void UpdateCurrentSequence()
		{
			int sCurrentSequence = GetCurrentSequence();

			if (currentSequence != sCurrentSequence)
			{
				currentSequence = sCurrentSequence;



			}
		}

		protected int GetCurrentSequence()
		{
			int result = 0;

			DateTime t = _vvPlayer.time;

			for (int i = 0; i < _vvPlayer.playbackInfo.sequences.sequences.Count; i++)
			{
				if (t >= _vvPlayer.playbackInfo.sequences.sequences[i].startTime && t < _vvPlayer.playbackInfo.sequences.sequences[i].endTime)
				{
					result = i;

					break;
				}
			}

			return result;
		}


	}
}
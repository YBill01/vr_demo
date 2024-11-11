using AEstAR.VolumetricVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VVServiceTest : MonoBehaviour
{
	[SerializeField]
	private TextAsset sequenceJSONInfo;


	public DateTime time;

	private IVVPlayer _vvPlayer;


	private void Start()
	{
		VVPlayerEvents.ReadyToInit += OnVVPlayerReadyToInit;
		VVPlayerEvents.ChangeState += OnVVPlayerChangeState;

		time = DateTime.UtcNow;
	}

	

	private void Update()
	{
		time = DateTime.UtcNow;
	}

	private void OnVVPlayerReadyToInit(IVVPlayer vvPlayer)
	{
		_vvPlayer = vvPlayer;

		InitVVPlayer();
	}
	private void OnVVPlayerChangeState(IVVPlayer vvPlayer)
	{


		Debug.Log(vvPlayer.state);
	}

	public void InitVVPlayer()
	{
		SetNewTime();

		//_vvPlayer.Init(time, new VVPlaybackInfo(sequenceJSONInfo.text));
	}

	public void SetNewTime()
	{
		_vvPlayer.Reset();

		VVPlaybackInfo vVPlaybackInfo = new VVPlaybackInfo(sequenceJSONInfo.text);
		vVPlaybackInfo.sequences.startDate = DateTime.UtcNow.AddSeconds(5);
		vVPlaybackInfo.Init();

		//_vvPlayer.Init(time, vVPlaybackInfo);




	}

	public void SetNewTime2()
	{
		_vvPlayer.Reset();

		VVPlaybackInfo vVPlaybackInfo = new VVPlaybackInfo(sequenceJSONInfo.text);
		vVPlaybackInfo.sequences.startDate = DateTime.UtcNow.AddSeconds(-28);
		vVPlaybackInfo.Init();

		//_vvPlayer.Init(time, vVPlaybackInfo);




	}

}
using AEstAR.VolumetricVideo;
using System;

public interface IVVPlayer
{
	//void Init(DateTime time, VVPlaybackInfo playbackInfo);
	void Init(string info);

	IVVView view { get; }

	VVPlaybackState state { get; }

	void Play();

	void Stop();

	void Reset();

}
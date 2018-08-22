using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Video : MonoBehaviour {

	[SerializeField] VideoPlayer videoPlayer;
	public string videoUrl;

    public void Setup(string url)
	{
		videoUrl = url;
		videoPlayer.url = videoUrl;
		videoPlayer.Prepare();
		videoPlayer.frame = 0;
    }
    
	public void Play()
	{
		videoPlayer.Play();
	}

    public void Pause()
	{
		videoPlayer.Pause();
	}

    public void SetTime(float value)
	{
		videoPlayer.time = value;
	}

    public float GetVideoTime()
	{
		return (float)videoPlayer.time;
	}

    public float GetVideoLenght()
	{
		return videoPlayer.frameCount / videoPlayer.frameRate;
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class Video : MonoBehaviour {

	[SerializeField] VideoPlayer videoPlayer;
	public string videoUrl;
	public string filename;

    public void Setup(string url)
	{
		videoUrl = url;
		videoPlayer.url = videoUrl;
		videoPlayer.Prepare();
		videoPlayer.frame = 0;
		filename = Path.GetFileName(url).Split('.')[0];
		//videoPlayer.prepareCompleted += VideoPlayer_PrepareCompleted;

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

	//void VideoPlayer_PrepareCompleted(VideoPlayer source)
	//{
		//videoPlayer.frame = (int)(videoPlayer.frameRate / videoPlayer.time);
    //}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerPanel : MonoBehaviour {

	[SerializeField] Sprite playIcon, pauseIcon;
	[SerializeField] Image playPauseBtnImage;
	[SerializeField] Text currentTime;
	[SerializeField] Text videoLenght;
	[SerializeField] Slider slider;

	bool sliderSelected;

	Video video;
	bool playing;

	void Start()
	{
		video = GetComponent<Video>();
		//videoLenght.text = video.GetVideoLenght().ToString();
	}   

	void Update()
	{
		TimeSpan time = TimeSpan.Zero;
		if (playing)
		{
			time = TimeSpan.FromSeconds(video.GetVideoTime());
			currentTime.text = time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00")  + ":" + time.Milliseconds.ToString()[0];
		}
		time = TimeSpan.FromSeconds(video.GetVideoLenght());
		videoLenght.text = time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00") + ":" + time.Milliseconds.ToString()[0];
        
		if(!sliderSelected)
		{
			slider.maxValue = video.GetVideoLenght();
			slider.value = video.GetVideoTime();
        }
	}

    public void OnSliderChanged()
	{
		if(sliderSelected)
		    video.SetTime(slider.value);
	}

	public void PlayPauseBtn()
	{
		if (playing)
		{
			playPauseBtnImage.sprite = playIcon;
			video.Pause();
		}
		else
		{
			playPauseBtnImage.sprite = pauseIcon;
			video.Play();
		}

		playing = !playing;
    }

	public void SetSliderSelected(bool selected)
	{
		sliderSelected = selected;
	}
}
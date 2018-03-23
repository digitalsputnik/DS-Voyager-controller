using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationControllerScripts : MonoBehaviour {

    public tempAnimcontroller anim;
	public Button PlayButton;
	public Button PauseButton;


    public void TaskPlayButtonClick()
    {
		anim.layer.state = playbackState.play;
		PlayButton.gameObject.SetActive (false);
		PauseButton.gameObject.SetActive (true);
    }

	public void TaskPauseButtonClick()
	{
		anim.layer.state = playbackState.pause;
		PauseButton.gameObject.SetActive (false);
		PlayButton.gameObject.SetActive (true);
	}

    public void TaskStopButtonClick()
    {
        anim.layer.state = playbackState.stop;
    }

    public void TaskRepeatButtonClick()
    {
        anim.layer.mode = anim.layer.mode == playbackMode.loop? playbackMode.hold: playbackMode.loop ;
    }

	public void TaskNextButtonClick()
	{
		//TODO: run next animation
	}
}

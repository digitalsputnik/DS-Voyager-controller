using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultColorScript : MonoBehaviour {

    public ColorWheel colorwheel;
    public Button DefaultColorButton;

	// Use this for initialization
	void Start () {
        DefaultColorButton.onClick.AddListener(SetDefaultColor);
	}

    private void SetDefaultColor()
    {
        colorwheel.IntensitySlider.value = 50f;
        colorwheel.TemperatureSlider.value = 5600f;
        colorwheel.SaturationSlider.value = 0f;
        colorwheel.HueSlider.value = 0f;
        colorwheel.updateValues();
    }

    // Update is called once per frame
    void Update () {
		
	}
}

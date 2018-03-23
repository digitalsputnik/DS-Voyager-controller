using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlusMinusScripts : MonoBehaviour {

    public Slider ControlledSlider;
    public float increment = 1.0f;
	public float tincrement = 100.0f;
    public Button PlusButton { get; set; }
    public Button MinusButton { get; set; }

    // Use this for initialization
    void Start () {
        //Get buttons
        PlusButton = transform.Find("PlusButton").GetComponent<Button>();
        MinusButton = transform.Find("MinusButton").GetComponent<Button>();
        
        //Add listeners to buttons
        PlusButton.onClick.AddListener(OnPlusClick);
        MinusButton.onClick.AddListener(OnMinusClick);
    }

    private void OnMinusClick()
    {
		if (ControlledSlider.gameObject.name == "TemperatureSlider") {
			ControlledSlider.value = ControlledSlider.value - tincrement;
		} else {
			ControlledSlider.value = ControlledSlider.value - increment;
		}
    }

    private void OnPlusClick()
    {
		if (ControlledSlider.gameObject.name == "TemperatureSlider") {
			ControlledSlider.value = ControlledSlider.value + tincrement;
		} else {
			ControlledSlider.value = ControlledSlider.value + increment;
		}
    }
}

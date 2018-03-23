using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ColorButtonScript : MonoBehaviour, IPointerClickHandler {

    public tempAnimcontroller anim;
    public GameObject colorTool;
	public GameObject menuCanvas;

	public int iVal;
	public int tVal;
	public int sVal;
	public int hVal;

	Color rgbColor;
	float HColor;
	float SColor;
	float VColor;


    public void OnPointerClick(PointerEventData eventData)
    {
		//hide menu
		menuCanvas.SetActive(false);
        //Open colorwheel!
        ColorWheel ct = colorTool.GetComponent<ColorWheel>();
        ct.SetActive(true);
		ct.activator = gameObject.name;

		//save initial colors
		ct.initialIVal = iVal;
        ct.initialTVal = tVal;
        ct.initialSVal = sVal;
        ct.initialHVal = hVal;

        ct.iVal = iVal;
        ct.tVal = tVal;
        ct.sVal = sVal;
        ct.hVal = hVal;

        ct.updateValues();

        ct.IntensitySlider.value = (float)iVal;
		ct.TemperatureSlider.value = (float)tVal;
		ct.SaturationSlider.value = (float)sVal;
		ct.HueSlider.value = (float)hVal;


    }
}

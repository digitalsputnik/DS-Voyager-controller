using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorMenuImageScript : MonoBehaviour {

    private Slider ISlider;
    private Slider TSlider;
    private Slider SSlider;
    private Slider HSlider;

    public void SetImageColor()
    {
        SetImageColor(ISlider.value, TSlider.value, SSlider.value, HSlider.value);
    }

    public void SetImageColor(float I, float T, float S, float H)
    {
        this.GetComponent<Image>().color = Color.HSVToRGB(H, S, I);
    }

    private void Start()
    {
        Transform ITSHMenuSliders = this.transform.parent.Find("Sliders");
        ISlider = ITSHMenuSliders.Find("ISlider").GetComponentInChildren<Slider>();
        TSlider = ITSHMenuSliders.Find("TSlider").GetComponentInChildren<Slider>();
        SSlider = ITSHMenuSliders.Find("SSlider").GetComponentInChildren<Slider>();
        HSlider = ITSHMenuSliders.Find("HSlider").GetComponentInChildren<Slider>();
    }
}

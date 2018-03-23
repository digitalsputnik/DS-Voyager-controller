using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SecondColorButtonScript : MonoBehaviour, IPointerClickHandler {

    public tempAnimcontroller anim;
    public GameObject colorTool;

	Color rgbColor;
	float HColor;
	float SColor;
	float VColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        ColorWheel ct = colorTool.GetComponent<ColorWheel>();
        ct.SetActive(true);
		rgbColor = GetComponent<Image> ().color;
		Color.RGBToHSV (rgbColor, out HColor, out SColor, out VColor);

		ct.IntensitySlider.value = VColor * 100;
		ct.SaturationSlider.value = SColor * 100;
		ct.HueSlider.value = HColor * 360;
		ct.TemperatureSlider.value = 3200;


        //setInput ITSH
        //ct.setValues(anim.secI, anim.secT, anim.secS, anim.secH);

		ct.activator = this.gameObject.name;
    }

    public void SetColor(float H, float S, float V)
    {
        this.gameObject.GetComponent<Image>().color = Color.HSVToRGB(H, S, V);
    }
}

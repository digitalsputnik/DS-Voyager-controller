using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorWheel : MonoBehaviour {

	[Header("Menu")]
	public GameObject menuCanvas;
	[Header("Buttons")]
	public Button OkButton;
	public Button CancelButton;
	public Button CopyButton;
	public Button PasteButton;
    [Header("Sliders")]
    public Slider IntensitySlider;
    public Slider TemperatureSlider;
    public Slider SaturationSlider;
    public Slider HueSlider;
    [Header("External buttons")]
    public GameObject SetupButton;
    [Header("Tools")]
	public DrawMode drawTool;
    public AnimationSender animSender;
	[Header("Color Panel")]
	public GameObject colorPanel;
    [Header("Color buttons")]
    public Image ColorButton;
    public Image SecondaryColorButton;

    float CameraFieldOfView;
    Vector3 CameraInitialPosition;
	Transform colorName;

    [System.Serializable]
	public class ClickHandeledObjects { 
		public GameObject cross;
		public GameObject hueAndSatWheel;
		public GameObject brightnessWheel;
		public GameObject WhiteblanceWheel;
	}

	public ClickHandeledObjects clickable;
	[System.Serializable]
	public class Labels {
		public Text iLabel;
		public Text tLabel;
		public Text sLabel;
		public Text hLabel;
	}
	public Labels labels;
	[Header("Values")]
	public int iVal;
	public int tVal;
	public int sVal;
	public int hVal;
	[Header("Temp")]
	public string activator;

    public int initialIVal;
    public int initialTVal;
    public int initialSVal;
    public int initialHVal;

    public bool disableColorWheel;

    // Use this for initialization
    void Start () {
        IntensitySlider.onValueChanged.AddListener(OnIntensitySliderValueChanged);
        TemperatureSlider.onValueChanged.AddListener(OnTemperatureSliderValueChanged);
        SaturationSlider.onValueChanged.AddListener(OnSaturationSliderValueChanged);
        HueSlider.onValueChanged.AddListener(OnHueSliderValueChanged);
		OkButton.onClick.AddListener(TaskOkClick);
		CancelButton.onClick.AddListener(TaskCancelClick);
		CopyButton.onClick.AddListener(CopyColor);
		PasteButton.onClick.AddListener(PasteColor);

		if (!HasColorPasteAvailable())
			PasteButton.interactable = false;
    }

	private void OnIntensitySliderValueChanged(float arg0)
	{
		iVal = (int)IntensitySlider.value;
		colorName = colorPanel.transform.Find (activator);
		colorName.gameObject.GetComponent<ColorButtonScript> ().iVal = iVal;
		updateValues();
		SetAnimationValues();
		//TODO: Use new properties!
		animSender.SendAnimationWithUpdate();
	}

	private void OnTemperatureSliderValueChanged(float arg0)
    {
		tVal = (int)TemperatureSlider.value;
		colorName = colorPanel.transform.Find (activator);
		colorName.gameObject.GetComponent<ColorButtonScript> ().tVal = tVal;
		updateValues();
		SetAnimationValues();
		//TODO: Use new properties!
		animSender.SendAnimationWithUpdate();
    }

	private void OnSaturationSliderValueChanged(float arg0)
	{
		sVal = (int)SaturationSlider.value;
		colorName = colorPanel.transform.Find (activator);
		colorName.gameObject.GetComponent<ColorButtonScript> ().sVal = sVal;
		updateValues();
		SetAnimationValues();
		//TODO: Use new properties!
		animSender.SendAnimationWithUpdate();
	}

	private void OnHueSliderValueChanged(float arg0)
	{
		hVal = (int)HueSlider.value;
		colorName = colorPanel.transform.Find (activator);
		colorName.gameObject.GetComponent<ColorButtonScript> ().hVal = hVal;
		updateValues();
		SetAnimationValues();
		//TODO: Use new properties!
		animSender.SendAnimationWithUpdate();
	}

	private void TaskOkClick(){
		drawTool.SetActive (true);
		SetupButton.SetActive(true);
		//unhide menu
		menuCanvas.SetActive (true);

		GameObject cButton = GameObject.Find (activator);
        drawTool.setITSH(iVal, tVal, sVal, hVal);
        cButton.GetComponent<Image>().color = drawTool.uiColor;

        ////set layer ITSH color
        //if (activator == "Color1")
        //{
        //    drawTool.setITSH(iVal, tVal, sVal, hVal);

        //    cButton.GetComponent<Image>().color = drawTool.uiColor;
        //}
        //else if (activator == "Color2")
        //{
        //    //drawTool.anim.secI = iVal;
        //    //drawTool.anim.secT = tVal;
        //    //drawTool.anim.secS = sVal;
        //    //drawTool.anim.secH = hVal;
        //    //very around the corner RGB values for the button
        //    int tempI = drawTool.iVal;
        //    int tempT = drawTool.tVal;
        //    int tempS = drawTool.sVal;
        //    int tempH = drawTool.hVal;
        //    drawTool.setITSH(iVal, tVal, sVal, hVal);
        //    Color secondaryUiColor = drawTool.uiColor;
        //    drawTool.setITSH(tempI, tempT, tempS, tempH);

        //    //set color button
        //    drawTool.auxColor.GetComponent<Renderer>().materials[1].SetColor("_Color", secondaryUiColor);

        //    cButton.GetComponent<Image>().color = secondaryUiColor;
        //}

        animSender.SendAnimationWithUpdate();

		//Update pixels
		//drawTool.updatePixels();

		//hide colorwheel
		SetActive (false);
	}

	private void TaskCancelClick(){
		iVal = initialIVal;
		tVal = initialTVal;
		sVal = initialSVal;
		hVal = initialHVal;
        colorName = colorPanel.transform.Find(activator);
        colorName.gameObject.GetComponent<ColorButtonScript>().iVal = iVal;
        colorName.gameObject.GetComponent<ColorButtonScript>().tVal = tVal;
        colorName.gameObject.GetComponent<ColorButtonScript>().sVal = sVal;
        colorName.gameObject.GetComponent<ColorButtonScript>().hVal = hVal;
        animSender.SendAnimationWithUpdate();

        SetAnimationValues();
		drawTool.setITSH(iVal, tVal, sVal, hVal);
		//drawTool.updatePixels();

		drawTool.SetActive (true);
		SetupButton.SetActive(true);
		//unhide menu
		menuCanvas.SetActive (true);
		//hide colorwheel
		SetActive (false);

		//TODO remove temp
		//drawTool.anim.animTag=false;
	}

    public void DisableColorWheel()
    {
        disableColorWheel = true;
    }

    public void EnableColorWheel()
    {
        disableColorWheel = false;
    }



    // Update is called once per frame
    void Update () {
		if (Input.GetMouseButton(0))
            if (EventSystem.current.IsPointerOverGameObject())
                return;

        if (Input.touchCount > 0)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

		Camera.main.fieldOfView = 60;
		Camera.main.transform.position = CameraInitialPosition;
		if (!Input.GetMouseButton (0))
			return;

        if (disableColorWheel)
            return;

		RaycastHit hit;
		if (!Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 100))
			return;

		//color values
		if (hit.transform.gameObject == clickable.hueAndSatWheel) {
			Vector2 pixelUV = hit.textureCoord;	
			pixelUV.x = (pixelUV.x - 0.5f) * 1.553f * 2;
			pixelUV.y = (pixelUV.y - 0.5f) * 1.553f * 2;

			hVal = ((int)cart2float (pixelUV.x, pixelUV.y));
			sVal = ((int)(Mathf.Sqrt (Mathf.Pow (pixelUV.x, 2) + Mathf.Pow (pixelUV.y, 2)) * 120));

			HueSlider.value = (float)hVal;
			SaturationSlider.value = (float)sVal;

			updateValues ();
		
		}

		//brightness value
		//if (hit.transform.gameObject == clickable.brightnessWheel) {
		//	Vector2 pixelUV = hit.textureCoord2;
		//	hit.transform.GetComponent<Renderer> ().material.SetFloat ("_visible", pixelUV.x);
		//	iVal = (int)(pixelUV.x * 100);
		//	updateValues ();
		//}

		iVal = (int)IntensitySlider.value;
		tVal = (int)TemperatureSlider.value;
		sVal = (int)SaturationSlider.value;
		hVal = (int)HueSlider.value;
		updateValues ();
		SetAnimationValues ();
		//drawTool.setITSH(iVal, tVal, sVal, hVal);
		//drawTool.updatePixels();

		if (hit.transform.gameObject == clickable.WhiteblanceWheel) {
			clickable.cross.SetActive (false);
			hVal = 0;
			sVal = 0;
			HueSlider.value = (float)hVal;
			SaturationSlider.value = (float)sVal;
			updateValues ();
		}
	}

    
	float cart2float(float xCord, float yCord) {
		//case 1st quad
		if (xCord >= 0 && yCord >= 0)
			return Mathf.Atan (xCord / yCord) * (180 / Mathf.PI);
		//case 2nd quad
		if (xCord >= 0 && yCord <= 0)
			return Mathf.Atan (xCord / yCord) * (180 / Mathf.PI) + 180;
		//case 3rd quad
		if (xCord <= 0 && yCord <= 0)
			return Mathf.Atan (xCord / yCord) * (180 / Mathf.PI) + 180;
		//case 4th quad
		return Mathf.Atan (xCord / yCord) * (180 / Mathf.PI) + 360;
	}

	public void setValues(int inIVal, int inTVal, int inSVal, int inHVal) {
		//set var
		iVal = inIVal;
		tVal = inTVal;
		sVal = inSVal;
		hVal = inHVal;
		//set ui
		updateValues();
	}

	public void updateValues() {
		labels.iLabel.text = iVal.ToString()+"%";
		labels.tLabel.text = tVal.ToString()+"K 0";
		labels.sLabel.text = sVal.ToString()+"%";
		labels.hLabel.text = hVal.ToString()+"º";

		if (sVal >= 0) {
			//move the cross

			float rotax = 0;
			float rotay = 0;

			float yCord = sVal * Mathf.Cos (hVal * Mathf.PI / 180f);
			float xCord = sVal * Mathf.Sin (hVal * Mathf.PI / 180f);

			//sectors
			if (xCord < 0) 
				rotax = xCord * 0.49f;
			if (xCord > 0)
				rotax = xCord * 0.57f;

			rotay = yCord * 0.6f;

			//rotate the cross
			clickable.cross.SetActive (true);
			clickable.cross.transform.eulerAngles = new Vector3 (0, 100 - rotax, 0 + rotay);

		} 

	}
	public void SetActive(bool inVal) {
        if (inVal)
        {
            CameraFieldOfView = Camera.main.fieldOfView;
            CameraInitialPosition = Camera.main.transform.position;
        }
        else
        {
            Camera.main.fieldOfView = CameraFieldOfView;
        }

        //transform (geometry)
		this.gameObject.SetActive(inVal);
        //initial colors
        initialIVal = iVal;
        initialTVal = tVal;
        initialSVal = sVal;
        initialHVal = hVal;
		//buttons
		OkButton.gameObject.SetActive(inVal);
		CancelButton.gameObject.SetActive (inVal);

	}

    public void SetAnimationValues()
    {
        if (activator == "Color1")// (activator == "main")
        {
            //drawTool.anim.oldI = iVal;
            //drawTool.anim.oldT = tVal;
            //drawTool.anim.oldS = sVal;
            //drawTool.anim.oldH = hVal;
        }
        else if (activator == "Color2") // (activator == "sec")
        {
            //drawTool.anim.secI = iVal;
            //drawTool.anim.secT = tVal;
            //drawTool.anim.secS = sVal;
            //drawTool.anim.secH = hVal;
        }
    }

    public void CopyColor()
	{
		PlayerPrefs.SetInt("color_buf_I", iVal);
		PlayerPrefs.SetInt("color_buf_T", tVal);
		PlayerPrefs.SetInt("color_buf_S", sVal);
		PlayerPrefs.SetInt("color_buf_H", hVal);
		PasteButton.interactable = true;
	}

    public void PasteColor()
	{
		colorName = colorPanel.transform.Find(activator);
        ColorButtonScript cBtnScript = colorName.gameObject.GetComponent<ColorButtonScript>();

		iVal = PlayerPrefs.GetInt("color_buf_I");
        tVal = PlayerPrefs.GetInt("color_buf_T");
        sVal = PlayerPrefs.GetInt("color_buf_S");
        hVal = PlayerPrefs.GetInt("color_buf_H");
        
        cBtnScript.iVal = iVal;
        cBtnScript.tVal = tVal;
        cBtnScript.sVal = sVal;
        cBtnScript.hVal = hVal;

        updateValues();
        SetAnimationValues();
        animSender.SendAnimationWithUpdate();
	}

    public void ClearColorPaste()
	{
		PlayerPrefs.DeleteKey("color_buf_I");
		PlayerPrefs.DeleteKey("color_buf_T");
		PlayerPrefs.DeleteKey("color_buf_S");
        PlayerPrefs.DeleteKey("color_buf_H");
	}

    bool HasColorPasteAvailable()
	{
		if (PlayerPrefs.HasKey("color_buf_I") &&
			PlayerPrefs.HasKey("color_buf_T") &&
			PlayerPrefs.HasKey("color_buf_S") &&
			PlayerPrefs.HasKey("color_buf_H"))
			return true;

		return false;
	}
}

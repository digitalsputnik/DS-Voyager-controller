using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawMode : MonoBehaviour {

	[Header("Buttons")]
	public GameObject toolButton;
	public GameObject colorButton;
	public GameObject drawButtons;
    public GameObject setupButton;
	[Header("Tools")]
	public GameObject colorTool;
    public SetupMode setupTool;
    public Dropdown BrushDropdown;
    //TODO: Detection tool?
	[Header("Page state")]
	private bool active;

	private bool paintLamp;

	[Space (10)]
	[Header("Color Value")]
	public int iVal;
	public int tVal;
	public int sVal;
	public int hVal;
	[Header("Raw Color Values")]
	public Vector4 rawRGBW;
	[Header("UI Color")]
	public Color uiColor;
	[Header("Temp")]
    //public tempAnimcontroller anim;
    public GameObject auxColor;
    public AnimationSender animSender;
    public DrawScripts drawScripts;
    [Header("Stroke list")]
    public Transform StrokeListMenu;
    public GameObject StrokeOptionTemplate;
    public GameObject ActiveStroke;
    public Text StrokeText;

	Pixel lastPixel;
	int numCurrentPixel = 0;
	int numLastPixel = 0;
    float colorIntensityOffset = 0.3f;
	GameObject currentLamp;
	GameObject lastLamp;
	Transform temp;

    //private RaycastHit lastHit;

    public static int StrokeIndex = 1;

    // Use this for initialization
    void Start () {
        //run active script based on the input
        //Debug.Log("DrawMode started........");
        this.SetActive(active);
	}
	
	// Update is called once per frame
	void Update () {
		//TODO temp for long press
		if(Input.GetMouseButtonUp (0)) {

			numCurrentPixel = 0;
			numLastPixel = 0;
			currentLamp = null;
			lastLamp = null;
			//test for fire

			//test for blinker
		}

		//Check for mouse down and ray hit
		if (!Input.GetMouseButton (0))
			return;

        if (Input.touchCount == 2)
        {
            return;

        }
        else

        //if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer) {
        if (Input.GetMouseButtonDown(0))
        {

            //clicking on lights
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100) && hit.transform.tag == "lamp")
            {
                //Debug.Log("OBJECT THAT WAS HIT IS: " + hit.transform.name + " ......................");
                //Why was this done? Temporarily disabling the check as its intefering with video stream handling
                //as Canvas applied to videoStreamBackground is an EventSystem gameobject.
                //if (!EventSystem.current.IsPointerOverGameObject())
                //{
                    //Debug.Log("Lamp pixel was hit.........................");
                    //hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.white; 
                    currentLamp = hit.transform.parent.parent.gameObject;
                    lastLamp = currentLamp;
                    paintLamp = true;
                //}
            }
            else
            {
                //Debug.Log("OBJECT THAT WAS HIT IS: " + hit.transform.name + " ......................");
            }
        }
 
				
			
/*		} else {
			if (Input.touchCount == 1 && TouchPhase.Began) {
				//clicking on lights
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 100) && hit.transform.tag == "lamp") {
					currentLamp = hit.transform.parent.gameObject.transform.parent.gameObject;
					lastLamp = currentLamp;
					paintLamp = true;
				}
			}
		}*/

		if (paintLamp && Input.GetMouseButton (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast (ray, out hit, 100);
			if (Physics.Raycast (ray, out hit, 100) && hit.transform.tag == "lamp") {

				if (BrushDropdown.value == 0) { //If lamp brush!
					var lamp = hit.transform.parent.parent;
					for (int i = 0; i < lamp.childCount; i++) {
						Pixel pixel = lamp.GetChild (i).GetComponent<Pixel> ();
						if (pixel != null) {
                            animSender.ActiveStroke.AddPixel(pixel);
						}
					}
                    animSender.SendAnimationWithUpdate();
                } else if (BrushDropdown.value == 1) { //IF pixel brush!
					//updateLight (hit.transform.gameObject);
					//get current Lamp
					currentLamp = hit.transform.parent.parent.gameObject;
					int pixelsToWrite = 0;
					Pixel current = hit.transform.parent.gameObject.GetComponent<Pixel> ();
					//For smooth drawing -- DONT CHANGE NAMES OF PIXELS
					int nameLength = current.name.Length;
					if (nameLength == 7) {
						numCurrentPixel = System.Convert.ToInt32 (current.name.Substring (5, 2));
					} else {
						numCurrentPixel = System.Convert.ToInt32 (current.name.Substring (5, 1));
					}
					//Debug.Log ("Current Pixel number is: "+numCurrentPixel);
					//Debug.Log ("Last Pixel number is: "+numLastPixel);

					//if different lamp, start counting again
					if (currentLamp.name != lastLamp.name){
						numLastPixel = 0;
					}

					//find number of pixels to write
					if (numLastPixel > 0 && numLastPixel != numCurrentPixel && numCurrentPixel > numLastPixel) {
						pixelsToWrite = numCurrentPixel - numLastPixel;
					} else if(numLastPixel > 0 && numLastPixel != numCurrentPixel && numLastPixel > numCurrentPixel) {
						pixelsToWrite = numLastPixel - numCurrentPixel;
					}
					if (pixelsToWrite > 1) {
				
						for (int i = 0; i <= pixelsToWrite; i++) {
							//Debug.Log ("Inside Loop.........");
							string pix;
							if (numCurrentPixel > numLastPixel) {
								pix = "pixel" + (numLastPixel + i);
							} else {
								pix = "pixel" + (numLastPixel - i);
							}
							//Debug.Log ("Finding Pixel "+pix);
							temp = current.gameObject.transform.parent.Find (pix);
							current = temp.gameObject.GetComponent<Pixel> ();

                            //add pixels to array
                            animSender.ActiveStroke.AddPixel(current);
						}

					} else {
                        //Debug.Log ("Writing one Pixel!");

                        //add pixels to array
                        animSender.ActiveStroke.AddPixel(current);

						numLastPixel = numCurrentPixel;
						lastLamp = currentLamp;
					}

                    animSender.SendAnimationWithUpdate();

                } else if (BrushDropdown.value == 3) {//Select stroke option
                    Pixel current = hit.transform.parent.gameObject.GetComponent<Pixel> ();

                    animSender.SelectActiveStrokeFromPixel(current);
				}
            }
		}
					
	}

    //Called when "Add Stroke" button is clicked
    public void OnNewStrokeAddClick()
    {
        if (BrushDropdown.value > 1)
        {
            BrushDropdown.value = 0;
        }
        animSender.CreateNewActiveStroke();
    }

    public void SetActive(bool inVal) {
        this.gameObject.SetActive(true);
        if (drawButtons!=null)
			drawButtons.SetActive (inVal);
		//update editor UI...
		active = inVal;
	}

	public void setITSH(int inIVal, int inTVal, int intSVal, int inHVal) {
		//set var
		iVal = inIVal;
		tVal = inTVal;
		sVal = intSVal;
		hVal = inHVal;

		colorCalc ();
	}

	public void setITSH(float inVal, int variable) {
		//set
		if (variable == 0)
			iVal = (int)(inVal * 100);
		if (variable == 1)
			tVal = (int)inVal;
		if (variable == 2)
			sVal = (int)(inVal * 100);
		if (variable == 3)
			hVal = (int)(inVal * 360);

		colorCalc ();
	}

	void colorCalc() { 
		//calculate RGBW
		Vector4 white = new Vector4(1,0.75f,0.3f,1);
		Vector4 finalColor = new Vector4(0,0,0,0);
		finalColor = (iVal / 100f) * white;
		//if there is saturation
		if (sVal > 0) {
			Color hue = Color.HSVToRGB (hVal / 360f, 1f, iVal / 100f);
			hue = hue * sVal / 100f;
			finalColor = finalColor * (100 - sVal) / 100f;
			//merge wb and color
			finalColor.x = finalColor.x + hue.r;
			finalColor.y = finalColor.y + hue.g;
			finalColor.z = finalColor.z + hue.b;
		}


		rawRGBW = finalColor;

		//calculate RGB(UI)

		uiColor = Color.HSVToRGB (hVal / 360f, sVal/100f, iVal / 100f);

        uiColor.r = uiColor.r * (1 - colorIntensityOffset) + Mathf.Ceil(uiColor.r) * colorIntensityOffset;
        uiColor.g = uiColor.g * (1 - colorIntensityOffset) + Mathf.Ceil(uiColor.g) * colorIntensityOffset;
        uiColor.b = uiColor.b * (1 - colorIntensityOffset) + Mathf.Ceil(uiColor.b) * colorIntensityOffset;
        uiColor.a = uiColor.a * (1 - colorIntensityOffset) + Mathf.Ceil(uiColor.a) * colorIntensityOffset;

        //set the color tool

		if(colorButton!=null) 
			colorButton.GetComponent<Renderer> ().materials[1].SetColor ("_Color", uiColor);
	}

	//public void updatePixel(Pixel inPixel) {
	//	inPixel.updatePixel(new Vector4(iVal/100f,tVal/10000f,sVal/100f,hVal/360f),uiColor);
	//}

 //   public void updatePixels()
 //   {
 //       foreach (var pixel in anim.layer.controlledPixels)
 //       {
 //           updatePixel(pixel);
 //       }
 //   }

    //public void AddPixelToLayers(Pixel pixel)
    //{
    //    for (int i = 0; i < StrokeListMenu.childCount; i++)
    //    {
    //        if (!StrokeListMenu.GetChild(i).gameObject.activeSelf)
    //            continue;

    //        //TODO: Use new animation properties!
    //        var strokeAnimation = StrokeListMenu.GetChild(i).GetComponent<StrokeOptionScripts>().Animation;
    //        //Check if selected animation
    //        if (strokeAnimation == anim)
    //        {
    //            break;
    //        }
    //        else
    //        {
    //            if (!strokeAnimation.layer.controlledPixelsOnTop.Contains(pixel))
    //            {
    //                strokeAnimation.layer.controlledPixelsOnTop.Add(pixel);
    //            }
    //        }
    //    }

    //}

}

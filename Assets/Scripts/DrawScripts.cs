using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static class HelperMethods
{
	public static List<GameObject> GetChildren(this GameObject go)
	{
		List<GameObject> children = new List<GameObject>();
		foreach (Transform tran in go.transform)
		{
			children.Add(tran.gameObject);
		}
		return children;
	}

	public static List<GameObject> GetChildrenWithTag(this GameObject go, string tag)
	{
		List<GameObject> children = new List<GameObject>();
		foreach (Transform tran in go.transform)
		{
			if (tran.gameObject.tag == tag) {
				children.Add(tran.gameObject);
			}
		}
		return children;
	}
}


public class Property
{
	public string name;
	public string type;
	public object startValue;
	public object minValue;
	public object maxValue;

	public Property(string newName, string newType, object newStartValue, object newMinValue, object newMaxValue)
	{
		name = newName;
		type = newType;
		startValue = newStartValue;
		minValue = newMinValue;
		maxValue = newMaxValue;
	}

}


public class LightAnims
{
	public string AnimName { get; set; }
	public List<Property> AnimProperties = new List<Property>();

}


public class Anim
{
	public string AnimName { get; set; }
	public Dictionary<string, int[]> Properties = new Dictionary<string, int[]>();

}
	

public class DrawScripts : MonoBehaviour {

    public AnimationSender animSender;
    public Dropdown AnimationDropdown;
    public tempAnimcontroller anim;
	public GameObject colorPanel;
	public GameObject numberPanel;
    public GameObject timePanel;
    public GameObject timeTemplate;
    public GameObject timePanelWindow;
    public GameObject firstColor;
	public GameObject numberTemplate;
    public Button startTimeButton;
    public Button cancelButton;
    public Button okButton;


    //public GameObject colorButton;
    //public GameObject auxColor;
    public GameObject animationMenu;
    public GameObject strokeMenu;

	public Dropdown toolsDropdown;
	public GameObject workSpace;
	public GameObject drawMode;
	Color buttonColor;
//	public GameObject drawTools;
//	public GameObject setupMode;
	List<GameObject> colors;
	List<GameObject> numberValues;

    float colorIntensityOffset = 0.3f;

    //List of animations
    public List<LightAnims> animations = new List<LightAnims>();

    // Use this for initialization
    void Start () {

		//Debug.Log ("Inside DrawScripts.....");

		//setupAnimations ();

        AnimationDropdown.onValueChanged.AddListener(ChangeAnimation);
		toolsDropdown.onValueChanged.AddListener(ChangeTool);
        startTimeButton.onClick.AddListener(TaskSetStartTimeButtonClick);
        cancelButton.onClick.AddListener(TaskCancelButtonClick);
        okButton.onClick.AddListener(TaskOkButtonClick);
    }

    public void setupAnimations() {
        //List<LightAnims> animations = new List<LightAnims>();
        int[] itshColor = { 100, 5400, 120, 120 };
        int[] itshColor1 = { 100, 5400, 120, 0 };
        int[] itshColor2 = { 100, 5400, 120, 240 };
        int[] itshColor3 = { 100, 5400, 120, 60 };
        int[] itshBackGround = { 0, 5400, 0, 0 };

        //populate animations list
        LightAnims newAnim1 = new LightAnims ();
		newAnim1.AnimName = "Single Color";
		newAnim1.AnimProperties.Add (new Property ("Color1", "color", itshColor, 0, 0));
        newAnim1.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim1);
		LightAnims newAnim2 = new LightAnims ();
		newAnim2.AnimName = "Gradient";
		newAnim2.AnimProperties.Add (new Property ("Color1", "color", itshColor, 0, 0));
		newAnim2.AnimProperties.Add (new Property ("Color2", "color", itshColor2, 0, 0));
        newAnim2.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim2);
		LightAnims newAnim3 = new LightAnims ();
        newAnim3.AnimName = "Fire";
        newAnim3.AnimProperties.Add(new Property("Color1", "color", itshColor1, 0, 0));
        newAnim3.AnimProperties.Add(new Property("Color2", "color", itshColor3, 0, 0));
        newAnim3.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim3);
        LightAnims newAnim4 = new LightAnims();
        newAnim4.AnimName = "Police";
        newAnim4.AnimProperties.Add(new Property("Color1", "color", itshColor1, 0, 0));
        newAnim4.AnimProperties.Add(new Property("Color2", "color", itshColor2, 0, 0));
        newAnim4.AnimProperties.Add(new Property("Color3", "color", itshBackGround, 0, 0));
        newAnim4.AnimProperties.Add(new Property("Speed", "int", 60, 0, 500));
        newAnim4.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim4);
        LightAnims newAnim5 = new LightAnims();
        newAnim5.AnimName = "Chaser";
        newAnim5.AnimProperties.Add(new Property("Color1", "color", itshColor, 0, 0));
        newAnim5.AnimProperties.Add(new Property("Color2", "color", itshBackGround, 0, 0));
        newAnim5.AnimProperties.Add(new Property("Speed", "int", 30, 0, 100));
        newAnim5.AnimProperties.Add(new Property("Width", "int", 10, 0, 100));
        int[] startValue = { 0, 0, 0, 0 };
        int[] minValue = { 0, 0, 0, 0 };
        int[] maxValue = { 23, 59, 59, 1000 };
        //newAnim5.AnimProperties.Add(new Property("StartTime", "time", startValue, minValue, maxValue));
        newAnim5.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim5);

        LightAnims newAnim6 = new LightAnims();
        newAnim6.AnimName = "Chaser Grad1";
        newAnim6.AnimProperties.Add(new Property("Color1", "color", itshColor, 0, 0));
        newAnim6.AnimProperties.Add(new Property("Color2", "color", itshColor2, 0, 0));
        newAnim6.AnimProperties.Add(new Property("Color3", "color", itshBackGround, 0, 0));
        newAnim6.AnimProperties.Add(new Property("Color4", "color", itshBackGround, 0, 0));
        newAnim6.AnimProperties.Add(new Property("Speed", "int", 30, 0, 100));
        newAnim6.AnimProperties.Add(new Property("Width", "int", 10, 0, 100));
        newAnim6.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim6);

        LightAnims newAnim7 = new LightAnims();
        newAnim7.AnimName = "Chaser Grad2";
        newAnim7.AnimProperties.Add(new Property("Color1", "color", itshColor, 0, 0));
        newAnim7.AnimProperties.Add(new Property("Color2", "color", itshColor2, 0, 0));
        newAnim7.AnimProperties.Add(new Property("Color3", "color", itshBackGround, 0, 0));
        newAnim7.AnimProperties.Add(new Property("Color4", "color", itshBackGround, 0, 0));
        newAnim7.AnimProperties.Add(new Property("Speed", "int", 30, 0, 100));
        newAnim7.AnimProperties.Add(new Property("Width", "int", 10, 0, 100));
        newAnim7.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim7);

        LightAnims newAnim8 = new LightAnims();
        newAnim8.AnimName = "Draw On";
        newAnim8.AnimProperties.Add(new Property("Color1", "color", itshColor, 0, 0));
        newAnim8.AnimProperties.Add(new Property("Color2", "color", itshBackGround, 0, 0));
        newAnim8.AnimProperties.Add(new Property("Speed", "int", 30, 0, 200));
        newAnim8.AnimProperties.Add(new Property("Hold", "int", 1000, 0, 10000));
        newAnim8.AnimProperties.Add(new Property("DMX offset", "int", 1, 1, 500));
        animations.Add(newAnim8);

        //Add animation names to Dropdown
        List<string> animNames = new List<string>();
		for(int i=0; i < animations.Count; i++){
			animNames.Add(animations[i].AnimName);
		}
		AnimationDropdown.AddOptions(animNames);


		//find first animation properties and show corresponding UI controls
		int numProperties = animations[0].AnimProperties.Count;
		//Debug.Log ("Number of properties is: "+numProperties);
		int numColors = 1;
		int numSpeeds = 1;

		int iVal = 0;
		int tVal = 0;
		int sVal = 0;
		int hVal = 0;

        //count properties by type
        for (int i = 0; i < numProperties; i++)
        {
            if (animations[0].AnimProperties[i].type == "color")
            {
                var newColorButton = Instantiate(firstColor, colorPanel.transform);
                newColorButton.name = "Color" + numColors.ToString();
                newColorButton.tag = "color";
                colorPanel.SetActive(true);
                newColorButton.SetActive(true);
                //Debug.Log ("Second value is: "+((int[])animations [0].AnimProperties [i].startValue)[0]);
                iVal = ((int[])animations[0].AnimProperties[i].startValue)[0];
                tVal = ((int[])animations[0].AnimProperties[i].startValue)[1];
                sVal = ((int[])animations[0].AnimProperties[i].startValue)[2];
                hVal = ((int[])animations[0].AnimProperties[i].startValue)[3];
                buttonColor = Color.HSVToRGB(hVal / 360f, sVal / 100f, iVal / 100f);
                newColorButton.GetComponent<Image>().color = buttonColor;
                newColorButton.transform.Find("Text").gameObject.GetComponent<Text>().text = numColors.ToString();
                newColorButton.GetComponent<ColorButtonScript>().iVal = iVal;
                newColorButton.GetComponent<ColorButtonScript>().tVal = tVal;
                newColorButton.GetComponent<ColorButtonScript>().sVal = sVal;
                newColorButton.GetComponent<ColorButtonScript>().hVal = hVal;

                numColors++;
            }

            if (animations[0].AnimProperties[i].type == "int")
            {
                var newNum = Instantiate(numberTemplate, numberPanel.transform);
                newNum.name = "DMX-offset"; //"Number" + numSpeeds.ToString();
                newNum.tag = "int";
                numberPanel.SetActive(true);
                newNum.SetActive(true);
                numSpeeds += 1;

                
                GameObject numInput = newNum.transform.Find("numberInput").gameObject;
                numInput.GetComponent<InputField>().text = animations[0].AnimProperties[i].startValue.ToString();
                

            }

            if (animations[0].AnimProperties[i].type == "time")
            {
                Debug.Log("TimePanel created in SetupAnimations...");
                timePanel.SetActive(true);
                var newTime = Instantiate(timeTemplate, timePanel.transform);
                newTime.name = "StartTime";
                newTime.tag = "starttime";
                newTime.SetActive(true);
                timePanel.GetComponent<Text>().text = newTime.name;
            }
        }

        ChangeAnimation(0);
     
	}

	private void ChangeAnimation(int arg0)
    {
		//Debug.Log ("Inside ChangeAnimation....");
		int animNum = AnimationDropdown.value;

        Debug.Log ("Selected animation number: "+animNum);
		int numProperties = animations[animNum].AnimProperties.Count;
		int numColors = 1;

		int iVal = 0;
		int tVal = 0;
		int sVal = 0;
		int hVal = 0;

    	//remove all previous colors andhide colorPanel
		colors = colorPanel.GetChildrenWithTag("color");
		//Debug.Log ("Children with Tag color: "+colors.Count);
		foreach (GameObject col in colors) {
			Destroy (col);
		}
		colorPanel.SetActive (false);

		//hide speedPanel
		numberValues = numberPanel.GetChildrenWithTag("int");
		//Debug.Log ("Children with Tag speed: "+colors.Count);
		foreach (GameObject num in numberValues) {
			num.SetActive (false);
			Destroy(num);
		}

        //hide start time
        //var startTime = timePanel.transform.Find("StartTime").gameObject;
        timePanel.SetActive(false);


		//hide animation menu
		//animationMenu.SetActive(false);

		for (int i = 0; i < numProperties; i++) {
			if (animations [animNum].AnimProperties [i].type == "color") {
				var newColorButton = Instantiate (firstColor, colorPanel.transform);
				newColorButton.name = animations [animNum].AnimProperties [i].name;
				newColorButton.tag = "color";
				colorPanel.SetActive (true);
				newColorButton.SetActive (true);
				//Debug.Log ("First color value is: "+((int[])animations [animNum].AnimProperties [i].startValue)[0]);
				iVal = ((int[])animations [animNum].AnimProperties [i].startValue)[0];
				tVal = ((int[])animations [animNum].AnimProperties [i].startValue)[1];
				sVal = ((int[])animations [animNum].AnimProperties [i].startValue)[2];
				hVal = ((int[])animations [animNum].AnimProperties [i].startValue)[3];
				buttonColor = Color.HSVToRGB (hVal / 360f, sVal / 100f, iVal / 100f);
				newColorButton.GetComponent<Image> ().color = buttonColor;
				newColorButton.transform.Find("Text").gameObject.GetComponent<Text>().text = numColors.ToString ();
				newColorButton.GetComponent<ColorButtonScript> ().iVal = iVal;
				newColorButton.GetComponent<ColorButtonScript> ().tVal = tVal;
				newColorButton.GetComponent<ColorButtonScript> ().sVal = sVal;
				newColorButton.GetComponent<ColorButtonScript> ().hVal = hVal;

                //OLD VERSION
                //if (numColors == 1)
                //{
                    //anim.oldI = iVal;
                //    anim.oldT = sVal;
                //    anim.oldS = tVal;
                //    anim.oldH = hVal;
                //}
                //else
                //{
                //    anim.secI = iVal;
                //    anim.secT = sVal;
                //    anim.secS = tVal;
                //    anim.secH = hVal;
                //}

                numColors++;
			}

			if (animations [animNum].AnimProperties [i].type == "int") {
				var newNum = Instantiate (numberTemplate, numberPanel.transform);
				newNum.name = animations [animNum].AnimProperties [i].name;
				newNum.tag = "int";
				newNum.GetComponent<Text> ().text = newNum.name;
				newNum.SetActive (true);
				GameObject numObject = newNum.transform.Find("numberInput").gameObject;
				InputField numInput = numObject.GetComponent<InputField> ();
				//numInput.onEndEdit.AddListener(delegate {CheckInput(numInput); });
				numInput.onValueChanged.AddListener(delegate {CheckInput(numInput); });
				numObject.GetComponent<MinMaxValues>().minValue = (int)animations [animNum].AnimProperties [i].minValue;
				numObject.GetComponent<MinMaxValues>().maxValue = (int)animations [animNum].AnimProperties [i].maxValue;
				numObject.GetComponent<MinMaxValues>().startValue = (int)animations [animNum].AnimProperties [i].startValue;
				numInput.text = animations [animNum].AnimProperties [i].startValue.ToString();
				numberPanel.SetActive (true);
			}

            if (animations[animNum].AnimProperties[i].type == "time")
            {
                Debug.Log("Setting Start time...");
                timePanel.SetActive(true);
                var timePanels = timePanel.GetChildrenWithTag("starttime");
                if (timePanels.Count > 0)
                {
                    Debug.Log("Making previous timePanel active...");
                    timePanels[0].SetActive(true);
                }
                else
                {
                    Debug.Log("Creating new time...");
                    var newTime = Instantiate(timeTemplate, timePanel.transform);
                    newTime.name = animations[animNum].AnimProperties[i].name;
                    newTime.tag = "starttime";
                    newTime.GetComponent<Text>().text = newTime.name;
                    newTime.SetActive(true);
                    int[] startValues = ((int[])animations[animNum].AnimProperties[i].startValue);
                    int[] minValues = ((int[])animations[animNum].AnimProperties[i].minValue);
                    int[] maxValues = ((int[])animations[animNum].AnimProperties[i].maxValue);
                    newTime.transform.Find("Hours").GetComponent<Text>().text = startValues[0].ToString();
                    newTime.transform.Find("Minutes").GetComponent<Text>().text = startValues[1].ToString();
                    newTime.transform.Find("Seconds").GetComponent<Text>().text = startValues[2].ToString();
                    newTime.transform.Find("Milliseconds").GetComponent<Text>().text = startValues[3].ToString();

                    newTime.transform.Find("StartTimeButton").GetComponent<Button>().onClick.AddListener(TaskSetStartTimeButtonClick);
                }
                
            }

        }
    }

	public Anim GetAnimation()
	{
        int[] numValue;

        var currentAnim = new Anim();
       
		//find selected animation index
		int animNum = AnimationDropdown.value;
		//Debug.Log ("animNum is: " + animNum);
		//find number of properties of selected animation
		int numProperties = animations[animNum].AnimProperties.Count;
		//Debug.Log ("numProperties is: "+numProperties);
		//find animation name and add to currentAnim
		currentAnim.AnimName = AnimationDropdown.transform.Find ("Label").gameObject.GetComponent<Text> ().text; //animations[animNum].AnimName;

		//find name and value of each property and add to dictionary
		Transform colorName;

		for (int i = 0; i < numProperties; i++) {
			if (animations [animNum].AnimProperties [i].type == "color") {
				//Debug.Log ("Property name is: "+animations [animNum].AnimProperties [i].name);
				int[] itshColor = new int[4];
				colorName = colorPanel.transform.Find (animations [animNum].AnimProperties [i].name);
				itshColor [0] = colorName.gameObject.GetComponent<ColorButtonScript> ().iVal;
				itshColor [1] =	colorName.gameObject.GetComponent<ColorButtonScript> ().tVal;
				itshColor [2] =	colorName.gameObject.GetComponent<ColorButtonScript> ().sVal;
				itshColor [3] =	colorName.gameObject.GetComponent<ColorButtonScript> ().hVal;

				currentAnim.Properties.Add (animations [animNum].AnimProperties [i].name, itshColor );
			}

            if (animations[animNum].AnimProperties[i].type == "int") {
				GameObject numObject = numberPanel.transform.Find (animations[animNum].AnimProperties[i].name).gameObject;
				GameObject numInput = numObject.transform.Find("numberInput").gameObject;
				numInput.GetComponent<InputField>().onEndEdit.AddListener(delegate {CheckInput(numInput.GetComponent<InputField>()); });
				numValue = new int[] { Convert.ToInt32(numInput.transform.Find("Text").gameObject.GetComponent<Text>().text) }; 

				currentAnim.Properties.Add (animations [animNum].AnimProperties [i].name, numValue );
			}

            if (animations[animNum].AnimProperties[i].type == "time")
            {
                GameObject startTimePanel = timePanel.transform.Find(animations[animNum].AnimProperties[i].name).gameObject;
                int[] startTime = new int[4];
                startTime[0] = Convert.ToInt32(startTimePanel.transform.Find("Hours").GetComponent<Text>().text);
                startTime[1] = Convert.ToInt32(startTimePanel.transform.Find("Minutes").GetComponent<Text>().text);
                startTime[2] = Convert.ToInt32(startTimePanel.transform.Find("Seconds").GetComponent<Text>().text);
                startTime[3] = Convert.ToInt32(startTimePanel.transform.Find("Milliseconds").GetComponent<Text>().text);

                currentAnim.Properties.Add(animations[animNum].AnimProperties[i].name, startTime);
            }
        }

        return currentAnim;
	}

	public void SetAnimation(string animName, Dictionary<string, int[]> properties )
	{
		int animNum = 0;
		//Debug.Log ("Selected animation number: "+animNum);
		//string animationName = animName;
		int numProperties = properties.Count;


		int iVal = 0;
		int tVal = 0;
		int sVal = 0;
		int hVal = 0;

		string numValue;

        //If new animation name is different than current, set animation name and create new UI elements

        if (AnimationDropdown.transform.Find ("Label").gameObject.GetComponent<Text> ().text != animName) {

			//Debug.Log ("DIFFERENT ANIMATION....");


			int i=0;
			foreach (Dropdown.OptionData option in AnimationDropdown.options) {
				if (option.text == animName) {
					AnimationDropdown.value = i;
					animNum = i;
				}
				i++;
			}

			//remove all previous colors and hide colorPanel
			colors = colorPanel.GetChildrenWithTag ("color");
			//Debug.Log ("Children with Tag color: "+colors.Count);
			foreach (GameObject col in colors) {
				Destroy (col);
			}
			colorPanel.SetActive (false);

			//hide speedPanel
			numberValues = numberPanel.GetChildrenWithTag("int");
			//Debug.Log ("Children with Tag speed: "+colors.Count);
			foreach (GameObject num in numberValues) {
				num.SetActive (false);
				Destroy(num);
			}

            //hide start time
            //NOTE: Commented out to avoid errors!
            //var startTime = timePanel.transform.Find("StartTime").gameObject;
            //startTime.SetActive(false);

            //hide animation menu
            //NOTE: Commented out to avoid losing the dropdown!
            //animationMenu.SetActive (false);

            //set new values Q: would we get all the properties each time?
            i = 0;
			int numColors = 1;
			foreach (KeyValuePair < string, int[] > property in properties)
			{
				if (animations [animNum].AnimProperties [i].type == "color") {
					var newColorButton = Instantiate (firstColor, colorPanel.transform);
					newColorButton.name = animations [animNum].AnimProperties [i].name;
					newColorButton.tag = "color";
					colorPanel.SetActive (true);
					newColorButton.SetActive (true);
					//Debug.Log("Value of propertry is: "+property.Value);

					IEnumerable propertyValues = property.Value as IEnumerable;
					int[] colorValues = propertyValues.Cast<int>().ToArray();

					iVal = colorValues[0];
					tVal = colorValues[1];
					sVal = colorValues[2];
					hVal = colorValues[3];
					buttonColor = Color.HSVToRGB (hVal / 360f, sVal / 100f, iVal / 100f);
					//Debug.Log ("First color value is: "+((int[])animations [animNum].AnimProperties [i].startValue)[0]);
					newColorButton.GetComponent<Image> ().color = buttonColor;
					newColorButton.transform.Find("Text").gameObject.GetComponent<Text>().text = numColors.ToString ();
					newColorButton.GetComponent<ColorButtonScript> ().iVal = iVal;
					newColorButton.GetComponent<ColorButtonScript> ().tVal = tVal;
					newColorButton.GetComponent<ColorButtonScript> ().sVal = sVal;
					newColorButton.GetComponent<ColorButtonScript> ().hVal = hVal;
					numColors++;
                    
                }
				if (animations [animNum].AnimProperties [i].type == "int") {
					var newNum = Instantiate (numberTemplate, numberPanel.transform);
					newNum.name = property.Key;
					newNum.tag = "int";
					newNum.GetComponent<Text> ().text = newNum.name;
					newNum.SetActive (true);
					GameObject numObject = newNum.transform.Find("numberInput").gameObject;
					InputField numInput = numObject.GetComponent<InputField> ();
					//numInput.onEndEdit.AddListener(delegate {CheckInput(numInput); });
					numInput.onValueChanged.AddListener(delegate {CheckInput(numInput); });
					numObject.GetComponent<MinMaxValues>().minValue = (int)animations [animNum].AnimProperties [i].minValue;
					numObject.GetComponent<MinMaxValues>().maxValue = (int)animations [animNum].AnimProperties [i].maxValue;
					numObject.GetComponent<MinMaxValues>().startValue = (int)animations [animNum].AnimProperties [i].startValue;
					numInput.text = property.Value[0].ToString();
					numberPanel.SetActive (true);

                }
                if (animations[animNum].AnimProperties[i].type == "time" )
                {
                    //var newTime = Instantiate(timeTemplate, timePanel.transform);
                    //newTime.name = property.Key;
                    //newTime.tag = "starttime";
                    //newTime.GetComponent<Text>().text = newTime.name;
                    var newTime = timePanel.transform.Find("StartTime").gameObject;
                    newTime.SetActive(true);
                    int[] startValues = ((int[])animations[animNum].AnimProperties[i].startValue);
                    int[] minValues = ((int[])animations[animNum].AnimProperties[i].minValue);
                    int[] maxValues = ((int[])animations[animNum].AnimProperties[i].maxValue);
                    newTime.transform.Find("Hours").GetComponent<Text>().text = property.Value[0].ToString();
                    newTime.transform.Find("Minutes").GetComponent<Text>().text = property.Value[1].ToString();
                    newTime.transform.Find("Seconds").GetComponent<Text>().text = property.Value[2].ToString();
                    newTime.transform.Find("Milliseconds").GetComponent<Text>().text = property.Value[3].ToString();

                    newTime.transform.Find("StartTimeButton").GetComponent<Button>().onClick.AddListener(TaskSetStartTimeButtonClick);
                    timePanel.SetActive(true);

                }

                i++;

			}
		} else {

			//Debug.Log ("SAME ANIMATION....");

			//change properties only 
			animNum = AnimationDropdown.value;
			GameObject colorButton;
			int i = 0;
			foreach (KeyValuePair < string, int[] > property in properties)
			{
				if (animations [animNum].AnimProperties [i].type == "color") {
					//int[] newColor = (int[])property.Value;
					//Debug.Log("Value of propertry is: "+property.Value);

                    IEnumerable propertyValues = property.Value as IEnumerable;
                    int[] colorValues = propertyValues.Cast<int>().ToArray();

                    iVal = colorValues[0];
					tVal = colorValues[1];
					sVal = colorValues[2];
                    hVal = colorValues[3];

                    buttonColor = Color.HSVToRGB (hVal / 360.0f, sVal / 100.0f, iVal / 100.0f);

                    buttonColor.r = buttonColor.r * (1 - colorIntensityOffset) + Mathf.Ceil(buttonColor.r) * colorIntensityOffset;
                    buttonColor.g = buttonColor.g * (1 - colorIntensityOffset) + Mathf.Ceil(buttonColor.g) * colorIntensityOffset;
                    buttonColor.b = buttonColor.b * (1 - colorIntensityOffset) + Mathf.Ceil(buttonColor.b) * colorIntensityOffset;
                    buttonColor.a = buttonColor.a * (1 - colorIntensityOffset) + Mathf.Ceil(buttonColor.a) * colorIntensityOffset;

                    //find colorbutton gameObject and apply color to it
                    colorButton = colorPanel.transform.Find (property.Key.ToString ()).gameObject;
					colorButton.GetComponent<ColorButtonScript> ().iVal = iVal;
					colorButton.GetComponent<ColorButtonScript> ().tVal = tVal;
					colorButton.GetComponent<ColorButtonScript> ().sVal = sVal;
					colorButton.GetComponent<ColorButtonScript> ().hVal = hVal;
					colorButton.GetComponent<Image> ().color = buttonColor;

				}

				if (animations [animNum].AnimProperties [i].type == "int") {
					numberPanel.SetActive (true);
					numValue = property.Value[0].ToString();
					GameObject numObject = numberPanel.transform.Find (property.Key).gameObject;
					GameObject numInput = numObject.transform.Find ("numberInput").gameObject;
					numInput.GetComponent<InputField> ().text = numValue;

				}

                if (animations[animNum].AnimProperties[i].type == "time")
                {
                    //var newTime = Instantiate(timeTemplate, timePanel.transform);
                    //newTime.name = property.Key;
                    //newTime.tag = "starttime";
                    //newTime.GetComponent<Text>().text = newTime.name;
                    timePanel.SetActive(true);
                    var newTime = timePanel.transform.Find("StartTime").gameObject;
                    newTime.SetActive(true);
                    int[] startValues = ((int[])animations[animNum].AnimProperties[i].startValue);
                    int[] minValues = ((int[])animations[animNum].AnimProperties[i].minValue);
                    int[] maxValues = ((int[])animations[animNum].AnimProperties[i].maxValue);
                    newTime.transform.Find("Hours").GetComponent<Text>().text = property.Value[0].ToString();
                    newTime.transform.Find("Minutes").GetComponent<Text>().text = property.Value[1].ToString();
                    newTime.transform.Find("Seconds").GetComponent<Text>().text = property.Value[2].ToString();
                    newTime.transform.Find("Milliseconds").GetComponent<Text>().text = property.Value[3].ToString();

                    newTime.transform.Find("StartTimeButton").GetComponent<Button>().onClick.AddListener(TaskSetStartTimeButtonClick);
                    
                }

                i++;
			}
				
		}


	}


/*	// Invoked when the value of the text field changes.
	public void ValueChanged(string newValue)
	{
		Debug.Log ("New Value is: " + newValue);
		GameObject numObject = numberPanel.transform.Find (property.Key).gameObject;
		int min = gameObject.GetComponent<MinMaxValues> ().minValue;
		int max = gameObject.GetComponent<MinMaxValues> ().maxValue;

	}*/


	// Checks if there is anything entered into the input field.
	void CheckInput(InputField input)
	{
		//Debug.Log ("Value Changed....");
		int minValue = input.gameObject.GetComponent<MinMaxValues> ().minValue;
		int maxValue = input.gameObject.GetComponent<MinMaxValues> ().maxValue;
		int startValue = input.gameObject.GetComponent<MinMaxValues> ().startValue;

		if(input.text.Length == 0) {
			input.text = startValue.ToString();
		}


		int newValue = Convert.ToInt32 (input.text);
		if (newValue < minValue)
		{
			newValue = minValue;
		} else if(newValue > maxValue) {
			newValue = maxValue;
		}
		input.text = newValue.ToString ();

	}

    void TaskSetStartTimeButtonClick () {
        Debug.Log("Start Time button clicked!");
        var startTime = timePanel.transform.Find("StartTime");
        timePanelWindow.SetActive(true);
        timePanelWindow.transform.Find("HPanel").Find("numberInput").gameObject.GetComponent<InputField>().text = startTime.transform.Find("Hours").GetComponent<Text>().text;
        timePanelWindow.transform.Find("MPanel").Find("numberInput").gameObject.GetComponent<InputField>().text = startTime.transform.Find("Minutes").GetComponent<Text>().text;
        timePanelWindow.transform.Find("SPanel").Find("numberInput").gameObject.GetComponent<InputField>().text = startTime.transform.Find("Seconds").GetComponent<Text>().text;
        timePanelWindow.transform.Find("MSPanel").Find("numberInput").gameObject.GetComponent<InputField>().text = startTime.transform.Find("Milliseconds").GetComponent<Text>().text;
    }

    void TaskOkButtonClick()
    {
        var startTime = timePanel.transform.Find("StartTime");
        startTime.transform.Find("Hours").GetComponent<Text>().text = timePanelWindow.transform.Find("HPanel").Find("numberInput").gameObject.GetComponent<InputField>().text;
        startTime.transform.Find("Minutes").GetComponent<Text>().text = timePanelWindow.transform.Find("MPanel").Find("numberInput").gameObject.GetComponent<InputField>().text;
        startTime.transform.Find("Seconds").GetComponent<Text>().text = timePanelWindow.transform.Find("SPanel").Find("numberInput").gameObject.GetComponent<InputField>().text;
        startTime.transform.Find("Milliseconds").GetComponent<Text>().text = timePanelWindow.transform.Find("MSPanel").Find("numberInput").gameObject.GetComponent<InputField>().text;

        timePanelWindow.SetActive(false);

    }

    void TaskCancelButtonClick()
    {
        timePanelWindow.SetActive(false);

    }




    /*    private void ChangeAnimation(int arg0)
        {
            switch (AnimationDropdown.value)
            {
                case 1:
                    anim.layer.specialPlaybackMode = specialMode.gradient;
                    anim.layer.mode = playbackMode.loop;
                    anim.layer.state = playbackState.play;
                    anim.layer.timelineLength = 2000;
                    auxColor.SetActive(true);
                    animationMenu.SetActive(false);
                    break;
                case 2:
                    anim.layer.specialPlaybackMode = specialMode.fire;
                    anim.layer.mode = playbackMode.loop;
                    anim.layer.timelineLength = 5000;
                    auxColor.SetActive(true);
                    animationMenu.SetActive(false);
                    break;
                case 3:
                    anim.layer.specialPlaybackMode = specialMode.chaser;
                    anim.layer.mode = playbackMode.loop;
                    anim.layer.timelineLength = 2000;
                    auxColor.SetActive(true);
                    animationMenu.SetActive(false);
                    break;
                case 4:
                    anim.layer.specialPlaybackMode = specialMode.police;
                    anim.layer.mode = playbackMode.loop;
                    anim.layer.timelineLength = 2000;
                    auxColor.SetActive(true);
                    animationMenu.SetActive(false);
                    break;
                default:
                    anim.layer.specialPlaybackMode = specialMode.normal;
                    anim.layer.mode = playbackMode.loop;
                    anim.layer.state = playbackState.play;
                    anim.layer.timelineLength = 1000;
                    auxColor.SetActive(false);
                    animationMenu.SetActive(false);
                    break;
            }
        }
    */

    private void ChangeTool(int arg0)
	{
		switch (toolsDropdown.value)
		{
			case 1: //Paint Pixel
				drawMode.SetActive(true);
				int lightCount = workSpace.transform.childCount;
				for (int i = 0; i < lightCount; i++)
				{
					var light = workSpace.transform.GetChild(i);
					light.Find("DragAndDrop1").gameObject.SetActive(false);
					light.Find("DragAndDrop2").gameObject.SetActive(false);
					light.Find("Canvas").gameObject.SetActive(false);
				}

				break;
			case 2: //Move Lamp
				drawMode.SetActive(false);
				
				lightCount = workSpace.transform.childCount;
				for (int i = 0; i < lightCount; i++)
				{
					var light = workSpace.transform.GetChild(i);
					light.Find("DragAndDrop1").gameObject.SetActive(true);
					light.Find("DragAndDrop2").gameObject.SetActive(true);
					light.Find("Canvas").gameObject.SetActive(true);
				}
				break;
			default: //Paint Lamp
				drawMode.SetActive(true);
				lightCount = workSpace.transform.childCount;
				for (int i = 0; i < lightCount; i++)
				{
					var light = workSpace.transform.GetChild(i);
					light.Find("DragAndDrop1").gameObject.SetActive(false);
					light.Find("DragAndDrop2").gameObject.SetActive(false);
					light.Find("Canvas").gameObject.SetActive(false);
				}			
				break;
		}
	}


    public void OnSelectStrokeButtonClick()
    {
        strokeMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
    }
}

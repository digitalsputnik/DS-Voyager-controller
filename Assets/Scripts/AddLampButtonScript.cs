using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;

public class AddLampButtonScript : MonoBehaviour {

    public DetectedLampProperties detectedLampProperties { get; set; }
    public string LampIP;
    public int LampLength;
	public int BatteryLevel;
	public string MacName;
    public string LampProps;
    public GameObject Lamp;
	public GameObject ColorDataReceiver;
    public GameObject AddAllLampsButton;
	int numLamps = 0;

    private void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        CreateLamp(1);
		Startup start = GameObject.Find ("Main Camera").GetComponent<Startup> ();

		if (start.tutorialMode){
			start.action2 = true;
			//start.tutorial.transform.Find ("HelpWindow").GetComponent<RectTransform> ().sizeDelta = new Vector2(550f, 340f);
		}

        //Check for other lamp buttons
        int addButtonCount = this.transform.parent.childCount;

        if (addButtonCount == 5)
        {
            AddAllLampsButton.SetActive(false);
        }

        //Close window and destroy object
        GameObject.Find("LampsList").SetActive(false);
        Destroy(this.gameObject);

    }

    public void TaskOnClickOverride(int lampNum, int lampsToAdd)
    {
        CreateLamp(lampNum, lampsToAdd);
        //Destroy object only
        Destroy(this.gameObject);
    }

    public void CreateLamp(int lampNum = 1, int lampsToAdd = 1)
    {
        //Find number of sets in scene
        detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();

        var setObjectsList = GameObject.FindGameObjectsWithTag("set");
        GameObject selectedSet = null;

        //Find correct Z position for new light
        Vector3 lightPosition = Lamp.transform.position;
        Quaternion lightRotation = Lamp.transform.rotation;
        Vector3 lightScale = Lamp.transform.localScale;
        if (setObjectsList.Length > 0)
        {
            foreach (var set in setObjectsList)
            {
                if (set.transform.Find("Set").Find("Highlight").gameObject.activeSelf == true)
                {
                    selectedSet = set;
                    lightPosition = new Vector3(Lamp.transform.position.x, Lamp.transform.position.y, set.transform.position.z + Lamp.transform.position.z);
                    lightRotation = set.transform.rotation;
                    lightScale = set.transform.localScale;
                    Debug.Log("Set Scale is: "+ lightScale);

                }
            }

        }

        //Find correct Y position of new light

        //Find how many lamps are already in the scene
        numLamps = GameObject.FindGameObjectsWithTag ("lampparent").Length;

        //Find visible area
        var viewportHeight = Camera.main.pixelHeight;

        //Find Y distance between lamps so they are evenly spreaded
        int yDistance = viewportHeight / (lampsToAdd + 1);

        float yPosition = viewportHeight * lampNum / (lampsToAdd + 1) + UnityEngine.Random.Range(-yDistance/2, yDistance/2);

        //Find default Position of light
        var lightDefaultPosition = Camera.main.WorldToScreenPoint(lightPosition);

        //Convert yPosition to world position
        var lightWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(lightDefaultPosition.x, yPosition, lightDefaultPosition.z));

        //Create NewLamp gameobject and set its position, parent etc. This will be the parent of our new light.
        Debug.Log("Creating newLamp GameObject...");
        GameObject newLamp = new GameObject();
        Transform parentObject;
        if (setObjectsList.Length > 0)
        {
            parentObject = selectedSet.transform;
            newLamp.transform.position = new Vector3(0, 0, selectedSet.transform.position.z);
        }
        else
        {
            parentObject = GameObject.Find("WorkSpace").transform;
            newLamp.transform.position = Vector3.zero;
        }
        newLamp.transform.parent = parentObject;
        Debug.Log("Assigned to Parent");

        Debug.Log("Setting Lamp tag...");
        newLamp.tag = "lampparent";
        int lampCount = GameObject.FindGameObjectsWithTag("lampparent").Length;
        Debug.Log("Number of lamps is: " + lampCount.ToString());

        newLamp.name = "Lamp" + lampCount.ToString();
        Debug.Log("NewLamp " + newLamp.name + " created!");

        //Instantsiate light with IP and length!
        var newLight = Instantiate(Lamp, lightWorldPosition, Quaternion.identity, newLamp.transform);
        Debug.Log("Light Position is: " + newLight.transform.position);
        newLight.transform.rotation = lightRotation;
        Debug.Log("Lamp Scale BEFORE: " + newLamp.transform.localScale);
        newLamp.transform.localScale = lightScale;
        Debug.Log("Lamp Scale AFTER: " + newLamp.transform.localScale);
        Debug.Log("Light Scale is: " + newLight.transform.localScale);

        //newLight.name = "Lamp" + numLamps.ToString ();
        newLight.GetComponent<Ribbon>().IP = LampIP;
        newLight.GetComponent<Ribbon>().Mac = MacName;
        newLight.GetComponent<Ribbon>().pipeLength = LampLength;
        newLight.GetComponent<Ribbon>().Port = 30000;


        var VoyagerName = newLight.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>();
        VoyagerName.text = LampProps;

        //Add lamp to lampslist in Set
        detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        var setsList = detectedLampProperties.SetsList;
        if (setsList.Count > 0)
        {
            foreach (var set in setsList)
            {
                if (set.isSelected)
                {
                    set.lampslist.Add(new LampProperties
                    {
                        lampID = lampCount,
                        IP = LampIP,
                        LampLength = LampLength,
                        batteryLevel = BatteryLevel,
                        macName = MacName
                    });

                }
            }
        }

        //Register controller to lamp
        GameObject AnimationSender = GameObject.Find("AnimationControl");
        AnimationSender.GetComponent<AnimationSender>().RegisterControllerToDevice(true, LampIP);
        AnimationSender.GetComponent<AnimationSender>().StartPollingLayers(LampIP);
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(10);
            AnimationSender.GetComponent<AnimationSender>().SetDetectionMode(false, LampIP);
        }

		ColorDataReceiver.GetComponent<GetLampColorData> ().IP.Add (LampIP);
		ColorDataReceiver.SetActive (true);
    }
}

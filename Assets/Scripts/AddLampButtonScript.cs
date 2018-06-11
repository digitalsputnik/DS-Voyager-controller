using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;

public class AddLampButtonScript : MonoBehaviour {

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
		//Find how many lamps are already in the scene
		numLamps = GameObject.FindGameObjectsWithTag ("light").Length;

        //Find visible area
        var viewportHeight = Camera.main.pixelHeight;

        //Find Y distance between lamps so they are evenly spreaded
        int yDistance = viewportHeight / (lampsToAdd + 1);

        float yPosition = viewportHeight * lampNum / (lampsToAdd + 1) + UnityEngine.Random.Range(-yDistance/2, yDistance/2);

        //Find default xPosition of lamp
        var lampDefaultPosition = Camera.main.WorldToScreenPoint(Lamp.transform.position);

        //Convert yPosition to world position
        var lampWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(lampDefaultPosition.x, yPosition, lampDefaultPosition.z));

        //Instantsiate lamp with IP and length!
        //Vector3 NewLampPosition = new Vector3(Lamp.transform.position.x, lampWorldPosition.y, Lamp.transform.position.z); 

        var newLamp = Instantiate(Lamp, lampWorldPosition, Quaternion.identity);
		newLamp.name = "Lamp" + numLamps.ToString ();
        newLamp.GetComponent<Ribbon>().IP = LampIP;
        newLamp.GetComponent<Ribbon>().Mac = MacName;
        newLamp.GetComponent<Ribbon>().pipeLength = LampLength;
        newLamp.GetComponent<Ribbon>().Port = 30000;
        //Place lamp in the centre of workspace! 
        GameObject Workspace = GameObject.Find("WorkSpace");
        newLamp.transform.SetParent(Workspace.transform);
        var VoyagerName = newLamp.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>();
        VoyagerName.text = LampProps;
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

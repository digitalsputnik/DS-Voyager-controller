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
	int numLamps = 0;

    private void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        CreateLamp();
		Startup start = GameObject.Find ("Main Camera").GetComponent<Startup> ();

		if (start.tutorialMode){
			start.action2 = true;
			//start.tutorial.transform.Find ("HelpWindow").GetComponent<RectTransform> ().sizeDelta = new Vector2(550f, 340f);
		}
        //Close window and destroy object
        GameObject.Find("LampsList").SetActive(false);
        Destroy(this.gameObject);
    }

    public void TaskOnClickOverride()
    {
        CreateLamp();
        //Destroy object only
        Destroy(this.gameObject);
    }

    public void CreateLamp()
    {
		//Find how many lamps are already in the scene
		numLamps = GameObject.FindGameObjectsWithTag ("light").Length;
			
        //Instantsiate lamp with IP and length!
        var NewLampPosition = Lamp.transform.position;
        var newLamp = Instantiate(Lamp, NewLampPosition, Quaternion.identity);
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

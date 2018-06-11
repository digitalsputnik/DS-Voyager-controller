using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadDetectedLampsScript : MonoBehaviour {

    //public DetectedLampProperties detectedLamps;
    public GameObject shortVoyager;
    public GameObject longVoyager;
	public GameObject shortVoyager3;
	public GameObject longVoyager3;
    public Transform WorkSpace;
    public GameObject AnimationSender;
    public GameObject ColorDataReceiver;

    public DetectedLampProperties detectedLamps { get; set; }

    // Use this for initialization
    void Start () {
        DetectedLampProperties detectedLamps = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        if (detectedLamps.AddLamps)
        {
            if (detectedLamps.DetectedLamps.Count > 0)
            {
                foreach (var detectedLamp in detectedLamps.DetectedLamps)
                {
                    if (detectedLamp.LampLength == 39) //Short
                        generateLamp(shortVoyager, detectedLamp);
                    else if (detectedLamp.LampLength == 82) //Long
                        generateLamp(longVoyager, detectedLamp);
					else if (detectedLamp.LampLength == 42) //New Short
						generateLamp(shortVoyager3, detectedLamp);
					else if (detectedLamp.LampLength == 83) //New Long
						generateLamp(longVoyager3, detectedLamp);
                }
            }
            detectedLamps.AddLamps = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateLamp(GameObject lamp, LampProperties properties)
    {
        //Lamp itself
        Vector3 LampStartVector = Camera.main.ScreenToWorldPoint(new Vector3((float)properties.StartPoint.x * Screen.width, (float)properties.StartPoint.y * Screen.height, lamp.transform.position.z - Camera.main.transform.position.z));
        Vector3 LampEndVector = Camera.main.ScreenToWorldPoint(new Vector3((float)properties.EndPoint.x * Screen.width, (float)properties.EndPoint.y * Screen.height, lamp.transform.position.z - Camera.main.transform.position.z));
        var newLamp = Instantiate(lamp, LampStartVector, Quaternion.identity, WorkSpace);

        //TODO: rotation and scaling
        var initialDirection = Vector3.right;
        var newDirection = (LampEndVector - LampStartVector);
        newLamp.transform.rotation = Quaternion.FromToRotation(initialDirection, newDirection);
        var initialLength = (newLamp.transform.Find("DragAndDrop2").position - newLamp.transform.position).magnitude;
        var HandlerScale = newLamp.transform.Find("DragAndDrop2").localScale;
        if (newDirection.magnitude > 0)
        {
            newLamp.transform.localScale = newLamp.transform.localScale / initialLength * newDirection.magnitude;
            newLamp.transform.Find("DragAndDrop1").localScale = HandlerScale / newDirection.magnitude * initialLength;
            newLamp.transform.Find("DragAndDrop2").localScale = HandlerScale / newDirection.magnitude * initialLength;
        }

        newLamp.GetComponent<Ribbon>().IP = properties.IP;
        newLamp.GetComponent<Ribbon>().pipeLength = properties.LampLength;
        newLamp.GetComponent<Ribbon>().Port = 30000;
        newLamp.GetComponent<Ribbon>().Mac = properties.macName;
        var VoyagerName = newLamp.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>();
        string VoyagerText = properties.LampLength < 50 ? "Short Voyager " : "Long Voyager ";
        VoyagerName.text = VoyagerText + properties.macName + " " + properties.batteryLevel.ToString() + "% charged";
        AnimationSender.GetComponent<AnimationSender>().StartPollingLayers(properties.IP);

        if (!ColorDataReceiver.activeSelf)
            {
            ColorDataReceiver.SetActive(true);
        }

        if (!ColorDataReceiver.GetComponent<GetLampColorData>().IP.Contains(properties.IP))
        {
            ColorDataReceiver.GetComponent<GetLampColorData>().IP.Add(properties.IP);
        }
        
    }
}


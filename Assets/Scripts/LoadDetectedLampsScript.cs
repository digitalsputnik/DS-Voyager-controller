using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LoadDetectedLampsScript : MonoBehaviour {

    //public DetectedLampProperties detectedLamps;
    public GameObject shortVoyager;
    public GameObject longVoyager;
	public GameObject shortVoyager3;
	public GameObject longVoyager3;
    public Transform WorkSpace;
    public GameObject AnimationSender;
    public GameObject ColorDataReceiver;
    public GameObject SetTemplate;
    public GameObject newSet;

    public DetectedLampProperties detectedLampProperties { get; set; }
    public Material background;

    // Use this for initialization
    void Start () {
        detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        if (detectedLampProperties.AddBackground)
        {
            Debug.Log("Adding background...");
            if (detectedLampProperties.SetsList.Count > 0)
            {
                foreach (var set in detectedLampProperties.SetsList)
                {
                    newSet = Instantiate(SetTemplate, WorkSpace);
                    newSet.name = "Set" + set.setID;
                    newSet.GetComponentInChildren<Text>().text = newSet.name;
                    Debug.Log("Created: " + newSet.name);
                    if (detectedLampProperties.SetsList.Count < 2)
                    {
                        newSet.transform.position = set.position;
                        Debug.Log("Set position is " + newSet.transform.position);
                    }
                    else
                    {
                        newSet.transform.position = set.position;
                        newSet.transform.rotation = set.rotation;
                        newSet.transform.localScale = set.scale;
                        Debug.Log("Set position is " + newSet.transform.position);
                        Debug.Log("Set rotation is " + newSet.transform.rotation);
                        Debug.Log("Set scale is " + newSet.transform.localScale);
                    }

                    //newSet.transform.position = set.position;


                    newSet.tag = "set";
                    newSet.SetActive(true);
                    var filePath = set.imagePath;
                    Debug.Log("filePath is: " + filePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        Debug.Log("Image file found...");
                        // Image file exists - load bytes into texture 
                        var bytes = System.IO.File.ReadAllBytes(filePath);
                        var tex = new Texture2D(1, 1);
                        if (tex.LoadImage(bytes))
                        {
                            Debug.Log("Texture loaded...");
                        }
                        else
                        {
                            Debug.Log("Texture NOT loaded...");
                        }
                        //background.mainTexture = tex;
                        //Debug.Log ("Texture set to background");

                        // Apply to Plane
                        Material setMaterial = newSet.transform.Find("Set").GetComponent<MeshRenderer>().material;
                        //MeshRenderer mr = newSet.transform.Find("Set").GetComponent<MeshRenderer> ();
                        Debug.Log("Got Mesh Material...");
                        //mr.material = background;
                        setMaterial.mainTexture = tex;
                        Debug.Log("Applied texture...");
                    }

                    if (set.lampslist.Count > 0)
                    {
                        foreach (var detectedLamp in set.lampslist)
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

                }
                detectedLampProperties.SetsList.Last().isSelected = true;
                newSet.transform.Find("Set").Find("Highlight").gameObject.SetActive(true);
            }
            detectedLampProperties.AddBackground = false;
        }


    /*    if (detectedLampProperties.AddLamps)
        {
            if (detectedLampProperties.DetectedLamps.Count > 0)
            {
                foreach (var detectedLamp in detectedLampProperties.DetectedLamps)
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
            detectedLampProperties.AddLamps = false;
        }
        */
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void generateLamp(GameObject lamp, LampProperties properties)
    {

        Debug.Log("!!! GENERATING LAMP !!!");
        var setObjectsList = GameObject.FindGameObjectsWithTag("set");
        detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        var setsList = detectedLampProperties.SetsList;

        Vector3 lightPosition;
        Quaternion lightRotation;
        Vector3 lightScale;

        if (setsList.Count > 0)
        {
            lightPosition = new Vector3(lamp.transform.position.x, lamp.transform.position.y, newSet.transform.position.z + lamp.transform.position.z);
        }
        else
        {
            lightPosition = lamp.transform.position;

        }


        //Lamp itself
        Vector3 LampStartVector = Camera.main.ScreenToWorldPoint(new Vector3((float)properties.StartPoint.x * Screen.width, (float)properties.StartPoint.y * Screen.height, lightPosition.z - Camera.main.transform.position.z));
        Vector3 LampEndVector = Camera.main.ScreenToWorldPoint(new Vector3((float)properties.EndPoint.x * Screen.width, (float)properties.EndPoint.y * Screen.height, lightPosition.z - Camera.main.transform.position.z));

        Debug.Log("Creating newLamp GameObject...");
        GameObject newLamp = new GameObject();

        if (GameObject.FindGameObjectsWithTag("set").Length >= 1)
        {
            newLamp.transform.position = new Vector3(0, 0, newSet.transform.position.z);
        }
        else
        {
            newLamp.transform.position = Vector3.zero;
        }

        newLamp.tag = "lampparent";
        int lampCount = GameObject.FindGameObjectsWithTag("lampparent").Count();
        Debug.Log("Number of lamps is: " + lampCount.ToString());
        newLamp.name = "Lamp" + lampCount.ToString();

        Debug.Log("NewLamp " + newLamp.name + " created!");

        Transform parentObject;
        if (detectedLampProperties.SetsList.Count > 0)
        {
            parentObject = newSet.transform;
        }
        else
        {
            parentObject = WorkSpace;
        }
        newLamp.transform.parent = parentObject;
        Debug.Log("Assigned to Parent");

        var newLight = Instantiate(lamp, LampStartVector, Quaternion.identity, newLamp.transform);
        Debug.Log("Light created...");
        Debug.Log("Lamp position is: " + newLamp.transform.position);
        Debug.Log("Light position is: " + newLight.transform.position);



        //TODO: rotation and scaling
        var initialDirection = Vector3.right;
        var newDirection = (LampEndVector - LampStartVector);
        newLight.transform.rotation = Quaternion.FromToRotation(initialDirection, newDirection);
        Debug.Log("NewLight rotated...");

        var initialLength = (newLight.transform.Find("Handle2").Find("DragAndDrop2").position - newLight.transform.position).magnitude;
        var HandlerScale = newLight.transform.Find("Handle2").Find("DragAndDrop2").localScale;
        if (newDirection.magnitude > 0)
        {
            newLight.transform.localScale = newLight.transform.localScale / initialLength * newDirection.magnitude;
            newLight.transform.Find("Handle1").Find("DragAndDrop1").localScale = HandlerScale / newDirection.magnitude * initialLength;
            newLight.transform.Find("Handle2").Find("DragAndDrop2").localScale = HandlerScale / newDirection.magnitude * initialLength;
        }
        Debug.Log("NewLight scaled...");

        Debug.Log("Set position is: " + newSet.transform.position);
        Debug.Log("Lamp position is: " + newLamp.transform.position);
        Debug.Log("Light position is: " + newLight.transform.position);

        newLight.GetComponent<Ribbon>().IP = properties.IP;
        newLight.GetComponent<Ribbon>().pipeLength = properties.LampLength;
        newLight.GetComponent<Ribbon>().Port = 30000;
        newLight.GetComponent<Ribbon>().Mac = properties.macName;
        var VoyagerName = newLight.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>();
        string VoyagerText = properties.LampLength < 50 ? "Short Voyager " : "Long Voyager ";
        VoyagerName.text = VoyagerText + properties.macName + " " + properties.batteryLevel.ToString() + "% charged";
        Debug.Log("VoyagerText set to: " + VoyagerName.text);

        AnimationSender.GetComponent<AnimationSender>().StartPollingLayers(properties.IP);

        if (!ColorDataReceiver.activeSelf)
        {
            ColorDataReceiver.SetActive(true);
        }

        if (!ColorDataReceiver.GetComponent<GetLampColorData>().IP.Contains(properties.IP))
        {
            ColorDataReceiver.GetComponent<GetLampColorData>().IP.Add(properties.IP);
        }

        Debug.Log("Receiving color data.....");
    }
}


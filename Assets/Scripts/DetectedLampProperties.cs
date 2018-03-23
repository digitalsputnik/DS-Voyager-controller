using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LampProperties
{
    public string  IP { get; set; }
    public int LampLength { get; set; }
	public int batteryLevel { get; set; }
	public string macName { get; set; }
    public Vector2 StartPoint { get; set; }
    public Vector2 EndPoint { get; set; }
}

public class DetectedLampProperties : MonoBehaviour {

    public static DetectedLampProperties DLP;

    public List<LampProperties> DetectedLamps = new List<LampProperties>();
    public bool AddLamps = false;
    public Dictionary<IPAddress, int> LampIPtoLengthDictionary { get; set; }

    void Awake()
    {
        SingletonApplication();

        LampIPtoLengthDictionary = new Dictionary<IPAddress, int>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //if (scene.name == "Main")
        //{
        //    if (AddLamps)
        //    {
        //        if (DetectedLamps.Count > 0)
        //        {
        //            foreach (var detectedLamp in DetectedLamps)
        //            {
        //                if (detectedLamp.LampLength == 0) //Short
        //                    Instantiate(shortVoyager, Camera.main.ScreenToWorldPoint(new Vector3((float)detectedLamp.StartPoint.x, (float)detectedLamp.StartPoint.y, shortVoyager.transform.position.z - Camera.main.transform.position.z)), Quaternion.identity, WorkSpace);
        //                else if (detectedLamp.LampLength == 1) //Long
        //                    Instantiate(longVoyager, Camera.main.ScreenToWorldPoint(new Vector3((float)detectedLamp.StartPoint[0], (float)detectedLamp.StartPoint[1], shortVoyager.transform.position.z - Camera.main.transform.position.z)), Quaternion.identity, WorkSpace);
        //            }
        //        }
        //    }
        //}
    }

    void SingletonApplication()
    {
        if(DLP == null)
        {
            DontDestroyOnLoad(gameObject);
            DLP = this;
        }
        else
        {
            if (DLP != this)
            {
                Destroy(gameObject);
            }
        }
    }

    

    // Use this for initialization
    void Start () {


    }
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (DetectedLamps.Count > 0)
        //    {
        //        foreach (var detectedLamp in DetectedLamps)
        //        {
        //            if (detectedLamp.LampLength == 0) //Short
        //                Instantiate(shortVoyager, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 24.78f)), Quaternion.identity, WorkSpace);
        //            else if (detectedLamp.LampLength == 1) //Long
        //                Instantiate(longVoyager, Camera.main.ScreenToWorldPoint(new Vector3((float)detectedLamp.StartPoint[0], (float)detectedLamp.StartPoint[1], 10f)), Quaternion.identity, WorkSpace);
        //        }
        //    }
        //}
	}
}

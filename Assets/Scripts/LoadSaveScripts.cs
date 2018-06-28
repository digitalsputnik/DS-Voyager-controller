using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class LoadSaveScripts : MonoBehaviour {
    [Header("Buttons")]
    public Button SaveButton;
    public Button LoadButton;
    [Header("Saved data")]
    public GameObject Workspace;
    [Header("Lamp templates")]
    public GameObject ShortVoyager;
    public GameObject LongVoyager;
    public GameObject ShortVoyager3;
    public GameObject LongVoyager3;
    public SetupScripts setupScripts;
    public GameObject ColorDataReceiver;
    public GameObject AnimationSender;
    public GameObject VideoStream;
    public GameObject LampsListParent;

    void Start () {
        //Add listeners to buttons
        SaveButton.onClick.AddListener(SaveButtonClick);
        LoadButton.onClick.AddListener(LoadButtonClick);
	}

    /// <summary>
    /// Load workspace data from file
    /// </summary>
    private void LoadButtonClick()
    {
        Debug.Log("Load clicked!");
        if (File.Exists(Application.persistentDataPath + "/workspace.dat"))
        {
            Debug.Log("File exists!");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/workspace.dat", FileMode.Open);
            WorkspaceData data = (WorkspaceData)bf.Deserialize(file);
            file.Close();

            //Remove all previous!
            for (int i = 0; i < Workspace.transform.childCount; i++)
            {
                GameObject.Destroy(Workspace.transform.GetChild(i).gameObject);
            }

            //VideoStreamPosition
            VideoStream.transform.position = data.VideoStreamPosition.Position;
            Debug.Log(data.VideoStreamPosition.Position);
            VideoStream.transform.rotation = data.VideoStreamPosition.Rotation;
            VideoStream.transform.localScale = data.VideoStreamPosition.Scale;

            //Add lamps to workspace from data received
            LoadLampsToWorkspace(data);

        }
    }

    /// <summary>
    /// Save workspace data to file
    /// </summary>
    private void SaveButtonClick()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/workspace.dat");

        //Create workspace data to be saved!
        List<LampData> LampsData = new List<LampData>();
        for (int i = 0; i < Workspace.transform.childCount; i++)
        {
            GameObject lamp = Workspace.transform.GetChild(i).gameObject;
            LampData lampData = GetLampData(lamp);
            LampsData.Add(lampData);
        }
        VideoStreamData vsdata = GetVideoStreamData(VideoStream);

        WorkspaceData data = new WorkspaceData(LampsData, vsdata);

        //Data saving
        bf.Serialize(file, data);
        file.Close();
    }

    /// <summary>
    /// Removes all lamps which are loaded from add lamps.
    /// </summary>
    /// <param name="LampMacList"></param>
    private void RemoveAddLampButtons(List<string> LampMacList)
    {
        List<GameObject> ButtonsToBeDestroyed = new List<GameObject>();
        GameObject AddAllLampsButton = new GameObject();
        for (int c = 0; c < LampsListParent.transform.childCount; c++)
        {
            var LampButtonScript = LampsListParent.transform.GetChild(c).GetComponent<AddLampButtonScript>();
            if (LampButtonScript != null)
            {
                if (LampMacList.Contains(LampButtonScript.MacName))
                {
                    ButtonsToBeDestroyed.Add(LampsListParent.transform.GetChild(c).gameObject);
                }
            }

            if (LampsListParent.transform.GetChild(c).GetComponent<AddAllLampsScript>() != null)
            {
                AddAllLampsButton = LampsListParent.transform.GetChild(c).gameObject;
            }
        }

        foreach (var button in ButtonsToBeDestroyed)
        {
            Destroy(button);
        }

        //Removes add all lamps button if it is the only one remaining
        if(LampsListParent.transform.childCount == 5)
        {
            AddAllLampsButton.SetActive(false);
        }
    }

    public VideoStreamData GetVideoStreamData(GameObject videoStream)
    {
        VideoStreamData vsData = new VideoStreamData();
        vsData.Position = videoStream.transform.position;
        Debug.Log(vsData.Position);
        vsData.Rotation = videoStream.transform.rotation;
        vsData.Scale = videoStream.transform.localScale;
        return vsData;
    }

    public LampData GetLampData(GameObject lampObject)
    {
        LampData lampData = new LampData();
        lampData.Position = lampObject.transform.position;
        lampData.Rotation = lampObject.transform.rotation;
        lampData.Scale = lampObject.transform.localScale;
        lampData.IP = lampObject.GetComponent<Ribbon>().IP;
        lampData.Mac = lampObject.GetComponent<Ribbon>().Mac;
        lampData.Port = lampObject.GetComponent<Ribbon>().Port;
        lampData.LampLength = lampObject.GetComponent<Ribbon>().pipeLength;
        lampData.LampLabel = lampObject.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().text;
        
        //Get lamp pixels
        var pixels = lampObject.GetComponentsInChildren<Pixel>();
        lampData.PixelColors = new Dictionary<int, SerializableVector4>();
        foreach (var pixel in pixels)
        {
            lampData.PixelColors.Add(pixel.ID, pixel.ITSH);
        }
        return lampData;
    }

    public void LoadLampsToWorkspace(WorkspaceData wsData)
    {
		foreach(GameObject lamp in Workspace.GetChildren())
		{
			Destroy(lamp);
        }

        var LampMacList = wsData.Lamps.Select(x => x.Mac).ToList();
        RemoveAddLampButtons(LampMacList);

        foreach (LampData lamp in wsData.Lamps)
        {
            //TODO: Create general dictionary of lamps! This is hard-code!
            GameObject templateLamp = null;// lamp.LampLength > 0 ? LongVoyager : ShortVoyager;
            switch (lamp.LampLength)
            {
                case 39:
                    templateLamp = ShortVoyager;
                    break;
                case 42:
                    templateLamp = ShortVoyager3;
                    break;
                case 82:
                    templateLamp = LongVoyager;
                    break;
                case 83:
                    templateLamp = LongVoyager3;
                    break;
            }

            var HandlerScale = templateLamp.transform.Find("DragAndDrop2").localScale;
            var initialScaleMagnitude = templateLamp.transform.localScale.magnitude;
            
            var createdLamp = Instantiate(templateLamp, lamp.Position, lamp.Rotation, Workspace.transform);
            createdLamp.transform.localScale = lamp.Scale;
            createdLamp.GetComponent<Ribbon>().IP = lamp.IP;
            createdLamp.GetComponent<Ribbon>().Port = lamp.Port;
            createdLamp.GetComponent<Ribbon>().pipeLength = lamp.LampLength;
            createdLamp.GetComponent<Ribbon>().Mac = lamp.Mac;

            //Handles
            createdLamp.transform.Find("DragAndDrop1").localScale = HandlerScale / createdLamp.transform.localScale.magnitude * initialScaleMagnitude;
            createdLamp.transform.Find("DragAndDrop2").localScale = HandlerScale / createdLamp.transform.localScale.magnitude * initialScaleMagnitude;

			if (!setupScripts.LampIPtoLengthDictionary.ContainsKey(IPAddress.Parse(lamp.IP)))
                setupScripts.LampIPtoLengthDictionary.Add(IPAddress.Parse(lamp.IP), lamp.LampLength);

            createdLamp.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().text = lamp.LampLabel;

            AnimationSender.GetComponent<AnimationSender>().StartPollingLayers(lamp.IP);

            if (!ColorDataReceiver.activeSelf)
            {
                ColorDataReceiver.SetActive(true);
            }

            if (!ColorDataReceiver.GetComponent<GetLampColorData>().IP.Contains(lamp.IP))
            {
                ColorDataReceiver.GetComponent<GetLampColorData>().IP.Add(lamp.IP);
            }
            ////Set pixel colors
            //var pixels = createdLamp.GetComponentsInChildren<Pixel>();
            //foreach (var pixel in pixels)
            //{
            //    var uiColor = Color.HSVToRGB(lamp.PixelColors[pixel.ID].w, lamp.PixelColors[pixel.ID].z, lamp.PixelColors[pixel.ID].x);
            //    pixel.updatePixel(lamp.PixelColors[pixel.ID], uiColor);
            //}
        }
    }

    [Serializable]
    public class WorkspaceData
    {
        public List<LampData> Lamps { get; set; }
        public VideoStreamData VideoStreamPosition { get; set; }
        //Constructor
        public WorkspaceData(List<LampData> LampsData, VideoStreamData vsData)
        {
            Lamps = LampsData;
            VideoStreamPosition = vsData;
        }
    }

    [Serializable]
    public class VideoStreamData
    {
        public SerializableVector3 Position { get; set; }
        public SerializableQuaternion Rotation { get; set; }
        public SerializableVector3 Scale { get; set; }
    }

    [Serializable]
    public class LampData
    {
        public SerializableVector3 Position { get; set; }
        public SerializableQuaternion Rotation { get; set; }
        public SerializableVector3 Scale { get; set; }
        public string IP { get; set; }
        public string Mac { get; set; }
        public int Port { get; set; }
        public int LampLength { get; set; }
        public string LampLabel { get; set; }
        public Dictionary<int, SerializableVector4> PixelColors {get; set;}
    }

    [Serializable]
    public struct SerializableVector3
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }

    [Serializable]
    public struct SerializableVector4
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// w component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        /// <param name="rW"></param>
        public SerializableVector4(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        /// <summary>
        /// Automatic conversion from SerializableQuaternion to Quaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector4(SerializableVector4 rValue)
        {
            return new Vector4(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        /// <summary>
        /// Automatic conversion from Quaternion to SerializableQuaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector4(Vector4 rValue)
        {
            return new SerializableVector4(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }

    [Serializable]
    public struct SerializableQuaternion
    {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// w component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        /// <param name="rW"></param>
        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        /// <summary>
        /// Automatic conversion from SerializableQuaternion to Quaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Quaternion(SerializableQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        /// <summary>
        /// Automatic conversion from Quaternion to SerializableQuaternion
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableQuaternion(Quaternion rValue)
        {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }


}

﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
//Networking
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class UDPResponse
{
    public byte[] IP { get; set; }
    public int length { get; set; }
    public int battery_level { get; set; }
    public int[] BQ_temp { get; set; }
    public int CHIP_temp { get; set; }
    public int[] charging_status { get; set; }
    public int[] LPC_version { get; set; }
    public int[] CHIP_version { get; set; }
    public int[] animation_version { get; set; }
    public string serial_name { get; set; }
}

public class ExtraProperties
{
    public string LampMac;
    public int BatteryLevel;
}

public class SetupScripts : MonoBehaviour {

    public Button AddLampButton;
    public Button AddAllLampsButton;
    public GameObject AvailableLampsDialog;
    public GameObject LampAddButtonTemplate;
    public Transform AvailableLampsList;
    public GameObject LongLamp;
    public GameObject ShortLamp;
    public GameObject LongLamp3;
    public GameObject ShortLamp3;
    public GameObject DS3Lamp;
    public GameObject DSBeam;
    public AnimationSender animSender;
    public GameObject UpdateWindow;
	public GameObject AboutWindow;
     public MenuPullScript PullMenu;
    public GameObject LampTools;

    //for Photo handling
    public Button AddBackgroundButton;
    public GameObject MenuBackGround;
    public RawImage BackgroundRawImage;
    private Texture BackgroundTexture;
    private Texture2D freezTexture;
    private Texture2D flippedTexture;
    private WebCamTexture webcamTexture = null;
    private bool camAvailable = false;
    public AspectRatioFitter fit;
    public GameObject WorkSpace;
    public Button BottomMenuShutterButton;
    public Button BottomMenuReturnButton;
    public Button BottomMenuOkButton;
    public Button BottomMenuCancelButton;
    public MenuPullScript PullScript;

    //public Button ListenerToggleButton;

    Dictionary<IPAddress, ExtraProperties> IPtoProps = new Dictionary<IPAddress, ExtraProperties>();

    //int batteryLevel;
    //string lampMacName;
    private bool CancelDetection = false;
    private int[] LampAnimationSoftwareVersion = new int[] { 0, 0 };
    private int[] LampUDPSoftwareVersion = new int[] { 0, 31 };
    private int[] LampUDPSoftwareVersion3 = new int[] { 0, 44 };
    private int[] LampLPCSoftwareVersion = new int[] {0, 185 };
    private bool isPollingActive = true;
    private bool addLampAutomatically = true;

    public Dictionary<IPAddress, int> LampIPtoLengthDictionary { get; set; }

    void Start()
    {
        try
        {
            var versionText = Resources.Load("version") as TextAsset;
            LampAnimationSoftwareVersion = Array.ConvertAll<string, int>(versionText.text.ToString().Split('.'), int.Parse);
        }
        catch (Exception)
        {
            throw new Exception("Couldn't receive latest light version software.");
        }

        AddBackgroundButton.onClick.AddListener(OnAddBackgroundClick);
        BottomMenuShutterButton.onClick.AddListener(TaskOnShutterButtonClick);
        BottomMenuReturnButton.onClick.AddListener(TaskOnReturnButtonClick);
        BottomMenuOkButton.onClick.AddListener(TaskOnOkButtonClick);
        BottomMenuCancelButton.onClick.AddListener(TaskOnCancelButtonClick);
        AddLampButton.onClick.AddListener(OnAddLampsClick);
        //ListenerToggleButton.onClick.AddListener(TogglePacketListener);


        LampIPtoLengthDictionary = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>().LampIPtoLengthDictionary;

        var detectedLamps = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>().DetectedLamps;

        RemoveDetectedLampsFromPool(detectedLamps);

        SetDetectionMode(false);

        //StartCoroutine("GetAvailableLamp");

        GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>().LampIPtoLengthDictionary = LampIPtoLengthDictionary;
    }

    private void OnEnable()
    {
        StartCoroutine("GetAvailableLamp");
    }

 
    private void RemoveDetectedLampsFromPool(List<LampProperties> detectedLamps)
    {
        List<IPAddress> lampsToRemove = new List<IPAddress>();

        foreach (var lampIP in LampIPtoLengthDictionary.Keys)
        {
            if (!detectedLamps.Any(l => l.IP == lampIP.ToString()))
            {
                lampsToRemove.Add(lampIP);
            }
        }

        foreach (var lamp in lampsToRemove)
        {
            LampIPtoLengthDictionary.Remove(lamp);
        }
    }


    void OnAddBackgroundClick()
    {
        //BottomMenuOkButton.gameObject.SetActive(true);
        //BottomMenuCancelButton.gameObject.SetActive(true);

        PullScript = MenuBackGround.transform.Find("MenuPullImage").gameObject.GetComponent<MenuPullScript>();
        PullScript.CloseMenu();
        //PullScript.Start();
        BottomMenuShutterButton.gameObject.SetActive(true);
        BottomMenuReturnButton.gameObject.SetActive(true);
        BackgroundRawImage.gameObject.SetActive(true);


        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("No Camera Detected!");
            camAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                webcamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);

            }
        }

        if (webcamTexture == null)
        {
            Debug.Log("Unable to find back camera");
            return;
        }

        webcamTexture.Play();
        BackgroundRawImage.material.mainTexture = webcamTexture;

        camAvailable = true;

    }

    void TaskOnShutterButtonClick()
    {
        BackgroundTexture = BackgroundRawImage.material.mainTexture;
        Debug.Log("BackgroundImage size is: " + BackgroundTexture.width + " x" + BackgroundTexture.height);

        freezTexture = new Texture2D(BackgroundTexture.width, BackgroundTexture.height, TextureFormat.ARGB32, false);
        Debug.Log("FreezTexture created...");

        Color[] pixels = webcamTexture.GetPixels();
        System.Array.Reverse(pixels, 0, pixels.Length);

        //Save the image to the Texture2D
        freezTexture.SetPixels(pixels);
        freezTexture.Apply();
        //flippedTexture = FlipTexture(freezTexture);

        Debug.Log("freezTexture size is: " + freezTexture.width + " x" + freezTexture.height);

        webcamTexture.Pause();
        camAvailable = false;

        BottomMenuShutterButton.gameObject.SetActive(false);
        BottomMenuReturnButton.gameObject.SetActive(false);
        BottomMenuOkButton.gameObject.SetActive(true);
        BottomMenuCancelButton.gameObject.SetActive(true);
    }


    void TaskOnReturnButtonClick()
    {
        webcamTexture.Stop();
        camAvailable = false;
        BottomMenuShutterButton.gameObject.SetActive(false);
        BottomMenuReturnButton.gameObject.SetActive(false);
        BackgroundRawImage.gameObject.SetActive(false);
        //MenuBackGround.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {

        if (!camAvailable)
            return;

        if (!isPollingActive)
        {
            StartCoroutine("GetAvailableLamp");
            isPollingActive = true;
        }

        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        fit.aspectRatio = ratio;


        float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        BackgroundRawImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webcamTexture.videoRotationAngle;
        BackgroundRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }



    void TaskOnCancelButtonClick()
    {
        webcamTexture.Stop();
        BackgroundRawImage.gameObject.SetActive(false);
        camAvailable = false;
        BottomMenuOkButton.gameObject.SetActive(false);
        BottomMenuCancelButton.gameObject.SetActive(false);
    }

    void TaskOnOkButtonClick()
    {
        //Stop the webcam
        webcamTexture.Stop();

        //Prepare the new set and add to SetsList
        StartCoroutine(PrepareNewSet());

        //Hide buttons
        BottomMenuOkButton.gameObject.SetActive(false);
        BottomMenuCancelButton.gameObject.SetActive(false);
        BackgroundRawImage.gameObject.SetActive(false);

        //Save current transforms of all added sets

        //Get list of all Sets
        DetectedLampProperties detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        var allSetsList = detectedLampProperties.SetsList;

        //Get screen setObjects
        var setObjectsList = WorkSpace.GetChildren();
        if (setObjectsList.Count > 0)
        {
            foreach (var set in allSetsList)
            {
                
                foreach (var setObject in setObjectsList)
                {
                    var setObjectID = Convert.ToInt32(setObject.name.Substring(3));
                    if (setObjectID == set.setID)
                    {
                        set.position = setObject.transform.position;
                        set.rotation = setObject.transform.rotation;
                        set.scale = setObject.transform.localScale;
                        Debug.Log("Set Transform Saved...");

                        //Get list of all lamps
                        var allLampsList = set.lampslist;
                        if (allLampsList.Count > 0)
                        {
                            //Get all lampObjects belonging to this set
                            var lampObjectsList = setObject.GetChildrenWithTag("lampparent");
                            foreach (var lamp in allLampsList)
                            {
                                foreach (var lampObject in lampObjectsList)
                                {
                                    var lampObjectID = Convert.ToInt32(lampObject.name.Substring(4));
                                    if (lampObjectID == lamp.lampID)
                                    {
                                        var lightObject = lampObject.transform.GetChild(0);
                                        lamp.position = lightObject.position;
                                        lamp.rotation = lightObject.rotation;
                                        lamp.scale = lightObject.localScale;
                                        Debug.Log("Lamp Transform Saved...");
                                    }

                                }
                            }
                        }

                    }

 

                }

                
            }
        }

        //MenuBackGround.SetActive(true);
        SceneManager.LoadScene("Main");
    }


    public IEnumerator PrepareNewSet()
    {

        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();

        // File path
        string folderPath = Application.persistentDataPath + "/images";
        Debug.Log("Folder path is: " + folderPath);
        string savePath = Application.persistentDataPath + "/images/" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
        Debug.Log("Save path is: " + savePath);

        // Create the folder beforehand if not exists
        if (!System.IO.Directory.Exists(folderPath))
            System.IO.Directory.CreateDirectory(folderPath);
        Debug.Log("Folder exist...");

        //Encode it as a PNG.

        byte[] bytes = freezTexture.EncodeToPNG();
        Debug.Log("Texture encoded to PNG...");

        //Save it in a file.
        File.WriteAllBytes(savePath, bytes);

        DetectedLampProperties detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
        detectedLampProperties.AddBackground = true;

        Set newSet = new Set();
        //newSet.lampslist = detectedLampProperties.DetectedLamps;
        newSet.imagePath = savePath;

        //newSet.lampslist.AddRange(detectedLampProperties.DetectedLamps);
        if (detectedLampProperties.SetsList.Count >= 1)
        {
            newSet.position = new Vector3(0, 0, detectedLampProperties.SetsList.Last().position.z - 0.5f);
        }
        else
        {
            newSet.position = new Vector3(0, 0, -0.5f);
        }
        newSet.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        newSet.scale = new Vector3(1.0f, 1.0f, 1.0f);
        newSet.setID = detectedLampProperties.SetsList.Count;
        detectedLampProperties.SetsList.Add(newSet);
        Debug.Log("New Set added");
        foreach (var set in detectedLampProperties.SetsList)
        {
            Debug.Log("Z Position of Set is: " + set.position.z);
        }


    }


    void OnAddLampsClick()
    {
         if (!UpdateWindow.activeSelf)
        {
            AvailableLampsDialog.SetActive(true);
        }
    }

    IEnumerator GetAvailableLamp()
    {
        while (true)
        {
            int Port = 30000;
            IPEndPoint SendingEndpoint = new IPEndPoint(IPAddress.Broadcast, Port);
            //Poll message
            byte[] message = new byte[] { 0xD5, 0x0A, 0x80, 0x10 };
            byte[] Authentication = new byte[] { 0xD5, 0x0A, 0x80, 0x30 };

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            //Select wireless interface for use on PC
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            var WirelessInterface = adapters.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && x.SupportsMulticast && x.OperationalStatus == OperationalStatus.Up && x.GetIPProperties().GetIPv4Properties() != null).FirstOrDefault();
            if (WirelessInterface == null)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            var localIP = WirelessInterface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().Address.Address;
            IPEndPoint localEndpoint = new IPEndPoint(localIP, 0);
            //Send poll message
            if (localEndpoint == null)
            {
                continue;
            }
            UdpClient client = new UdpClient(localEndpoint);
#else
            UdpClient client = new UdpClient();
#endif

            client.EnableBroadcast = true;
            //Send 5 poll messages
            for (int i = 0; i < 15; i++)
            {
                //Debug.Log ("Sending poll message...");
                client.Send(message, message.Length, SendingEndpoint);
            }

            yield return new WaitForSeconds(0.2f);

            //Receive messages from lights
            IPEndPoint ReceivalEndpoint = new IPEndPoint(IPAddress.Any, 0);
            Dictionary<IPAddress, int> newLampIPtoLengthDictionary = new Dictionary<IPAddress, int>();
            List<string> UpdateLampWithIPs = new List<string>();
            List<IPAddress> ErrorLamps = new List<IPAddress>();
            byte[] IDbytes = new byte[4];

            while (client.Available > 0)
            {
                var ReceivedMessageBytes = client.Receive(ref ReceivalEndpoint);

                //Necessary information on lamps
                IPAddress LightIP = new IPAddress(0);
                int batteryLevel = 0;
                int numPixels = 0;
                string lampMacName = "";
                int[] lampSoftwareVersion = new int[] { 0, 0 };
                int[] lampAnimationVersion = new int[] { 0, 0 };
                int[] lpcSoftwareVersion = new int[] { 0, 0 };
                bool dontUseThisDevice = false;

                //Verification!
                Array.Copy(ReceivedMessageBytes, 0, IDbytes, 0, 4);

                if (ByteArrayCompare(IDbytes, Authentication))
                {
                    //Old byte ut
                    //Parsing
                    byte[] IPbytes = new byte[4];
                    Array.Copy(ReceivedMessageBytes, 4, IPbytes, 0, 4);
                    LightIP = new IPAddress(IPbytes);
                    //lightLength = ReceivedMessageBytes[8] - 1;
                    batteryLevel = ReceivedMessageBytes[9];
                    numPixels = ReceivedMessageBytes[25];
                    byte[] macName = new byte[6];
                    Array.Copy(ReceivedMessageBytes, 10, macName, 0, 6);
                    lampMacName = System.Text.Encoding.UTF8.GetString(macName);
                    Array.Copy(ReceivedMessageBytes, 21, lpcSoftwareVersion, 0, 2);
                    Array.Copy(ReceivedMessageBytes, 23, lampSoftwareVersion, 0, 2);
                    Array.Copy(ReceivedMessageBytes, 26, lampAnimationVersion, 0, 2);

                }
                else
                {
                    try
                    {
                        UDPResponse response = JsonConvert.DeserializeObject<UDPResponse>(Encoding.UTF8.GetString(ReceivedMessageBytes));
                        //Debug.Log("Got lamp");
                        //Debug.Log(Encoding.UTF8.GetString(ReceivedMessageBytes));
                        LightIP = new IPAddress(response.IP);
                        numPixels = response.length;
                        lampMacName = response.serial_name;
                        batteryLevel = response.battery_level;
                        lampSoftwareVersion = response.CHIP_version;
                        lampAnimationVersion = response.animation_version;
                        lpcSoftwareVersion = response.LPC_version;
                    }
                    catch (Exception)
                    {
                        dontUseThisDevice = true;
                    }
                }

                if (dontUseThisDevice)
                {
                    continue;
                }

                if (!IPtoProps.ContainsKey(LightIP))
                {
                    IPtoProps.Add(LightIP, new ExtraProperties
                    {
                        BatteryLevel = batteryLevel,
                        LampMac = lampMacName
                    });
                }

                if (!LampIPtoLengthDictionary.ContainsKey(LightIP))
                {
                    LampIPtoLengthDictionary.Add(LightIP, numPixels);
                    newLampIPtoLengthDictionary.Add(LightIP, numPixels);
                }



                //Update checking
                if (!UpdateLampWithIPs.Contains(LightIP.ToString()) && numPixels != 3 && numPixels != 18)
                {
                    //Debug!!
                    //UpdateLampWithIPs.Add(LightIP.ToString());

                    try
                    {
                        if (ReceivedMessageBytes.Length >= 27 && !ErrorLamps.Contains(LightIP))
                        {
                            var lampUDPSoftwareVersion = (numPixels != 42 && numPixels != 83) ? LampUDPSoftwareVersion : LampUDPSoftwareVersion3;

                            //Lamp software
                            if (lampUDPSoftwareVersion[0] > lampSoftwareVersion[0] || (lampUDPSoftwareVersion[0] == lampSoftwareVersion[0] && lampUDPSoftwareVersion[1] > lampSoftwareVersion[1]))
                            {
                                if (lampSoftwareVersion[0] == 0 && lampSoftwareVersion[1] == 0)
                                {
                                    ErrorLamps.Add(LightIP);
                                }
                                else
                                {
                                    if (!UpdateLampWithIPs.Contains(LightIP.ToString()))
                                        UpdateLampWithIPs.Add(LightIP.ToString());
                                }
                            }

                            //Animation software
                            if (LampAnimationSoftwareVersion[0] > lampAnimationVersion[0] || (LampAnimationSoftwareVersion[0] == lampAnimationVersion[0] && LampAnimationSoftwareVersion[1] > lampAnimationVersion[1]))
                            {
                                if (!UpdateLampWithIPs.Contains(LightIP.ToString()))
                                    UpdateLampWithIPs.Add(LightIP.ToString());
                            }

                            //LPC software - check only for Rev3 - update disabled
                            if (!(numPixels != 42 && numPixels != 83))
                            {
                                if (LampLPCSoftwareVersion[0] > lpcSoftwareVersion[0] || (LampLPCSoftwareVersion[0] == lpcSoftwareVersion[0] && LampLPCSoftwareVersion[1] > lpcSoftwareVersion[1]))
                                {
                                    if (!UpdateLampWithIPs.Contains(LightIP.ToString()))
                                        UpdateLampWithIPs.Add(LightIP.ToString());
                                }
                            }
                        }
                        else
                        {
                            if (!UpdateLampWithIPs.Contains(LightIP.ToString()))
                                UpdateLampWithIPs.Add(LightIP.ToString());
                        }
                    }
                    catch (Exception)
                    {
                        //Do nothing
                    }
                }

                if (numPixels == 0 && !ErrorLamps.Contains(LightIP) && !UpdateLampWithIPs.Contains(LightIP.ToString()))
                {
                    ErrorLamps.Add(LightIP);
                }
            }

            //Remove error lamps from lists
            foreach (var errorLamp in ErrorLamps)
            {
                if (LampIPtoLengthDictionary.ContainsKey(errorLamp))
                    LampIPtoLengthDictionary.Remove(errorLamp);
                if (newLampIPtoLengthDictionary.ContainsKey(errorLamp))
                    newLampIPtoLengthDictionary.Remove(errorLamp);
                if (UpdateLampWithIPs.Contains(errorLamp.ToString()))
                    UpdateLampWithIPs.Remove(errorLamp.ToString());
            }


            //client.Close();

            //var UpdateCandidateLamps = LampIPtoLengthDictionary.Keys.Select(p => p.ToString()).ToList();
            //var UpdateLamps = UpdateWindow.GetComponent<UpdateChecker>().GetUpdateableLamps(UpdateCandidateLamps);

            if (UpdateLampWithIPs.Count > 0)
            {
                IPtoProps.Clear();
                LampIPtoLengthDictionary.Clear();
                newLampIPtoLengthDictionary.Clear();
                CancelDetection = true;
                UpdateWindow.GetComponent<UpdateChecker>().UpdateLampsSoftware(UpdateLampWithIPs);
                isPollingActive = false;
            }

            
            if (newLampIPtoLengthDictionary.Count > 0)
            {
                AddAllLampsButton.gameObject.SetActive(true);
            }

            AddLampButtons(newLampIPtoLengthDictionary);

        /*    if (LampIPtoLengthDictionary.Count == 1 && !LampIPtoLengthDictionary.ContainsKey(IPAddress.Parse("172.20.1.1")) && addLampAutomatically)
            {
                addLampAutomatically = false;
                //Add lamp!
                AddAllLampsButton.GetComponent<AddAllLampsScript>().AddAllLamps();

                //Go to draw mode
                var menu = transform.parent.GetComponent<MenuMode>();
                if (menu != null)
                {
                    menu.TaskOnDrawClicked();
                    if (!PullMenu.isPulled)
                    {
                        PullMenu.MoveMenu();
                        yield return null;
                    }
                }
            }
            */

            yield return null;
        }
    }
    
    private void AddLampButtons(Dictionary<IPAddress, int> newLampIPtoLengthDictionary)
    {
        foreach (var item in newLampIPtoLengthDictionary)
        {
            var AddLampButton = Instantiate(LampAddButtonTemplate, AvailableLampsList);
            var LampProperties = AddLampButton.GetComponent<AddLampButtonScript>();
            LampProperties.LampIP = item.Key.ToString();
            LampProperties.LampLength = item.Value;
			LampProperties.BatteryLevel = IPtoProps[item.Key].BatteryLevel;
			LampProperties.MacName = IPtoProps[item.Key].LampMac;

			Debug.Log ("Number of LEDs is: "+item.Value.ToString());

            string LampName = "";
            switch (item.Value)
            {
                case 39:
                    LampProperties.Lamp = ShortLamp;
                    LampName = "Short Voyager";
                    break;
                case 42:
                    LampProperties.Lamp = ShortLamp3;
                    LampName = "Short Voyager";
                    break;
                case 82:
                    LampProperties.Lamp = LongLamp;
                    LampName = "Long Voyager";
                    break;
                case 83:
                    LampProperties.Lamp = LongLamp3;
                    LampName = "Long Voyager";
                    break;
                case 3:
                    LampProperties.Lamp = DS3Lamp;
                    LampName = "DS3 x 3";
                    break;
                case 18:
                    LampProperties.Lamp = DSBeam;
                    LampName = "DSBeam";
                    break;
            }
            
			string LampProps = LampName + " "+LampProperties.MacName+" " + LampProperties.BatteryLevel.ToString ()+"% charged"; //item.Key.ToString();
            LampProperties.LampProps = LampProps;
            AddLampButton.GetComponentInChildren<Text>().text = LampProps;
            AddLampButton.SetActive(true);
        }
    }


    public void OnLampSettingsButtonClick()
    {
        LampTools.SetActive(true);
        var lightsList = GameObject.FindGameObjectsWithTag("light");
        foreach (var light in lightsList)
        {
            light.transform.Find("DragAndDrop1").gameObject.SetActive(false);
            light.transform.Find("DragAndDrop2").gameObject.SetActive(false);
            //light.transform.Find("Canvas").gameObject.SetActive(false);
        }

        var videoStreamParent = GameObject.Find("VideoStreamParent");
        if (videoStreamParent.transform.Find("VideoStreamBackground").gameObject.activeSelf)
        {
            var videoStreamBackground = videoStreamParent.transform.Find("VideoStreamBackground");
            videoStreamBackground.transform.Find("Handle1Parent").Find("Handle1").gameObject.SetActive(false);
            videoStreamBackground.transform.Find("Handle2Parent").Find("Handle2").gameObject.SetActive(false);
        }
        this.gameObject.SetActive(false);
    }





    public void TaskOnExitClick()
    {
//#if UNITY_IOS
//        System.Diagnostics.Process.GetCurrentProcess().Kill();
//#endif
        Application.Quit();
    }

    public void OnDetectionModeClick()
    {
        //LampIPtoLengthDictionary.Add(IPAddress.Parse("172.20.0.1"), 1);
        //Send calibration colors
        //LampCommunication lampComm = new LampCommunication();
        //lampComm.SetupPermutations();
        //lampComm.LampIPtoLengthDictionary = LampIPtoLengthDictionary;
        //lampComm.permutationCounter = 0;
        //lampComm.SendPermutationsToNewLamps(LampIPtoLengthDictionary);
        
        //GetAvailableLamps();

        if (!CancelDetection)
        {
            SetDetectionMode(true);

            SceneManager.LoadScene("VisionCam");
        }
        else
        {
            CancelDetection = false;
        }
    }

    private void SetDetectionMode(bool detectionMode)
    {
        foreach (var IPLengthPair in LampIPtoLengthDictionary)
        {
            for (int i = 0; i < 5; i++)
            {
                animSender.SetDetectionMode(detectionMode, IPLengthPair.Key.ToString());
                Thread.Sleep(10);
            }
            
        }
    }

	public void TaskOnAboutClick()
	{
		var version = AboutWindow.transform.Find ("Text3").gameObject;
        version.GetComponent<Text>().text = "Version " + Application.version;
		AboutWindow.SetActive(true);
	}

    static bool ByteArrayCompare(byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length)
            return false;

        for (int i = 0; i < a1.Length; i++)
            if (a1[i] != a2[i])
                return false;

        return true;
    }
}

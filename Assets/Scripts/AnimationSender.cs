using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;


[Serializable]
public class DetectionModeDTO
{
    public bool DetectionMode;
}

[Serializable]
public class RegisterDeviceDTO
{
    public bool RegisterDevice;
}

[Serializable]
public class LayerPollDTO
{
    public bool PollLayers;
}

[Serializable]
public class ArtNetActivationDTO
{
    public bool ArtNetActive;
}

[Serializable]
public class Stroke
{
    public string StrokeID;
    public double CreationTimestamp;
    public double TimeStamp;
    public double StartTime;
    public List<int[]> Colors;
    public string Animation;
    public Dictionary<string, int[]> Properties;
    public float Duration;
    
    //Pixel information for lamp
    public int TotalPixelCount;
    public Dictionary<string, SortedDictionary<int, int>> PixelQueueToControlledPixel;
    //public Dictionary<string, SortedDictionary<float, int>> PixelTimestampToControlledPixel;

    [NonSerialized]
    public Layer layer;
    //Pixel information for UI
    [NonSerialized]
    public List<Pixel> ControlledPixels;
    //[NonSerialized]
    //public SortedDictionary<int, Pixel> TimestampToPixel;

    public Stroke(string ID, Layer ParentLayer, Dictionary<string, SortedDictionary<int, int>> LampPixQueueToPixel = null, int pixelCount = 0, string animation = "", Dictionary<string, int[]> properties = null)
    {
        //Timestamps
        CreationTimestamp = GetCurrentTimestampUTC();
        Duration = 2000f;
        StartTime = GetStartTimeFromProperties(properties);

        //Add layer reference!
        layer = ParentLayer;
        ControlledPixels = new List<Pixel>();
        ChangeStroke(ID, LampPixQueueToPixel, pixelCount, animation, properties);
        if (layer != null)
            layer.AddStroke(this);
    }

    public void ChangeStroke(string ID = "", Dictionary<string, SortedDictionary<int, int>> LampPixQueueToPixel = null, int pixelCount = 0, string animation = "", Dictionary<string, int[]> properties = null)
    {
        StrokeID = ID;
        TimeStamp = GetCurrentTimestampUTC();
        StartTime = GetStartTimeFromProperties(properties);

        if (LampPixQueueToPixel == null)
        {
            LampPixQueueToPixel = new Dictionary<string, SortedDictionary<int, int>>();
        }
        PixelQueueToControlledPixel = LampPixQueueToPixel;
        TotalPixelCount = pixelCount;
        Animation = animation;
        Properties = properties;
    }

    public void AddLatestTimestamp()
    {
        TimeStamp = GetCurrentTimestampUTC();
    }

    private double GetCurrentTimestampUTC()
    {
        return (double)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private double GetStartTimeFromProperties(Dictionary<string, int[]> properties = null)
    {
        var response = (double)(DateTime.Today - new DateTime(1970, 1, 1)).TotalSeconds;

        if (properties != null)
        {
            if (properties.ContainsKey("StartTime"))
            {
                var startTime = properties["StartTime"];
                var today = DateTime.Today;
                var time = new TimeSpan(0,startTime[0], startTime[1], startTime[2], startTime[3]);
                //TODO: Take time offset into account!
                response = (double)((today + time) - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }
        return response;
    }

    /// <summary>
    /// Adds pixel to stroke and layer
    /// </summary>
    /// <param name="pixel"></param>
    public void AddPixel(Pixel pixel, bool isStrokeActive = true)
    {
        TimeStamp = GetCurrentTimestampUTC();
        //If pixel has been added, the order is not changed!
        if (ControlledPixels.Contains(pixel))
            return;

        ControlledPixels.Add(pixel);

        //Add pixel to queues
        string LampMac = pixel.transform.parent.GetComponent<Ribbon>().Mac;
        if (PixelQueueToControlledPixel.ContainsKey(LampMac))
        {
            PixelQueueToControlledPixel[LampMac].Add(TotalPixelCount, pixel.ID);
        }
        else
        {
            var QueueToControlledPixel = new SortedDictionary<int, int>();
            QueueToControlledPixel.Add(TotalPixelCount, pixel.ID);
            PixelQueueToControlledPixel.Add(LampMac, QueueToControlledPixel);
        }
        TotalPixelCount++;

        //Add stroke to pixels
        if (layer.PixelToStrokeIDDictionary.ContainsKey(pixel))
        {
            var pixelStrokes = layer.PixelToStrokeIDDictionary[pixel];
            //var strokesOnTop = pixelStrokes.Any(s => s.CreationTimestamp > ActiveStroke.CreationTimestamp);
            //Add stroke to pixel and order
            pixelStrokes.Add(this);
            pixelStrokes = pixelStrokes.OrderByDescending(s => s.CreationTimestamp).ToList();
            layer.PixelToStrokeIDDictionary[pixel] = pixelStrokes;
        }
        else
        {
            List<Stroke> strokes = new List<Stroke> { this };
            layer.PixelToStrokeIDDictionary.Add(pixel, strokes);
        }

        if (isStrokeActive)
        {
            PixelSelectionOn(pixel);
        }

        layer.RemoveInvisibleStrokes();
    }

    public void RemovePixel(Pixel pixel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Turns on pixel selection bar
    /// </summary>
    /// <param name="pixel"></param>
    public void PixelSelectionOn(Pixel pixel)
    {
        if (layer.PixelToStrokeIDDictionary[pixel].FirstOrDefault().StrokeID == StrokeID)
        {
            //Stroke is visible!
            pixel.updateSelectionPixel(1);
        }
        else
        {
            //Stroke is not visible/under another stroke
            pixel.updateSelectionPixel(2);
        }
    }

    /// <summary>
    /// Turns on selection for entire stroke
    /// </summary>
    public void SelectionOn()
    {
        foreach (var pixel in ControlledPixels)
        {
            PixelSelectionOn(pixel);
        }
    }

    /// <summary>
    /// Turns off selection for entire stroke
    /// </summary>
    public void SelectionOff()
    {
        foreach (var pixel in ControlledPixels)
        {
            pixel.updateSelectionPixel(0);
        }
    }
}

[Serializable]
public class Layer
{
    public string LayerID;
    public List<Stroke> Strokes;
    public bool LayerActive;
    [NonSerialized]
    public Scene scene;

    [NonSerialized]
    public Dictionary<Pixel, List<Stroke>> PixelToStrokeIDDictionary;

    public Layer(string ID, Scene ParentScene, bool Active = true)
    {
        LayerID = ID;
        scene = ParentScene;
        if (ParentScene != null)
            scene.AddLayer(this);
        
        LayerActive = Active;
        PixelToStrokeIDDictionary = new Dictionary<Pixel, List<Stroke>>();
    }

    public void AddStroke(Stroke stroke)
    {
        if (Strokes == null)
            Strokes = new List<Stroke>();

        //Add stroke to list
        Strokes.Add(stroke);
        stroke.layer = this;
        //Order list
        Strokes = Strokes.OrderByDescending(s => s.CreationTimestamp).ToList();
    }

    /// <summary>
    /// Removes stroke from layer
    /// </summary>
    /// <param name="stroke"></param>
    public void RemoveStroke(Stroke stroke)
    {
        if (!Strokes.Contains(stroke))
            return;

        foreach (var pixel in stroke.ControlledPixels)
        {
            PixelToStrokeIDDictionary[pixel].Remove(stroke);
        }
        //TODO: Remove!?
        stroke.ControlledPixels.Clear();

        Strokes.Remove(stroke);
    }

    public Stroke GetStrokeByID(string strokeID)
    {
        var latestStroke = Strokes.Last();
        foreach (var stroke in Strokes)
        {
            if (stroke.StrokeID == strokeID)
            {
                return stroke;
            }
            if (stroke.ControlledPixels != null)
            {
                if (stroke.ControlledPixels.Count > 0)
                {
                    latestStroke = stroke;
                }
            }
        }

        return latestStroke;
    }

    /// <summary>
    /// Implements "Select Stroke functionality"
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public Stroke SelectStrokeFromPixel(Pixel pixel)
    {
        if (PixelToStrokeIDDictionary.ContainsKey(pixel))
        {
            return PixelToStrokeIDDictionary[pixel].FirstOrDefault();
        }
        else
        {
            //TODO: Better default return
            return null;
        }
    }

    /// <summary>
    /// Removes invisible strokes from layer
    /// </summary>
    public void RemoveInvisibleStrokes(Stroke IgnoreStroke = null)
    {
        var reverseStrokes = Strokes.OrderBy(s => s.CreationTimestamp).ToList();
        List<string> VisibleStrokeIDs = PixelToStrokeIDDictionary.Select(x => x.Value.FirstOrDefault().StrokeID).ToList();
        foreach (var stroke in reverseStrokes)
        {
            if (!VisibleStrokeIDs.Contains(stroke.StrokeID))
            {
                if (IgnoreStroke == null)
                {
                    RemoveStroke(stroke);
                }
                else
                {
                    if (IgnoreStroke.StrokeID != stroke.StrokeID)
                    {
                        RemoveStroke(stroke);
                    }
                }
            }
        }
    }

}

[Serializable]
public class Scene
{
    public double TimeStamp { get; set; }
    public List<Layer> Layers { get; set; }
    public bool ArtNetMode { get; set; }
    public bool sACNMode { get; set; }
    public void AddLayer(Layer layer)
    {
        if (Layers == null)
            Layers = new List<Layer>();

        Layers.Add(layer);
    }

    public void AddLatestTimeStamp()
    {
        TimeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}

public class AnimationSender : MonoBehaviour
{

    public Scene scene;
    public DrawScripts drawScripts;
    public DrawMode draw;
    public Stroke ActiveStroke;

    public UdpClient LampCommunicationClient;
    public UdpClient LampPollClient;

    public IPEndPoint localEndpoint;

    public double TimeOffset;

    List<string> StopLampCommunication = new List<string>();

    //Color calibration
    int[] WhiteCalibrationTemperatureNodes;
    int[][] WhiteCalibrationTable;
    int[][] HueCalibrationTable;

    private void Awake()
    {
        /* if (LampCommunicationClient == null)
         {
             LampCommunicationClient = new UdpClient(31000);
         }*/

        if (LampPollClient == null)
        {
            LampPollClient = new UdpClient(30001);
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        //Get local wireless endpoint
        SetLocalEndpoint();
#endif
    }

    private void SetLocalEndpoint()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        var WirelessInterface = adapters.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && x.SupportsMulticast && x.OperationalStatus == OperationalStatus.Up && x.GetIPProperties().GetIPv4Properties() != null).FirstOrDefault();
        if (WirelessInterface == null)
        {
            localEndpoint = null;
        }
        else
        {
            var localIP = WirelessInterface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().Address.Address;
            localEndpoint = new IPEndPoint(localIP, 0);
        }
    }

    // Use this for initialization
    void Start()
    {
        scene = new Scene();
        scene.ArtNetMode = false;
        scene.sACNMode = false;

        //Generate 5 layers with GUIDs or fixed names
        //Only one layer for now to avoid confusion
        var layer1 = new Layer("Layer", scene, true);
        //var layer2 = new Layer("Layer2", scene, false);
        //var layer3 = new Layer("Layer3", scene, false);
        //var layer4 = new Layer("Layer4", scene, false);
        //var layer5 = new Layer("Layer5", scene, false);
        
        //Generate empty stroke to each layer, first layer stroke being the active
        ActiveStroke = new Stroke(Guid.NewGuid().ToString(), layer1);
        //var NewStroke2 = new Stroke(Guid.NewGuid().ToString(), layer2);
        //var NewStroke3 = new Stroke(Guid.NewGuid().ToString(), layer3);
        //var NewStroke4 = new Stroke(Guid.NewGuid().ToString(), layer4);
        //var NewStroke5 = new Stroke(Guid.NewGuid().ToString(), layer5);

        drawScripts.setupAnimations();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Removes pixel from layers. This is used in the case of lamp deletion
    /// </summary>
    /// <param name="pixel"></param>
    public void RemovePixelFromLayers(Pixel pixel)
    {
        foreach (var layer in scene.Layers)
        {
            foreach (var stroke in layer.Strokes)
            {
                if (stroke.ControlledPixels.Contains(pixel))
                {
                    stroke.layer.PixelToStrokeIDDictionary.Remove(pixel);
                    stroke.ControlledPixels.Remove(pixel);
                }
            }
        }
    }

    /// <summary>
    /// Merges imported scene to current UI scene
    /// </summary>
    /// <param name="importScene"></param>
    public void MergeScenes(Scene importScene)
    {

        if (scene.TimeStamp == importScene.TimeStamp)
            return;

        foreach (var newLayer in importScene.Layers)
        {
            var LayerIndex = scene.Layers.FindIndex(l => l.LayerID == newLayer.LayerID);
            if (LayerIndex != -1)
            {
                scene.Layers[LayerIndex] = MergeLayers(scene.Layers[LayerIndex], newLayer);
            }
            else
            {
                //TODO: This part may not be used
                scene.AddLayer(newLayer);
            }
        }
        ActiveStroke.SelectionOn();
    }

    /// <summary>
    /// Merges two layers with same ID
    /// </summary>
    /// <param name="OriginalLayer"></param>
    /// <param name="ImportLayer"></param>
    /// <returns></returns>
    public Layer MergeLayers(Layer OriginalLayer, Layer ImportLayer)
    {
        if (OriginalLayer.LayerID != ImportLayer.LayerID)
        {
            throw new Exception("Layers are not with same ID!");
        }

        //TODO: Check for timestamps

        foreach (var newStroke in ImportLayer.Strokes)
        {
            //Add layer reference to imported stroke
            //TODO: Refactor
            newStroke.layer = OriginalLayer;

            var StrokeIndex = OriginalLayer.Strokes.FindIndex(s => s.StrokeID == newStroke.StrokeID);
            if (StrokeIndex != -1)
            {
                //Stroke is present in imported layer
                OriginalLayer.Strokes[StrokeIndex] = MergeStrokes(OriginalLayer.Strokes[StrokeIndex], newStroke);
            }
            else
            {
                //Stroke is not present in imported layer
                //Insert stroke to layer
                OriginalLayer.AddStroke(newStroke);
                SelectPixelsFromDictionary(newStroke);
            }
        }

        OriginalLayer.RemoveInvisibleStrokes(ActiveStroke);

        return OriginalLayer;
    }

    /// <summary>
    /// Method for merging strokes
    /// </summary>
    /// <param name="OriginalStroke"></param>
    /// <param name="ImportStroke"></param>
    /// <returns></returns>
    public Stroke MergeStrokes(Stroke OriginalStroke, Stroke ImportStroke)
    {
        if (OriginalStroke.StrokeID != ImportStroke.StrokeID)
        {
            throw new Exception("Strokes are not with same ID!");
        }

        var MergedStroke = OriginalStroke;
        if (ImportStroke.TimeStamp > OriginalStroke.TimeStamp)
        {
            MergedStroke = ImportStroke;
            MergedStroke.layer = ActiveStroke.layer;
            if (ActiveStroke.StrokeID == ImportStroke.StrokeID)
            {
                ActiveStroke.SelectionOff();
            }
            //TODO: Create getter and setter to workspace for lamp handling
        }
        else
        {
            MergedStroke = OriginalStroke;
        }

        SelectPixelsFromDictionary(MergedStroke);

        return MergedStroke;
    }

    private static void SelectPixelsFromDictionary(Stroke MergedStroke)
    {
        Transform Workspace = GameObject.Find("WorkSpace").transform;
        int lampCount = Workspace.childCount;

        SortedDictionary<int, Pixel> QueueToPixel = new SortedDictionary<int, Pixel>();

        for (int i = 0; i < lampCount; i++)
        {
            Transform lamp = Workspace.GetChild(i);
            var lampRibbon = lamp.GetComponent<Ribbon>();
            if (lampRibbon == null)
                continue;

            if (MergedStroke.PixelQueueToControlledPixel.ContainsKey(lampRibbon.Mac))
            {
                foreach (var QueuePixelKeyValue in MergedStroke.PixelQueueToControlledPixel[lampRibbon.Mac])
                {
                    int pixelQueueNumber = QueuePixelKeyValue.Key;
                    Pixel pix = lamp.Find("pixel" + QueuePixelKeyValue.Value).GetComponent<Pixel>();
                    if (pix == null)
                    {
                        throw new Exception("Couldn't retrieve pixel when merging!");
                    }
                    QueueToPixel.Add(pixelQueueNumber, pix);
                }
            }
        }

        MergedStroke.ControlledPixels = QueueToPixel.Values.ToList();

        //Add pixels to layer!
        //TODO: Refactor!!
        foreach (var pixel in MergedStroke.ControlledPixels)
        {
            if (MergedStroke.layer.PixelToStrokeIDDictionary.ContainsKey(pixel))
            {
                var pixelStrokes = MergedStroke.layer.PixelToStrokeIDDictionary[pixel];
                //var strokesOnTop = pixelStrokes.Any(s => s.CreationTimestamp > ActiveStroke.CreationTimestamp);
                //Add stroke to pixel and order
                if (!pixelStrokes.Any(s => s.StrokeID == MergedStroke.StrokeID))
                {
                    pixelStrokes.Add(MergedStroke);
                    pixelStrokes = pixelStrokes.OrderByDescending(s => s.CreationTimestamp).ToList();
                    MergedStroke.layer.PixelToStrokeIDDictionary[pixel] = pixelStrokes;
                }
            }
            else
            {
                List<Stroke> strokes = new List<Stroke> { MergedStroke };
                MergedStroke.layer.PixelToStrokeIDDictionary.Add(pixel, strokes);
            }
        }
    }

    /// <summary>
    /// Selects new active stroke (Select stroke tool)
    /// </summary>
    /// <param name="pixel"></param>
    public void SelectActiveStrokeFromPixel(Pixel pixel)
    {
        Stroke SelectedStroke = ActiveStroke.layer.SelectStrokeFromPixel(pixel);

        //Check for empty stroke
        if (SelectedStroke == null)
            return;

        //Check for same stroke
        //if (SelectedStroke.StrokeID == ActiveStroke.StrokeID)
        //    return;

        //Turn off current stroke
        ActiveStroke.SelectionOff();

        //Set active stroke as the selected stroke
        //TODO: Properties selection
        ActiveStroke = SelectedStroke;

        //Turn on new selected stroke
        ActiveStroke.SelectionOn();
    }

    /// <summary>
    /// Creates new empty active stroke
    /// </summary>
    public void CreateNewActiveStroke()
    {
        //Turns off current active stroke
        ActiveStroke.SelectionOff();

        //Create new active stroke
        var AddedStroke = new Stroke(Guid.NewGuid().ToString(), ActiveStroke.layer);
        ActiveStroke = AddedStroke;
    }

    public void SetDetectionMode(bool detectionMode, string IP)
    {
        SendJSONToLamp(new DetectionModeDTO() { DetectionMode = detectionMode }, new IPEndPoint(IPAddress.Parse(IP), 30001));
    }

    public void RegisterControllerToDevice(bool register, string IP)
    {
        SendJSONToLamp(new RegisterDeviceDTO() { RegisterDevice = register }, new IPEndPoint(IPAddress.Parse(IP), 30001));
    }

    public void SendAnimationWithUpdate()
    {
        ActiveStroke.AddLatestTimestamp();
        scene.AddLatestTimeStamp();
        SendAnimationToLamps();
    }

    public void SendAnimationToLamps()
    {
        //Get current animation properties
        var CurrentAnimation = drawScripts.GetAnimation();

        if (ActiveStroke.Animation == CurrentAnimation.AnimName)
        {
            if (!ActiveStroke.Properties.All(x => CurrentAnimation.Properties[x.Key].SequenceEqual(x.Value)))
            {
                scene.AddLatestTimeStamp();
            }
        }
        else
        {
            scene.AddLatestTimeStamp();
        }

        ActiveStroke.Animation = CurrentAnimation.AnimName;
        ActiveStroke.Properties = CurrentAnimation.Properties;

        //Get colors
        List<Vector4> ITSHColors = new List<Vector4>();

        foreach (var property in ActiveStroke.Properties)
        {
            if (property.Key.StartsWith("Color"))
            {
                ITSHColors.Add(new Vector4((float)property.Value[0] / 100.0f, (float)property.Value[1] / 10000f, (float)property.Value[2] / 120.0f, (float)property.Value[3] / 360.0f));
            }
        };

        List<int[]> RGBWColors = new List<int[]>();

        foreach (var ITSHColor in ITSHColors)
        {
            RGBWColors.Add(ITSHtoRGBW(ITSHColor));
        }

        //TODO: This and previous part should be removed and calibration should happen on device
        ActiveStroke.Colors = RGBWColors;

        //Get lamps to send the strokes to
        List <string> LampIPList = new List<string>();

        foreach (var stroke in ActiveStroke.layer.Strokes)
        {
            foreach (var pixel in stroke.ControlledPixels)
            {
                string LampIP = pixel.transform.parent.GetComponent<Ribbon>().IP;
                if (!LampIPList.Contains(LampIP))
                {
                    LampIPList.Add(LampIP);
                }
            }
        }

        foreach (var LampIP in LampIPList)
        {
            //Send stroke information to all lamps!
            SendJSONToLamp(scene, new IPEndPoint(IPAddress.Parse(LampIP), 30001));
        }
    }

    void GetStrokeFromLamp(string sceneJSON)
    {
        Scene lampScene = JsonConvert.DeserializeObject<Scene>(sceneJSON);

        MergeScenes(lampScene);
    }

    public void RemoveLampFromStrokes(Transform lamp, string lampIP)
    {
        int LampChildCount = lamp.childCount;
        for (int p = 0; p < LampChildCount; p++)
        {
            Pixel pix;

            if (lamp.GetChild(p).GetComponent<Pixel>() == null)
            {
                continue;
            }
            else
            {
                pix = lamp.GetChild(p).GetComponent<Pixel>();
            }

            RemovePixelFromLayers(pix);
        }

        StopLampCommunication.Add(lampIP);
        scene.TimeStamp -= 1;
    }

    public void StartPollingLayers(string LampIP)
    {
        //TODO: Generalize so it isn't lamp dependent.
        StartCoroutine("PollLayersFromLamp", LampIP);
        StartCoroutine("SetDetectionFalse", LampIP);
        StartCoroutine("RegisterDevice", LampIP);
        StartCoroutine("SendAnimationWorker", LampIP);
        //StartCoroutine
    }

    void SendJSONToLamp(object messageObject, IPEndPoint lampEndPoint)
    {
        var jsonString = JsonConvert.SerializeObject(messageObject);
        Debug.Log(jsonString);
        byte[] data = Encoding.ASCII.GetBytes(jsonString);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (localEndpoint == null)
        {
            SetLocalEndpoint();
            return;
        }
        UdpClient client = new UdpClient(localEndpoint);
#else
        UdpClient client = new UdpClient();
#endif
        client.Send(data, data.Length, lampEndPoint);
    }

    IEnumerator ActivateArtNet(string LampIP)
    {
        while (true)
        {

        }
    }

    IEnumerator SendAnimationWorker(string lampIP)
    {
        while (true)
        {
            SendAnimationToLamps();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator SetDetectionFalse(string lampIP)
    {
        while (true)
        {
            SetDetectionMode(false, lampIP);
            yield return new WaitForSeconds(2.44f);
        }
    }

    IEnumerator RegisterDevice(string lampIP)
    {
        while (true)
        {
            //Register constantly!
            RegisterControllerToDevice(true, lampIP);
            yield return new WaitForSeconds(1.33f);
        }
    }

    IEnumerator PollLayersFromLamp(string lampIP)
    {
        //TODO: Broadcast and take latest layer
        var client = LampPollClient;
        var messageDTO = new LayerPollDTO() { PollLayers = true };
        var messageJSON = JsonConvert.SerializeObject(messageDTO);
        byte[] data = Encoding.ASCII.GetBytes(messageJSON);
        var lampPort = 30001;
        while (true)
        {
            if (StopLampCommunication.Contains(lampIP))
            {
                StopLampCommunication.Remove(lampIP);
                break;
            }

            IPEndPoint lampEndPoint = new IPEndPoint(IPAddress.Parse(lampIP), lampPort);
            client.Send(data, data.Length, lampEndPoint);
            Thread.Sleep(10);
            if (client.Available > 0)
            {
                var receivedMessageBytes = client.Receive(ref lampEndPoint);
                string sceneJSON = Encoding.UTF8.GetString(receivedMessageBytes);
                GetStrokeFromLamp(sceneJSON);
            }
            else
            {
                //Backup for direct connection
                IPEndPoint DirectLampEndpoint = new IPEndPoint(IPAddress.Parse("172.20.0.1"), lampPort);
                client.Send(data, data.Length, DirectLampEndpoint);
                Thread.Sleep(10);
                if (client.Available > 0)
                {
                    var receivedMessageBytes = client.Receive(ref DirectLampEndpoint);
                    string sceneJSON = Encoding.UTF8.GetString(receivedMessageBytes);
                    GetStrokeFromLamp(sceneJSON);
                }
            }

            //Receive structure and send to 
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }

    //Color calibration and converstion from ITSH to RGBW
    void SetupCalibrationTables()
    {
        WhiteCalibrationTemperatureNodes = new int[] { 0, 7710, 13107, 20817, 25445, 28527, 65535, 65535 };

        WhiteCalibrationTable = new int[][]
        {
            new int[] {255, 46, 0, 0},	 // 1500 K						// ToDo: needs some CCT tweaking
		    new int[] {250, 92, 0, 64}, 	 // 2500 K						// "
		    new int[] {232, 98, 0, 201},	     // 3200 K ~ 3100...3200 K		// "
		    new int[] {126, 108, 46, 255},	 // 4000 K ~ 4200...4300 K		// "
		    new int[] {96, 135, 109, 201},  // 4800 K ~ 4900 K				// "
		    new int[] {88, 154, 155, 176},	 // 5600 K ~ 5200 K				// "
		    new int[] {46, 148, 255, 117},  // 10000 K
		    new int[] {46, 148, 255, 117} 	 // 10000 K aux
        };

        HueCalibrationTable = new int[][]
        {
            new int[] {255, 0, 0},	// 0	<---- Red
		    new int[] {255, 4, 0},	// 10
		    new int[] {255, 16, 0},	// 20
		    new int[] {255, 64, 0},	// 30
		    new int[] {255, 128, 0},	// 40
		    new int[] {255, 192, 0},	// 50
		    new int[] {255, 255, 0},	// 60	<---- Yellow
		    new int[] {192, 255, 0},	// 70
		    new int[] {128, 255, 0},	// 80
		    new int[] {64, 255, 0},	// 90
		    new int[] {16, 255, 0},	// 100
		    new int[] {4, 255, 0},	// 110
		    new int[] {0, 255, 0},	// 120	<---- Green
		    new int[] {0, 255, 4},	// 130
		    new int[] {0, 255, 16},	// 140
		    new int[] {0, 255, 64},	// 150
		    new int[] {0, 255, 128},	// 160
		    new int[] {0, 255, 192},	// 170
		    new int[] {0, 255, 255},	// 180	<---- Cyan
		    new int[] {0, 192, 255},	// 190
		    new int[] {0, 128, 255},	// 200
		    new int[] {0, 64, 255},	// 210
		    new int[] {0, 16, 255},	// 220
		    new int[] {0, 4, 255},	// 230
		    new int[] {0, 0, 255},	// 240	<---- Blue
		    new int[] {4, 0, 255},	// 250
		    new int[] {16, 0, 255},	// 260
		    new int[] {64, 0, 255},	// 270
		    new int[] {128, 0, 255},	// 280
		    new int[] {192, 0, 255},	// 290
		    new int[] {255, 0, 255},	// 300	<---- Magenta
		    new int[] {255, 0, 192},	// 310
		    new int[] {255, 0, 128},	// 320
		    new int[] {255, 0, 64},	// 330
		    new int[] {255, 0, 16},	// 340
		    new int[] {255, 0, 4},	// 350
		    // Auxiliary
		    new int[] {255, 0, 0},	// 360  <---- Red (auxiliary wrap)
        };
    }

    public int[] ITSHtoRGBW(Vector4 ITSH)
    {
        if (WhiteCalibrationTable == null || WhiteCalibrationTemperatureNodes == null || HueCalibrationTable == null)
            SetupCalibrationTables();

        //T should be 0-65535 ~ 1500K - 10000K
        ITSH.y = (ITSH.y - 0.15f) * (10000f / 8500f);

        //Vector conversion
        Vector4 ITSH_16 = ITSH * 65535;
        int i = 0;
        int interpolation = 0;

        //White balance
        int[] WB_RGBW = new int[] { 0, 0, 0, 0 };

        int T = (int)ITSH_16.y;
        for (i = 0; i < WhiteCalibrationTemperatureNodes.Length - 2; i++)
        {
            if (T >= WhiteCalibrationTemperatureNodes[i] && T < WhiteCalibrationTemperatureNodes[i + 1])
            {
                //i = index;
                break;
            }
        }

        if (WhiteCalibrationTemperatureNodes[i + 1] == WhiteCalibrationTemperatureNodes[i])
        {
            interpolation = 0;
        }
        else
        {
            interpolation = ((10000 * (T - WhiteCalibrationTemperatureNodes[i]) / (WhiteCalibrationTemperatureNodes[i + 1] - WhiteCalibrationTemperatureNodes[i])));
        }


        for (int c = 0; c < 4; c++)
        {
            WB_RGBW[c] = WhiteCalibrationTable[i][c] + (WhiteCalibrationTable[i + 1][c] - WhiteCalibrationTable[i][c]) * interpolation / 10000;
        }

        //Hue
        int[] HS_RGBW = new int[] { 0, 0, 0, 0 };
        int H = (int)ITSH_16.w;

        for (i = 0; i < HueCalibrationTable.Length - 2; i++)
        {
            if (H / 1820 >= i && H / 1820 < i + 1)
            {
                //i = index;
                break;
            }
        }

        interpolation = (H - i * 1820) * 10000 / 1820;
        //NOTE: For some odd reason, this was c < 3
        for (int c = 0; c < 3; c++)
        {
            HS_RGBW[c] = HueCalibrationTable[i][c] + (HueCalibrationTable[i + 1][c] - HueCalibrationTable[i][c]) * interpolation / 10000;
        }

        //Saturation (Blend between White Balance and Hue)
        int[] Blend_RGBW = new int[] { 0, 0, 0, 0 };
        int S = (int)ITSH_16.z;

        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = HS_RGBW[c] * S / 65535 + WB_RGBW[c] * (65535 - S) / 65535;
        }

        //Intensity
        int I = (int)ITSH_16.x;
        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = Blend_RGBW[c] * I / 65535;
        }

        //Correction for overflow
        for (int c = 0; c < 4; c++)
        {
            Blend_RGBW[c] = Mathf.Min(255, Math.Max(0, Blend_RGBW[c]));
        }

        return Blend_RGBW;
    }

}

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
using Voyager.Lamps;
using Voyager.Networking;
using Voyager.Animation;

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

public class AnimationSender : MonoBehaviour
{
	[SerializeField] bool Debugging;
	[Header("Settings")]
	[SerializeField] float AskForScenesInterval = 0.2f;
	[Space(5)]
    public Scene scene;
    public DrawScripts drawScripts;
    //public SetupScripts setupScripts;
    public DrawMode draw;
    public Stroke ActiveStroke;

    public UdpClient LampCommunicationClient;
    public UdpClient LampPollClient;

    public IPEndPoint localEndpoint;

	LampManager lampManager;

    public double TimeOffset;

    List<string> StopLampCommunication = new List<string>();
    
	Dictionary<string, SceneSendingPackage> SendingPackages = new Dictionary<string, SceneSendingPackage>();
    public Dictionary<string,Dictionary<int, int[]>> LampIPVideoStreamPixelToColor = new Dictionary<string, Dictionary<int, int[]>>();

    public Vector4 LastVideoStreamColor = new Vector4(1f,0.56f,1f,0f); //Normal picture with 5600K
	public float LastGamma = 1f;
    
    //Color calibration
    int[] WhiteCalibrationTemperatureNodes;
    int[][] WhiteCalibrationTable;
    int[][] HueCalibrationTable;

	[Space(10)]
	public int videoStreamFPS = 30;
	float videoStreamInterval;
	Dictionary<string, int[][]> jsonDict = new Dictionary<string, int[][]>();
	string videoString = "VideoStream";
    string lastVideoJSON = "";

    void Awake()
    {
		/* if (LampCommunicationClient == null)
         {
             LampCommunicationClient = new UdpClient(31000);
         }*/

		lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();
		NetworkManager.OnLampSceneResponse += NetworkManager_OnLampSceneResponse;
		InvokeRepeating("AskForScenes", 0.0f, AskForScenesInterval);

        if (LampPollClient == null)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			var endPoint = new IPEndPoint(NetworkManager.GetWifiInterfaceAddress(), 30001);
            socket.Bind(endPoint);

            LampPollClient = new UdpClient();
            LampPollClient.Client = socket;
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        //Get local wireless endpoint
        SetLocalEndpoint();
#endif
    }

    void AskForScenes()
	{
		foreach(Lamp lamp in lampManager.GetLamps())
			NetworkManager.AskLampLayers(lamp.IP);
	}

	void NetworkManager_OnLampSceneResponse(string response, IPAddress ip)
    {
        Scene newScene = JsonConvert.DeserializeObject<Scene>(response);
		newScene = RemoveSceneTimeOffset(newScene);
		if (Debugging) Debug.Log(response);
		MergeScenes(newScene);
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
            var localIP = WirelessInterface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().Address;
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

		//StartCoroutine("SendAnimationWorker");

        videoStreamInterval = 1.0f / videoStreamFPS;
        jsonDict.Add(videoString, new int[1][]);
        InvokeRepeating("SendVideoStream", 1.0f, videoStreamInterval);
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
		if (scene.TimeStamp <= importScene.TimeStamp)
            return;

        foreach (var newLayer in importScene.Layers)
        {
            int LayerIndex = scene.Layers.FindIndex(l => l.LayerID == newLayer.LayerID);
            if (LayerIndex != -1)
            {
                scene.Layers[LayerIndex] = MergeLayers(scene.Layers[LayerIndex], newLayer);
            }
            //else
            //{
            //    //TODO: This part may not be used
            //    scene.AddLayer(newLayer);
            //}
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
		List<Stroke> sortedStrokes = ImportLayer.Strokes.OrderBy(x => x.TimeStamp).ToList();

		foreach (var newStroke in sortedStrokes)
        {
            //Add layer reference to imported stroke
            //TODO: Refactor
            newStroke.layer = OriginalLayer;

            var StrokeIndex = OriginalLayer.Strokes.FindIndex(s => s.StrokeID == newStroke.StrokeID);
            if (StrokeIndex != -1)
            {
                //Stroke is present in imported layer
				OriginalLayer.Strokes[StrokeIndex] = MergeStrokes(OriginalLayer.Strokes[StrokeIndex], newStroke);
				//OriginalLayer.Strokes[StrokeIndex] = newStroke;
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

		Stroke MergedStroke = OriginalStroke;
        
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

        //Merge partial strokes!!
		MergedStroke.PixelQueueToControlledPixel = OriginalStroke.PixelQueueToControlledPixel.Concat(ImportStroke.PixelQueueToControlledPixel).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.FirstOrDefault().Value);
        SelectPixelsFromDictionary(MergedStroke);

        return MergedStroke;
    }

    static void SelectPixelsFromDictionary(Stroke MergedStroke)
    {
		LampManager lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();      
        SortedDictionary<int, Pixel> QueueToPixel = new SortedDictionary<int, Pixel>();
        
		foreach(Lamp lamp in lampManager.GetLampsInWorkplace())
		{
			PhysicalLamp physicalLamp = lamp.physicalLamp;
			Ribbon ribbon = physicalLamp.GetComponent<Ribbon>();

			if (ribbon == null) continue;
            
			if (MergedStroke.PixelQueueToControlledPixel.ContainsKey(lamp.Serial))
			{
				foreach (var QueuePixelKeyValue in MergedStroke.PixelQueueToControlledPixel[lamp.Serial])
                {
					int pixelQueueNumber = QueuePixelKeyValue.Key;
					Pixel pixel = ribbon.pixelsParent.Find("pixel" + QueuePixelKeyValue.Value).GetComponent<Pixel>();
					if(pixel == null) throw new Exception("Couldn't retrieve pixel when merging!");
					if (!QueueToPixel.ContainsKey(pixelQueueNumber))
                        QueueToPixel.Add(pixelQueueNumber, pixel);
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

        //Set animation values
        drawScripts.SetAnimation(ActiveStroke.Animation, ActiveStroke.Properties);
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

    public void SendAnimationWithUpdate(Anim currentAnimation = null)
    {
        ActiveStroke.AddLatestTimestamp();
        scene.AddLatestTimeStamp();
        SendAnimationToLamps(currentAnimation);
    }

    public void SendAnimationToLamps(Anim currentAnimation = null)
    {      
        if(ActiveStroke.layer.PixelToStrokeIDDictionary.Any(x => x.Value.FirstOrDefault().Animation == "DMX"))
        {
            scene.ArtNetMode = true;
            scene.sACNMode = true;
        }
        else
        {
            scene.ArtNetMode = false;
            scene.sACNMode = false;
        }

		if (lampManager.GetLamps().Count == 0)
			return;

        //Get current animation properties
        var CurrentAnimation = currentAnimation == null? drawScripts.GetAnimation(): currentAnimation;

        if (ActiveStroke.Animation == CurrentAnimation.AnimName && ActiveStroke.Animation != "Video Stream")
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
        ActiveStroke.Properties = new Dictionary<string, int[]>(CurrentAnimation.Properties);

        if (ActiveStroke.Animation == "Video Stream")
        {
            var VideoColor = Array.ConvertAll(ActiveStroke.Properties["Color1"], c => (float)c);
            LastVideoStreamColor = new Vector4(VideoColor[0] / 100f, VideoColor[1] / 10000f, VideoColor[2] / 120f, VideoColor[3] / 360f);
			LastGamma = (float)ActiveStroke.Properties["Gammax10"][0];
        }

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

        if (ActiveStroke.Animation == "DMX")
        {
            int UnitSize = ActiveStroke.Properties["Unit size"][0];
            if (UnitSize == 0 || UnitSize == -1)
            {
                for (int c = 0; c < ActiveStroke.Properties["Division"][0]; c++)
                {
                    RGBWColors.Add(new int[] { (int)(ITSHColors[0].x * 100f), (int)((ITSHColors[0].y) * 10000f), (int)(ITSHColors[0].z * 120f), (int)(ITSHColors[0].w * 360f) });
                }
                
            }
            else //Pixel chunks
            {
                for (int c = 0; c <= ActiveStroke.TotalPixelCount / ActiveStroke.Properties["Unit size"][0]; c++)
                {
                    RGBWColors.Add(new int[] { (int)(ITSHColors[0].x * 100f), (int)((ITSHColors[0].y) * 10000f), (int)(ITSHColors[0].z * 120f), (int)(ITSHColors[0].w * 360f) });
                }
            }
        }
        else
        {
            foreach (var ITSHColor in ITSHColors)
            {
                RGBWColors.Add(ITSHtoRGBW(ITSHColor));
            }
        }

        if(ActiveStroke.ControlledPixels.Count == 0)
        {
            return;
        }

        //Adding DMX offsets so individual lamp can be controlled with offset
        Dictionary<string,Dictionary<string, int>> StrokeIDtoLampMactoDMXoffsetDictionary = new Dictionary<string, Dictionary<string, int>>();

        //TODO: General layer resolution
        foreach (var stroke in scene.Layers[0].Strokes)
        {
            if (stroke.Animation == "DMX" && stroke.Properties["Unit size"][0] == -1)
            {
                var LampDMXOrder = stroke.PixelQueueToControlledPixel.OrderBy(x => x.Value.FirstOrDefault().Key).GroupBy(x => x.Key).Select(x=> x.FirstOrDefault().Key).ToList();
                var DMXOffset = stroke.Properties["DMX offset"][0];

                Dictionary<string, int> LampMactoDMXoffsetDictionary = new Dictionary<string, int>();
                foreach (var lamp in LampDMXOrder)
                {
                    LampMactoDMXoffsetDictionary.Add(lamp, DMXOffset);
                    DMXOffset += 4;
                }
                StrokeIDtoLampMactoDMXoffsetDictionary.Add(stroke.StrokeID, LampMactoDMXoffsetDictionary);
            }
        }

        //TODO: This and previous part should be removed and calibration should happen on device
        ActiveStroke.Colors = RGBWColors;

		foreach (var lamp in lampManager.GetLampsInWorkplace())
        {
            //Partition strokes for each lamp!
            //Create copy of scene
            var partitionedScene = JsonConvert.DeserializeObject<Scene>(JsonConvert.SerializeObject(scene));
            for (int l = 0; l < scene.Layers.Count; l++)
            {
                for (int s = 0; s < scene.Layers[l].Strokes.Count; s++)
                {
					var initialDictionary = partitionedScene.Layers[l].Strokes[s].PixelQueueToControlledPixel.Where(d => d.Key == lamp.Serial).ToDictionary(d => d.Key, d => d.Value);
                    partitionedScene.Layers[l].Strokes[s].PixelQueueToControlledPixel = initialDictionary;

                    //Add DMX offset and universe!
                    if (scene.Layers[l].Strokes[s].Animation == "DMX")
                    {
                        if (scene.Layers[l].Strokes[s].Properties["Unit size"][0] == -1)
                        {
							int DMXOffset = StrokeIDtoLampMactoDMXoffsetDictionary[partitionedScene.Layers[l].Strokes[s].StrokeID][lamp.Serial];
                            if (DMXOffset > 508)
                            {
                                partitionedScene.Layers[l].Strokes[s].Universe = (DMXOffset + 4) / 512 + scene.Layers[l].Strokes[s].Properties["Universe offset"][0];
                                DMXOffset = DMXOffset % 512;
                                partitionedScene.Layers[l].Strokes[s].DMXOffset = DMXOffset;
                            }
                            else
                            {
                                partitionedScene.Layers[l].Strokes[s].Universe = scene.Layers[l].Strokes[s].Properties["Universe offset"][0];
                                partitionedScene.Layers[l].Strokes[s].DMXOffset = DMXOffset;
                            }
                        }
                        else
                        {
                            partitionedScene.Layers[l].Strokes[s].Universe = partitionedScene.Layers[l].Strokes[s].Properties["Universe offset"][0];
                            partitionedScene.Layers[l].Strokes[s].DMXOffset = partitionedScene.Layers[l].Strokes[s].Properties["DMX offset"][0];
                        }
                    }
                }
            }
			//Send stroke information to all lamps!
			//StrokeJSONData = SendJSONToLamp(scene, new IPEndPoint(IPAddress.Parse(LampIP), 30001));

			//New version to relieve the udp message length!

			if (SendingPackages.ContainsKey(lamp.Serial))
				SendingPackages[lamp.Serial] = SendJSONToLamp(partitionedScene, new IPEndPoint(lamp.IP, 30001));
			else
				SendingPackages.Add(lamp.Serial, SendJSONToLamp(partitionedScene, new IPEndPoint(lamp.IP, 30001)));
        }
    }

    void GetStrokeFromLamp(string sceneJSON)
    {
        Scene lampScene = JsonConvert.DeserializeObject<Scene>(sceneJSON);

        try
        {
            MergeScenes(lampScene);
        }
        catch (Exception e)
        {
            Debug.LogError("Scene merging failed! Error: " + e.ToString());
        }
        
    }

    public void RemoveLampFromStrokes(Transform lamp, string lampMac)
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

        StopLampCommunication.Add(lampMac);
        scene.TimeStamp -= 1;
    }

    public void StartPollingLayers(string LampMac)
    {
        //TODO: Generalize so it isn't lamp dependent.
        StartCoroutine(PollLayersFromLamp(LampMac));
    }
    
	SceneSendingPackage SendJSONToLamp(Scene sce, IPEndPoint lampEndPoint)
    {
		SceneSendingPackage package = new SceneSendingPackage
		{
			endpoint = lampEndPoint,
			scene = sce
		};
        SendSceneToLamp(package);
        return package;
    }

	Scene AddSceneTimeOffsets(Scene sce)
	{
		Scene returnScene = sce;

		returnScene.TimeStamp += NetworkManager.GetTimesyncOffset();
        
		foreach(Layer layer in scene.Layers)
			foreach (Stroke stroke in layer.Strokes)
                stroke.TimeStamp += NetworkManager.GetTimesyncOffset();

		return returnScene;
	}

    Scene RemoveSceneTimeOffset(Scene sce)
	{
		Scene returnScene = sce;

        returnScene.TimeStamp -= NetworkManager.GetTimesyncOffset();

        foreach (Layer layer in scene.Layers)
            foreach (Stroke stroke in layer.Strokes)
                stroke.TimeStamp -= NetworkManager.GetTimesyncOffset();

        return returnScene;
	}

	void SendSceneToLamp(SceneSendingPackage package)
    {
		Scene sceneToSend = AddSceneTimeOffsets(package.scene);
		string jsonData = JsonConvert.SerializeObject(sceneToSend);
		if (Debugging) Debug.Log("[OUT]" + jsonData);
		byte[] byteData = Encoding.ASCII.GetBytes(jsonData);
		NetworkManager.SendMessage(package.endpoint, byteData);
    }

    IEnumerator SendAnimationWorker()
    {
        while (true)
        {
			if (SendingPackages != null && lampManager.GetLamps().Count > 0)
            {
				foreach (var lamp in lampManager.GetConnectedLamps())
                {
					if(SendingPackages.ContainsKey(lamp.Serial))
						SendSceneToLamp(SendingPackages[lamp.Serial]);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
       
    void SendVideoStream()
    {
        //Dictionary<string, Dictionary<int, int[]>> jsonDict = new Dictionary<string, Dictionary<int, int[]>>();

		if (SendingPackages != null && lampManager.GetConnectedLamps().Count > 0)
        {
			if (ActiveStroke.layer.PixelToStrokeIDDictionary.Any(x => x.Value.FirstOrDefault().Animation == "Video Stream"))
            {
                foreach (var lamp in lampManager.GetConnectedLamps())
                {
                    string lampIP = lamp.IP.ToString();
					if(LampIPVideoStreamPixelToColor.ContainsKey(lampIP))
					{
						jsonDict[videoString] = LampIPVideoStreamPixelToColor[lampIP].Values.ToArray();
						var messageJSON = JsonConvert.SerializeObject(jsonDict);
						if (messageJSON != lastVideoJSON)
						{
							lastVideoJSON = messageJSON;
							var data = Encoding.ASCII.GetBytes(messageJSON);
							NetworkManager.SendMessage(new IPEndPoint(IPAddress.Parse(lampIP), 30001), data);
						}
                    }
                }
            }
        }
    }

    IEnumerator PollLayersFromLamp(string LampMac)
    {
        //TODO: Broadcast and take latest layer
        var client = LampPollClient;
        var messageDTO = new LayerPollDTO() { PollLayers = true };
        var messageJSON = JsonConvert.SerializeObject(messageDTO);
        byte[] data = Encoding.ASCII.GetBytes(messageJSON);
        var lampPort = 30001;
        while (true)
        {
			Lamp lamp = lampManager.GetLamp(LampMac);
            if (lamp == null) break;

			IPEndPoint lampEndPoint = new IPEndPoint(lamp.IP, lampPort);
            client.Send(data, data.Length, lampEndPoint);
            Thread.Sleep(10);
            if (client.Available > 0)
            {
                var receivedMessageBytes = client.Receive(ref lampEndPoint);
                string sceneJSON = Encoding.UTF8.GetString(receivedMessageBytes);
                Debug.Log(lampEndPoint.Address.ToString() + " = " + sceneJSON);
                GetStrokeFromLamp(sceneJSON);
            }
            //else
            //{
            //    //Backup for direct connection
            //    IPEndPoint DirectLampEndpoint = new IPEndPoint(IPAddress.Parse("172.20.0.1"), lampPort);
            //    client.Send(data, data.Length, DirectLampEndpoint);
            //    Thread.Sleep(10);
            //    if (client.Available > 0)
            //    {
            //        var receivedMessageBytes = client.Receive(ref DirectLampEndpoint);
            //        string sceneJSON = Encoding.UTF8.GetString(receivedMessageBytes);
            //        GetStrokeFromLamp(sceneJSON);
            //    }
            //}

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

public struct SceneSendingPackage
{
	public IPEndPoint endpoint;
	public Scene scene;
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Voyager.Lamps;
using Voyager.Workspace;

public class VideoStream : MonoBehaviour {


    //For VideoStream
    GameObject drawTools;
    GameObject videoStreamBackground;
    public GameObject minXY;
    public GameObject maxX;
    public GameObject maxY;
    WebCamTexture webcamTexture = null;
    List<int> pixelsToDraw;
    Texture2D tex = null;
    bool videoRunning = false;
    Color32[] colorArray = null;
    Color color = Color.white;

	Ribbon ribbon;
	DrawScripts drawScripts;

    List<Color> previousColors = new List<Color>();

    AnimationSender animSender;
    string IP;
    int[] blackColor = { 0, 0, 0, 0 };
    Color BlackColor = new Color(0, 0, 0, 0);

	UnityAction workPlaceListener;

	Dictionary<int, float> xPositions = new Dictionary<int, float>();
	Dictionary<int, float> yPositions = new Dictionary<int, float>();

	PhysicalLamp physicalLamp;

	[SerializeField] Transform PixelsParent;

	// Use this for initialization

	void Awake()
	{
		workPlaceListener = new UnityAction(OnWorkplaceObjectMoved);
	}

	void OnEnable()
	{
		EventManager.StartListening("WorkplaceObjectMoved", workPlaceListener);
	}

	void OnDisable()
	{
		EventManager.StopListening("WorkplaceObjectMoved", workPlaceListener);
	}

	void Start() {
		physicalLamp = GetComponent<PhysicalLamp>();
		ribbon = GetComponent<Ribbon>();
		//drawScripts = drawTools.GetComponent<DrawScripts>();
		//NOTE: Quick hack because switching all the references is too much work at this point
    }
    
	void SetupVideoStream(Transform videoStream)
	{
		drawScripts = GameObject.Find("MenuBackground").GetComponent<MenuMode>().drawTools.GetComponent<DrawScripts>();
        animSender = GameObject.Find("AnimationControl").GetComponent<AnimationSender>();
		Transform videoStreamCon = videoStream.GetChild(0).GetChild(0);
		minXY = videoStreamCon.GetChild(0).gameObject;
		maxX = videoStreamCon.GetChild(1).gameObject;
		maxY = videoStreamCon.GetChild(2).gameObject;

        StartCoroutine(CheckForVideo());

        //Initialization of video stream array
        IP = ribbon.IP;
        if (!animSender.LampIPVideoStreamPixelToColor.ContainsKey(IP))
        {
            animSender.LampIPVideoStreamPixelToColor.Add(IP, new Dictionary<int, int[]>());
        }
        animSender.LampIPVideoStreamPixelToColor[IP].Clear();
        int PixelCount = ribbon.pipeLength;
        for (int p = 0; p < PixelCount; p++)
        {
            animSender.LampIPVideoStreamPixelToColor[IP].Add(p, blackColor);
            previousColors.Add(BlackColor);
        }
	}

    IEnumerator CheckForVideo() {
        while (!videoRunning)
        {         
			if (drawScripts.videoTexture != null)
			{
				videoRunning = true;
				OnWorkplaceObjectMoved();
            }

            yield return new WaitForSeconds(1);
        }
    }

    void OnWorkplaceObjectMoved()
	{
		if (drawScripts == null || minXY == null)
			return;
		
		if (drawScripts.videoTexture != null)
        {
			var numPixelsToDraw = ribbon.pipeLength;
            
            string pixelName;

			xPositions.Clear();
			yPositions.Clear();

            for (int i = 0; i < numPixelsToDraw; i++)
            {
				pixelName = "pixel" + i;

                //TODO: Move calculation to OnDrag!
				var lampPixelLED = PixelsParent.Find(pixelName).Find("LEDmodule");
                var lampPixelCenter = lampPixelLED.GetComponent<Renderer>().bounds.center;

                //Find position of video stream pixel corresponding to lampPixelCenter
                var Xs = maxX.transform.position - minXY.transform.position;
                var Ys = maxY.transform.position - minXY.transform.position;

                var Vp = lampPixelCenter - minXY.transform.position;

                var Vx = Vector3.Project(Vp, Xs);
                var Vy = Vector3.Project(Vp, Ys);

                //Check if in limits
                if (Vx.normalized == Xs.normalized && Vy.normalized == Ys.normalized &&
                    Vx.magnitude <= Xs.magnitude && Vy.magnitude <= Ys.magnitude)
                {
                    Color pixelColor = Color.white; // default color

					if (drawScripts.videoTexture != null)
                    {
						xPositions.Add(i, drawScripts.videoTexture.width * (Vx.magnitude / Xs.magnitude));
						yPositions.Add(i, drawScripts.videoTexture.height * (Vy.magnitude / Ys.magnitude));
					}
                }
                else
					animSender.LampIPVideoStreamPixelToColor[IP][i] = blackColor;      
            }
        }
	}

	void DrawPixels()
	{
		if (drawScripts.videoTexture != null)
        {
            var numPixelsToDraw = ribbon.pipeLength;

            for (int i = 0; i < numPixelsToDraw; i++)
            {  
				if(xPositions.ContainsKey(i) && yPositions.ContainsKey(i))
				{
					Color pixelColor = Color.white;

                    if (drawScripts.videoTexture != null)
                        pixelColor = drawScripts.videoTexture.GetPixel((int)xPositions[i], (int)yPositions[i]);

					if (!((pixelColor - previousColors[i]).maxColorComponent >= 0.02f || (previousColors[i] - pixelColor).maxColorComponent >= 0.02f))
						continue;
					
					previousColors[i] = pixelColor;
                    float I = 0;
                    float S = 0;
                    float H = 0;
                    Color.RGBToHSV(pixelColor, out H, out S, out I);

                    var c = animSender.LastVideoStreamColor;
                    //ITSH with color correction
                    Vector4 itsh = new Vector4(I * c.x, c.y, S * c.z, (H + c.w) % 1f);

                    if (ribbon.pipeLength < 30)
                        animSender.LampIPVideoStreamPixelToColor[IP][i] = new int[]
                        {
                            (int)(itsh.x * 100),
                            (int)(itsh.y * 8500 + 1500),
                            (int)(itsh.z * 120),
                            (int)(itsh.w * 360)
                        };
                    else
						animSender.LampIPVideoStreamPixelToColor[IP][i] = GammaCorrection(ribbon.ITSHtoRGBW(itsh), animSender.LastGamma/10f);
				}
            }
        }
	}

	private int[] GammaCorrection(int[] pixelColor, float GammaValue)
	{
		for (int i = 0; i < pixelColor.Length; i++) {
			float G = Mathf.Pow ((float)pixelColor [i] / 255f, 1 / GammaValue);
			pixelColor [i] = (int)(G * (float)pixelColor [i]);
		}
		return pixelColor;
	}
 

     // Update is called once per frame
    void Update()
    {
		if(minXY == null || maxX == null || maxY == null)
		{
			if (Workspace.GetVideoSteam() != null)
                SetupVideoStream(Workspace.GetVideoSteam());
		}

		if(animSender != null)
        {
			if (animSender.ActiveStroke.Animation != "Video Stream")
			{
				if (!animSender.ActiveStroke.layer.PixelToStrokeIDDictionary.Any(x => x.Value.FirstOrDefault().Animation == "Video Stream"))
				{
					if(Workspace.ContainsVideoStream())
					{
						//Transform videoStream = Workspace.GetVideoSteam();
						//Workspace.DestroyItem(videoStream.GetComponent<WorkspaceItem>());
					}
                }
			}
			else
			{
				//if (!Workspace.ContainsVideoStream())
					//SetupVideoStream(Workspace.InstantiateVideoStream());
			}
			
			DrawPixels();
        }
    }
}
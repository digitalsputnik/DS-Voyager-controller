using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VideoStream : MonoBehaviour {


    //For VideoStream
    public GameObject drawTools;
    public GameObject videoStreamBackground;
    public GameObject minXY;
    public GameObject maxX;
    public GameObject maxY;
    WebCamTexture webcamTexture = null;
    List<int> pixelsToDraw;
    Texture2D tex = null;
    bool videoRunning = false;
    Color32[] colorArray = null;
    Color color = Color.white;

    List<Color> previousColors = new List<Color>();

    AnimationSender animSender;
    string IP;
    int[] blackColor = new int[] { 0, 0, 0, 0 };
    Color BlackColor = new Color(0, 0, 0, 0);


    // Use this for initialization
    void Start() {
        Debug.Log("VideoStream script started.......");
        
        //NOTE: Quick hack because switching all the references is too much work at this point
        animSender = GameObject.Find("AnimationControl").GetComponent<AnimationSender>();

        StartCoroutine(CheckForVideo());

        //Initialization of video stream array
        IP = this.gameObject.GetComponent<Ribbon>().IP;
        if (!animSender.LampIPVideoStreamPixelToColor.ContainsKey(IP))
        {
            animSender.LampIPVideoStreamPixelToColor.Add(IP, new Dictionary<int, int[]>());
        }

        int PixelCount = this.gameObject.GetComponent<Ribbon>().pipeLength;
        for (int p = 0; p < PixelCount; p++)
        {
            animSender.LampIPVideoStreamPixelToColor[IP].Add(p, blackColor);
            previousColors.Add(BlackColor);
        }
    }

    IEnumerator CheckForVideo() {
        while (!videoRunning)
        {
            if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
            {
                videoRunning = true;
            }

            yield return new WaitForSeconds(1);

        }

        StartCoroutine(DrawVideoOnPixels());
    }


    IEnumerator DrawVideoOnPixels() {

        while (true)
        {

            //Pick webCamTexture pixels
            if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
            {
                //Find total number of lamp pixels
                var numPixelsToDraw = this.gameObject.GetComponent<Ribbon>().pipeLength;

                //pixelsToDraw = this.gameObject.GetComponent<DragAndDropHandler>().VideoPixels;
                string pixelName;

                //Start drawing pixels
                for (int i = 0; i < numPixelsToDraw; i++)
                {
                    //if (pixelsToDraw.Count > 0)
                    //{
                    //    Debug.Log("DragAndDropHandler returned pixels to draw........");
                    //    pixelName = "pixel" + pixelsToDraw[i];
                    //}
                    //else
                    //{
                        pixelName = "pixel" + i;
                    //}

                    //TODO: Move calculation to OnDrag!
                    var lampPixelLED = transform.Find(pixelName).Find("LEDmodule");
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
                        var pointX = 0.0f;
                        var pointY = 0.0f;
                        Color pixelColor = Color.white; // default color

                        if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
                        {
                            pointX = drawTools.GetComponent<DrawScripts>().webcamTexture.width * (Vx.magnitude / Xs.magnitude);
                            pointY = drawTools.GetComponent<DrawScripts>().webcamTexture.height * (Vy.magnitude / Ys.magnitude);

                            //Get the color
                            pixelColor = drawTools.GetComponent<DrawScripts>().webcamTexture.GetPixel((int)pointX, (int)pointY);
                        }
                        //else if (mediaPlayer.IsPlaying && mediaPlayer.FramePixels.Length > 0)
                        //{

                        //    //Debug.Log("Frame Pixel data: "+ mediaPlayer.FramePixels.ToString());
                        //    if (tex == null)
                        //    {
                        //        tex = new Texture2D(mediaPlayer.VideoWidth, mediaPlayer.VideoHeight, TextureFormat.BGRA32, false);
                        //    }

                        //    var texData = mediaPlayer.FramePixels;

                        //    if (texData.Length > 0)
                        //    {
                        //        tex.LoadRawTextureData(texData);
                        //        tex.Apply();
                        //    }

                        //    pointX = mediaPlayer.VideoWidth * (Vx.magnitude / Xs.magnitude);
                        //    pointY = mediaPlayer.VideoHeight * (Vy.magnitude / Ys.magnitude);

                        //    //Get the color
                        //    pixelColor = tex.GetPixel((int)pointX, (int)pointY);
                        //    Debug.Log("Pixel color is: " + pixelColor.ToString());

                        //}

                        //NOTE: Since maxColorComponent doesn't account for negative maximum, both sides are checked
                        if (!((pixelColor - previousColors[i]).maxColorComponent >= 0.02f || (previousColors[i] - pixelColor).maxColorComponent >= 0.02f))
                        {
                            //previousColors[i] = pixelColor;
                            continue;
                        }

                        previousColors[i] = pixelColor;
                        float I = 0;
                        float S = 0;
                        float H = 0;
                        Color.RGBToHSV(pixelColor, out H, out S, out I);

                        var c = animSender.LastVideoStreamColor;
                        //ITSH with color correction
                        Vector4 itsh = new Vector4(I*c.x, c.y, S*c.z, (H + c.w) % 1f);

                        //Apply color
                        //lampPixelLED.GetComponent<Renderer>().material.color = pixelColor;
                        if (this.gameObject.GetComponent<Ribbon>().pipeLength < 30)
                        {
                            animSender.LampIPVideoStreamPixelToColor[IP][i] = new int[] { (int)(itsh.x * 100), (int)(itsh.y * 8500 + 1500), (int)(itsh.z * 120), (int)(itsh.w * 360) };
                        }
                        else
                        {
                            animSender.LampIPVideoStreamPixelToColor[IP][i] = this.gameObject.GetComponent<Ribbon>().ITSHtoRGBW(itsh);//new int[] { (int)(I * 100), 0, (int)(S * 120), (int)(H*360) };
                        }

                    }
                    else
                    {
                        //lampPixelLED.GetComponent<Renderer>().material.color = Color.black;
                        animSender.LampIPVideoStreamPixelToColor[IP][i] = blackColor;
                    }



                }
            }
            else
            {
                //Debug.Log("Video not playing...");

            }
            yield return new WaitForSeconds(0.0f);
        }

    }

 

     // Update is called once per frame
    void Update()
    {

     /*   if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
        {
            //Debug.Log("Found WebCamTexture!!!");
            webcamTexture = drawTools.GetComponent<DrawScripts>().webcamTexture;
            //VideoPixels = new Dictionary<int, Color>();
        }

        //Pick webCamTexture pixels
        if (webcamTexture != null)
        {
            //Find total number of lamp pixels
            var totalLampPixels = this.gameObject.GetComponent<Ribbon>().pipeLength;

            pixelsToDraw = this.gameObject.GetComponent<DragAndDropHandler>().VideoPixels;
            var numPixelsToDraw = 0;
            string pixelName;

            //Get pixels to be drawn
            if (pixelsToDraw.Count > 0)
            {
                numPixelsToDraw = pixelsToDraw.Count;

            }
            else
            {
                numPixelsToDraw = totalLampPixels;
            }


            for (int i = 0; i < numPixelsToDraw; i++)
            {
                if (pixelsToDraw.Count > 0)
                {
                    pixelName = "pixel" + pixelsToDraw[i];
                }
                else
                {
                    pixelName = "pixel" + i;
                }
                var lampPixelLED = transform.Find(pixelName).Find("LEDmodule");
                var lampPixelCenter = lampPixelLED.GetComponent<Renderer>().bounds.center;

                RaycastHit hit;
                float rayLength = 0.5f;
                Ray ray = new Ray(lampPixelLED.position, Vector3.forward);
                Color pixelColor = Color.white; // default color when the raycast fails for some reason ;)
                if (Physics.Raycast(ray, out hit, rayLength))
                {
                    Debug.Log("Succesfully hit object: " + hit.transform.name);
                    //if (!texCreated)
                    //{
                        Texture BackgroundTexture = videoStreamBackground.GetComponent<Renderer>().material.mainTexture;
                        tex = new Texture2D(BackgroundTexture.width, BackgroundTexture.height, TextureFormat.ARGB32, false);
                        //Texture2D tex = videoStreamBackground.GetComponent<Renderer>().material.mainTexture as Texture2D;
                        Color[] pixels = webcamTexture.GetPixels();
                        tex.SetPixels(pixels);
                        tex.Apply();
                        //texCreated = true;
                        //Debug.Log("Texture created!!!");
                    //}

                    //Texture2D tex = new Texture2D(videoStreamBackground.GetComponent<Renderer>().material.mainTexture.width, videoStreamBackground.GetComponent<Renderer>().material.mainTexture.height);
                    //IntPtr pointer = webcamTexture.GetNativeTexturePtr();
                    //tex.UpdateExternalTexture(pointer);

                    //tex = videoStreamBackground.GetComponent<Renderer>().material.mainTexture as Texture2D;
                    pixelColor = tex.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
                    //Debug.Log("Pixel color is: "+ pixelColor.ToString());
                }


                //var screenPoint = Camera.main.WorldToScreenPoint(pixelPosition);
                lampPixelLED.GetComponent<Renderer>().material.color = pixelColor; //webcamTexture.GetPixel(Convert.ToInt32(screenPoint.x), Convert.ToInt32(screenPoint.y));
                                                                                   //Debug.Log("Pixel: " + pixelName);

                //Debug.Log("World point: " + pixelPosition);
                //Debug.Log("Screen point: " + screenPoint);
                
            }

        }
        else
        {
            Debug.Log("Video not running...");
        }
        */

    }
        

    
}

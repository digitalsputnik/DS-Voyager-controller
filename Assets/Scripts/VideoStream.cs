using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoStream : MonoBehaviour {


    //For VideoStream
    public GameObject drawTools;
    public GameObject videoStreamBackground;
    WebCamTexture webcamTexture = null;
    List<int> pixelsToDraw;
    Texture2D tex = null;
    bool videoRunning = false;


    // Use this for initialization
    void Start () {
        StartCoroutine(CheckForVideo());
    }

    IEnumerator CheckForVideo() {
        while (!videoRunning)
        {
            if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
            {
                //Debug.Log("Found WebCamTexture!!!");
                webcamTexture = drawTools.GetComponent<DrawScripts>().webcamTexture;
                //VideoPixels = new Dictionary<int, Color>();
                videoRunning = true;
            }

            yield return new WaitForSeconds(2);

        }

        StartCoroutine(DrawVideoOnPixels());
    }


    IEnumerator DrawVideoOnPixels() {

        while (true)
        {

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

                //Start drawing pixels
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
                        Texture BackgroundTexture = videoStreamBackground.GetComponent<Renderer>().material.mainTexture;
                        tex = new Texture2D(BackgroundTexture.width, BackgroundTexture.height, TextureFormat.ARGB32, false);
                        //tex = videoStreamBackground.GetComponent<Renderer>().material.mainTexture as Texture2D;
                        Color[] pixels = webcamTexture.GetPixels();
                        tex.SetPixels(pixels);
                        tex.Apply();

                        pixelColor = tex.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
                        //Debug.Log("Pixel color is: "+ pixelColor.ToString());
                    }


                    //var screenPoint = Camera.main.WorldToScreenPoint(pixelPosition);
                    lampPixelLED.GetComponent<Renderer>().material.color = pixelColor;
                    //Debug.Log("Pixel: " + pixelName);

                    //Debug.Log("World point: " + pixelPosition);
                    //Debug.Log("Screen point: " + screenPoint);

                }
                //yield return new WaitForSeconds(1);

            }
            else
            {
                Debug.Log("Video not running...");
            }
            yield return new WaitForSeconds(3);
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
